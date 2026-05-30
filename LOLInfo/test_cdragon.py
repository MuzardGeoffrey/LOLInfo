"""
Test CDragon — validation complète sur tous les champions.

Reproduit la logique C# (ChampionNameNormalizer + CdragonChampionParser)
en Python pour valider avant intégration WPF.

Collecte :
  - Champions introuvables sur CDragon (problème de normalisation)
  - Types __type non gérés dans notre parser
  - Valeurs mStat hors de notre enum ChampionStat
  - Champions sans aucun calcul dans leurs sorts
  - Sorts sans formule malgré la présence de mSpellCalculations
  - Exceptions / erreurs de parsing
"""

import asyncio
import aiohttp
import json
import os
import re
import unicodedata
from collections import defaultdict

# ── Configuration ────────────────────────────────────────────────────────────

DDRAGON_VERSION_URL = "https://ddragon.leagueoflegends.com/api/versions.json"
DDRAGON_CHAMPIONS_URL = "https://ddragon.leagueoflegends.com/cdn/{version}/data/fr_FR/champion.json"
CDRAGON_BIN_URL = "https://raw.communitydragon.org/latest/game/data/characters/{name}/{name}.bin.json"

# Types de formulaParts que notre C# gère
KNOWN_TYPES = {
    "NumberCalculationPart",
    "EffectValueCalculationPart",
    "NamedDataValueCalculationPart",
    "StatByNamedDataValueCalculationPart",
}

# Notre enum ChampionStat (0–11)
KNOWN_STATS = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11}

# Concurrence max pour ne pas flood CDragon
MAX_CONCURRENCY = 8

# ── Normalisation (miroir de ChampionNameNormalizer.cs) ──────────────────────

def normalize_name(name: str) -> str:
    """Reproduit ChampionNameNormalizer.Normalize()."""
    lower = name.lower()
    # Supprime les accents (NFD → ASCII)
    nfd = unicodedata.normalize("NFD", lower)
    ascii_only = "".join(c for c in nfd if unicodedata.category(c) != "Mn")
    # Supprime tout ce qui n'est pas a-z ou 0-9
    return re.sub(r"[^a-z0-9]", "", ascii_only)

# ── Format d'une formule (miroir de SpellCalculation.Format()) ───────────────

def format_part(part: dict, effect_amount: list, data_values: dict) -> str:
    t = part.get("__type", "")

    if t == "NumberCalculationPart":
        v = part.get("mNumber", 0)
        return str(int(v)) if v == int(v) else f"{v:.2g}"

    if t == "EffectValueCalculationPart":
        idx = part.get("mEffectIndex", 1) - 1
        if 0 <= idx < len(effect_amount):
            vals = effect_amount[idx]
            return "/".join(str(int(v)) if v == int(v) else f"{v:.2g}" for v in vals)
        return "?"

    if t == "NamedDataValueCalculationPart":
        key = part.get("mDataValue", "")
        vals = data_values.get(key, [])
        if vals:
            return "/".join(str(int(v)) if v == int(v) else f"{v:.2g}" for v in vals)
        return f"?({key})"

    if t == "StatByNamedDataValueCalculationPart":
        key   = part.get("mDataValue", "")
        stat  = part.get("mStat", -1)
        vals  = data_values.get(key, [])
        stat_labels = {1:"AD", 2:"vit.att", 3:"PA", 4:"armure", 5:"RM",
                       6:"PV", 7:"vit.dépl", 8:"crit", 9:"régén.PV",
                       10:"mana", 11:"régén.mana"}
        label = stat_labels.get(stat, f"stat#{stat}")
        if vals:
            pct = "/".join(
                (str(int(round(v*100))) + "%" if stat in {1,3,4,5,6,10} else f"{v:.2g}")
                for v in vals
            )
            return f"+{pct} {label}"
        return f"+?{label}"

    return f"[{t}?]"

def format_calculation(calc: dict, effect_amount: list, data_values: dict) -> str:
    parts = calc.get("mFormulaParts", [])
    if not parts:
        return ""
    return " + ".join(format_part(p, effect_amount, data_values) for p in parts)

# ── Parsing d'un bin.json ─────────────────────────────────────────────────────

def parse_bin(data: dict, norm_name: str) -> dict:
    """
    Extrait les sorts et leurs formules depuis le bin.json CDragon.
    Retourne un dict avec les résultats et les anomalies trouvées.
    """
    result = {
        "spells_found": 0,
        "spells_with_calcs": 0,
        "total_formulas": 0,
        "unknown_types": defaultdict(int),   # type → count
        "unknown_stats": defaultdict(int),   # stat_id → count
        "empty_formulas": [],                # (spell_id, calc_name)
        "spells_without_calcs": [],          # spell_id
        "parse_errors": [],
        "sample_formulas": {},               # spell_id → {calc_name: formula_str}
    }

    for key, val in data.items():
        if "/spells/" not in key.lower():
            continue
        if not isinstance(val, dict):
            continue
        # Les données de sort sont dans mSpell (SpellObject) ou directement (ancien format)
        spell_data = val.get("mSpell", val) if val.get("__type") == "SpellObject" else val
        if not isinstance(spell_data, dict):
            continue

        spell_id = key.split("/")[-1]
        result["spells_found"] += 1

        # Prépare effect_amount
        raw_effect = spell_data.get("mEffectAmount", [])
        effect_amount = [row if isinstance(row, list) else [] for row in raw_effect]

        # DataValues peut s'appeler "DataValues" ou "mDataValues" selon le patch
        raw_dv = spell_data.get("DataValues") or spell_data.get("mDataValues") or []
        data_values = {}
        for dv in raw_dv:
            if not isinstance(dv, dict):
                continue
            name = dv.get("mName") or dv.get("name")
            vals = dv.get("mValues") or dv.get("values")
            if name and vals:
                data_values[name] = vals

        calcs = spell_data.get("mSpellCalculations")
        if not calcs:
            result["spells_without_calcs"].append(spell_id)
            continue

        result["spells_with_calcs"] += 1
        result["sample_formulas"][spell_id] = {}

        for calc_name, calc in calcs.items():
            if not isinstance(calc, dict):
                continue

            parts = calc.get("mFormulaParts", [])
            if not parts:
                continue

            result["total_formulas"] += 1

            # Vérifie les types inconnus et stats inconnues
            for p in parts:
                if not isinstance(p, dict):
                    continue
                t = p.get("__type", "")
                if t and t not in KNOWN_TYPES:
                    result["unknown_types"][t] += 1

                if t == "StatByNamedDataValueCalculationPart":
                    stat = p.get("mStat", -1)
                    if stat not in KNOWN_STATS:
                        result["unknown_stats"][stat] += 1

            # Essaie de formater
            try:
                formula = format_calculation(calc, effect_amount, data_values)
                if not formula.strip():
                    result["empty_formulas"].append((spell_id, calc_name))
                else:
                    result["sample_formulas"][spell_id][calc_name] = formula
            except Exception as e:
                result["parse_errors"].append(f"{spell_id}/{calc_name}: {e}")

    return result

# ── Fetch asynchrone ──────────────────────────────────────────────────────────

async def fetch_champion(session: aiohttp.ClientSession, name: str, norm_name: str,
                          sem: asyncio.Semaphore) -> dict:
    url = CDRAGON_BIN_URL.format(name=norm_name)
    async with sem:
        try:
            async with session.get(url, timeout=aiohttp.ClientTimeout(total=30)) as resp:
                if resp.status == 404:
                    return {"champion": name, "norm": norm_name, "status": "404_not_found"}
                if resp.status != 200:
                    return {"champion": name, "norm": norm_name, "status": f"http_{resp.status}"}

                text = await resp.text()
                data = json.loads(text)
                parsed = parse_bin(data, norm_name)
                parsed["champion"] = name
                parsed["norm"] = norm_name
                parsed["status"] = "ok"
                return parsed

        except asyncio.TimeoutError:
            return {"champion": name, "norm": norm_name, "status": "timeout"}
        except json.JSONDecodeError as e:
            return {"champion": name, "norm": norm_name, "status": f"json_error: {e}"}
        except Exception as e:
            return {"champion": name, "norm": norm_name, "status": f"error: {e}"}

async def main():
    # 1. Récupère la version courante
    async with aiohttp.ClientSession() as session:
        async with session.get(DDRAGON_VERSION_URL) as r:
            versions = await r.json(content_type=None)
            version = versions[0]
        print(f"Version DataDragon : {version}")

        # 2. Récupère la liste des champions
        url = DDRAGON_CHAMPIONS_URL.format(version=version)
        async with session.get(url) as r:
            champ_data = await r.json(content_type=None)

    champions = champ_data["data"]  # dict id → champion
    print(f"{len(champions)} champions trouvés\n")

    # 3. Teste chaque champion
    sem = asyncio.Semaphore(MAX_CONCURRENCY)
    results = []

    async with aiohttp.ClientSession() as session:
        tasks = [
            fetch_champion(session, champ["name"], normalize_name(champ["id"]), sem)
            for champ in champions.values()
        ]

        done = 0
        for coro in asyncio.as_completed(tasks):
            r = await coro
            results.append(r)
            done += 1
            status = r.get("status", "?")
            icon = "OK " if status == "ok" else "ERR"
            print(f"  [{done:3}/{len(tasks)}] {icon} {r['champion']} ({r['norm']}) - {status}")

    # 4. Agrège les résultats
    aggregate = {
        "version": version,
        "total_champions": len(champions),
        "ok": [],
        "not_found": [],
        "errors": [],
        "no_spell_calcs": [],       # ok mais aucun sort avec mSpellCalculations
        "all_unknown_types": defaultdict(int),
        "all_unknown_stats": defaultdict(int),
        "all_empty_formulas": [],
        "all_parse_errors": [],
        "total_spells": 0,
        "total_spells_with_calcs": 0,
        "total_formulas": 0,
        "sample_formulas": {},      # champion → spell_id → {calc: formula}
    }

    for r in results:
        name = r["champion"]
        status = r.get("status", "?")

        if status == "404_not_found":
            aggregate["not_found"].append((name, r["norm"]))
        elif status != "ok":
            aggregate["errors"].append((name, r["norm"], status))
        else:
            aggregate["ok"].append(name)
            aggregate["total_spells"]            += r["spells_found"]
            aggregate["total_spells_with_calcs"] += r["spells_with_calcs"]
            aggregate["total_formulas"]          += r["total_formulas"]

            for t, c in r["unknown_types"].items():
                aggregate["all_unknown_types"][t] += c
            for s, c in r["unknown_stats"].items():
                aggregate["all_unknown_stats"][s] += c

            aggregate["all_empty_formulas"].extend(
                [(name, sid, cn) for sid, cn in r["empty_formulas"]]
            )
            aggregate["all_parse_errors"].extend(r["parse_errors"])

            if r["spells_with_calcs"] == 0:
                aggregate["no_spell_calcs"].append(name)

            if r["sample_formulas"]:
                aggregate["sample_formulas"][name] = r["sample_formulas"]

    # 5. Sauvegarde les résultats bruts
    base_dir = os.path.dirname(os.path.abspath(__file__))
    out_path = os.path.join(base_dir, "cdragon_results.json")
    with open(out_path, "w", encoding="utf-8") as f:
        json.dump(aggregate, f, ensure_ascii=False, indent=2, default=str)

    print(f"\n=== Résultats sauvegardés dans {out_path} ===")
    return aggregate, base_dir


def generate_markdown_report(results: dict, base_dir: str):
    """Génère un rapport Markdown complet à partir des résultats."""
    from datetime import datetime  # noqa (os already imported at top)
    now = datetime.now().strftime("%d/%m/%Y %H:%M")

    ok_count    = len(results['ok'])
    nf_count    = len(results['not_found'])
    err_count   = len(results['errors'])
    nc_count    = len(results['no_spell_calcs'])
    ef_count    = len(results['all_empty_formulas'])
    pe_count    = len(results['all_parse_errors'])
    unk_types   = dict(results['all_unknown_types'])
    unk_stats   = dict(results['all_unknown_stats'])

    total       = results['total_champions']
    total_sp    = results['total_spells']
    total_sc    = results['total_spells_with_calcs']
    total_fo    = results['total_formulas']
    version     = results['version']

    pct_ok      = round(ok_count / total * 100, 1) if total else 0
    pct_sc      = round(total_sc / total_sp * 100, 1) if total_sp else 0

    lines = []
    lines.append(f"# LOLInfo — Rapport de Validation CDragon\n")
    lines.append(f"**Généré le** : {now}  ")
    lines.append(f"**Version DataDragon** : {version}\n")

    # ── Résumé ──────────────────────────────────────────────────────────────────
    lines.append("## 1. Résumé exécutif\n")
    lines.append("| Métrique | Valeur |")
    lines.append("|---|---|")
    lines.append(f"| Champions testés | {total} |")
    lines.append(f"| ✅ Champions OK | {ok_count} ({pct_ok}%) |")
    lines.append(f"| ❌ Introuvables (404) | {nf_count} |")
    lines.append(f"| ⚠️ Erreurs réseau/parse | {err_count} |")
    lines.append(f"| Sans calculs CDragon | {nc_count} |")
    lines.append(f"| Total sorts analysés | {total_sp} |")
    lines.append(f"| Sorts avec formules | {total_sc} ({pct_sc}%) |")
    lines.append(f"| Total formules | {total_fo} |")
    lines.append(f"| Formules vides | {ef_count} |")
    lines.append(f"| Erreurs de parsing | {pe_count} |")
    lines.append(f"| Types `__type` inconnus | {len(unk_types)} |")
    lines.append(f"| Valeurs `mStat` inconnues | {len(unk_stats)} |")
    lines.append("")

    # ── Champions introuvables ───────────────────────────────────────────────────
    lines.append("## 2. Champions introuvables sur CDragon (404)\n")
    if results['not_found']:
        lines.append("| Champion | Nom normalisé |")
        lines.append("|---|---|")
        for name, norm in results['not_found']:
            lines.append(f"| {name} | `{norm}` |")
    else:
        lines.append("✅ Aucun champion introuvable. Normalisation correcte pour tout le roster.")
    lines.append("")

    # ── Erreurs réseau ────────────────────────────────────────────────────────────
    lines.append("## 3. Erreurs réseau / parse\n")
    if results['errors']:
        lines.append("| Champion | Nom normalisé | Erreur |")
        lines.append("|---|---|---|")
        for name, norm, status in results['errors']:
            lines.append(f"| {name} | `{norm}` | {status} |")
    else:
        lines.append("✅ Aucune erreur réseau.")
    lines.append("")

    # ── Types inconnus ────────────────────────────────────────────────────────────
    lines.append("## 4. Types `__type` non gérés par le parser C#\n")
    if unk_types:
        lines.append("| `__type` inconnu | Occurrences | Action requise |")
        lines.append("|---|---|---|")
        for t, c in sorted(unk_types.items(), key=lambda x: -x[1]):
            lines.append(f"| `{t}` | {c} | Créer une classe `IFormulaPart` dédiée |")
    else:
        lines.append("✅ Aucun type inconnu — notre parser couvre 100 % des types rencontrés.")
    lines.append("")

    # ── mStat inconnus ────────────────────────────────────────────────────────────
    lines.append("## 5. Valeurs `mStat` inconnues\n")
    if unk_stats:
        lines.append("| mStat | Occurrences | Action requise |")
        lines.append("|---|---|---|")
        for s, c in sorted(unk_stats.items(), key=lambda x: -x[1]):
            lines.append(f"| {s} | {c} | Ajouter à l'enum `ChampionStat` |")
    else:
        lines.append("✅ Toutes les valeurs `mStat` sont dans l'enum `ChampionStat` (0–11).")
    lines.append("")

    # ── Sans calculs ──────────────────────────────────────────────────────────────
    lines.append("## 6. Champions sans `mSpellCalculations`\n")
    if results['no_spell_calcs']:
        lines.append(f"{nc_count} champion(s) n'ont aucun sort avec des formules CDragon :\n")
        lines.append(", ".join(f"`{c}`" for c in sorted(results['no_spell_calcs'])))
    else:
        lines.append("✅ Tous les champions ont au moins un sort avec des formules CDragon.")
    lines.append("")

    # ── Formules vides ────────────────────────────────────────────────────────────
    lines.append("## 7. Formules vides après `Format()`\n")
    if results['all_empty_formulas']:
        lines.append(f"{ef_count} formule(s) vide(s) :\n")
        lines.append("| Champion | Sort | Calcul |")
        lines.append("|---|---|---|")
        for champ, spell, calc in results['all_empty_formulas'][:50]:
            lines.append(f"| {champ} | `{spell}` | `{calc}` |")
        if ef_count > 50:
            lines.append(f"\n_... et {ef_count - 50} autre(s). Voir le JSON pour la liste complète._")
    else:
        lines.append("✅ Aucune formule vide détectée.")
    lines.append("")

    # ── Erreurs de parsing ────────────────────────────────────────────────────────
    lines.append("## 8. Erreurs de parsing\n")
    if results['all_parse_errors']:
        lines.append(f"{pe_count} erreur(s) :\n")
        for e in results['all_parse_errors'][:30]:
            lines.append(f"- `{e}`")
        if pe_count > 30:
            lines.append(f"\n_... et {pe_count - 30} autre(s)._")
    else:
        lines.append("✅ Aucune exception levée lors du parsing.")
    lines.append("")

    # ── Exemples de formules ──────────────────────────────────────────────────────
    lines.append("## 9. Exemples de formules générées\n")
    sample_champs = list(results.get('sample_formulas', {}).items())[:8]
    if sample_champs:
        for champ, spells in sample_champs:
            lines.append(f"### {champ}\n")
            lines.append("| Sort | Calcul | Formule |")
            lines.append("|---|---|---|")
            count = 0
            for spell, calcs in spells.items():
                for calc_name, formula in calcs.items():
                    lines.append(f"| `{spell}` | `{calc_name}` | {formula} |")
                    count += 1
                    if count >= 8:
                        break
                if count >= 8:
                    break
            lines.append("")
    else:
        lines.append("_Aucun exemple disponible._")
    lines.append("")

    # ── Recommandations ───────────────────────────────────────────────────────────
    lines.append("## 10. Recommandations\n")
    recs = []
    if unk_types:
        recs.append(("🔴 Haute", "Implémenter les nouveaux `__type`",
                     f"Créer une classe `IFormulaPart` pour : {', '.join(f'`{t}`' for t in unk_types)}"))
    if unk_stats:
        recs.append(("🔴 Haute", "Étendre `ChampionStat`",
                     f"Ajouter les valeurs mStat : {', '.join(str(s) for s in unk_stats)}"))
    if results['not_found']:
        recs.append(("🔴 Haute", "Corriger la normalisation",
                     f"Ajouter des exceptions dans `ChampionNameNormalizer` pour : {', '.join(n for n,_ in results['not_found'])}"))
    if ef_count > 0:
        recs.append(("🟡 Moyenne", "Investiguer les formules vides",
                     f"{ef_count} cas à analyser — vérifier si `mFormulaParts` est vide (normal) ou si `Format()` échoue"))
    if nc_count > 0:
        recs.append(("🟢 Faible", "Masquer la section Formules",
                     f"Pour les {nc_count} champions sans données CDragon, `HasFormulas=false` masque déjà la section"))
    recs.append(("🟢 Faible", "Cache bin.json par patch",
                 "Stocker dans `%AppData%\\LOLInfo\\cache\\{version}\\{champion}.json`"))
    recs.append(("🟢 Faible", "Logging de couverture",
                 "Logger `{champion}: {n} sorts, {m} avec formules ({pct}%)` dans `CdragonClient`"))

    if recs:
        lines.append("| Priorité | Action | Détail |")
        lines.append("|---|---|---|")
        for prio, action, detail in recs:
            lines.append(f"| {prio} | {action} | {detail} |")
    lines.append("")

    # Sauvegarde
    md_path = os.path.join(base_dir, "CDragon_Rapport_Validation.md")
    with open(md_path, "w", encoding="utf-8") as f:
        f.write("\n".join(lines))

    print(f"\n=== Rapport Markdown sauvegardé dans {md_path} ===")
    return md_path


if __name__ == "__main__":
    results, base_dir = asyncio.run(main())

    # Affiche un résumé console
    print("\n" + "="*60)
    print("RÉSUMÉ")
    print("="*60)
    print(f"Champions OK          : {len(results['ok'])}")
    print(f"Non trouvés (404)     : {len(results['not_found'])}")
    print(f"Erreurs réseau/parse  : {len(results['errors'])}")
    print(f"Sans calcul de sort   : {len(results['no_spell_calcs'])}")
    print(f"Total sorts trouvés   : {results['total_spells']}")
    print(f"Sorts avec formules   : {results['total_spells_with_calcs']}")
    print(f"Total formules        : {results['total_formulas']}")
    print(f"Types inconnus        : {dict(results['all_unknown_types'])}")
    print(f"mStat inconnus        : {dict(results['all_unknown_stats'])}")
    print(f"Formules vides        : {len(results['all_empty_formulas'])}")
    print(f"Erreurs de parsing    : {len(results['all_parse_errors'])}")

    # Génère le rapport Markdown
    md_path = generate_markdown_report(results, base_dir)
    print(f"\nRapport disponible : {md_path}")

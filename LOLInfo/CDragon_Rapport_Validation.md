# LOLInfo — Rapport de Validation CDragon

**Généré le** : 28/05/2026 22:22  
**Version DataDragon** : 16.11.1

## 1. Résumé exécutif

| Métrique | Valeur |
|---|---|
| Champions testés | 172 |
| ✅ Champions OK | 172 (100.0%) |
| ❌ Introuvables (404) | 0 |
| ⚠️ Erreurs réseau/parse | 0 |
| Sans calculs CDragon | 0 |
| Total sorts analysés | 4730 |
| Sorts avec formules | 821 (17.4%) |
| Total formules | 1632 |
| Formules vides | 0 |
| Erreurs de parsing | 0 |
| Types `__type` inconnus | 14 |
| Valeurs `mStat` inconnues | 3 |

## 2. Champions introuvables sur CDragon (404)

✅ Aucun champion introuvable. Normalisation correcte pour tout le roster.

## 3. Erreurs réseau / parse

✅ Aucune erreur réseau.

## 4. Types `__type` non gérés par le parser C#

| `__type` inconnu | Occurrences | Action requise |
|---|---|---|
| `StatByCoefficientCalculationPart` | 689 | Créer une classe `IFormulaPart` dédiée |
| `ByCharLevelBreakpointsCalculationPart` | 178 | Créer une classe `IFormulaPart` dédiée |
| `ByCharLevelInterpolationCalculationPart` | 139 | Créer une classe `IFormulaPart` dédiée |
| `ProductOfSubPartsCalculationPart` | 46 | Créer une classe `IFormulaPart` dédiée |
| `StatBySubPartCalculationPart` | 41 | Créer une classe `IFormulaPart` dédiée |
| `BuffCounterByNamedDataValueCalculationPart` | 24 | Créer une classe `IFormulaPart` dédiée |
| `SumOfSubPartsCalculationPart` | 21 | Créer une classe `IFormulaPart` dédiée |
| `BuffCounterByCoefficientCalculationPart` | 20 | Créer une classe `IFormulaPart` dédiée |
| `ByCharLevelFormulaCalculationPart` | 11 | Créer une classe `IFormulaPart` dédiée |
| `AbilityResourceByCoefficientCalculationPart` | 8 | Créer une classe `IFormulaPart` dédiée |
| `ClampSubPartsCalculationPart` | 6 | Créer une classe `IFormulaPart` dédiée |
| `{f3cbe7b2}` | 4 | Créer une classe `IFormulaPart` dédiée |
| `{ee18a47b}` | 3 | Créer une classe `IFormulaPart` dédiée |
| `{b22609db}` | 1 | Créer une classe `IFormulaPart` dédiée |

## 5. Valeurs `mStat` inconnues

| mStat | Occurrences | Action requise |
|---|---|---|
| -1 | 357 | Ajouter à l'enum `ChampionStat` |
| 12 | 45 | Ajouter à l'enum `ChampionStat` |
| 18 | 1 | Ajouter à l'enum `ChampionStat` |

## 6. Champions sans `mSpellCalculations`

✅ Tous les champions ont au moins un sort avec des formules CDragon.

## 7. Formules vides après `Format()`

✅ Aucune formule vide détectée.

## 8. Erreurs de parsing

✅ Aucune exception levée lors du parsing.

## 9. Exemples de formules générées

### Udyr

| Sort | Calcul | Formule |
|---|---|---|
| `UdyrE` | `MoveSpeedBonus` | [ByCharLevelInterpolationCalculationPart?] + [StatByCoefficientCalculationPart?] |
| `UdyrE` | `{aedeb4cc}` | [ClampSubPartsCalculationPart?] |
| `UdyrE` | `MoveSpeed` | 0.19/0.25/0.31/0.37/0.43/0.49/0.55 + [StatByCoefficientCalculationPart?] |
| `UdyrR` | `PercentHPBlast` | [ByCharLevelInterpolationCalculationPart?] + +0.00035/0.00035/0.00035/0.00035/0.00035/0.00035/0.00035 stat#-1 |
| `UdyrR` | `StormDamage` | 4/20/36/52/68/84/100 + [StatByCoefficientCalculationPart?] |
| `UdyrR` | `EmpoweredSlow` | 0.05 |
| `UdyrR` | `PulseDamage` | [ByCharLevelInterpolationCalculationPart?] + [StatByCoefficientCalculationPart?] |
| `UdyrR` | `{817f8a73}` | [ByCharLevelInterpolationCalculationPart?] |

### Urgot

| Sort | Calcul | Formule |
|---|---|---|
| `UrgotW` | `DamagePerShot` | 12/12/12/12/12/12/12 + +0.17/0.2/0.23/0.27/0.31/0.34/0.38 vit.att |
| `UrgotW` | `RangeCheck` | 450 + [StatByCoefficientCalculationPart?] |
| `UrgotE` | `ETotalShieldHealth` | 35/55/75/95/115/135/155 + +1.5/1.4/1.4/1.4/1.4/1.4/1.4 vit.att + +0/0.14/0.14/0.14/0.14/0.14/0.14 stat#12 |
| `UrgotE` | `EDamage` | 60/90/120/150/180/210/240 + [StatByCoefficientCalculationPart?] |
| `UrgotR` | `RCalculatedDamage` | 100/100/225/350/475/600/725 + +0.5/0.5/0.5/0.5/0.5/0.5/0.5 vit.att |
| `UrgotQ` | `TotalDamage` | -20/25/70/115/160/205/250 + [StatByCoefficientCalculationPart?] |
| `UrgotPassive` | `PerLegCD` | [ByCharLevelBreakpointsCalculationPart?] |
| `UrgotPassive` | `ADDamage` | [StatBySubPartCalculationPart?] |

### Leona

| Sort | Calcul | Formule |
|---|---|---|
| `LeonaSunlightPassive` | `TotalDamage` | [ByCharLevelFormulaCalculationPart?] |
| `LeonaShieldOfDaybreak` | `TotalDamageTooltip` |  + [StatByCoefficientCalculationPart?] |
| `LeonaZenithBlade` | `TotalDamageTooltip` |  + [StatByCoefficientCalculationPart?] |
| `LeonaSolarFlare` | `{f4c90e5c}` | 20/30/40/50/60/70/80 + +0.15/0.15/0.15/0.15/0.15/0.15/0.15 stat#-1 |
| `LeonaSolarFlare` | `ExplosionCalculatedDamage` | 75/150/225/300/375/450/525 + +0.8/0.8/0.8/0.8/0.8/0.8/0.8 stat#-1 |
| `LeonaSolarBarrier` | `BonusArmorTooltip` |  + [StatByCoefficientCalculationPart?] |
| `LeonaSolarBarrier` | `BonusMRTooltip` |  + [StatByCoefficientCalculationPart?] |
| `LeonaSolarBarrier` | `TotalDamageTooltip` |  + +0.4/0.4/0.4/0.4/0.4/0.4/0.4 stat#-1 |

### Twitch

| Sort | Calcul | Formule |
|---|---|---|
| `TwitchDeadlyVenomMarker` | `DamagePerSecond` | [ByCharLevelBreakpointsCalculationPart?] + +0.03/0.03/0.03/0.03/0.03/0.03/0.03 stat#-1 |
| `TwitchExpunge` | `PhysicalDamagePerStack` | 10/15/20/25/30/35/40 + +0.35/0.35/0.35/0.35/0.35/0.35/0.35 vit.att |
| `TwitchExpunge` | `MagicDamagePerStack` | +0.35/0.35/0.35/0.35/0.35/0.35/0.35 stat#-1 |
| `TwitchExpunge` | `MaxPhysicalDamage` | 10/20/30/40/50/60/70 + [ProductOfSubPartsCalculationPart?] + [StatBySubPartCalculationPart?] |
| `TwitchVenomCask` | `TotalSlowAmount` | 25/30/35/40/45/50/55 + +0.06/0.06/0.06/0.06/0.06/0.06/0.06 stat#-1 |

### Lee Sin

| Sort | Calcul | Formule |
|---|---|---|
| `LeeSinPassive` | `EnergyReturn` | [ByCharLevelBreakpointsCalculationPart?] |
| `LeeSinQOne` | `InitialDamage` | 35/65/95/125/155/185/215 + [StatByCoefficientCalculationPart?] |
| `LeeSinQOne` | `RecastDamage` | 35/65/95/125/155/185/215 + +0.95/0.95/0.95/0.95/0.95/0.95/0.95 vit.att |
| `LeeSinR` | `Damage` | -50/175/400/625/850/1075/1300 + [StatByCoefficientCalculationPart?] |
| `LeeSinEOne` | `InitialDamage` | 10/35/60/85/110/135/160 + [StatByCoefficientCalculationPart?] |
| `LeeSinWOne` | `ShieldAmount` | 15/60/105/150/195/240/285 + [StatByCoefficientCalculationPart?] |

### Lissandra

| Sort | Calcul | Formule |
|---|---|---|
| `LissandraW` | `TotalDamage` |  + [StatByCoefficientCalculationPart?] |
| `LissandraR` | `CalculatedDamage` | 50/150/250/350/450/550/650 + +0.75/0.75/0.75/0.75/0.75/0.75/0.75 stat#-1 |
| `LissandraR` | `HealAmount` | 50/100/150/200/250/300/350 + +0.55/0.55/0.55/0.55/0.55/0.55/0.55 stat#-1 |
| `LissandraE` | `TotalDamage` |  + [StatByCoefficientCalculationPart?] |
| `LissandraQ` | `TotalDamage` | ?(Damage) + [StatByCoefficientCalculationPart?] |
| `LissandraPassive` | `TotalDamage` | [ByCharLevelBreakpointsCalculationPart?] + [StatByCoefficientCalculationPart?] |

### Lillia

| Sort | Calcul | Formule |
|---|---|---|
| `LilliaR` | `TotalDamage` | 50/100/150/200/250/300/350 + [StatByCoefficientCalculationPart?] |
| `LilliaE` | `ImpactDamageTotal` | 35/60/85/110/135/160/185 + +0.5/0.5/0.5/0.5/0.5/0.5/0.5 stat#-1 |
| `LilliaPDoT` | `{77823978}` | [ByCharLevelInterpolationCalculationPart?] |
| `LilliaPDoT` | `HealPerSecond` | [ByCharLevelBreakpointsCalculationPart?] + [StatByCoefficientCalculationPart?] |
| `LilliaPDoT` | `{44c2f9d7}` | 10/10/10/10/10/10/10 + [StatByCoefficientCalculationPart?] |
| `LilliaPDoT` | `{8abdf228}` | [ByCharLevelBreakpointsCalculationPart?] + [StatByCoefficientCalculationPart?] |
| `LilliaPDoT` | `DotPercentTotal` | 0.05/0.05/0.05/0.05/0.05/0.05/0.05 |
| `LilliaPDoT` | `MonsterDamageCap` | [ByCharLevelInterpolationCalculationPart?] |

### Annie

| Sort | Calcul | Formule |
|---|---|---|
| `AnnieR` | `InitialBurstDamage` | 25/150/275/400/525/650/775 + [StatByCoefficientCalculationPart?] |
| `AnnieR` | `TibbersAuraDamage` | 4/8/12/16/20/24/28 + [StatByCoefficientCalculationPart?] |
| `AnnieR` | `TibbersAADamage` | 15/30/45/60/75/90/105 + +0.1/0.1/0.1/0.1/0.1/0.1/0.1 stat#-1 |
| `AnnieR` | `TibbersTotalHP` | [ProductOfSubPartsCalculationPart?] + [StatByCoefficientCalculationPart?] |
| `AnnieR` | `TibbersTotalResists` | [ByCharLevelBreakpointsCalculationPart?] |
| `AnniePassive` | `StunDuration` | [ByCharLevelBreakpointsCalculationPart?] |
| `AnnieQ` | `TotalDamage` | 35/80/125/170/215/260/305 + +0.8/0.8/0.8/0.8/0.8/0.8/0.8 stat#-1 |
| `AnnieE` | `DamageReturn` | 15/25/35/45/55/65/75 + [StatByCoefficientCalculationPart?] |


## 10. Recommandations

| Priorité | Action | Détail |
|---|---|---|
| 🔴 Haute | Implémenter les nouveaux `__type` | Créer une classe `IFormulaPart` pour : `ByCharLevelInterpolationCalculationPart`, `StatByCoefficientCalculationPart`, `ClampSubPartsCalculationPart`, `ByCharLevelBreakpointsCalculationPart`, `{ee18a47b}`, `ProductOfSubPartsCalculationPart`, `StatBySubPartCalculationPart`, `ByCharLevelFormulaCalculationPart`, `SumOfSubPartsCalculationPart`, `AbilityResourceByCoefficientCalculationPart`, `{f3cbe7b2}`, `BuffCounterByNamedDataValueCalculationPart`, `BuffCounterByCoefficientCalculationPart`, `{b22609db}` |
| 🔴 Haute | Étendre `ChampionStat` | Ajouter les valeurs mStat : -1, 12, 18 |
| 🟢 Faible | Cache bin.json par patch | Stocker dans `%AppData%\LOLInfo\cache\{version}\{champion}.json` |
| 🟢 Faible | Logging de couverture | Logger `{champion}: {n} sorts, {m} avec formules ({pct}%)` dans `CdragonClient` |

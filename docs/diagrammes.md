# LOLInfo — Diagrammes de l'application

Application **WPF (.NET)** suivant le patron **MVVM**, avec **injection de dépendances**
(`Microsoft.Extensions.DependencyInjection`) et une couche de services isolée derrière
des interfaces. Les données proviennent de deux CDN Riot (**DataDragon** et
**CommunityDragon**) et sont mises en cache sur le disque local.

- **Vues** (XAML) : ne contiennent que de la présentation + un mince code-behind.
- **ViewModels** : état et logique de présentation, exposés via des interfaces.
- **Services** : accès réseau (clients CDN), persistance locale (favoris, langue,
  cache d'images) et navigation.
- **Modèles** : DTO Riot/CDragon + logique de domaine (calcul de stats, recette).

> Les diagrammes ci-dessous sont en **Mermaid** (rendus par GitHub et l'aperçu VS Code).

---

## 1. Diagramme de classes — Architecture en couches

```mermaid
classDiagram
    direction LR

    class ObservableObject { <<MVVM Toolkit>> }
    class BaseViewModel
    ObservableObject <|-- BaseViewModel

    %% ───────────── Composition root ─────────────
    namespace CompositionRoot {
        class App {
            +IServiceProvider Services
            +Restart()
            +OnStartup()
        }
        class MainWindow
    }

    %% ───────────── Vues (WPF) ─────────────
    namespace Vues {
        class AllChampionPage
        class DetailChampionPage
        class ItemBrowserControl
        class ItemDetailControl
        class AutoCompleteBox
        class RecipeTreeNode
    }

    %% ───────────── ViewModels ─────────────
    namespace ViewModels {
        class MainViewModel {
            +CurrentPage
            +Items
            +Languages
            +CurrentLanguageCode
        }
        class AllChampionViewModel {
            +ChampionsView
            +NameFilter
            +TagFilters
            +SortOptions
            +GetAllChampions()
        }
        class DetailChampionViewModel {
            +Champion
            +Spells
            +Skins
            +ChampionStats
            +EquippedItems
            +Items
            +EquipCurrent()
            +LoadAsync()
        }
        class ItemsViewModel {
            +ItemsView
            +NameFilter
            +SelectedGameMode
            +SortOptions
            +StatFilters
            +SearchSuggestions
            +SelectedItem
            +LoadAsync()
        }
    }

    %% ───────────── Contrats ViewModels ─────────────
    namespace ContratsVM {
        class IAllChampionViewModel { <<interface>> }
        class IDetailChampionViewModel { <<interface>> }
        class IItemsViewModel { <<interface>> }
    }

    %% ───────────── Services + contrats ─────────────
    namespace Services {
        class IViewManager { <<interface>> }
        class ViewManager
        class IRiotClient { <<interface>> }
        class RiotClient
        class ICdragonClient { <<interface>> }
        class CdragonClient
        class IItemClient { <<interface>> }
        class ItemClient
        class IPatchVersionService { <<interface>> }
        class PatchVersionService
        class IFavoritesService { <<interface>> }
        class FavoritesService
        class ILanguageService { <<interface>> }
        class LanguageService
        class IImageCacheService { <<interface>> }
        class ImageCacheService
    }

    %% ───────────── Utilitaires ─────────────
    namespace Utilitaires {
        class DataDragonCdn
        class AppLocalization
        class AutoCompleteFilter
        class CachedImage
        class ChampionStatsCalculator
    }

    %% Héritage des ViewModels
    BaseViewModel <|-- MainViewModel
    BaseViewModel <|-- AllChampionViewModel
    BaseViewModel <|-- DetailChampionViewModel
    BaseViewModel <|-- ItemsViewModel

    %% Réalisation des interfaces
    IAllChampionViewModel <|.. AllChampionViewModel
    IDetailChampionViewModel <|.. DetailChampionViewModel
    IItemsViewModel <|.. ItemsViewModel
    IViewManager <|.. ViewManager
    IRiotClient <|.. RiotClient
    ICdragonClient <|.. CdragonClient
    IItemClient <|.. ItemClient
    IPatchVersionService <|.. PatchVersionService
    IFavoritesService <|.. FavoritesService
    ILanguageService <|.. LanguageService
    IImageCacheService <|.. ImageCacheService

    %% Composition / DataContext des vues
    App --> MainWindow : crée
    App ..> ViewManager : DI
    MainWindow --> MainViewModel : DataContext
    AllChampionPage --> IAllChampionViewModel
    DetailChampionPage --> IDetailChampionViewModel
    ItemBrowserControl ..> IItemsViewModel : DataContext
    ItemDetailControl ..> ItemsViewModel : SelectedItem
    AutoCompleteBox ..> AutoCompleteFilter

    %% Dépendances ViewModels → contrats
    MainViewModel --> IViewManager
    MainViewModel --> IItemsViewModel
    MainViewModel --> ILanguageService
    AllChampionViewModel --> IRiotClient
    AllChampionViewModel --> IFavoritesService
    DetailChampionViewModel --> IRiotClient
    DetailChampionViewModel --> ICdragonClient
    DetailChampionViewModel --> IItemsViewModel
    DetailChampionViewModel ..> ChampionStatsCalculator
    ItemsViewModel --> IItemClient

    %% Navigation
    ViewManager ..> AllChampionPage : crée
    ViewManager ..> DetailChampionPage : crée

    %% Services → utilitaires / externes
    RiotClient ..> DataDragonCdn
    ItemClient ..> DataDragonCdn
    CdragonClient ..> DataDragonCdn
    PatchVersionService ..> DataDragonCdn : fixe Version
    LanguageService ..> AppLocalization
    DataDragonCdn ..> AppLocalization : DataLocale
    CachedImage ..> IImageCacheService
```

---

## 2. Diagramme de classes — Modèle de domaine

```mermaid
classDiagram
    direction LR

    %% ───────────── DTO Riot (DataDragon) ─────────────
    class Champion {
        +string Id
        +string Name
        +Stats
        +Info Info
        +Spells
        +Skins
    }
    class Info { +Attack +Magic +Difficulty }
    class Image { +Full }
    class Spell
    class Passive
    class Skin
    class Item {
        +string Id
        +string Name
        +Stats
        +Maps
        +ItemGold Gold
        +From
    }
    class ItemGold { +Total +Base +Purchasable }

    Champion o-- Info
    Champion o-- Image
    Champion o-- "*" Spell
    Champion o-- "*" Skin
    Champion o-- Passive
    Item o-- Image
    Item o-- ItemGold

    %% ───────────── Wrappers de présentation ─────────────
    class ItemViewModel {
        +Name
        +Gold
        +RawStats
        +Maps
        +Recipe
        +IsAvailableOn(mapId)
        +IsOnAnyMap
        +From(Item)$
    }
    class ItemRecipeNode {
        +Name
        +Components
        +Build(item, index)$
    }
    class ChampionListItemViewModel {
        +Champion
        +DamageType
        +IsRanged
        +IsFavorite
        +ToggleFavoriteCommand
    }
    class SpellViewModel
    class SkinViewModel
    class ChampionStatRow { +Label +Value }
    class ItemSortOption { +Key +Label }
    class FilterItemViewModel { +Key +Label +IsSelected }
    class LanguageOption { +Code +NativeName }

    ItemViewModel ..> Item : From()
    ItemViewModel o-- "0..1" ItemRecipeNode
    ItemRecipeNode o-- "*" ItemRecipeNode : sous-composants
    ChampionListItemViewModel o-- Champion
    SpellViewModel ..> Spell
    SpellViewModel ..> Passive
    SkinViewModel ..> Skin

    %% ───────────── Domaine : calcul des stats ─────────────
    class ChampionStatsCalculator {
        +Compute(stats, level, bonusObjets)$
    }
    class ChampionStatValue { +Kind +Value }
    class ChampionStatKind { <<enumeration>> Health Mana Armor ... }
    ChampionStatsCalculator ..> ChampionStatValue : produit
    ChampionStatValue --> ChampionStatKind
    DetailChampionViewModel ..> ChampionStatValue
    DetailChampionViewModel o-- "*" ChampionStatRow

    %% ───────────── Filtres / tri (énumérations) ─────────────
    class GameModeFilter { <<enumeration>> Tous NormalClasse Aram Arena }
    class DamageTypeFilter { <<enumeration>> Tous AD AP Mixte }
    class RangeTypeFilter { <<enumeration>> Tous Melee Range }
    class SortOption { <<enumeration>> NomAZ NomZA DifficulteAsc DifficulteDesc }

    ItemsViewModel o-- "*" ItemViewModel
    ItemsViewModel o-- "*" ItemSortOption
    ItemsViewModel o-- "*" FilterItemViewModel
    ItemsViewModel ..> GameModeFilter
    AllChampionViewModel o-- "*" ChampionListItemViewModel
    AllChampionViewModel ..> DamageTypeFilter
    AllChampionViewModel ..> RangeTypeFilter
    AllChampionViewModel ..> SortOption
    MainViewModel o-- "*" LanguageOption
```

---

## 3. Diagramme de fonctionnement — Démarrage de l'application

```mermaid
sequenceDiagram
    autonumber
    participant App
    participant DI as ServiceProvider
    participant Lang as LanguageService
    participant Loc as AppLocalization
    participant Cache as ImageCacheService
    participant Patch as PatchVersionService
    participant CDN as DataDragon
    participant VM as ViewManager
    participant Items as ItemsViewModel

    Note over App: Constructeur
    App->>DI: ConfigureServices()
    App->>Lang: CurrentLanguage (lit settings.json)
    App->>Loc: ApplyCulture(langue)

    Note over App: OnStartup
    App-)Cache: PurgeOldFiles(30 j) (arrière-plan)
    App->>Patch: InitializeAsync()
    Patch->>CDN: GET versions.json
    CDN-->>Patch: dernière version
    Patch->>Loc: DataDragonCdn.Version = version
    App->>VM: NavigateToAllChampion()
    VM-->>App: AllChampionPage (page courante)
    App-)Items: LoadAsync() (préchargement arrière-plan)
    Items->>CDN: GET item.json
    CDN-->>Items: objets → ItemViewModel (dédup, recette, obsolètes filtrés)
```

---

## 4. Diagramme de fonctionnement — Navigation & flux de données

```mermaid
flowchart TD
    U([Utilisateur])

    subgraph Vues
        MW[MainWindow<br/>onglets + sélecteur de langue]
        ACP[AllChampionPage]
        DCP[DetailChampionPage]
        IBC[ItemBrowserControl]
        IDC[ItemDetailControl]
    end

    subgraph ViewModels
        MVM[MainViewModel]
        ACVM[AllChampionViewModel]
        DCVM[DetailChampionViewModel]
        IVM[ItemsViewModel]
    end

    subgraph Services
        VMG[ViewManager]
        RC[RiotClient]
        CC[CdragonClient]
        IC[ItemClient]
        FAV[FavoritesService]
        LANG[LanguageService]
        IMG[ImageCacheService]
    end

    subgraph Externe
        DD[(DataDragon CDN)]
        CD[(CommunityDragon CDN)]
        DISK[(Disque local<br/>settings.json, favoris, cache images)]
    end

    U --> MW
    MW --> MVM
    MW -->|onglet Champions| ACP --> ACVM
    ACVM -->|clic champion| VMG
    VMG -->|crée| DCP --> DCVM
    MW -->|onglet Objets| IBC --> IVM
    DCP -->|réutilise| IBC
    DCP -->|détail| IDC

    ACVM --> RC --> DD
    ACVM --> FAV --> DISK
    DCVM --> RC
    DCVM --> CC --> CD
    DCVM --> IVM
    IVM --> IC --> DD
    MVM --> VMG
    MVM --> LANG --> DISK

    IBC -. images .-> IMG
    IDC -. images .-> IMG
    ACP -. images .-> IMG
    IMG --> DISK
    IMG --> DD
```

---

## 5. Diagramme de fonctionnement — Équiper un objet et recalculer les stats

Illustre la nouvelle interface partagée (onglet **Stats** du champion = même navigateur
d'objets que l'onglet **Objets**).

```mermaid
sequenceDiagram
    autonumber
    actor U as Utilisateur
    participant Browser as ItemBrowserControl
    participant IVM as ItemsViewModel
    participant Detail as ItemDetailControl
    participant DVM as DetailChampionViewModel
    participant Calc as ChampionStatsCalculator

    U->>Browser: clic sur une icône d'objet
    Browser->>IVM: SelectedItem = objet
    IVM-->>Detail: PropertyChanged → affiche le détail (bleu)

    U->>DVM: clic « Équiper » (EquipCurrent)
    DVM->>IVM: lit SelectedItem
    alt place disponible (< 6 objets)
        DVM->>DVM: EquippedItems.Add(objet) (zone verte)
        DVM->>DVM: AggregateEquippedStats()
        DVM->>Calc: Compute(stats champion, niveau, bonus objets)
        Calc-->>DVM: liste de ChampionStatValue
        DVM-->>U: ChampionStats mis à jour (zone gauche)
    else 6 objets déjà équipés
        DVM-->>U: aucune action
    end

    U->>DVM: clic sur un objet équipé (Unequip)
    DVM->>DVM: EquippedItems.Remove + recalcul
```

---

## 6. Pipeline d'affichage des images (cache)

```mermaid
flowchart LR
    IMGV[Image XAML<br/>CachedImage.SourcePath] --> ACH{En cache disque ?}
    ACH -- oui --> LOAD[Charge depuis le disque]
    ACH -- non --> DLD[Télécharge depuis le CDN] --> SAVE[Enregistre sur le disque] --> LOAD
    LOAD --> SHOW[Affiche l'image gelée]
```

---

### Notes de conception

- **Une seule instance partagée d'`ItemsViewModel`** (singleton DI) : l'onglet Objets
  et le navigateur de l'onglet Stats du champion partagent filtres et sélection.
- **`ItemBrowserControl` / `ItemDetailControl`** sont réutilisés aux deux endroits (DRY).
- **Changement de langue** : `LanguageService` persiste le choix, puis l'application
  **redémarre** (`App.Restart`) pour recharger l'UI et les données dans la nouvelle locale.
- **Dédup & obsolètes** : `ItemsViewModel` ne garde qu'une variante par nom selon le mode
  et masque les objets présents sur aucune carte (cf. `RebuildKeptIds`).

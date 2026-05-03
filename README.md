# DoubleCheck — D365 Duplicate Clean-Up Tool

Utilitaire C# qui se connecte à une instance **Dynamics 365** via l'API Dataverse et scanne les enregistrements pour détecter les doublons potentiels. Génère un rapport **CSV** permettant de valider les fusions avant toute modification.

---

## Fonctionnalités

- Connexion à Dynamics 365 via l'API Dataverse (auth client secret)
- Détection de doublons sur les **Contacts** et les **Comptes (Accounts)**
- Deux algorithmes de détection :
  - **Email identique** — correspondance exacte insensible à la casse
  - **Nom similaire** — distance de Levenshtein avec seuil configurable
- Export CSV structuré pour revue humaine avant toute fusion
- Mode **données locales** pour tourner sans licence D365

---

## Stack

| Technologie | Usage |
|---|---|
| C# / .NET 8 | Langage et runtime |
| Microsoft.PowerPlatform.Dataverse.Client | Connexion à D365 |
| Algorithme de Levenshtein | Comparaison de chaînes |
| CsvHelper | Export CSV |
| Microsoft.Extensions.DependencyInjection | Injection de dépendances |

---

## Structure du projet

```
DoubleCheck/
├── Algorithms/
│   └── LevenshteinCalculator.cs     # Implémentation DP de la distance de Levenshtein
├── Configuration/
│   └── AppSettings.cs               # POCO de configuration
├── Infrastructure/
│   └── ServiceCollectionExtensions  # Enregistrement DI
├── Models/
│   ├── ContactRecord.cs
│   ├── AccountRecord.cs
│   └── DuplicatePair.cs             # Résultat d'une paire détectée
├── Reporting/
│   └── CsvReportWriter.cs           # Export CSV via CsvHelper
├── Services/
│   ├── DataverseService.cs          # Connexion réelle à D365
│   ├── DuplicateDetectionService.cs # Logique de détection
│   └── LocalDataService.cs          # Mode mock (JSON local)
├── data/
│   ├── contacts.json                # Données de démonstration
│   └── accounts.json
├── appsettings.example.json
└── Program.cs
```

---

## Démarrage rapide

### Mode démo (sans licence D365)

1. Cloner le repo
2. Copier `appsettings.example.json` → `appsettings.json`
3. Laisser `"UseLocalData": true`
4. Lancer le projet — le rapport sera généré dans `output/duplicates.csv`

### Mode D365 réel

1. Créer une **App Registration** dans Azure AD et récupérer `ClientId`, `ClientSecret`, `TenantId`
2. Ajouter l'app comme **Application User** dans D365 avec un Security Role
3. Remplir `appsettings.json` :

```json
{
  "DoubleCheck": {
    "UseLocalData":        false,
    "DataverseUrl":        "https://yourorg.crm.dynamics.com",
    "ClientId":            "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "ClientSecret":        "your-secret",
    "TenantId":            "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy",
    "SimilarityThreshold": 85,
    "OutputCsvPath":       "output/duplicates.csv"
  }
}
```

> `appsettings.json` est dans le `.gitignore` — les credentials ne sont jamais commités.

---

## Format du rapport CSV

| Colonne | Description |
|---|---|
| Entity Type | `Contact` ou `Account` |
| Record 1 ID | GUID de l'enregistrement 1 |
| Record 1 Name | Nom de l'enregistrement 1 |
| Record 2 ID | GUID de l'enregistrement 2 |
| Record 2 Name | Nom de l'enregistrement 2 |
| Match Reason | `ExactEmail` ou `SimilarName` |
| Matched Value | Email partagé (si ExactEmail) |
| Similarity % | Score de similarité (0–100) |

---

## Configuration

| Paramètre | Défaut | Description |
|---|---|---|
| `SimilarityThreshold` | `85` | Seuil minimum (%) pour signaler deux noms comme similaires |
| `UseLocalData` | `false` | `true` pour utiliser les JSON locaux au lieu de D365 |
| `LocalDataPath` | `data` | Dossier contenant `contacts.json` et `accounts.json` |
| `OutputCsvPath` | `output/duplicates.csv` | Chemin du rapport généré |

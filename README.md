# GarageTrack — frontend .NET MAUI (phase A)

GarageTrack Automobile est une application mobile optimisant la gestion des réparations et devis, offrant aux clients un suivi en temps réel et un historique transparent.

Cette phase A couvre le frontend uniquement : les données sont fictives et gardées en mémoire (`Services/MockDataService.cs`), aucun backend.

## Lancer le projet

1. Ouvrir `GarageTrack.csproj` dans **Visual Studio 2022 (17.14+) ou 2026** avec la charge de travail **.NET MAUI** et le SDK **.NET 10**.
2. Restaurer les packages NuGet (automatique au premier build).
3. Choisir la cible **Windows Machine** (fenêtre au format téléphone) ou un **émulateur Android**, puis F5.

> Projet en .NET 9 ? Remplacer `net10.0` par `net9.0` dans le `.csproj` et la version de `Microsoft.Extensions.Logging.Debug` par `9.0.0`.

Connexion de démonstration : les champs sont préremplis, il suffit de toucher **Se connecter**.

## Écrans (fidèles aux maquettes)

| Maquette | Page |
|---|---|
| Connexion | `Views/LoginPage` |
| Créez votre atelier | `Views/RegisterPage` (jauge de mot de passe, CGU) |
| Bonjour Rodrigue (Atelier) | `Views/DashboardPage` (recherche, KPI filtrants, OR récents, bouton +) |
| Client & véhicule (2 onglets) | `Views/ClientVehiclePage` |
| Nouvel ordre de réparation · 1/3 | `Views/NewRepairOrderPage` |
| Pièces & main-d’œuvre · 2/3 | `Views/PartsLaborPage` (totaux HT/TVA/TTC recalculés) |
| Validation du devis · 3/3 | `Views/QuoteValidationPage` (signature tactile) |
| Historique & statuts | `Views/VehicleHistoryPage` (timeline) |
| Onglets Clients / Devis / Compte | `Views/ClientsPage`, `QuotesPage`, `AccountPage` |

## Architecture (MVVM + Shell)

```
Models/        Client, Vehicle, RepairOrder, QuoteLine, Intervention, OrderStatus, TaskItem
Services/      MockDataService (singleton, données de démo)
ViewModels/    un ViewModel par page (CommunityToolkit.Mvvm : [ObservableProperty], [RelayCommand])
Views/         pages XAML avec liaisons compilées (x:DataType)
Controls/      composants du système d'interface : Icon, ActionButton, LicensePlate,
               StatusBadge, StepProgress, SignaturePad
Resources/     Colors.xaml (palette du design), Styles.xaml, police Inter, icônes d’onglets
```

- Navigation : `AppShell` (TabBar Atelier / Clients / Devis / Compte + routes empilées),
  paramètres passés par query string et lus avec `IQueryAttributable`.
- Injection de dépendances : pages et ViewModels enregistrés dans `MauiProgram.cs`.
- Icônes : tracés vectoriels au trait dans `Helpers/Icons.cs` (aucune police d’icônes nécessaire).

## Parcours à tester

Atelier → **+** → choisir client / véhicule → **Générer le devis** → ajouter une pièce
(les totaux se recalculent ; toucher une ligne pour changer la quantité) → **Continuer** →
signer avec le doigt + cocher l’accord → **Valider le devis signé** → l’historique du véhicule
affiche l’OR « En réparation ».

## Brancher un backend plus tard

Remplacer `MockDataService` par un service qui appelle l’API (même méthodes publiques) :
les ViewModels et les pages n’ont pas à changer.

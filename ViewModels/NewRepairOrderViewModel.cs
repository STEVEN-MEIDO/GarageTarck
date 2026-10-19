using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageTrack.Helpers;
using GarageTrack.Models;
using GarageTrack.Services;

namespace GarageTrack.ViewModels;

/// <summary>Étape 1/3 : création de l'ordre de réparation.</summary>
public partial class NewRepairOrderViewModel : BaseViewModel, IQueryAttributable
{
    private readonly MockDataService _data;

    public ObservableCollection<Client> Clients { get; }
    public ObservableCollection<Vehicle> Vehicles { get; } = new();
    public ObservableCollection<TaskItem> Tasks { get; }
    public List<string> Mechanics => _data.Mechanics;

    [ObservableProperty] private Client? selectedClient;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasVehicle), nameof(LastReadingText))]
    private Vehicle? selectedVehicle;

    [ObservableProperty] private string mileage = string.Empty;
    [ObservableProperty] private string symptoms = string.Empty;
    [ObservableProperty] private string? selectedMechanic;
    [ObservableProperty] private string? errorMessage;

    public bool HasVehicle => SelectedVehicle is not null;
    public string HeaderText => $"{_data.PeekNextOrderNumber()} · {DateTime.Today:d MMMM yyyy}";

    public string LastReadingText => SelectedVehicle is null
        ? "Sélectionnez d’abord un véhicule."
        : $"Dernier relevé : {Money.Km(SelectedVehicle.LastReadingKm)} le {SelectedVehicle.LastReadingDate:dd/MM/yyyy}";

    public NewRepairOrderViewModel(MockDataService data)
    {
        _data = data;
        Clients = new ObservableCollection<Client>(data.Clients.OrderBy(c => c.FullName));
        Tasks = new ObservableCollection<TaskItem>(data.TaskCatalog.Select((t, i) => new TaskItem { Name = t, IsChecked = i < 3 }));
        SelectedMechanic = data.Mechanics.FirstOrDefault();
        Symptoms = "Bruit métallique au freinage et vibrations dans le volant. Contrôler le train avant.";
        SelectedClient = Clients.FirstOrDefault(c => c.FullName.StartsWith("Atangana")) ?? Clients.FirstOrDefault();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("clientId", out var cId))
            SelectedClient = Clients.FirstOrDefault(c => c.Id == cId?.ToString());
        if (query.TryGetValue("vehicleId", out var vId))
            SelectedVehicle = Vehicles.FirstOrDefault(v => v.Id == vId?.ToString()) ?? SelectedVehicle;
    }

    partial void OnSelectedClientChanged(Client? value)
    {
        Vehicles.Clear();
        if (value is null) { SelectedVehicle = null; return; }
        foreach (var v in _data.VehiclesOf(value.Id)) Vehicles.Add(v);
        SelectedVehicle = Vehicles.FirstOrDefault();
    }

    partial void OnSelectedVehicleChanged(Vehicle? value) =>
        Mileage = value is null ? string.Empty : value.Mileage.ToString("N0");

    [RelayCommand]
    private async Task ChooseVehicle()
    {
        if (Vehicles.Count == 0)
        {
            await Ui.Alert("Véhicule", "Aucun véhicule associé à ce client.");
            return;
        }
        var choice = await Ui.Choose("Véhicule", Vehicles.Select(v => v.ToString()).ToArray());
        if (choice is not null)
            SelectedVehicle = Vehicles.First(v => v.ToString() == choice);
    }

    [RelayCommand]
    private void ToggleTask(TaskItem task) => task.IsChecked = !task.IsChecked;

    [RelayCommand]
    private async Task SaveOrder()
    {
        var order = Create();
        if (order is null) return;
        await Ui.Alert("OR enregistré", $"{order.Number} a été enregistré. Le devis pourra être généré plus tard.");
        await Ui.Back();
    }

    [RelayCommand]
    private async Task GenerateQuote()
    {
        var order = Create();
        if (order is null) return;
        order.Status = OrderStatus.QuoteInProgress;
        await Ui.GoTo($"../{Routes.PartsLabor}?order={order.Number}");
    }

    private RepairOrder? Create()
    {
        ErrorMessage = null;
        var km = int.TryParse(new string(Mileage.Where(char.IsDigit).ToArray()), out var k) ? k : 0;
        var tasks = Tasks.Where(t => t.IsChecked).Select(t => t.Name).ToList();

        if (SelectedClient is null || SelectedVehicle is null || SelectedMechanic is null || string.IsNullOrWhiteSpace(Symptoms))
            ErrorMessage = "Complétez les champs obligatoires.";
        else if (km <= 0)
            ErrorMessage = "Kilométrage invalide. Indiquez le relevé du compteur.";
        else if (km < SelectedVehicle.LastReadingKm)
            ErrorMessage = $"Le kilométrage ne peut pas être inférieur au dernier relevé ({Money.Km(SelectedVehicle.LastReadingKm)}).";
        else if (tasks.Count == 0)
            ErrorMessage = "Sélectionnez au moins un travail avant de générer le devis.";

        if (ErrorMessage is not null) return null;

        return _data.CreateOrder(SelectedClient!, SelectedVehicle!, km, Symptoms.Trim(), tasks, SelectedMechanic!);
    }
}

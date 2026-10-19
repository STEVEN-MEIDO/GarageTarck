using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageTrack.Helpers;
using GarageTrack.Models;
using GarageTrack.Services;

namespace GarageTrack.ViewModels;

/// <summary>Dossier client : onglets « Client » et « Véhicule ».</summary>
public partial class ClientVehicleViewModel : BaseViewModel, IQueryAttributable
{
    private readonly MockDataService _data;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVehicleTab))]
    private bool isClientTab;

    [ObservableProperty] private Client? client;
    [ObservableProperty] private Vehicle? vehicle;
    [ObservableProperty] private RepairOrder? currentOrder;

    public bool IsVehicleTab => !IsClientTab;
    public bool HasCurrentOrder => CurrentOrder is not null;
    public bool HasVehicle => Vehicle is not null;

    public string Subtitle => Client is null ? string.Empty : $"{Client.FullName} · {Client.SinceText}";
    public string VehicleOwnerTitle => Client is null ? string.Empty : $"VÉHICULE DE {Client.FullName.ToUpperInvariant()}";
    public string MileageText => Vehicle?.MileageText ?? "—";
    public string FirstRegistrationText => Vehicle?.FirstRegistration.ToString("d MMMM yyyy") ?? "—";
    public string AssociatedVehicleLine => Vehicle is null ? string.Empty : $"{Vehicle.Name} · {Vehicle.MileageText}";

    public string HistorySummary
    {
        get
        {
            var h = Vehicle?.History;
            if (h is null || h.Count == 0) return "La première intervention apparaîtra ici.";
            var last = h.Max(i => i.Date);
            return $"{h.Count} intervention{(h.Count > 1 ? "s" : "")} · dernière le {last:dd/MM/yyyy}";
        }
    }

    public ClientVehicleViewModel(MockDataService data) => _data = data;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("vehicleId", out var vId))
        {
            Vehicle = _data.GetVehicle(vId?.ToString());
            Client = _data.GetClient(Vehicle?.ClientId);
        }
        else if (query.TryGetValue("clientId", out var cId))
        {
            Client = _data.GetClient(cId?.ToString());
            Vehicle = Client is null ? null : _data.VehiclesOf(Client.Id).FirstOrDefault();
        }

        IsClientTab = query.TryGetValue("tab", out var tab) && tab?.ToString() == "client";
        Refresh();
    }

    public void Refresh()
    {
        CurrentOrder = Vehicle is null ? null : _data.CurrentOrderFor(Vehicle.Id);
        OnPropertyChanged(nameof(HasCurrentOrder));
        OnPropertyChanged(nameof(HasVehicle));
        OnPropertyChanged(nameof(Subtitle));
        OnPropertyChanged(nameof(VehicleOwnerTitle));
        OnPropertyChanged(nameof(MileageText));
        OnPropertyChanged(nameof(FirstRegistrationText));
        OnPropertyChanged(nameof(AssociatedVehicleLine));
        OnPropertyChanged(nameof(HistorySummary));
    }

    [RelayCommand] private void ShowClientTab() => IsClientTab = true;
    [RelayCommand] private void ShowVehicleTab() => IsClientTab = false;

    [RelayCommand]
    private async Task EditVehicle()
    {
        if (Vehicle is null) return;
        var page = Shell.Current.CurrentPage;
        var result = await page.DisplayPromptAsync("Kilométrage actuel", "Nouveau relevé (km)",
            "Enregistrer", "Annuler", initialValue: Vehicle.Mileage.ToString(), keyboard: Keyboard.Numeric);
        if (int.TryParse(result?.Replace(" ", ""), out var km) && km > 0)
        {
            Vehicle.Mileage = km;
            Refresh();
        }
    }

    [RelayCommand]
    private Task EditClient() =>
        Ui.Alert("Modifier les coordonnées", "Le formulaire de modification sera branché au backend.");

    [RelayCommand]
    private async Task Call()
    {
        if (Client is null) return;
        try { await Launcher.OpenAsync($"tel:{Client.Phone.Replace(" ", "")}"); }
        catch { await Ui.Alert("Appeler", $"Composez le {Client.Phone}."); }
    }

    [RelayCommand]
    private async Task SendEmail()
    {
        if (Client is null) return;
        try { await Launcher.OpenAsync($"mailto:{Client.Email}"); }
        catch { await Ui.Alert("Envoyer un email", Client.Email); }
    }

    [RelayCommand]
    private Task OpenHistory() =>
        Vehicle is null ? Task.CompletedTask : Ui.GoTo($"{Routes.History}?vehicleId={Vehicle.Id}");

    [RelayCommand]
    private Task CreateOrder() =>
        Ui.GoTo($"{Routes.NewOrder}?clientId={Client?.Id}&vehicleId={Vehicle?.Id}");

    [RelayCommand]
    private Task OpenCurrentOrder()
    {
        if (CurrentOrder is null) return Task.CompletedTask;
        return CurrentOrder.Status switch
        {
            OrderStatus.Received or OrderStatus.QuoteInProgress => Ui.GoTo($"{Routes.PartsLabor}?order={CurrentOrder.Number}"),
            OrderStatus.QuotePending or OrderStatus.QuoteToSign => Ui.GoTo($"{Routes.QuoteValidation}?order={CurrentOrder.Number}"),
            _ => OpenHistory(),
        };
    }
}

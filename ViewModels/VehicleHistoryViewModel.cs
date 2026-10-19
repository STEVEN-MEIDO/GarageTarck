using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageTrack.Helpers;
using GarageTrack.Models;
using GarageTrack.Services;

namespace GarageTrack.ViewModels;

/// <summary>Suivi du véhicule : OR en cours (timeline) + interventions passées.</summary>
public partial class VehicleHistoryViewModel : BaseViewModel, IQueryAttributable
{
    private readonly MockDataService _data;

    [ObservableProperty] private Vehicle? vehicle;
    [ObservableProperty] private Client? client;
    [ObservableProperty] private RepairOrder? currentOrder;

    public List<Intervention> Interventions => Vehicle?.History.OrderByDescending(i => i.Date).ToList() ?? new();

    public string Subtitle => $"{Client?.FullName} · {Vehicle?.Name}";
    public string VehicleLine => Vehicle is null ? string.Empty : $"{Vehicle.MileageText} · {Vehicle.Engine} · {Vehicle.Year}";
    public string InterventionsCount => $"{Interventions.Count} intervention{(Interventions.Count > 1 ? "s" : "")}";
    public bool HasHistory => Interventions.Count > 0;
    public bool HasCurrentOrder => CurrentOrder is not null;

    // ----- Timeline Réceptionné → En réparation → Terminé -----
    public int Step => CurrentOrder?.Status switch
    {
        OrderStatus.Done => 3,
        OrderStatus.InRepair or OrderStatus.MissingPart => 2,
        _ => 1,
    };
    public bool Step1Done => Step > 1;
    public bool Step2Done => Step > 2;
    public bool Step2Active => Step == 2;
    public bool Step3Active => Step == 3;
    public bool Step1Pending => !Step1Done;
    public bool Step2Pending => Step < 2;
    public bool Step3Pending => Step < 3;
    public bool Step1Line => Step >= 2;
    public bool Step2Line => Step >= 3;
    public bool Step1LineOff => !Step1Line;
    public bool Step2LineOff => !Step2Line;

    public string CurrentTitle => CurrentOrder is null ? string.Empty : $"{CurrentOrder.Title} · {CurrentOrder.Mechanic}";
    public string ReceptionText => CurrentOrder is null ? string.Empty
        : CurrentOrder.ReceivedAt.Date == DateTime.Today
            ? $"Réception : aujourd’hui, {CurrentOrder.ReceivedAt:HH:mm}"
            : $"Réception : {CurrentOrder.ReceivedAt:dd/MM/yyyy}, {CurrentOrder.ReceivedAt:HH:mm}";
    public string QuoteText => CurrentOrder?.QuoteAcceptedAt is { } at
        ? $"Devis {CurrentOrder.QuoteNumber} accepté à {at:HH:mm} · {Money.Format(CurrentOrder.TotalTtc)} TTC"
        : "Devis en attente de validation";
    public string ReturnText => CurrentOrder?.PlannedReturn is { } r
        ? (r.Date == DateTime.Today ? $"Restitution prévue aujourd’hui à {r:HH:mm}" : $"Restitution prévue le {r:dd/MM} à {r:HH:mm}")
        : "Restitution à planifier";

    public VehicleHistoryViewModel(MockDataService data) => _data = data;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("vehicleId", out var id)) return;
        Vehicle = _data.GetVehicle(id?.ToString());
        Client = _data.GetClient(Vehicle?.ClientId);
        CurrentOrder = Vehicle is null ? null : _data.CurrentOrderFor(Vehicle.Id);
        OnPropertyChanged(string.Empty);
    }

    [RelayCommand]
    private Task OpenInvoice(Intervention i) =>
        Ui.Alert($"Facture {i.OrderNumber}", $"{i.Title} · {i.TotalText}\n{i.Description}");

    [RelayCommand]
    private Task ExportPdf() =>
        Ui.Alert("Exporter l’historique", "Le PDF de l’historique sera généré par le backend.");
}

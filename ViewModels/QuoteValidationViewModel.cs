using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.ApplicationModel.Communication;
using CommunityToolkit.Mvvm.Input;
using GarageTrack.Helpers;
using GarageTrack.Models;
using GarageTrack.Services;

namespace GarageTrack.ViewModels;

/// <summary>Étape 3/3 : aperçu du devis, signature du client et validation.</summary>
public partial class QuoteValidationViewModel : BaseViewModel, IQueryAttributable
{
    private readonly MockDataService _data;

    [ObservableProperty] private RepairOrder? order;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ValidateCommand))]
    [NotifyPropertyChangedFor(nameof(SignatureCaption))]
    private bool hasSignature;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ValidateCommand))]
    private bool accepted;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ValidateCommand))]
    [NotifyPropertyChangedFor(nameof(SignatureCaption), nameof(BadgeStatus))]
    private bool isValidated;

    public string GarageName => _data.GarageName;
    public string GarageAddress => _data.GarageAddress;

    public string Header => Order is null ? string.Empty : $"{Order.Number} · {Order.Client.FullName}";
    public string QuoteTitle => $"DEVIS {Order?.QuoteNumber}";
    public string IssuedText => $"Émis le {DateTime.Today:dd/MM/yyyy} · valable 30 jours";
    public string ClientName => Order?.Client.FullName ?? string.Empty;
    public string ClientAddress => Order?.Client.FullAddress ?? string.Empty;
    public string ClientContact => Order is null ? string.Empty : $"{Order.Client.Email} · {Order.Client.Phone}";
    public string Plate => Order?.Vehicle.Plate ?? string.Empty;
    public string VehicleName => Order?.Vehicle.Name ?? string.Empty;
    public string VehicleKm => Order is null ? string.Empty : Money.Km(Order.EntryMileage);

    public List<QuoteLine> Lines => Order is null ? new() : Order.Parts.Concat(Order.Labor).ToList();
    public string TotalHt => Money.Format(Order?.TotalHt ?? 0);
    public string Vat => Money.Format(Order?.Vat ?? 0);
    public string TotalTtc => Money.Format(Order?.TotalTtc ?? 0);
    public string AcceptText => $"J’accepte le devis de {TotalTtc} TTC et autorise les travaux indiqués.";

    public OrderStatus BadgeStatus => IsValidated ? OrderStatus.InRepair : OrderStatus.QuoteToSign;

    public string SignatureCaption =>
        IsValidated ? "Devis signé · modification verrouillée."
        : HasSignature ? $"{ClientName} · signature recueillie"
        : "Le client doit signer avant validation.";

    public QuoteValidationViewModel(MockDataService data) => _data = data;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("order", out var number))
        {
            Order = _data.GetOrder(number?.ToString());
            if (Order is not null && string.IsNullOrEmpty(Order.QuoteNumber))
                _data.PrepareQuote(Order);
            OnPropertyChanged(string.Empty);
        }
    }

    [RelayCommand]
    private void ToggleAccepted()
    {
        if (!IsValidated) Accepted = !Accepted;
    }

    private bool CanValidate() => HasSignature && Accepted && !IsValidated;

    [RelayCommand(CanExecute = nameof(CanValidate))]
    private async Task Validate()
    {
        if (Order is null) return;
        _data.AcceptQuote(Order);
        IsValidated = true;
        await Ui.Alert("Devis validé", $"{Order.QuoteNumber} signé par {ClientName}. Les travaux peuvent commencer.");
        await Ui.GoTo($"{Routes.Main}/{Routes.History}?vehicleId={Order.Vehicle.Id}");
    }

    [RelayCommand]
    private Task ExportPdf() =>
        Ui.Alert("Exporter en PDF", $"Le PDF de {Order?.QuoteNumber} sera généré par le backend.");

    [RelayCommand]
    private async Task SendSms()
    {
        if (Order is null) return;
        var body = $"{GarageName} : votre devis {Order.QuoteNumber} de {TotalTtc} TTC est prêt.";
        try { await Sms.Default.ComposeAsync(new SmsMessage(body, Order.Client.Phone.Replace(" ", ""))); }
        catch { await Ui.Alert("Envoyer par SMS", body); }
    }

    [RelayCommand]
    private async Task SendEmail()
    {
        if (Order is null) return;
        try
        {
            await Email.Default.ComposeAsync(new EmailMessage
            {
                Subject = $"Devis {Order.QuoteNumber} · {GarageName}",
                Body = $"Bonjour {ClientName},\n\nVeuillez trouver votre devis d’un montant de {TotalTtc} TTC.\n\n{GarageName}",
                To = new List<string> { Order.Client.Email },
            });
        }
        catch { await Ui.Alert("Envoyer par email", $"Devis à envoyer à {Order.Client.Email}."); }
    }
}

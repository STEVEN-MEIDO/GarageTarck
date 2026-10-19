using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageTrack.Helpers;
using GarageTrack.Models;
using GarageTrack.Services;

namespace GarageTrack.ViewModels;

/// <summary>Étape 2/3 : pièces & main-d'œuvre, totaux recalculés automatiquement.</summary>
public partial class PartsLaborViewModel : BaseViewModel, IQueryAttributable
{
    private readonly MockDataService _data;

    [ObservableProperty] private RepairOrder? order;

    public ObservableCollection<QuoteLine> Parts => Order?.Parts ?? new();
    public ObservableCollection<QuoteLine> Labor => Order?.Labor ?? new();

    public string Header => Order is null ? string.Empty : $"{Order.Number} · {Order.Client.FullName}";
    public string Plate => Order?.Vehicle.Plate ?? string.Empty;
    public string PartsCount => $"{Parts.Count} référence{(Parts.Count > 1 ? "s" : "")}";
    public string TotalHt => Money.Format(Order?.TotalHt ?? 0);
    public string Vat => Money.Format(Order?.Vat ?? 0);
    public string TotalTtc => Money.Format(Order?.TotalTtc ?? 0);
    public bool HasLines => Parts.Count + Labor.Count > 0;

    public PartsLaborViewModel(MockDataService data) => _data = data;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("order", out var number)) return;
        Order = _data.GetOrder(number?.ToString());
        if (Order is null) return;

        Order.Parts.CollectionChanged += OnLinesChanged;
        Order.Labor.CollectionChanged += OnLinesChanged;
        foreach (var line in Order.Parts.Concat(Order.Labor))
            line.PropertyChanged += OnLineChanged;

        OnPropertyChanged(string.Empty); // rafraîchit toutes les liaisons
        ContinueCommand.NotifyCanExecuteChanged();
    }

    private void OnLinesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
            foreach (QuoteLine l in e.NewItems) l.PropertyChanged += OnLineChanged;
        if (e.OldItems is not null)
            foreach (QuoteLine l in e.OldItems) l.PropertyChanged -= OnLineChanged;
        RecalculateTotals();
    }

    private void OnLineChanged(object? sender, PropertyChangedEventArgs e) => RecalculateTotals();

    private void RecalculateTotals()
    {
        OnPropertyChanged(nameof(PartsCount));
        OnPropertyChanged(nameof(TotalHt));
        OnPropertyChanged(nameof(Vat));
        OnPropertyChanged(nameof(TotalTtc));
        OnPropertyChanged(nameof(HasLines));
        ContinueCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task AddPart()
    {
        if (Order is null) return;
        var choice = await Ui.Choose("Ajouter une pièce",
            _data.PartCatalog.Select(p => $"{p.Label} · {Money.Format(p.UnitPrice)}").ToArray());
        var model = _data.PartCatalog.FirstOrDefault(p => choice?.StartsWith(p.Label) == true);
        if (model is null) return;

        var existing = Order.Parts.FirstOrDefault(p => p.Reference == model.Reference);
        if (existing is not null) existing.Quantity += 1;
        else Order.Parts.Add(new QuoteLine { Label = model.Label, Reference = model.Reference, Quantity = 1, UnitPrice = model.UnitPrice });
    }

    [RelayCommand]
    private async Task AddLabor()
    {
        if (Order is null) return;
        var choice = await Ui.Choose("Ajouter une prestation",
            _data.LaborCatalog.Select(l => $"{l.Label} · {Money.Number(l.Quantity)} h").ToArray());
        var model = _data.LaborCatalog.FirstOrDefault(l => choice?.StartsWith(l.Label) == true);
        if (model is null) return;

        Order.Labor.Add(new QuoteLine
        {
            Label = model.Label, Reference = model.Reference, Quantity = model.Quantity,
            UnitPrice = model.UnitPrice, Mechanic = Order.Mechanic, IsLabor = true,
        });
    }

    /// <summary>Touche sur une ligne : modifier la quantité (ou les heures), 0 supprime la ligne.</summary>
    [RelayCommand]
    private async Task EditLine(QuoteLine line)
    {
        if (Order is null) return;
        var unit = line.IsLabor ? "Heures" : "Quantité";
        var result = await Shell.Current.CurrentPage.DisplayPromptAsync(line.Label,
            $"{unit} (0 pour supprimer la ligne)", "Valider", "Annuler",
            initialValue: line.QuantityText, keyboard: Keyboard.Numeric);
        if (result is null) return;

        if (!decimal.TryParse(result.Replace('.', ','), System.Globalization.NumberStyles.Number,
                new System.Globalization.CultureInfo("fr-CM"), out var qty) || qty < 0)
        {
            await Ui.Alert("Quantité invalide", "Indiquez au moins 1 pièce.");
            return;
        }

        if (qty == 0)
        {
            if (line.IsLabor) Order.Labor.Remove(line); else Order.Parts.Remove(line);
        }
        else line.Quantity = qty;
    }

    [RelayCommand(CanExecute = nameof(HasLines))]
    private Task Continue()
    {
        if (Order is null) return Task.CompletedTask;
        _data.PrepareQuote(Order);
        return Ui.GoTo($"{Routes.QuoteValidation}?order={Order.Number}");
    }
}

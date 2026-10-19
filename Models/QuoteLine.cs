using CommunityToolkit.Mvvm.ComponentModel;
using GarageTrack.Helpers;

namespace GarageTrack.Models;

/// <summary>Ligne de devis : pièce (quantité × PU) ou main-d'œuvre (heures × taux).</summary>
public partial class QuoteLine : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Total), nameof(TotalText), nameof(QuantityText), nameof(Detail))]
    private decimal quantity = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Total), nameof(TotalText), nameof(UnitPriceText), nameof(Detail))]
    private decimal unitPrice;

    public string Label { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string? Mechanic { get; set; }
    public bool IsLabor { get; set; }

    public decimal Total => Quantity * UnitPrice;
    public string TotalText => Money.Format(Total);
    public string UnitPriceText => Money.Format(UnitPrice);
    public string QuantityText => Money.Number(Quantity);

    /// <summary>Sous-titre du tableau : « BOS-0986479R » ou « MO-FREIN · Ibrahim Moussa ».</summary>
    public string SubLine => string.IsNullOrEmpty(Mechanic) ? Reference : $"{Reference} · {Mechanic}";

    /// <summary>Détail de l'aperçu du devis : « 1 × 75 000 FCFA HT · BOS-0986479R ».</summary>
    public string Detail => IsLabor
        ? $"{QuantityText} h × {UnitPriceText} HT · {Reference}"
        : $"{QuantityText} × {UnitPriceText} HT · {Reference}";
}

using GarageTrack.Helpers;

namespace GarageTrack.Models;

/// <summary>Intervention passée affichée dans l'historique du véhicule.</summary>
public class Intervention
{
    public DateTime Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal TotalTtc { get; set; }

    public string DateText => Date.ToString("d MMMM yyyy");
    public string Reference => $"{OrderNumber} · {Money.Km(Mileage)}";
    public string TotalText => $"{Money.Format(TotalTtc)} TTC";
}

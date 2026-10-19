using System.Collections.ObjectModel;
using GarageTrack.Helpers;

namespace GarageTrack.Models;

/// <summary>Ordre de réparation (OR).</summary>
public class RepairOrder
{
    public string Number { get; set; } = string.Empty;
    public Client Client { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;
    public OrderStatus Status { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Mechanic { get; set; } = string.Empty;
    public string Symptoms { get; set; } = string.Empty;
    public int EntryMileage { get; set; }
    public List<string> CheckedTasks { get; set; } = new();

    public string QuoteNumber { get; set; } = string.Empty;
    public DateTime? QuoteAcceptedAt { get; set; }
    public DateTime? PlannedReturn { get; set; }

    public ObservableCollection<QuoteLine> Parts { get; set; } = new();
    public ObservableCollection<QuoteLine> Labor { get; set; } = new();

    public const decimal VatRate = 0.1925m; // TVA Cameroun : 17,5 % + 10 % de centimes additionnels communaux
    public decimal TotalHt => Parts.Sum(p => p.Total) + Labor.Sum(l => l.Total);
    public decimal Vat => Math.Round(TotalHt * VatRate, 0);
    public decimal TotalTtc => TotalHt + Vat;

    // Textes d'affichage
    public string VehicleAndClient => $"{Vehicle.Name} · {Client.FullName}";
    public string NumberAndTime => ReceivedAt.Date == DateTime.Today
        ? $"{Number} · {ReceivedAt:HH:mm}"
        : ReceivedAt.Date == DateTime.Today.AddDays(-1) ? $"{Number} · Hier" : $"{Number} · {ReceivedAt:dd/MM}";
    public string TitleAndMechanic => $"{Title} · confié à {Mechanic}";
    public string ReceivedText => ReceivedAt.Date == DateTime.Today
        ? $"Réceptionné aujourd’hui à {ReceivedAt:HH:mm}"
        : $"Réceptionné le {ReceivedAt:dd/MM/yyyy} à {ReceivedAt:HH:mm}";
    public string TotalTtcText => Money.Format(TotalTtc);
}

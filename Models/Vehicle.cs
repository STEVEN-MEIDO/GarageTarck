using GarageTrack.Helpers;

namespace GarageTrack.Models;

public class Vehicle
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ClientId { get; set; } = string.Empty;
    public string Plate { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Engine { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Fuel { get; set; } = "Diesel";
    public string Gearbox { get; set; } = "Manuelle";
    public int Mileage { get; set; }
    public string Vin { get; set; } = string.Empty;
    public DateTime FirstRegistration { get; set; }
    public int LastReadingKm { get; set; }
    public DateTime LastReadingDate { get; set; }

    public List<Intervention> History { get; set; } = new();

    public string Name => $"{Brand} {Model}";
    public string NameWithEngine => $"{Brand} {Model} · {Engine}";
    public string Specs => $"{Year}  •  {Fuel}  •  {Gearbox}";
    public string MileageText => Money.Km(Mileage);

    public override string ToString() => $"{Plate} · {Name}";
}

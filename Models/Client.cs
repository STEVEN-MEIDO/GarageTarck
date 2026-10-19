namespace GarageTrack.Models;

public class Client
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string FullName { get; set; } = string.Empty;
    public string Kind { get; set; } = "Particulier";
    public DateTime ClientSince { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public bool IsVerified { get; set; } = true;
    public bool IsWoman { get; set; }

    /// <summary>« cliente depuis 2024 » / « client depuis 2024 »</summary>
    public string SinceText => $"{(IsWoman ? "cliente" : "client")} depuis {ClientSince.Year}";
    public string SinceLongText => $"{Kind} · {(IsWoman ? "cliente" : "client")} depuis {ClientSince:MMMM yyyy}";

    public string Initials =>
        string.Concat(FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => char.ToUpper(p[0])));

    public string FullAddress => string.IsNullOrEmpty(City) ? AddressLine : $"{AddressLine} · {City}";

    public override string ToString() => FullName; // affichage dans les Picker
}

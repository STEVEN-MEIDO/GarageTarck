using System.Globalization;

namespace GarageTrack.Helpers;

public static class Money
{
    private static readonly CultureInfo Fr = new("fr-CM");

    /// <summary>65000 → « 65 000 FCFA » (le franc CFA n'a pas de centimes)</summary>
    public static string Format(decimal amount) => amount.ToString("N0", Fr) + " FCFA";

    /// <summary>124850 → « 124 850 km »</summary>
    public static string Km(int km) => km.ToString("N0", Fr) + " km";

    /// <summary>2.5 → « 2,5 »</summary>
    public static string Number(decimal value) => value.ToString("0.##", Fr);

    public static string Capitalize(string text) =>
        string.IsNullOrEmpty(text) ? text : char.ToUpper(text[0], Fr) + text[1..];
}

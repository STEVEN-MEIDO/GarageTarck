namespace GarageTrack.Helpers;

/// <summary>Petites boîtes de dialogue utilisées par les ViewModels.</summary>
public static class Ui
{
    private static Page? Current => Shell.Current?.CurrentPage ?? Application.Current?.Windows.FirstOrDefault()?.Page;

    public static Task Alert(string title, string message) =>
        Current?.DisplayAlert(title, message, "OK") ?? Task.CompletedTask;

    public static Task<bool> Confirm(string title, string message, string accept, string cancel = "Annuler") =>
        Current?.DisplayAlert(title, message, accept, cancel) ?? Task.FromResult(false);

    public static async Task<string?> Choose(string title, params string[] options)
    {
        if (Current is null) return null;
        var choice = await Current.DisplayActionSheet(title, "Annuler", null, options);
        return choice is null or "Annuler" ? null : choice;
    }

    public static Task GoTo(string route) => Shell.Current.GoToAsync(route);
    public static Task Back() => Shell.Current.GoToAsync("..");
}

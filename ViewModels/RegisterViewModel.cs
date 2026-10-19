using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageTrack.Helpers;
using GarageTrack.Services;

namespace GarageTrack.ViewModels;

public partial class RegisterViewModel : BaseViewModel
{
    private readonly MockDataService _data;

    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string garageName = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string phone = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Strength), nameof(StrengthText), nameof(StrengthColor),
                              nameof(Bar1), nameof(Bar2), nameof(Bar3), nameof(Bar4))]
    private string password = string.Empty;

    [ObservableProperty] private bool isPasswordHidden = true;
    [ObservableProperty] private bool acceptTerms;
    [ObservableProperty] private string? errorMessage;

    public RegisterViewModel(MockDataService data) => _data = data;

    public string PasswordGlyph => IsPasswordHidden ? Icons.EyeOff : Icons.Eye;
    partial void OnIsPasswordHiddenChanged(bool value) => OnPropertyChanged(nameof(PasswordGlyph));

    // ----- Jauge de robustesse (0 à 4) -----
    public int Strength
    {
        get
        {
            if (string.IsNullOrEmpty(Password)) return 0;
            var score = 0;
            if (Password.Length >= 8) score++;
            if (Password.Length >= 12) score++;
            if (Password.Any(char.IsDigit)) score++;
            if (Password.Any(c => !char.IsLetterOrDigit(c))) score++;
            if (Password.Length < 12) score = Math.Min(score, 2);
            return Math.Max(score, 1);
        }
    }

    public string StrengthText => Strength switch
    {
        0 => "Saisissez au moins 12 caractères.",
        1 => "Trop faible. Ajoutez chiffres et symboles.",
        2 => "Faible · allongez votre mot de passe.",
        3 => "Moyen · ajoutez un caractère spécial.",
        _ => "Fort · votre compte est bien protégé.",
    };

    public Color StrengthColor => Strength switch
    {
        <= 1 => Color.FromArgb("#EF4444"),
        2 or 3 => Color.FromArgb("#F59E0B"),
        _ => Color.FromArgb("#10B981"),
    };

    private Color BarColor(int index) => Strength >= index ? StrengthColor : Color.FromArgb("#E2E8F0");
    public Color Bar1 => BarColor(1);
    public Color Bar2 => BarColor(2);
    public Color Bar3 => BarColor(3);
    public Color Bar4 => BarColor(4);

    [RelayCommand]
    private void TogglePassword() => IsPasswordHidden = !IsPasswordHidden;

    [RelayCommand]
    private void ToggleTerms() => AcceptTerms = !AcceptTerms;

    [RelayCommand]
    private Task ReadTerms() => Ui.Alert("CGU et confidentialité",
        "Vos données d’atelier restent confidentielles et ne sont jamais revendues. (Texte complet à venir.)");

    [RelayCommand]
    private async Task CreateAccount()
    {
        ErrorMessage = Validate();
        if (ErrorMessage is not null) return;

        _data.UserFullName = FullName.Trim();
        _data.UserEmail = Email.Trim();
        _data.UserPhone = Phone.Trim();
        await Ui.Alert("Bienvenue !", $"L’atelier « {GarageName.Trim()} » a été créé.");
        await Ui.GoTo(Routes.Main);
    }

    private string? Validate()
    {
        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(GarageName) ||
            string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Phone))
            return "Complétez les champs obligatoires.";
        if (!Email.Contains('@') || !Email.Contains('.'))
            return "Utilisez une adresse de type nom@gmail.com.";
        if (Password.Length < 12 || !Password.Any(char.IsDigit) || Password.All(char.IsLetterOrDigit))
            return "Mot de passe : 12 caractères minimum, dont un chiffre et un symbole.";
        if (!AcceptTerms)
            return "Acceptez les CGU pour créer votre compte.";
        return null;
    }

    [RelayCommand]
    private Task Login() => Ui.Back();
}

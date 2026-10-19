using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageTrack.Helpers;
using GarageTrack.Services;

namespace GarageTrack.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly MockDataService _data;

    // Valeurs de démonstration : n'importe quel email valide + 12 caractères fonctionnent
    [ObservableProperty] private string email = "rodrigue.fokou@gmail.com";
    [ObservableProperty] private string password = "GarageTrack#26";
    [ObservableProperty] private bool isPasswordHidden = true;
    [ObservableProperty] private string? errorMessage;

    public string PasswordGlyph => IsPasswordHidden ? Icons.EyeOff : Icons.Eye;

    public LoginViewModel(MockDataService data) => _data = data;

    partial void OnIsPasswordHiddenChanged(bool value) => OnPropertyChanged(nameof(PasswordGlyph));

    [RelayCommand]
    private void TogglePassword() => IsPasswordHidden = !IsPasswordHidden;

    [RelayCommand]
    private async Task Login()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains('@') || !Email.Contains('.'))
        {
            ErrorMessage = "Utilisez une adresse de type nom@gmail.com.";
            return;
        }
        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Saisissez votre mot de passe.";
            return;
        }

        _data.UserEmail = Email.Trim();
        await Ui.GoTo(Routes.Main);
    }

    [RelayCommand]
    private Task Register() => Ui.GoTo(Routes.Register);

    [RelayCommand]
    private Task ForgotPassword() =>
        Ui.Alert("Mot de passe oublié", $"Un lien de réinitialisation sera envoyé à {Email}.");
}

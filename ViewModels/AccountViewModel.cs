using CommunityToolkit.Mvvm.Input;
using GarageTrack.Helpers;
using GarageTrack.Services;

namespace GarageTrack.ViewModels;

public partial class AccountViewModel : BaseViewModel
{
    private readonly MockDataService _data;

    public AccountViewModel(MockDataService data) => _data = data;

    public string FullName => _data.UserFullName;
    public string Initials => string.Concat(_data.UserFullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => p[0]));
    public string Email => _data.UserEmail;
    public string Phone => _data.UserPhone;
    public string GarageName => _data.GarageName;
    public string GarageAddress => _data.GarageAddress;
    public string MechanicsText => string.Join(", ", _data.Mechanics);

    public void Refresh() => OnPropertyChanged(string.Empty);

    [RelayCommand]
    private async Task Logout()
    {
        if (await Ui.Confirm("Se déconnecter", "Voulez-vous vraiment vous déconnecter ?", "Se déconnecter"))
            await Ui.GoTo(Routes.Login);
    }
}

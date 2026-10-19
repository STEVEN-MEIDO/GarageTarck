using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageTrack.Helpers;

namespace GarageTrack.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [RelayCommand]
    protected virtual Task GoBack() => Ui.Back();
}

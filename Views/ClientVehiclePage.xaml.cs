using GarageTrack.ViewModels;

namespace GarageTrack.Views;

public partial class ClientVehiclePage : ContentPage
{
    private readonly ClientVehicleViewModel _vm;

    public ClientVehiclePage(ClientVehicleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _vm = viewModel;
    }

    // Les données peuvent avoir changé (nouvel OR, devis validé…) : on rafraîchit à chaque affichage.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Refresh();
    }
}

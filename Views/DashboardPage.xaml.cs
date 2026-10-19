using GarageTrack.Models;
using GarageTrack.ViewModels;

namespace GarageTrack.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _vm;

    public DashboardPage(DashboardViewModel viewModel)
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

    private void OnOrderTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: RepairOrder order })
            _vm.OpenOrderCommand.Execute(order);
    }
}

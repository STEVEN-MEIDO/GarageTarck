using GarageTrack.Models;
using GarageTrack.ViewModels;

namespace GarageTrack.Views;

public partial class VehicleHistoryPage : ContentPage
{
    private readonly VehicleHistoryViewModel _vm;

    public VehicleHistoryPage(VehicleHistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _vm = viewModel;
    }

    private void OnInvoiceTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: Intervention intervention })
            _vm.OpenInvoiceCommand.Execute(intervention);
    }
}

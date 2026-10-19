using GarageTrack.Models;
using GarageTrack.ViewModels;

namespace GarageTrack.Views;

public partial class QuotesPage : ContentPage
{
    private readonly QuotesViewModel _vm;

    public QuotesPage(QuotesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _vm = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Refresh();
    }

    private void OnQuoteTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: RepairOrder order })
            _vm.OpenCommand.Execute(order);
    }
}

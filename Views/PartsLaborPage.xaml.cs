using GarageTrack.Models;
using GarageTrack.ViewModels;

namespace GarageTrack.Views;

public partial class PartsLaborPage : ContentPage
{
    private readonly PartsLaborViewModel _vm;

    public PartsLaborPage(PartsLaborViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _vm = viewModel;
    }

    private void OnLineTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: QuoteLine line })
            _vm.EditLineCommand.Execute(line);
    }
}

using GarageTrack.Models;
using GarageTrack.ViewModels;

namespace GarageTrack.Views;

public partial class NewRepairOrderPage : ContentPage
{
    private readonly NewRepairOrderViewModel _vm;

    public NewRepairOrderPage(NewRepairOrderViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _vm = viewModel;
    }

    // Toucher le libellé coche / décoche la tâche (cible tactile plus large)
    private void OnTaskTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: TaskItem task })
            _vm.ToggleTaskCommand.Execute(task);
    }
}

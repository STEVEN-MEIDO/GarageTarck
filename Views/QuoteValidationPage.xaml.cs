using GarageTrack.ViewModels;

namespace GarageTrack.Views;

public partial class QuoteValidationPage : ContentPage
{
    public QuoteValidationPage(QuoteValidationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Pendant que le client signe, on bloque le défilement pour que le trait ne fasse pas scroller la page.
        Pad.StartInteraction += (_, _) => Scroller.Orientation = ScrollOrientation.Neither;
        Pad.EndInteraction += (_, _) => Scroller.Orientation = ScrollOrientation.Vertical;
        Pad.CancelInteraction += (_, _) => Scroller.Orientation = ScrollOrientation.Vertical;
    }

    private void OnClearSignature(object? sender, TappedEventArgs e) => Pad.Clear();
}

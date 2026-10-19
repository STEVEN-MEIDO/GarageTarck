using GarageTrack.Models;
using Microsoft.Maui.Controls.Shapes;

namespace GarageTrack.Controls;

/// <summary>
/// Badge de statut nommé et coloré (le texte reste explicite sans dépendre de la couleur).
/// </summary>
public class StatusBadge : Border
{
    private readonly Ellipse _dot;
    private readonly Label _label;

    public static readonly BindableProperty StatusProperty =
        BindableProperty.Create(nameof(Status), typeof(OrderStatus), typeof(StatusBadge), OrderStatus.Received,
            propertyChanged: (b, _, _) => ((StatusBadge)b).Apply());

    public OrderStatus Status { get => (OrderStatus)GetValue(StatusProperty); set => SetValue(StatusProperty, value); }

    public StatusBadge()
    {
        StrokeThickness = 0;
        StrokeShape = new RoundRectangle { CornerRadius = 8 };
        Padding = new Thickness(10, 5);
        HorizontalOptions = LayoutOptions.End;
        VerticalOptions = LayoutOptions.Center;

        _dot = new Ellipse { WidthRequest = 6, HeightRequest = 6, VerticalOptions = LayoutOptions.Center };
        _label = new Label { FontFamily = "InterSemiBold", FontSize = 14, VerticalOptions = LayoutOptions.Center };

        Content = new HorizontalStackLayout { Spacing = 7, Children = { _dot, _label } };
        Apply();
    }

    private void Apply()
    {
        var (text, dot, fg, bg) = Describe(Status);
        _label.Text = text;
        _label.TextColor = Color.FromArgb(fg);
        _dot.Fill = new SolidColorBrush(Color.FromArgb(dot));
        BackgroundColor = Color.FromArgb(bg);
    }

    public static (string Text, string Dot, string Fg, string Bg) Describe(OrderStatus status) => status switch
    {
        OrderStatus.Received => ("Réceptionné", "#64748B", "#475569", "#F1F5F9"),
        OrderStatus.QuoteInProgress => ("Devis en préparation", "#7B1E3A", "#7B1E3A", "#FDF2F4"),
        OrderStatus.QuotePending => ("Devis à valider", "#7B1E3A", "#7B1E3A", "#FDF2F4"),
        OrderStatus.QuoteToSign => ("À valider", "#7B1E3A", "#7B1E3A", "#FDF2F4"),
        OrderStatus.InRepair => ("En réparation", "#F59E0B", "#C2410C", "#FFF7ED"),
        OrderStatus.MissingPart => ("Pièce manquante", "#EF4444", "#B91C1C", "#FEF2F2"),
        OrderStatus.Done => ("Terminé", "#10B981", "#047857", "#ECFDF5"),
        _ => ("—", "#64748B", "#475569", "#F1F5F9")
    };
}

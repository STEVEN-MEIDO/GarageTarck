using Microsoft.Maui.Controls.Shapes;

namespace GarageTrack.Controls;

/// <summary>
/// Plaque d'immatriculation camerounaise : drapeau + « CMR » à gauche, numéro « LT 123 AB » (code région, chiffres, lettres).
/// </summary>
public class LicensePlate : Border
{
    private readonly Label _number;
    private readonly Label _country;
    private readonly Label _star;
    private readonly Grid _flag;
    private readonly BoxView _separator;
    private readonly VerticalStackLayout _left;

    public static readonly BindableProperty PlateProperty =
        BindableProperty.Create(nameof(Plate), typeof(string), typeof(LicensePlate), "LT 123 AB",
            propertyChanged: (b, _, n) => ((LicensePlate)b)._number.Text = (string)n);

    public static readonly BindableProperty IsLargeProperty =
        BindableProperty.Create(nameof(IsLarge), typeof(bool), typeof(LicensePlate), false,
            propertyChanged: (b, _, _) => ((LicensePlate)b).ApplySize());

    public string Plate { get => (string)GetValue(PlateProperty); set => SetValue(PlateProperty, value); }
    public bool IsLarge { get => (bool)GetValue(IsLargeProperty); set => SetValue(IsLargeProperty, value); }

    public LicensePlate()
    {
        var ink = Color.FromArgb("#1E293B");

        Stroke = ink;
        StrokeThickness = 1.5;
        BackgroundColor = Colors.White;
        Padding = 0;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;

        // Drapeau du Cameroun : vert, rouge (étoile jaune), jaune
        _star = new Label { Text = "★", TextColor = Color.FromArgb("#FCD116"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        _flag = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
            HorizontalOptions = LayoutOptions.Center,
        };
        _flag.Add(new BoxView { Color = Color.FromArgb("#007A5E") }, 0);
        _flag.Add(new BoxView { Color = Color.FromArgb("#CE1126") }, 1);
        _flag.Add(_star, 1);
        _flag.Add(new BoxView { Color = Color.FromArgb("#FCD116") }, 2);

        _country = new Label { Text = "CMR", TextColor = ink, FontFamily = "InterBold", HorizontalTextAlignment = TextAlignment.Center };
        _left = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, Children = { _flag, _country } };
        _separator = new BoxView { Color = ink, WidthRequest = 1, VerticalOptions = LayoutOptions.Fill };
        _number = new Label { Text = Plate, TextColor = ink, FontFamily = "InterBold", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
            }
        };
        grid.Add(_left, 0);
        grid.Add(_separator, 1);
        grid.Add(_number, 2);
        Content = grid;

        ApplySize();
    }

    private void ApplySize()
    {
        var large = IsLarge;
        StrokeShape = new RoundRectangle { CornerRadius = large ? 8 : 5 };
        HeightRequest = large ? 66 : 32;

        _left.Spacing = large ? 4 : 1;
        _left.Margin = large ? new Thickness(10, 0) : new Thickness(5, 0);
        _flag.WidthRequest = large ? 30 : 17;
        _flag.HeightRequest = large ? 20 : 11;
        _star.FontSize = large ? 9 : 5;
        _country.FontSize = large ? 12 : 7;
        _number.FontSize = large ? 40 : 20;
        _number.Margin = large ? new Thickness(18, 0) : new Thickness(9, 0);
        _number.CharacterSpacing = large ? 1 : 0.5;
    }
}

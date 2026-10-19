using System.Windows.Input;

namespace GarageTrack.Controls;

public enum ButtonKind { Primary, Secondary }

/// <summary>
/// Bouton du design : orange plein (action principale) ou blanc bordé (secondaire),
/// avec une icône optionnelle devant le texte.
/// </summary>
public class ActionButton : Border
{
    private readonly Icon _icon;
    private readonly Label _label;

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(ActionButton), string.Empty,
            propertyChanged: (b, _, n) => ((ActionButton)b)._label.Text = (string)n);

    public static readonly BindableProperty GlyphProperty =
        BindableProperty.Create(nameof(Glyph), typeof(string), typeof(ActionButton), null,
            propertyChanged: (b, _, n) => ((ActionButton)b).UpdateIcon());

    public static readonly BindableProperty KindProperty =
        BindableProperty.Create(nameof(Kind), typeof(ButtonKind), typeof(ActionButton), ButtonKind.Primary,
            propertyChanged: (b, _, _) => ((ActionButton)b).ApplyKind());

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ActionButton), null,
            propertyChanged: OnCommandChanged);

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ActionButton));

    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public string? Glyph { get => (string?)GetValue(GlyphProperty); set => SetValue(GlyphProperty, value); }
    public ButtonKind Kind { get => (ButtonKind)GetValue(KindProperty); set => SetValue(KindProperty, value); }
    public ICommand? Command { get => (ICommand?)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }
    public object? CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }

    public event EventHandler? Clicked;

    public ActionButton()
    {
        _icon = new Icon { Size = 22, IsVisible = false };
        _label = new Label { FontFamily = "InterSemiBold", FontSize = 17, VerticalOptions = LayoutOptions.Center };

        Content = new HorizontalStackLayout
        {
            Spacing = 12,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children = { _icon, _label }
        };

        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 };
        HeightRequest = 56;
        Padding = new Thickness(16, 0);

        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTapped;
        GestureRecognizers.Add(tap);

        ApplyKind();
    }

    private async void OnTapped(object? sender, TappedEventArgs e)
    {
        if (!IsEnabled) return;
        await this.FadeTo(0.7, 60);
        await this.FadeTo(1, 90);

        Clicked?.Invoke(this, EventArgs.Empty);
        if (Command?.CanExecute(CommandParameter) == true)
            Command.Execute(CommandParameter);
    }

    private void ApplyKind()
    {
        if (Kind == ButtonKind.Primary)
        {
            BackgroundColor = Color.FromArgb("#F97316");
            Stroke = Colors.Transparent;
            StrokeThickness = 0;
            _label.TextColor = Color.FromArgb("#1E293B");
            _label.FontSize = 17;
            _icon.Color = Color.FromArgb("#1E293B");
        }
        else
        {
            BackgroundColor = Colors.White;
            Stroke = Color.FromArgb("#E2E8F0");
            StrokeThickness = 1;
            _label.TextColor = Color.FromArgb("#7B1E3A");
            _label.FontSize = 16;
            _icon.Color = Color.FromArgb("#7B1E3A");
        }
    }

    private void UpdateIcon()
    {
        _icon.Glyph = Glyph;
        _icon.IsVisible = !string.IsNullOrEmpty(Glyph);
    }

    private static void OnCommandChanged(BindableObject b, object o, object n)
    {
        var button = (ActionButton)b;
        if (o is ICommand oldCmd) oldCmd.CanExecuteChanged -= button.OnCanExecuteChanged;
        if (n is ICommand newCmd) newCmd.CanExecuteChanged += button.OnCanExecuteChanged;
        button.RefreshEnabled();
    }

    private void OnCanExecuteChanged(object? sender, EventArgs e) => RefreshEnabled();

    private void RefreshEnabled()
    {
        var enabled = Command?.CanExecute(CommandParameter) ?? true;
        IsEnabled = enabled;
        Opacity = enabled ? 1 : 0.45;
    }
}

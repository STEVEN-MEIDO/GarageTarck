using Microsoft.Maui.Graphics;

namespace GarageTrack.Controls;

/// <summary>
/// Icône vectorielle au trait. Usage :
/// &lt;controls:Icon Glyph="{x:Static helpers:Icons.Mail}" Color="{StaticResource Muted}" Size="22" /&gt;
/// Dessinée via GraphicsView (trait seul) : un Shapes.Path déclenche un ClipPath que Win2D refuse
/// sur ces tracés (COMException « Élément introuvable » sous Windows).
/// </summary>
public class Icon : GraphicsView
{
    private readonly IconDrawable _drawable = new();

    public static readonly BindableProperty GlyphProperty =
        BindableProperty.Create(nameof(Glyph), typeof(string), typeof(Icon), null, propertyChanged: OnGlyphChanged);

    public static readonly BindableProperty ColorProperty =
        BindableProperty.Create(nameof(Color), typeof(Color), typeof(Icon), Colors.Black, propertyChanged: OnColorChanged);

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(double), typeof(Icon), 24d, propertyChanged: OnSizeChanged);

    public static readonly BindableProperty ThicknessProperty =
        BindableProperty.Create(nameof(Thickness), typeof(double), typeof(Icon), 2d, propertyChanged: OnThicknessChanged);

    public string? Glyph { get => (string?)GetValue(GlyphProperty); set => SetValue(GlyphProperty, value); }
    public Color Color { get => (Color)GetValue(ColorProperty); set => SetValue(ColorProperty, value); }
    public double Size { get => (double)GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    public double Thickness { get => (double)GetValue(ThicknessProperty); set => SetValue(ThicknessProperty, value); }

    public Icon()
    {
        InputTransparent = true;
        BackgroundColor = Colors.Transparent;
        VerticalOptions = LayoutOptions.Center;
        HorizontalOptions = LayoutOptions.Center;
        Drawable = _drawable;
        ApplySize();
    }

    private void ApplySize()
    {
        WidthRequest = Size;
        HeightRequest = Size;
        Invalidate();
    }

    private static void OnGlyphChanged(BindableObject b, object o, object n)
    {
        var icon = (Icon)b;
        icon._drawable.Path = n is string s && !string.IsNullOrWhiteSpace(s) ? PathBuilder.Build(s) : null;
        icon.Invalidate();
    }

    private static void OnColorChanged(BindableObject b, object o, object n)
    {
        var icon = (Icon)b;
        icon._drawable.Color = (Color)n;
        icon.Invalidate();
    }

    private static void OnSizeChanged(BindableObject b, object o, object n) => ((Icon)b).ApplySize();

    private static void OnThicknessChanged(BindableObject b, object o, object n)
    {
        var icon = (Icon)b;
        icon._drawable.Thickness = (float)(double)n;
        icon.Invalidate();
    }

    private sealed class IconDrawable : IDrawable
    {
        public PathF? Path { get; set; }
        public Color Color { get; set; } = Colors.Black;
        public float Thickness { get; set; } = 2f;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (Path is null || dirtyRect.Width <= 0 || dirtyRect.Height <= 0)
                return;

            // Le tracé garde sa grille 24 × 24 ; on le met à l'échelle pour atteindre la taille du contrôle.
            canvas.SaveState();
            canvas.Scale(dirtyRect.Width / 24f, dirtyRect.Height / 24f);
            canvas.StrokeColor = Color;
            canvas.StrokeSize = Thickness;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeLineJoin = LineJoin.Round;
            canvas.DrawPath(Path);
            canvas.RestoreState();
        }
    }
}

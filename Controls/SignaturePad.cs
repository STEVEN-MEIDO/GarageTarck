namespace GarageTrack.Controls;

/// <summary>
/// Zone de signature manuscrite tactile (doigt ou souris).
/// </summary>
public class SignaturePad : GraphicsView
{
    private readonly SignatureDrawable _drawable = new();

    public static readonly BindableProperty HasSignatureProperty =
        BindableProperty.Create(nameof(HasSignature), typeof(bool), typeof(SignaturePad), false, BindingMode.OneWayToSource);

    public static readonly BindableProperty IsLockedProperty =
        BindableProperty.Create(nameof(IsLocked), typeof(bool), typeof(SignaturePad), false);

    /// <summary>Vrai dès qu'un trait a été dessiné (lié au ViewModel).</summary>
    public bool HasSignature { get => (bool)GetValue(HasSignatureProperty); set => SetValue(HasSignatureProperty, value); }

    /// <summary>Signature verrouillée après validation du devis.</summary>
    public bool IsLocked { get => (bool)GetValue(IsLockedProperty); set => SetValue(IsLockedProperty, value); }

    public SignaturePad()
    {
        Drawable = _drawable;
        BackgroundColor = Colors.Transparent;

        StartInteraction += (_, e) =>
        {
            if (IsLocked) return;
            _drawable.Strokes.Add(new List<PointF>(e.Touches));
            Invalidate();
        };
        DragInteraction += (_, e) =>
        {
            if (IsLocked || _drawable.Strokes.Count == 0) return;
            _drawable.Strokes[^1].AddRange(e.Touches);
            Invalidate();
        };
        EndInteraction += (_, _) =>
        {
            if (IsLocked) return;
            HasSignature = _drawable.Strokes.Any(s => s.Count > 1);
        };
    }

    public void Clear()
    {
        if (IsLocked) return;
        _drawable.Strokes.Clear();
        HasSignature = false;
        Invalidate();
    }

    private sealed class SignatureDrawable : IDrawable
    {
        public List<List<PointF>> Strokes { get; } = new();

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeColor = Color.FromArgb("#7B1E3A");
            canvas.StrokeSize = 2.5f;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeLineJoin = LineJoin.Round;

            foreach (var stroke in Strokes)
            {
                if (stroke.Count < 2) continue;
                var path = new PathF(stroke[0]);
                for (var i = 1; i < stroke.Count; i++)
                    path.LineTo(stroke[i]);
                canvas.DrawPath(path);
            }
        }
    }
}

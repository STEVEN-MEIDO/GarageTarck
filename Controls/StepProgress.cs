using Microsoft.Maui.Controls.Shapes;

namespace GarageTrack.Controls;

/// <summary>Barre de progression en 3 segments de l'assistant d'intervention.</summary>
public class StepProgress : Grid
{
    public static readonly BindableProperty CurrentStepProperty =
        BindableProperty.Create(nameof(CurrentStep), typeof(int), typeof(StepProgress), 1,
            propertyChanged: (b, _, _) => ((StepProgress)b).Build());

    public static readonly BindableProperty StepCountProperty =
        BindableProperty.Create(nameof(StepCount), typeof(int), typeof(StepProgress), 3,
            propertyChanged: (b, _, _) => ((StepProgress)b).Build());

    public int CurrentStep { get => (int)GetValue(CurrentStepProperty); set => SetValue(CurrentStepProperty, value); }
    public int StepCount { get => (int)GetValue(StepCountProperty); set => SetValue(StepCountProperty, value); }

    public StepProgress()
    {
        ColumnSpacing = 6;
        HeightRequest = 4;
        Build();
    }

    private void Build()
    {
        Children.Clear();
        ColumnDefinitions.Clear();
        for (var i = 0; i < StepCount; i++)
        {
            ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            var segment = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 2 },
                BackgroundColor = i < CurrentStep ? Color.FromArgb("#F97316") : Color.FromArgb("#E2E8F0"),
            };
            this.Add(segment, i);
        }
    }
}

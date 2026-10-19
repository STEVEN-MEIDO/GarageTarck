using CommunityToolkit.Mvvm.ComponentModel;

namespace GarageTrack.Models;

/// <summary>Élément de la checklist des travaux.</summary>
public partial class TaskItem : ObservableObject
{
    [ObservableProperty] private bool isChecked;
    public string Name { get; init; } = string.Empty;
}

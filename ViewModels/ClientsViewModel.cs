using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageTrack.Helpers;
using GarageTrack.Models;
using GarageTrack.Services;

namespace GarageTrack.ViewModels;

public record ClientRow(Client Client, Vehicle? Vehicle)
{
    public string VehicleLine => Vehicle is null ? "Aucun véhicule associé" : $"{Vehicle.Name} · {Vehicle.MileageText}";
    public string Plate => Vehicle?.Plate ?? "—";
    public bool HasVehicle => Vehicle is not null;
}

public partial class ClientsViewModel : BaseViewModel
{
    private readonly MockDataService _data;

    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private bool isEmpty;

    public ObservableCollection<ClientRow> Rows { get; } = new();
    public string CountText => $"{_data.Clients.Count} clients";

    public ClientsViewModel(MockDataService data) => _data = data;

    public void Refresh() => Load();
    partial void OnSearchTextChanged(string value) => Load();

    private void Load()
    {
        var s = SearchText?.Trim() ?? string.Empty;
        var compact = s.Replace("-", "").Replace(" ", "");
        var rows = _data.Clients
            .OrderBy(c => c.FullName)
            .Select(c => new ClientRow(c, _data.VehiclesOf(c.Id).FirstOrDefault()))
            .Where(r => s.Length == 0
                || r.Client.FullName.Contains(s, StringComparison.OrdinalIgnoreCase)
                || (r.Vehicle?.Plate.Replace(" ", "").Contains(compact, StringComparison.OrdinalIgnoreCase) ?? false));

        Rows.Clear();
        foreach (var r in rows) Rows.Add(r);
        IsEmpty = Rows.Count == 0;
        OnPropertyChanged(nameof(CountText));
    }

    [RelayCommand]
    private Task Open(ClientRow row) =>
        Ui.GoTo($"{Routes.ClientVehicle}?clientId={row.Client.Id}&tab=client");

    [RelayCommand]
    private async Task AddClient()
    {
        var page = Shell.Current.CurrentPage;
        var name = await page.DisplayPromptAsync("Nouveau client", "Nom et prénom", "Créer", "Annuler");
        if (string.IsNullOrWhiteSpace(name)) return;
        var client = _data.AddClient(name.Trim(), "", "", "");
        Load();
        await Ui.GoTo($"{Routes.ClientVehicle}?clientId={client.Id}&tab=client");
    }
}

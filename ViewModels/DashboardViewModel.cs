using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageTrack.Helpers;
using GarageTrack.Models;
using GarageTrack.Services;

namespace GarageTrack.ViewModels;

public enum DashboardFilter { Recent, InProgress, Quotes }

public partial class DashboardViewModel : BaseViewModel
{
    private readonly MockDataService _data;

    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private bool showAll;
    [ObservableProperty] private DashboardFilter filter = DashboardFilter.Recent;
    [ObservableProperty] private string repairsCount = "00";
    [ObservableProperty] private string quotesCount = "00";
    [ObservableProperty] private bool isEmpty;

    public ObservableCollection<RepairOrder> Orders { get; } = new();

    public string GarageName => _data.GarageName.ToUpperInvariant();
    public string Greeting => $"Bonjour {_data.UserFirstName}";
    public string TodayText => Money.Capitalize(DateTime.Today.ToString("dddd d MMMM yyyy"));

    public string ListTitle => Filter switch
    {
        DashboardFilter.InProgress => "Réparations en cours",
        DashboardFilter.Quotes => "Devis à valider",
        _ => "OR récents",
    };

    public string SeeAllText => Filter != DashboardFilter.Recent ? "Effacer le filtre" : ShowAll ? "Réduire" : "Tout voir";
    public bool IsRepairsSelected => Filter == DashboardFilter.InProgress;
    public bool IsQuotesSelected => Filter == DashboardFilter.Quotes;

    public DashboardViewModel(MockDataService data) => _data = data;

    public void Refresh()
    {
        RepairsCount = _data.RepairsInProgress.ToString("00");
        QuotesCount = _data.QuotesToValidate.ToString("00");
        OnPropertyChanged(nameof(Greeting));
        LoadOrders();
    }

    partial void OnSearchTextChanged(string value) => LoadOrders();
    partial void OnShowAllChanged(bool value) { OnPropertyChanged(nameof(SeeAllText)); LoadOrders(); }
    partial void OnFilterChanged(DashboardFilter value)
    {
        OnPropertyChanged(nameof(ListTitle));
        OnPropertyChanged(nameof(SeeAllText));
        OnPropertyChanged(nameof(IsRepairsSelected));
        OnPropertyChanged(nameof(IsQuotesSelected));
        LoadOrders();
    }

    private void LoadOrders()
    {
        IEnumerable<RepairOrder> query = _data.RecentOrders;

        query = Filter switch
        {
            DashboardFilter.InProgress => query.Where(o => o.Status is OrderStatus.InRepair or OrderStatus.MissingPart or OrderStatus.Received),
            DashboardFilter.Quotes => query.Where(o => o.Status is OrderStatus.QuotePending or OrderStatus.QuoteToSign or OrderStatus.QuoteInProgress),
            _ => query,
        };

        var search = SearchText?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            var compact = search.Replace("-", "").Replace(" ", "");
            query = query.Where(o =>
                o.Vehicle.Plate.Replace(" ", "").Contains(compact, StringComparison.OrdinalIgnoreCase) ||
                o.Number.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                o.Client.FullName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }
        else if (!ShowAll && Filter == DashboardFilter.Recent)
        {
            query = query.Take(4);
        }

        Orders.Clear();
        foreach (var o in query) Orders.Add(o);
        IsEmpty = Orders.Count == 0;
    }

    [RelayCommand]
    private void SeeAll()
    {
        if (Filter != DashboardFilter.Recent) Filter = DashboardFilter.Recent;
        else ShowAll = !ShowAll;
    }

    [RelayCommand]
    private void FilterRepairs() =>
        Filter = Filter == DashboardFilter.InProgress ? DashboardFilter.Recent : DashboardFilter.InProgress;

    [RelayCommand]
    private void FilterQuotes() =>
        Filter = Filter == DashboardFilter.Quotes ? DashboardFilter.Recent : DashboardFilter.Quotes;

    [RelayCommand]
    private Task OpenOrder(RepairOrder order) =>
        Ui.GoTo($"{Routes.ClientVehicle}?vehicleId={order.Vehicle.Id}&tab=vehicle");

    [RelayCommand]
    private Task NewOrder() => Ui.GoTo(Routes.NewOrder);

    [RelayCommand]
    private Task Notifications() =>
        Ui.Alert("Notifications", "OR-2026-0139 : la pièce commandée arrive demain matin.");
}

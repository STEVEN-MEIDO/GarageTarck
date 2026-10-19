using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageTrack.Helpers;
using GarageTrack.Models;
using GarageTrack.Services;

namespace GarageTrack.ViewModels;

public partial class QuotesViewModel : BaseViewModel
{
    private readonly MockDataService _data;

    [ObservableProperty] private bool isEmpty;
    public ObservableCollection<RepairOrder> Quotes { get; } = new();

    public QuotesViewModel(MockDataService data) => _data = data;

    public void Refresh()
    {
        Quotes.Clear();
        foreach (var o in _data.RecentOrders.Where(o => !string.IsNullOrEmpty(o.QuoteNumber) || o.Status == OrderStatus.QuoteInProgress))
            Quotes.Add(o);
        IsEmpty = Quotes.Count == 0;
    }

    [RelayCommand]
    private Task Open(RepairOrder order) => order.Status switch
    {
        OrderStatus.QuoteInProgress or OrderStatus.Received => Ui.GoTo($"{Routes.PartsLabor}?order={order.Number}"),
        OrderStatus.QuotePending or OrderStatus.QuoteToSign => Ui.GoTo($"{Routes.QuoteValidation}?order={order.Number}"),
        _ => Ui.GoTo($"{Routes.History}?vehicleId={order.Vehicle.Id}"),
    };
}

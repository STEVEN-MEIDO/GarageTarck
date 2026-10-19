using GarageTrack.Views;

namespace GarageTrack;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Pages empilées (navigation avec flèche retour)
        Routing.RegisterRoute(Routes.Register, typeof(RegisterPage));
        Routing.RegisterRoute(Routes.ClientVehicle, typeof(ClientVehiclePage));
        Routing.RegisterRoute(Routes.NewOrder, typeof(NewRepairOrderPage));
        Routing.RegisterRoute(Routes.PartsLabor, typeof(PartsLaborPage));
        Routing.RegisterRoute(Routes.QuoteValidation, typeof(QuoteValidationPage));
        Routing.RegisterRoute(Routes.History, typeof(VehicleHistoryPage));
    }
}

public static class Routes
{
    public const string Login = "//login";
    public const string Main = "//main/atelier";
    public const string Register = "register";
    public const string ClientVehicle = "clientvehicle";
    public const string NewOrder = "neworder";
    public const string PartsLabor = "partslabor";
    public const string QuoteValidation = "quotevalidation";
    public const string History = "history";
}

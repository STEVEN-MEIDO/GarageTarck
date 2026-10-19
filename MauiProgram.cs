using GarageTrack.Services;
using GarageTrack.ViewModels;
using GarageTrack.Views;
using Microsoft.Extensions.Logging;

namespace GarageTrack;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Inter-Regular.ttf", "InterRegular");
                fonts.AddFont("Inter-Medium.ttf", "InterMedium");
                fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
                fonts.AddFont("Inter-Bold.ttf", "InterBold");
            });

        RemoveNativeUnderlines();

        // Services
        builder.Services.AddSingleton<MockDataService>();

        // Pages + ViewModels
        builder.Services.AddTransient<LoginPage>().AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterPage>().AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<DashboardPage>().AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<ClientsPage>().AddTransient<ClientsViewModel>();
        builder.Services.AddTransient<QuotesPage>().AddTransient<QuotesViewModel>();
        builder.Services.AddTransient<AccountPage>().AddTransient<AccountViewModel>();
        builder.Services.AddTransient<ClientVehiclePage>().AddTransient<ClientVehicleViewModel>();
        builder.Services.AddTransient<NewRepairOrderPage>().AddTransient<NewRepairOrderViewModel>();
        builder.Services.AddTransient<PartsLaborPage>().AddTransient<PartsLaborViewModel>();
        builder.Services.AddTransient<QuoteValidationPage>().AddTransient<QuoteValidationViewModel>();
        builder.Services.AddTransient<VehicleHistoryPage>().AddTransient<VehicleHistoryViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }

    /// <summary>
    /// Les champs du design sont dessinés dans des Border arrondis :
    /// on retire le soulignement / la bordure natifs des contrôles de saisie.
    /// </summary>
    private static void RemoveNativeUnderlines()
    {
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {
#if ANDROID
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif WINDOWS
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Background = null;
#elif IOS || MACCATALYST
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
        });

        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {
#if ANDROID
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif WINDOWS
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Background = null;
#endif
        });

        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {
#if ANDROID
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif WINDOWS
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Background = null;
#elif IOS || MACCATALYST
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
        });
    }
}

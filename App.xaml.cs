using System.Globalization;

namespace GarageTrack;

public partial class App : Application
{
    public App()
    {
        // Dates et montants affichés à la camerounaise (7 octobre 2026, 287 000 FCFA)
        var fr = new CultureInfo("fr-CM");
        CultureInfo.DefaultThreadCurrentCulture = fr;
        CultureInfo.DefaultThreadCurrentUICulture = fr;
        CultureInfo.CurrentCulture = fr;
        CultureInfo.CurrentUICulture = fr;

        InitializeComponent();
        UserAppTheme = AppTheme.Light; // le design est prévu en thème clair
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell()) { Title = "GarageTrack" };
#if WINDOWS
        // Format téléphone pour visualiser le design sur PC
        window.Width = 430;
        window.Height = 900;
#endif
        return window;
    }
}

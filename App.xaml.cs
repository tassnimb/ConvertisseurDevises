using ConvertisseurDevises.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ConvertisseurDevises;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        // 1) Charge d'abord les ressources de App.xaml (couleurs, styles)
        InitializeComponent();

        // 2) Mode sombre uniquement
        UserAppTheme = AppTheme.Dark;

        _services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // La page est créée ICI (après le chargement des ressources).
        // NavigationPage permet d'ouvrir la liste "Currencies" puis de revenir en arrière.
        var mainPage = _services.GetRequiredService<MainPage>();
        return new Window(new NavigationPage(mainPage));
    }
}
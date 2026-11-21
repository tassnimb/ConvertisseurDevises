using ConvertisseurDevises.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ConvertisseurDevises;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        InitializeComponent();

        UserAppTheme = AppTheme.Dark;

        _services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var loginPage = _services.GetRequiredService<LoginPage>();

        return new Window(
            new NavigationPage(loginPage));
    }
}
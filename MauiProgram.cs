using ConvertisseurDevises.Services;
using ConvertisseurDevises.ViewModels;
using ConvertisseurDevises.Views;
using Microsoft.Extensions.Logging;

namespace ConvertisseurDevises;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ===== Injection de dépendances =====
        builder.Services.AddSingleton<CurrencyService>();   // un seul service (cache des taux partagé)
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
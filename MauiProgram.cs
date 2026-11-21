
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

        // Services de l'application
        builder.Services.AddSingleton<CurrencyService>();

        // Base de données SQLite
        builder.Services.AddSingleton(
            new DatabaseService(
                Path.Combine(
                    FileSystem.AppDataDirectory,
                    "users.db3")));

        // ViewModels et pages
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<HistoryPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

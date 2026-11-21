
using ConvertisseurDevises.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ConvertisseurDevises.Views;

/// <summary>
/// Code-behind : navigation et fenêtres d'information.
/// Toute la logique métier reste dans MainViewModel.
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private readonly IServiceProvider _services;

    public MainPage(
        MainViewModel viewModel,
        IServiceProvider services)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _services = services;

        BindingContext = viewModel;

        viewModel.PickCurrencyRequested += OnPickCurrencyRequested;
        viewModel.InfoRequested += OnInfoRequested;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.InitializeAsync();
    }

    /// <summary>
    /// Ouvre la liste des devises.
    /// </summary>

    private async void OnPickCurrencyRequested(int row)
    {
        await _viewModel.LoadFavoritesAsync();

        var listViewModel =
            new CurrenciesViewModel(_viewModel.Currencies);

        listViewModel.CurrencySelected += async currency =>
        {
            _viewModel.SetCurrency(row, currency);
            await Navigation.PopAsync();
        };

        listViewModel.FavoriteToggleRequested += async currency =>
        {
            await _viewModel.ToggleFavoriteAsync(currency);
            listViewModel.RefreshCurrencies();
        };

        await Navigation.PushAsync(
            new CurrenciesPage(listViewModel));
    }


    /// <summary>
    /// Fenêtre d'information.
    /// </summary>
    private async void OnInfoRequested(string message)
    {
        await DisplayAlertAsync(
            "Informations",
            message,
            "OK");
    }

    /// <summary>
    /// Déconnexion de l'utilisateur.
    /// </summary>
    private async void Logout_Clicked(object sender, EventArgs e)
    {
        bool result = await DisplayAlertAsync(
            "Déconnexion",
            "Voulez-vous vraiment vous déconnecter ?",
            "Oui",
            "Non");

        if (!result)
            return;

        // Effacer l'utilisateur actuellement connecté
        _viewModel.ClearCurrentUser();

        // Retourner à la page de connexion
        Application.Current!.Windows[0].Page =
            new NavigationPage(
                _services.GetRequiredService<LoginPage>());
    }
    private async void History_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(
            _services.GetRequiredService<HistoryPage>());
    }
}

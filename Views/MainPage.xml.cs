using ConvertisseurDevises.ViewModels;

namespace ConvertisseurDevises.Views;

/// <summary>
/// Code-behind : seulement la navigation et les fenêtres d'information.
/// Toute la logique est dans MainViewModel (MVVM).
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel; // Data Binding XAML <-> ViewModel

        viewModel.PickCurrencyRequested += OnPickCurrencyRequested;
        viewModel.InfoRequested += OnInfoRequested;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Charge les taux de change (appel HTTP) à l'affichage de la page.
        await _viewModel.InitializeAsync();
    }

    /// <summary>Ouvre la page "Currencies" ; au choix d'une devise, on revient et on l'applique.</summary>
    private async void OnPickCurrencyRequested(int row)
    {
        var listViewModel = new CurrenciesViewModel(_viewModel.Currencies);

        listViewModel.CurrencySelected += async currency =>
        {
            _viewModel.SetCurrency(row, currency);
            await Navigation.PopAsync();
        };

        await Navigation.PushAsync(new CurrenciesPage(listViewModel));
    }

    private async void OnInfoRequested(string message)
    {
        await DisplayAlertAsync("Informations", message, "OK");
    }
}
using ConvertisseurDevises.ViewModels;

namespace ConvertisseurDevises.Views;

public partial class CurrenciesPage : ContentPage
{
    public CurrenciesPage(CurrenciesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    /// <summary>Flèche retour en haut à gauche.</summary>
    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
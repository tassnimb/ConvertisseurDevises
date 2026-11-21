
using ConvertisseurDevises.Models;
using ConvertisseurDevises.Services;
using ConvertisseurDevises.ViewModels;

namespace ConvertisseurDevises.Views;

public partial class HistoryPage : ContentPage
{
    private readonly HistoryViewModel _viewModel;

    public HistoryPage(
        DatabaseService database,
        MainViewModel mainViewModel)
    {
        InitializeComponent();

        int userId = mainViewModel.CurrentUserId ?? 0;

        _viewModel = new HistoryViewModel(
            database,
            userId);

        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadAsync();
    }

    private async void Delete_Clicked(
        object sender,
        EventArgs e)
    {
        if (sender is not Button button ||
            button.CommandParameter is not ConversionHistory conversion)
        {
            return;
        }

        bool result = await DisplayAlertAsync(
            "Supprimer",
            "Voulez-vous supprimer cette conversion ?",
            "Oui",
            "Non");

        if (!result)
            return;

        await _viewModel.DeleteAsync(conversion);
    }

    private async void ClearHistory_Clicked(
        object sender,
        EventArgs e)
    {
        if (_viewModel.Items.Count == 0)
        {
            await DisplayAlertAsync(
                "Historique",
                "L'historique est déjà vide.",
                "OK");

            return;
        }

        bool result = await DisplayAlertAsync(
            "Effacer l'historique",
            "Voulez-vous supprimer toutes les conversions ?",
            "Oui",
            "Non");

        if (!result)
            return;

        await _viewModel.ClearAsync();
    }
    private async void Back_Clicked(object sender, EventArgs e)
    {
        if (Navigation.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlertAsync(
                "Navigation",
                "Impossible de revenir à la page précédente.",
                "OK");
        }
    }
}

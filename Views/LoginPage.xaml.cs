
using ConvertisseurDevises.Services;
using ConvertisseurDevises.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ConvertisseurDevises.Views;

public partial class LoginPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly IServiceProvider _services;

    public LoginPage(
        DatabaseService database,
        IServiceProvider services)
    {
        InitializeComponent();

        _database = database;
        _services = services;
    }

    private async void Login_Clicked(object sender, EventArgs e)
    {
        string email = emailEntry.Text?.Trim() ?? "";
        string password = passwordEntry.Text ?? "";

        // Vérifier les champs
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlertAsync(
                "Erreur",
                "Veuillez remplir tous les champs.",
                "OK");

            return;
        }

        // Vérifier le format de l'adresse e-mail
        if (!email.Contains("@") || !email.Contains("."))
        {
            await DisplayAlertAsync(
                "Erreur",
                "Veuillez saisir une adresse e-mail valide.",
                "OK");

            return;
        }

        try
        {
            // Vérifier les identifiants dans SQLite
            var user = await _database.GetUserAsync(
                email,
                password);

            if (user == null)
            {
                await DisplayAlertAsync(
                    "Connexion refusée",
                    "Email ou mot de passe incorrect.",
                    "OK");

                return;
            }

            // Récupérer le ViewModel partagé
            var mainViewModel =
                _services.GetRequiredService<MainViewModel>();

            // Enregistrer l'utilisateur connecté
            mainViewModel.SetCurrentUser(user.Id);

            // Ouvrir la page principale
            Application.Current!.Windows[0].Page =
                new NavigationPage(
                    _services.GetRequiredService<MainPage>());
        }
        catch (Exception)
        {
            await DisplayAlertAsync(
                "Erreur",
                "Une erreur est survenue lors de la connexion. Veuillez réessayer.",
                "OK");
        }
    }

    private async void Register_LabelTapped(
        object sender,
        TappedEventArgs e)
    {
        await Navigation.PushAsync(
            _services.GetRequiredService<RegisterPage>());
    }
}

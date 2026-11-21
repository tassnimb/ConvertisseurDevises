
using ConvertisseurDevises.Models;
using ConvertisseurDevises.Services;
using System.Net.Mail;

namespace ConvertisseurDevises.Views;

public partial class RegisterPage : ContentPage
{
    private readonly DatabaseService _database;

    public RegisterPage(DatabaseService database)
    {
        InitializeComponent();

        _database = database;
    }

    private async void Register_Clicked(object sender, EventArgs e)
    {
        string email = emailEntry.Text?.Trim() ?? "";
        string password = passwordEntry.Text ?? "";
        string confirmPassword = confirmPasswordEntry.Text ?? "";

        // Vérifier les champs
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirmPassword))
        {
            await DisplayAlertAsync(
                "Erreur",
                "Veuillez remplir tous les champs.",
                "OK");

            return;
        }

        // Vérifier le format de l'email
        try
        {
            var mailAddress = new MailAddress(email);

            if (mailAddress.Address != email)
            {
                await DisplayAlertAsync(
                    "Erreur",
                    "Veuillez saisir une adresse e-mail valide.",
                    "OK");

                return;
            }
        }
        catch
        {
            await DisplayAlertAsync(
                "Erreur",
                "Veuillez saisir une adresse e-mail valide.",
                "OK");

            return;
        }

        // Vérifier la longueur du mot de passe
        if (password.Length < 8)
        {
            await DisplayAlertAsync(
                "Mot de passe",
                "Le mot de passe doit contenir au moins 8 caractères.",
                "OK");

            return;
        }

        // Vérifier la confirmation
        if (password != confirmPassword)
        {
            await DisplayAlertAsync(
                "Erreur",
                "Les mots de passe ne correspondent pas.",
                "OK");

            return;
        }

        try
        {
            // Vérifier si l'email existe déjà
            var existingUser =
                await _database.GetUserByEmailAsync(email);

            if (existingUser != null)
            {
                await DisplayAlertAsync(
                    "Compte existant",
                    "Cette adresse e-mail est déjà utilisée.",
                    "OK");

                return;
            }

            // Créer le compte
            var newUser = new User
            {
                Email = email,
                Password = password
            };

            await _database.AddUserAsync(newUser);

            await DisplayAlertAsync(
                "Succès",
                "Votre compte a été créé avec succès. Vous pouvez maintenant vous connecter.",
                "OK");

            // Retourner à Login
            await Navigation.PopAsync();
        }
        catch (Exception)
        {
            await DisplayAlertAsync(
                "Erreur",
                "Impossible de créer le compte. Vérifiez votre connexion à l'application et réessayez.",
                "OK");
        }
    }

    // Retour à la page Login
    private async void Login_Tapped(
        object sender,
        TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}

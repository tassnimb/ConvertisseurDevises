
using ConvertisseurDevises.Models;
using SQLite;

namespace ConvertisseurDevises.Services;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _database;

    public DatabaseService(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);

        _database.CreateTableAsync<User>().Wait();
        _database.CreateTableAsync<ConversionHistory>().Wait();
        _database.CreateTableAsync<FavoriteCurrency>().Wait();
    }

    // INSCRIPTION
    public Task<int> AddUserAsync(User user)
    {
        return _database.InsertAsync(user);
    }

    // CONNEXION
    public Task<User?> GetUserAsync(string email, string password)
    {
        return _database.Table<User>()
            .FirstOrDefaultAsync(
                u => u.Email == email &&
                     u.Password == password);
    }

    // RECHERCHER UN UTILISATEUR
    public Task<User?> GetUserByEmailAsync(string email)
    {
        return _database.Table<User>()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    // AJOUTER UNE CONVERSION À L'HISTORIQUE
    public Task<int> AddConversionAsync(ConversionHistory conversion)
    {
        return _database.InsertAsync(conversion);
    }

    // RÉCUPÉRER L'HISTORIQUE D'UN UTILISATEUR
    public Task<List<ConversionHistory>> GetHistoryAsync(int userId)
    {
        return _database.Table<ConversionHistory>()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.ConversionDate)
            .ToListAsync();
    }

    // SUPPRIMER UNE CONVERSION
    public Task<int> DeleteConversionAsync(int conversionId, int userId)
    {
        return _database.ExecuteAsync(
            "DELETE FROM ConversionHistory WHERE Id = ? AND UserId = ?",
            conversionId, userId);
    }

    // SUPPRIMER TOUT L'HISTORIQUE D'UN UTILISATEUR
    public Task<int> ClearHistoryAsync(int userId)
    {
        return _database.ExecuteAsync(
            "DELETE FROM ConversionHistory WHERE UserId = ?",
            userId);
    }

    public async Task<int> AddFavoriteAsync(FavoriteCurrency favorite)
    {
        var existing = await _database.Table<FavoriteCurrency>()
            .FirstOrDefaultAsync(f =>
                f.UserId == favorite.UserId &&
                f.CurrencyCode == favorite.CurrencyCode);

        if (existing != null)
            return 0;

        return await _database.InsertAsync(favorite);
    }

    public Task<List<FavoriteCurrency>> GetFavoritesAsync(int userId)
    {
        return _database.Table<FavoriteCurrency>()
            .Where(f => f.UserId == userId)
            .ToListAsync();
    }

    public Task<int> DeleteFavoriteAsync(int favoriteId, int userId)
    {
        return _database.ExecuteAsync(
            "DELETE FROM FavoriteCurrency WHERE Id = ? AND UserId = ?",
            favoriteId, userId);
    }

}


using SQLite;

namespace ConvertisseurDevises.Models;

public class FavoriteCurrency
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;
}

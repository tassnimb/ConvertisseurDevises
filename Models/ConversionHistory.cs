
using SQLite;

namespace ConvertisseurDevises.Models;

public class ConversionHistory
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; }

    public string FromCurrency { get; set; } = string.Empty;

    public string ToCurrency { get; set; } = string.Empty;

    public double Amount { get; set; }

    public double ConvertedAmount { get; set; }

    public DateTime ConversionDate { get; set; }
}

namespace ConvertisseurDevises.Models;

/// <summary>
/// Un taux de change : code de la devise + taux par rapport à la devise de base (USD).
/// Construit à partir du dictionnaire ApiResponse.Rates.
/// </summary>
public class CurrencyRate
{
    public string Code { get; set; } = string.Empty;
    public double Rate { get; set; }
}
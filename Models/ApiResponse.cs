using System.Text.Json.Serialization;

namespace ConvertisseurDevises.Models;

/// <summary>
/// Modèle C# correspondant à la réponse JSON de https://open.er-api.com/v6/latest/USD
/// Exemple simplifié de JSON reçu :
/// {
///   "result": "success",
///   "time_last_update_utc": "Sat, 21 Nov 2026 00:02:31 +0000",
///   "base_code": "USD",
///   "rates": { "USD": 1, "EUR": 0.86, "TND": 2.95, ... }
/// }
/// </summary>
public class ApiResponse
{
    // [JsonPropertyName] fait le lien entre le nom JSON (snake_case) et la propriété C#.
    [JsonPropertyName("result")]
    public string Result { get; set; } = string.Empty;

    [JsonPropertyName("base_code")]
    public string BaseCode { get; set; } = string.Empty;

    [JsonPropertyName("time_last_update_utc")]
    public string TimeLastUpdateUtc { get; set; } = string.Empty;

    // Dictionnaire : clé = code devise (ex. "EUR"), valeur = taux par rapport à la devise de base.
    [JsonPropertyName("rates")]
    public Dictionary<string, double> Rates { get; set; } = new();
}

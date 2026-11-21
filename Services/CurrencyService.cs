using System.Text.Json;
using ConvertisseurDevises.Models;

namespace ConvertisseurDevises.Services;

/// <summary>
/// Service chargé des appels HTTP vers l'API de taux de change
/// et de la désérialisation JSON (System.Text.Json).
/// </summary>
public class CurrencyService
{
    // API gratuite demandée par le professeur (base USD).
    private const string ApiUrl = "https://open.er-api.com/v6/latest/USD";

    // Les taux ne changent qu'une fois par jour : cache de 30 minutes.
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private ApiResponse? _cached;
    private DateTime _cachedAtUtc;

    public CurrencyService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    /// <summary>Appel HTTP GET + désérialisation JSON vers ApiResponse.</summary>
    public async Task<ApiResponse> GetLatestRatesAsync(bool forceRefresh = false, CancellationToken ct = default)
    {
        // 1) Cache encore valide ? on évite un appel réseau inutile.
        if (!forceRefresh && _cached != null && DateTime.UtcNow - _cachedAtUtc < CacheDuration)
            return _cached;

        // 2) Appel HTTP GET
        using HttpResponseMessage response = await _httpClient.GetAsync(ApiUrl, ct);
        response.EnsureSuccessStatusCode(); // lève HttpRequestException si code != 2xx

        // 3) JSON brut -> modèles C#
        string json = await response.Content.ReadAsStringAsync(ct);
        ApiResponse? data = JsonSerializer.Deserialize<ApiResponse>(json, JsonOptions);

        // 4) Vérification de la réponse
        if (data == null || data.Result != "success" || data.Rates.Count == 0)
            throw new InvalidOperationException("Réponse invalide reçue de l'API.");

        _cached = data;
        _cachedAtUtc = DateTime.UtcNow;
        return data;
    }

    /// <summary>Transforme le dictionnaire de la dernière réponse en liste de CurrencyRate.</summary>
    public List<CurrencyRate> GetRatesList()
    {
        if (_cached == null) return new List<CurrencyRate>();

        return _cached.Rates
            .Select(kv => new CurrencyRate { Code = kv.Key, Rate = kv.Value })
            .ToList();
    }

    /// <summary>Texte de date de mise à jour renvoyé par l'API.</summary>
    public string LastApiUpdate => _cached?.TimeLastUpdateUtc ?? "-";
}
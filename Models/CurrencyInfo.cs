namespace ConvertisseurDevises.Models;

/// <summary>
/// Une devise affichée dans l'application : code ISO, nom, drapeau.
/// Le drapeau est une image ronde chargée depuis flagcdn.com
/// (fonctionne aussi sous Windows, contrairement aux drapeaux emoji).
/// </summary>
public class CurrencyInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>Code pays ISO en minuscules, utilisé pour l'URL du drapeau (ex. "tn").</summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>Les devises "favorites" ont leur code affiché en jaune dans la liste.</summary>
    public bool IsFavorite { get; set; }

    public string FlagUrl => $"https://flagcdn.com/w160/{CountryCode}.png";

    public override string ToString() => $"{Name} ({Code})";

    /// <summary>Les devises de la liste, dans le même ordre que la capture d'écran.</summary>
    public static List<CurrencyInfo> GetSupported() => new()
    {
        new() { Code = "TND", Name = "Tunisian Dinar",         CountryCode = "tn", IsFavorite = true },
        new() { Code = "EUR", Name = "Euro",                   CountryCode = "eu", IsFavorite = true },
        new() { Code = "USD", Name = "US Dollar",              CountryCode = "us", IsFavorite = true },
        new() { Code = "DZD", Name = "Algerian Dinar",         CountryCode = "dz", IsFavorite = true },
        new() { Code = "TRY", Name = "Turkish Lira",           CountryCode = "tr", IsFavorite = true },
        new() { Code = "EGP", Name = "Egyptian Pound",         CountryCode = "eg" },
        new() { Code = "LYD", Name = "Libyan Dinar",           CountryCode = "ly" },
        new() { Code = "MAD", Name = "Moroccan Dirham",        CountryCode = "ma" },
        new() { Code = "SAR", Name = "Saudi Riyal",            CountryCode = "sa" },
        new() { Code = "XOF", Name = "West African CFA Franc", CountryCode = "ci" },
        new() { Code = "AED", Name = "UAE Dirham",             CountryCode = "ae" },
        new() { Code = "GBP", Name = "British Pound",          CountryCode = "gb" },
    };
}
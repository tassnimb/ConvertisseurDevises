
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ConvertisseurDevises.Models;

public class CurrencyInfo : INotifyPropertyChanged
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;

    private bool _isFavorite;

    public bool IsFavorite
    {
        get => _isFavorite;
        set
        {
            if (_isFavorite != value)
            {
                _isFavorite = value;
                OnPropertyChanged();
            }
        }
    }

    public string FlagUrl =>
        $"https://flagcdn.com/w160/{CountryCode}.png";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString() => $"{Name} ({Code})";

    public static List<CurrencyInfo> GetSupported() => new()
    {
        new() { Code = "TND", Name = "Tunisian Dinar", CountryCode = "tn" },
        new() { Code = "EUR", Name = "Euro", CountryCode = "eu" },
        new() { Code = "USD", Name = "US Dollar", CountryCode = "us" },
        new() { Code = "DZD", Name = "Algerian Dinar", CountryCode = "dz" },
        new() { Code = "TRY", Name = "Turkish Lira", CountryCode = "tr" },
        new() { Code = "EGP", Name = "Egyptian Pound", CountryCode = "eg" },
        new() { Code = "LYD", Name = "Libyan Dinar", CountryCode = "ly" },
        new() { Code = "MAD", Name = "Moroccan Dirham", CountryCode = "ma" },
        new() { Code = "SAR", Name = "Saudi Riyal", CountryCode = "sa" },
        new() { Code = "XOF", Name = "West African CFA Franc", CountryCode = "ci" },
        new() { Code = "AED", Name = "UAE Dirham", CountryCode = "ae" },
        new() { Code = "GBP", Name = "British Pound", CountryCode = "gb" }
    };
}

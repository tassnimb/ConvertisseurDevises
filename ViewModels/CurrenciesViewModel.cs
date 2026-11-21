
using System.Collections.ObjectModel;
using System.Windows.Input;
using ConvertisseurDevises.Models;

namespace ConvertisseurDevises.ViewModels;

public class CurrenciesViewModel : BaseViewModel
{
    private readonly List<CurrencyInfo> _all;

    public CurrenciesViewModel(IEnumerable<CurrencyInfo> currencies)
    {
        _all = currencies.ToList();

        FilteredCurrencies = new ObservableCollection<CurrencyInfo>();
        FavoriteCurrencies = new ObservableCollection<CurrencyInfo>();
        OtherCurrencies = new ObservableCollection<CurrencyInfo>();
        DisplayItems = new ObservableCollection<object>();

        SelectCommand = new Command<CurrencyInfo>(currency =>
        {
            if (currency != null)
                CurrencySelected?.Invoke(currency);
        });

        FavoriteCommand = new Command<CurrencyInfo>(currency =>
        {
            if (currency != null)
                FavoriteToggleRequested?.Invoke(currency);
        });

        ApplyFilter();
    }

    public ObservableCollection<CurrencyInfo> FilteredCurrencies { get; }
    public ObservableCollection<CurrencyInfo> FavoriteCurrencies { get; }
    public ObservableCollection<CurrencyInfo> OtherCurrencies { get; }
    public ObservableCollection<object> DisplayItems { get; }

    public bool HasFavorites => FavoriteCurrencies.Count > 0;
    public bool HasOtherCurrencies => OtherCurrencies.Count > 0;

    private string _searchText = string.Empty;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ApplyFilter();
        }
    }

    public ICommand SelectCommand { get; }
    public ICommand FavoriteCommand { get; }

    public event Action<CurrencyInfo>? CurrencySelected;
    public event Action<CurrencyInfo>? FavoriteToggleRequested;

    private void ApplyFilter()
    {
        string query = (SearchText ?? string.Empty).Trim();

        IEnumerable<CurrencyInfo> result = _all;

        if (!string.IsNullOrEmpty(query))
        {
            result = result.Where(c =>
                c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                c.Code.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        var favorites = result
            .Where(c => c.IsFavorite)
            .OrderBy(c => c.Name)
            .ToList();

        var others = result
            .Where(c => !c.IsFavorite)
            .OrderBy(c => c.Name)
            .ToList();

        FavoriteCurrencies.Clear();
        OtherCurrencies.Clear();
        FilteredCurrencies.Clear();
        DisplayItems.Clear();

        if (favorites.Count > 0)
        {
            DisplayItems.Add("⭐ FAVORIS");

            foreach (var currency in favorites)
            {
                FavoriteCurrencies.Add(currency);
                FilteredCurrencies.Add(currency);
                DisplayItems.Add(currency);
            }
        }

        if (others.Count > 0)
        {
            DisplayItems.Add("TOUTES LES DEVISES");

            foreach (var currency in others)
            {
                OtherCurrencies.Add(currency);
                FilteredCurrencies.Add(currency);
                DisplayItems.Add(currency);
            }
        }

        OnPropertyChanged(nameof(HasFavorites));
        OnPropertyChanged(nameof(HasOtherCurrencies));
    }

    public void RefreshCurrencies()
    {
        ApplyFilter();
    }
}

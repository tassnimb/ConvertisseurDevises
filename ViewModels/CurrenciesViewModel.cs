using System.Collections.ObjectModel;
using System.Windows.Input;
using ConvertisseurDevises.Models;

namespace ConvertisseurDevises.ViewModels;

/// <summary>
/// ViewModel de la page "Currencies" : liste filtrable par la barre de recherche.
/// </summary>
public class CurrenciesViewModel : BaseViewModel
{
    private readonly List<CurrencyInfo> _all;

    public CurrenciesViewModel(IEnumerable<CurrencyInfo> currencies)
    {
        _all = currencies.ToList();
        FilteredCurrencies = new ObservableCollection<CurrencyInfo>(_all);

        SelectCommand = new Command<CurrencyInfo>(currency =>
        {
            if (currency != null)
                CurrencySelected?.Invoke(currency);
        });
    }

    /// <summary>Liste affichée (mise à jour à chaque lettre tapée dans "Search").</summary>
    public ObservableCollection<CurrencyInfo> FilteredCurrencies { get; }

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

    /// <summary>Déclenché quand l'utilisateur touche une devise.</summary>
    public event Action<CurrencyInfo>? CurrencySelected;

    private void ApplyFilter()
    {
        string query = (SearchText ?? string.Empty).Trim();

        IEnumerable<CurrencyInfo> result = string.IsNullOrEmpty(query)
            ? _all
            : _all.Where(c =>
                c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                c.Code.Contains(query, StringComparison.OrdinalIgnoreCase));

        FilteredCurrencies.Clear();
        foreach (CurrencyInfo c in result)
            FilteredCurrencies.Add(c);
    }
}
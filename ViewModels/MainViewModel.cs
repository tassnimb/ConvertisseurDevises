
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using ConvertisseurDevises.Models;
using ConvertisseurDevises.Services;
using Microsoft.Maui.Graphics;

namespace ConvertisseurDevises.ViewModels;

/// <summary>
/// ViewModel de la page principale.
/// Gère la calculatrice, les conversions et l'historique SQLite.
/// </summary>
public class MainViewModel : BaseViewModel
{
    private const int MaxLength = 20;

    private static readonly Color SuccessColor =
        Color.FromArgb("#3DDC97");

    private static readonly Color ErrorColor =
        Color.FromArgb("#FF5252");

    private readonly CurrencyService _currencyService;
    private readonly DatabaseService _databaseService;

    // Utilisateur actuellement connecté
    private int? _currentUserId;

    // Taux de change : code devise -> taux
    private Dictionary<string, double> _rates = new();

    private DateTime? _lastUpdateUtc;
    private bool _isLoading;
    private bool _loadFailed;

    // Calculatrice
    private string _expression = "0";
    private bool _hasInput;
    private bool _justEvaluated;
    private int _activeIndex;
    private int _errorVersion;

    public MainViewModel(
        CurrencyService currencyService,
        DatabaseService databaseService)
    {
        _currencyService = currencyService;
        _databaseService = databaseService;

        Currencies = new ObservableCollection<CurrencyInfo>(
            CurrencyInfo.GetSupported());

        _topCurrency = Currencies.First(c => c.Code == "TND");
        _bottomCurrency = Currencies.First(c => c.Code == "EUR");

        KeyCommand = new Command<string>(
            async key => await OnKeyAsync(key));

        SelectRowCommand = new Command<string>(
            p => SetActiveRow(int.Parse(p ?? "0")));

        PickCurrencyCommand = new Command<string>(
            p => PickCurrencyRequested?.Invoke(
                int.Parse(p ?? "0")));

        RefreshCommand = new Command(
            async () => await LoadRatesAsync(
                forceRefresh: true));

        InfoCommand = new Command(
            () => InfoRequested?.Invoke(BuildInfoText()));
    }

    // ====================== Utilisateur connecté ======================

    /// <summary>
    /// Identifiant de l'utilisateur actuellement connecté.
    /// Utilisé par la page Historique.
    /// </summary>
    public int? CurrentUserId => _currentUserId;

    /// <summary>
    /// Définit l'utilisateur après une connexion réussie.
    /// </summary>
    public void SetCurrentUser(int userId)
    {
        _currentUserId = userId;
    }

    /// <summary>
    /// Efface l'utilisateur lors de la déconnexion.
    /// </summary>
    public void ClearCurrentUser()
    {
        _currentUserId = null;
    }

    // ====================== Collections / événements ======================

    public ObservableCollection<CurrencyInfo> Currencies { get; }

    public event Action<int>? PickCurrencyRequested;

    public event Action<string>? InfoRequested;

    // ====================== Commandes ======================

    public ICommand KeyCommand { get; }
    public ICommand SelectRowCommand { get; }
    public ICommand PickCurrencyCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand InfoCommand { get; }

    // ====================== Devises affichées ======================

    private CurrencyInfo _topCurrency;

    public CurrencyInfo TopCurrency
    {
        get => _topCurrency;
        set
        {
            if (SetProperty(ref _topCurrency, value))
                RefreshDisplay();
        }
    }

    private CurrencyInfo _bottomCurrency;

    public CurrencyInfo BottomCurrency
    {
        get => _bottomCurrency;
        set
        {
            if (SetProperty(ref _bottomCurrency, value))
                RefreshDisplay();
        }
    }

    public string TopText =>
        _activeIndex == 0 ? PrettyExpression : ConvertedText;

    public string BottomText =>
        _activeIndex == 1 ? PrettyExpression : ConvertedText;

    public double TopFontSize => FontSizeFor(TopText);

    public double BottomFontSize => FontSizeFor(BottomText);

    // ====================== Taux affiché ======================

    public string RateText
    {
        get
        {
            double? rate = TopCurrency.Code == BottomCurrency.Code
                ? 1
                : GetRate(TopCurrency, BottomCurrency);

            if (!rate.HasValue)
                return $"1 {TopCurrency.Code} = … {BottomCurrency.Code}";

            string format = rate.Value < 0.01
                ? "0.######"
                : "0.####";

            string rateText = rate.Value
                .ToString(format, CultureInfo.InvariantCulture)
                .Replace('.', ',');

            return $"1 {TopCurrency.Code} = {rateText} {BottomCurrency.Code}";
        }
    }

    public string UpdatedText
    {
        get
        {
            if (_isLoading) return "Updating…";
            if (_loadFailed) return "Offline";
            if (_lastUpdateUtc is null) return string.Empty;

            TimeSpan span = DateTime.UtcNow - _lastUpdateUtc.Value;

            if (span.TotalMinutes < 1)
                return "Just now";

            if (span.TotalHours < 1)
                return $"{(int)span.TotalMinutes} min ago";

            return $"{(int)span.TotalHours} h ago";
        }
    }

    public Color UpdatedColor =>
        _loadFailed ? ErrorColor : SuccessColor;

    // ====================== Erreurs ======================

    private string _errorMessage = string.Empty;

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
                OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    // ====================== Chargement des taux ======================

    public async Task InitializeAsync()
    {
        await LoadFavoritesAsync();

        if (_rates.Count == 0)
            await LoadRatesAsync(forceRefresh: false);
    }

    private async Task LoadRatesAsync(bool forceRefresh)
    {
        if (_isLoading)
            return;

        _isLoading = true;
        RefreshDisplay();

        try
        {
            await _currencyService.GetLatestRatesAsync(forceRefresh);

            _rates = _currencyService
                .GetRatesList()
                .ToDictionary(r => r.Code, r => r.Rate);

            _lastUpdateUtc = DateTime.UtcNow;
            _loadFailed = false;
        }
        catch (Exception ex)
        {
            _loadFailed = true;
            _ = ShowErrorAsync(GetFriendlyError(ex));
        }
        finally
        {
            _isLoading = false;
            RefreshDisplay();
        }
    }

    // ====================== Sélection des devises ======================

    public void SetCurrency(int row, CurrencyInfo currency)
    {
        if (row == 0)
        {
            if (currency.Code == BottomCurrency.Code)
                BottomCurrency = TopCurrency;

            TopCurrency = currency;
        }
        else
        {
            if (currency.Code == TopCurrency.Code)
                TopCurrency = BottomCurrency;

            BottomCurrency = currency;
        }

        RefreshDisplay();
    }

    private CurrencyInfo ActiveCurrency =>
        _activeIndex == 0 ? TopCurrency : BottomCurrency;

    private CurrencyInfo OtherCurrency =>
        _activeIndex == 0 ? BottomCurrency : TopCurrency;

    private double? GetRate(
        CurrencyInfo from,
        CurrencyInfo to)
    {
        if (_rates.TryGetValue(from.Code, out double rateFrom) &&
            _rates.TryGetValue(to.Code, out double rateTo) &&
            rateFrom != 0)
        {
            return rateTo / rateFrom;
        }

        return null;
    }

    private double Convert(
        double value,
        CurrencyInfo from,
        CurrencyInfo to)
    {
        if (from.Code == to.Code)
            return value;

        double? rate = GetRate(from, to);

        return rate.HasValue
            ? value * rate.Value
            : 0;
    }

    private string ConvertedText
    {
        get
        {
            if (!ExpressionEvaluator.TryEvaluate(
                _expression, out double value))
            {
                value = 0;
            }

            return FormatDisplay(
                Convert(value, ActiveCurrency, OtherCurrency));
        }
    }

    private void SetActiveRow(int index)
    {
        if (index == _activeIndex)
            return;

        if (!ExpressionEvaluator.TryEvaluate(
            _expression, out double value))
        {
            value = 0;
        }

        double converted = Convert(
            value, ActiveCurrency, OtherCurrency);

        _activeIndex = index;
        _expression = FormatRaw(converted);
        _hasInput = converted > 0;
        _justEvaluated = true;

        RefreshDisplay();
    }

    // ====================== Calculatrice ======================

    private async Task OnKeyAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;

        ErrorMessage = string.Empty;

        if (key == "C")
        {
            ResetExpression();
        }
        else if (key == "←")
        {
            Backspace();
        }
        else if (key == "⇅")
        {
            (TopCurrency, BottomCurrency) =
                (BottomCurrency, TopCurrency);
        }
        else if (key == "%")
        {
            Percent();
        }
        else if (key == "=")
        {
            if (!_justEvaluated)
            {
                bool valid = EvaluateAndValidate(
                    out double amount);

                if (valid)
                    await SaveConversionAsync(amount);
            }
        }
        else if (key == ",")
        {
            AppendComma();
        }
        else if (key.Length == 1 &&
                 ExpressionEvaluator.IsOperator(key[0]))
        {
            AppendOperator(key[0]);
        }
        else if (key.Length == 1 &&
                 char.IsDigit(key[0]))
        {
            AppendDigit(key[0]);
        }

        RefreshDisplay();
    }

    private void ResetExpression()
    {
        _expression = "0";
        _hasInput = false;
        _justEvaluated = false;
    }

    private void Backspace()
    {
        if (_justEvaluated)
        {
            ResetExpression();
            return;
        }

        _expression = _expression.Length <= 1
            ? "0"
            : _expression[..^1];

        if (_expression == "−")
            _expression = "0";

        if (_expression == "0")
            _hasInput = false;
    }

    private void AppendDigit(char digit)
    {
        if (_justEvaluated)
        {
            _expression = "0";
            _justEvaluated = false;
        }

        if (_expression.Length >= MaxLength)
            return;

        if (TrailingNumber() == "0")
            _expression = _expression[..^1] + digit;
        else
            _expression += digit;

        _hasInput = true;
    }

    private void AppendComma()
    {
        if (_justEvaluated)
        {
            _expression = "0";
            _justEvaluated = false;
        }

        if (_expression.Length >= MaxLength)
            return;

        string trailing = TrailingNumber();

        if (trailing.Contains(','))
            return;

        _expression += trailing.Length == 0
            ? "0,"
            : ",";

        _hasInput = true;
    }

    private void AppendOperator(char op)
    {
        _justEvaluated = false;

        char last = _expression[^1];

        if (ExpressionEvaluator.IsOperator(last))
        {
            _expression = _expression[..^1] + op;
        }
        else if (_expression.Length < MaxLength)
        {
            _expression += op;
        }
    }

    private void Percent()
    {
        string trailing = TrailingNumber();

        if (trailing.Length == 0)
            return;

        if (!double.TryParse(
            trailing.Replace(',', '.'),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out double value))
        {
            return;
        }

        _expression =
            _expression[..^trailing.Length] +
            FormatRaw(value / 100);

        _justEvaluated = false;
    }

    private bool EvaluateAndValidate(out double amount)
    {
        amount = 0;

        if (!_hasInput && _expression == "0")
        {
            _ = ShowErrorAsync(
                "Veuillez saisir un montant.");

            return false;
        }

        if (!ExpressionEvaluator.TryEvaluate(
            _expression, out double value))
        {
            _ = ShowErrorAsync(
                "Expression invalide (format incorrect ou division par zéro).");

            return false;
        }

        if (!InputValidator.TryParseAmount(
            value.ToString("R", CultureInfo.InvariantCulture),
            allowZeroOrNegative: false,
            out _, out string error))
        {
            _ = ShowErrorAsync(error);
            return false;
        }

        amount = value;

        _expression = FormatRaw(value);
        _justEvaluated = true;
        _hasInput = true;

        return true;
    }

    // ====================== Historique SQLite ======================

    private async Task SaveConversionAsync(double amount)
    {
        if (!_currentUserId.HasValue)
            return;

        CurrencyInfo from = ActiveCurrency;
        CurrencyInfo to = OtherCurrency;

        double convertedAmount = Convert(
            amount, from, to);

        if (amount <= 0 || convertedAmount <= 0)
        {
            if (from.Code != to.Code)
            {
                _ = ShowErrorAsync(
                    "Conversion impossible : taux de change indisponible.");
            }

            return;
        }

        try
        {
            var history = new ConversionHistory
            {
                UserId = _currentUserId.Value,
                FromCurrency = from.Code,
                ToCurrency = to.Code,
                Amount = amount,
                ConvertedAmount = convertedAmount,
                ConversionDate = DateTime.Now
            };

            await _databaseService.AddConversionAsync(history);
        }
        catch (Exception ex)
        {
            _ = ShowErrorAsync(
                "Impossible d'enregistrer l'historique : " +
                ex.Message);
        }
    }

    private string TrailingNumber()
    {
        int index = _expression.LastIndexOfAny(
            ExpressionEvaluator.Operators);

        return _expression[(index + 1)..];
    }

    // ====================== Affichage ======================

    private void RefreshDisplay()
    {
        OnPropertyChanged(nameof(TopText));
        OnPropertyChanged(nameof(BottomText));
        OnPropertyChanged(nameof(TopFontSize));
        OnPropertyChanged(nameof(BottomFontSize));
        OnPropertyChanged(nameof(RateText));
        OnPropertyChanged(nameof(UpdatedText));
        OnPropertyChanged(nameof(UpdatedColor));
    }

    private string PrettyExpression
    {
        get
        {
            var result = new StringBuilder();
            var token = new StringBuilder();

            foreach (char c in _expression)
            {
                if (ExpressionEvaluator.IsOperator(c))
                {
                    result.Append(
                        GroupNumber(token.ToString()));

                    token.Clear();

                    result.Append(' ')
                          .Append(c)
                          .Append(' ');
                }
                else
                {
                    token.Append(c);
                }
            }

            result.Append(
                GroupNumber(token.ToString()));

            return result.ToString().Trim();
        }
    }

    private static string GroupNumber(string token)
    {
        if (token.Length == 0)
            return string.Empty;

        int comma = token.IndexOf(',');

        string integerPart = comma >= 0
            ? token[..comma]
            : token;

        string rest = comma >= 0
            ? token[comma..]
            : string.Empty;

        var sb = new StringBuilder();

        for (int i = 0; i < integerPart.Length; i++)
        {
            if (i > 0 &&
                (integerPart.Length - i) % 3 == 0)
            {
                sb.Append('\u00A0');
            }

            sb.Append(integerPart[i]);
        }

        sb.Append(rest);

        return sb.ToString();
    }

    private static string FormatDisplay(double value)
    {
        if (Math.Abs(value) < 0.00005)
            value = 0;

        string format =
            Math.Abs(value) >= 1 || value == 0
                ? "#,0.##"
                : "#,0.####";

        return Math.Round(value, 4)
            .ToString(format, CultureInfo.InvariantCulture)
            .Replace(',', '\u00A0')
            .Replace('.', ',');
    }

    private static string FormatRaw(double value)
    {
        if (Math.Abs(value) < 0.00005)
            return "0";

        return Math.Round(value, 4)
            .ToString("0.####", CultureInfo.InvariantCulture)
            .Replace('.', ',')
            .Replace('-', '−');
    }

    private static double FontSizeFor(string text) =>
        text.Length switch
        {
            <= 7 => 44,
            <= 10 => 36,
            <= 13 => 28,
            _ => 22
        };

    // ====================== Messages ======================

    private async Task ShowErrorAsync(string message)
    {
        ErrorMessage = message;

        int version = ++_errorVersion;

        await Task.Delay(3500);

        if (version == _errorVersion)
            ErrorMessage = string.Empty;
    }

    private string BuildInfoText()
    {
        return "Source : open.er-api.com\n" +
               "Devise de base : USD\n" +
               $"Dernière mise à jour (API) : {_currencyService.LastApiUpdate}\n" +
               $"Taux chargés : {_rates.Count}";
    }

    private static string GetFriendlyError(Exception ex) => ex switch
    {
        HttpRequestException =>
            "Connexion impossible. Vérifiez votre accès Internet puis réessayez.",

        TaskCanceledException =>
            "Le serveur met trop de temps à répondre. Réessayez dans un instant.",

        JsonException =>
            "La réponse du serveur est illisible (JSON invalide).",

        _ =>
            $"Une erreur est survenue : {ex.Message}"
    };

    // ====================== GESTION DES FAVORIS ======================

    public async Task LoadFavoritesAsync()
    {
        if (!_currentUserId.HasValue)
            return;

        var favorites = await _databaseService.GetFavoritesAsync(
            _currentUserId.Value);

        var favoriteCodes = favorites
            .Select(f => f.CurrencyCode)
            .ToHashSet();

        foreach (var currency in Currencies)
        {
            currency.IsFavorite = favoriteCodes.Contains(currency.Code);
        }

        OnPropertyChanged(nameof(Currencies));
    }

    public async Task ToggleFavoriteAsync(CurrencyInfo currency)
    {
        if (!_currentUserId.HasValue)
        {
            InfoRequested?.Invoke(
                "Connectez-vous pour enregistrer vos favoris.");
            return;
        }

        if (currency.IsFavorite)
        {
            var favorites = await _databaseService.GetFavoritesAsync(
                _currentUserId.Value);

            var favorite = favorites.FirstOrDefault(
                f => f.CurrencyCode == currency.Code);

            if (favorite != null)
            {
                await _databaseService.DeleteFavoriteAsync(
                    favorite.Id,
                    _currentUserId.Value);
            }

            currency.IsFavorite = false;
        }
        else
        {
            await _databaseService.AddFavoriteAsync(
                new FavoriteCurrency
                {
                    UserId = _currentUserId.Value,
                    CurrencyCode = currency.Code
                });

            currency.IsFavorite = true;
        }

        OnPropertyChanged(nameof(Currencies));
    }

}

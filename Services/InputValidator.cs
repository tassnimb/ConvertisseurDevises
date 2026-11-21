using System.Globalization;

namespace ConvertisseurDevises.Services;

/// <summary>
/// Validation des saisies utilisateur : non vide, numérique, supérieur à zéro.
/// </summary>
public static class InputValidator
{
    /// <param name="text">Texte saisi.</param>
    /// <param name="allowZeroOrNegative">
    /// true pour les températures (0 °C et -5 °C sont valides) ; false pour les montants.
    /// </param>
    public static bool TryParseAmount(string? text, bool allowZeroOrNegative, out double value, out string error)
    {
        value = 0;
        error = string.Empty;

        // 1) Non vide
        if (string.IsNullOrWhiteSpace(text))
        {
            error = "Veuillez saisir un montant.";
            return false;
        }

        // 2) Numérique : on accepte la virgule ET le point comme séparateur décimal.
        string normalized = text.Trim().Replace(" ", "").Replace(',', '.');
        if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            || double.IsNaN(value) || double.IsInfinity(value))
        {
            value = 0;
            error = "Le montant doit être une valeur numérique valide (ex. 100 ou 12,5).";
            return false;
        }

        // 3) Strictement supérieur à zéro
        if (!allowZeroOrNegative && value <= 0)
        {
            error = "Le montant doit être supérieur à zéro.";
            return false;
        }

        return true;
    }
}

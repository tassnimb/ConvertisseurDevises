using System.Globalization;
using System.Text;

namespace ConvertisseurDevises.Services;

/// <summary>
/// Évalue une expression saisie sur la calculatrice, ex. "12,5+3×2".
/// Opérateurs : + − × ÷ (priorité de × et ÷ sur + et −).
/// Le séparateur décimal est la virgule.
/// </summary>
public static class ExpressionEvaluator
{
    public static readonly char[] Operators = { '÷', '×', '−', '+' };

    public static bool IsOperator(char c) => Array.IndexOf(Operators, c) >= 0;

    public static bool TryEvaluate(string? expression, out double result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(expression)) return false;

        // Un opérateur final est ignoré : "12+" est évalué comme "12".
        string expr = expression.TrimEnd(Operators);
        if (expr.Length == 0) return false;

        var numbers = new List<double>();
        var ops = new List<char>();
        var current = new StringBuilder();

        for (int i = 0; i < expr.Length; i++)
        {
            char c = expr[i];

            if (IsOperator(c))
            {
                if (current.Length == 0)
                {
                    // Signe moins en début d'expression (nombre négatif)
                    if (i == 0 && c == '−') { current.Append('-'); continue; }
                    return false;
                }

                if (!TryParseNumber(current.ToString(), out double n)) return false;
                numbers.Add(n);
                ops.Add(c);
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        if (!TryParseNumber(current.ToString(), out double lastNumber)) return false;
        numbers.Add(lastNumber);

        // Passe 1 : multiplications et divisions
        for (int i = 0; i < ops.Count;)
        {
            if (ops[i] == '×' || ops[i] == '÷')
            {
                if (ops[i] == '÷' && numbers[i + 1] == 0) return false; // division par zéro

                numbers[i] = ops[i] == '×'
                    ? numbers[i] * numbers[i + 1]
                    : numbers[i] / numbers[i + 1];

                numbers.RemoveAt(i + 1);
                ops.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }

        // Passe 2 : additions et soustractions
        double total = numbers[0];
        for (int i = 0; i < ops.Count; i++)
            total = ops[i] == '+' ? total + numbers[i + 1] : total - numbers[i + 1];

        if (double.IsNaN(total) || double.IsInfinity(total)) return false;

        result = total;
        return true;
    }

    private static bool TryParseNumber(string text, out double value)
        => double.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}
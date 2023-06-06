using System.Text;
using System.Globalization;

// namespace BugSearch.Shared.Extensions;

public static class StringExtensions
{
    public static string NormalizeComplex(this string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        string normalizedString = input.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new();

        foreach (char c in normalizedString)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
    }

    public static bool ContainsNormalized(this string? input, string? value)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(value))
            return false;

        return input.NormalizeComplex().Contains(value.NormalizeComplex());
    }

    public static bool Contains(this string? input, string? value)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(value))
            return false;

        return input.NormalizeComplex().Contains(value.NormalizeComplex());
    }
}
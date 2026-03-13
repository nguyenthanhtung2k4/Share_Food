using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ShareFood.Web.Helpers;

public static class SlugHelper
{
    public static string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Guid.NewGuid().ToString("N");
        }

        var normalized = input.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var withoutDiacritics = builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        var slug = Regex.Replace(withoutDiacritics, @"[^a-z0-9\s-]", string.Empty);
        slug = Regex.Replace(slug, @"\s+", "-").Trim('-');
        slug = Regex.Replace(slug, @"-+", "-");

        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N") : slug;
    }
}

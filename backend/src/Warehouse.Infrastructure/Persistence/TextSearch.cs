namespace Warehouse.Infrastructure.Persistence;

internal static class TextSearch
{
    public const string AccentInsensitive = "Latin1_General_100_CI_AI";

    public static string? ToContainsPattern(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return null;
        }

        return $"%{EscapeLike(search.Trim())}%";
    }

    private static string EscapeLike(string value)
    {
        return value
            .Replace("[", "[[]", StringComparison.Ordinal)
            .Replace("%", "[%]", StringComparison.Ordinal)
            .Replace("_", "[_]", StringComparison.Ordinal);
    }
}

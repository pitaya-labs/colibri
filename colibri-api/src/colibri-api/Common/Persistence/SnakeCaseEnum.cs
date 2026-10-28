using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace colibri_api.Common.Persistence;

/// <summary>
/// Persists an enum as snake_case text (<c>SystemAdmin</c> → <c>system_admin</c>), the form the
/// database design uses for <c>status</c>/<c>type</c> values.
/// </summary>
public sealed class SnakeCaseEnumConverter<TEnum>()
    : ValueConverter<TEnum, string>(v => SnakeCaseEnumMap<TEnum>.Names[v], s => SnakeCaseEnumMap<TEnum>.Values[s])
    where TEnum : struct, Enum;

internal static partial class SnakeCaseEnumMap<TEnum> where TEnum : struct, Enum
{
    public static readonly IReadOnlyDictionary<TEnum, string> Names =
        Enum.GetValues<TEnum>().ToDictionary(v => v, v => ToSnakeCase(v.ToString()));

    public static readonly IReadOnlyDictionary<string, TEnum> Values =
        Names.ToDictionary(kv => kv.Value, kv => kv.Key);

    private static string ToSnakeCase(string name) =>
        UpperCaseLetter().Replace(name, "_$1").TrimStart('_').ToLowerInvariant();

    [GeneratedRegex("([A-Z])")]
    private static partial Regex UpperCaseLetter();
}

public static class SnakeCaseEnum
{
    public static PropertyBuilder<TEnum> HasSnakeCaseConversion<TEnum>(this PropertyBuilder<TEnum> property)
        where TEnum : struct, Enum =>
        property.HasConversion(new SnakeCaseEnumConverter<TEnum>()).HasMaxLength(32);

    /// <summary>Builds a <c>column IN ('a', 'b')</c> expression for a CHECK that mirrors the enum.</summary>
    public static string CheckIn<TEnum>(string column) where TEnum : struct, Enum =>
        $"{column} IN ({string.Join(", ", SnakeCaseEnumMap<TEnum>.Names.Values.Select(n => $"'{n}'"))})";
}

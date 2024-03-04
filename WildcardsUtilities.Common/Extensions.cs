namespace WildcardsUtilities.Common;

internal static class Extensions
{
    // Returns true if at least one of the provided regexes matches with the specified input,
    // otherwise false.
    internal static bool AnyMatch(this IEnumerable<Regex> regexes, string input) =>
        regexes.Any(regex => regex.IsMatch(input));
}


namespace WildcardsUtilities.Scanner.PlainTextTables;

internal class TababularPlainTextTableGenerator : IPlainTextTableGenerator
{
    private static readonly TableFormatter _formatter =
        new(new() { MaxTableWidth = 100 });

    public string ToPlainText(IEnumerable<IReadOnlyDictionary<string, object>> table) =>
        _formatter.FormatDictionaries(table.Select(d => d.ToDictionary()));

    public string ToPlainText(IEnumerable objectsTable) =>
        _formatter.FormatObjects(objectsTable);
}

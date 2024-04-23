namespace WildcardsUtilities.Scanner.PlainTextTables;

public interface IPlainTextTableGenerator
{
    string ToPlainText(IEnumerable<IReadOnlyDictionary<string, object>> table);
}

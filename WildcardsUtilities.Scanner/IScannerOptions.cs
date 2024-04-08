
namespace WildcardsUtilities.Scanner;

public interface IScannerOptions
{
    IEnumerable<char> Drives { get; }
    IEnumerable<char> ExcludedDrives { get; }
    IEnumerable<string> Filters { get; }
    bool SkipChecksumComputations { get; }
}
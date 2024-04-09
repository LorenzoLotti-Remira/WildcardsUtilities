﻿namespace WildcardsUtilities.Scanner;

public sealed class ScannerOptions : IScannerOptions
{
    [Option("no-checksum", HelpText = "Skip checksum computations.")]
    public bool SkipChecksumComputations { get; set; }

    [Option('d', "drives", Min = 1, HelpText = "Input drive letters to be included.")]
    public IEnumerable<char> Drives { get; set; } = [];

    [Option('e', "except", Min = 1, HelpText = "Input drive letter to be excluded.")]
    public IEnumerable<char> ExcludedDrives { get; set; } = [];

    [Option('f', "filters", Default = new string[] { "**/*" }, Min = 1, HelpText = "Input filters to be applied.")]
    public IEnumerable<string> Filters { get; set; } = [];
}
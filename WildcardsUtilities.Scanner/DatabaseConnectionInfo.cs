namespace WildcardsUtilities.Scanner;

public record DatabaseConnectionInfo
(
    string Identifier,
    string Provider,
    string ConnectionString
);

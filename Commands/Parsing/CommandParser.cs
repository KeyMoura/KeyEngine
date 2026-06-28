namespace KeyEngine.Commands.Parsing;

/// <summary>
/// Parses command input.
/// </summary>
public sealed class CommandParser
{
    private readonly CommandLexer _lexer = new();

    /// <summary>
    /// Parses command input.
    /// </summary>
    /// <param name="input">
    /// The command text.
    /// </param>
    /// <returns>
    /// The parsed command request.
    /// </returns>
    public CommandRequest Parse(
        string input,
        ICommandSource source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        ArgumentNullException.ThrowIfNull(source);

        IReadOnlyList<string> tokens =
            _lexer.Tokenize(input);

        if (tokens.Count == 0)
        {
            throw new FormatException(
                "The command is empty.");
        }

        return new CommandRequest
        {
            RawText = input,
            Source = source,
            Name = tokens[0],
            Arguments = tokens.Skip(1).ToList()
        };
    }
}
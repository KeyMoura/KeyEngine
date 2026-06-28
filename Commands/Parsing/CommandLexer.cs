using System.Text;

namespace KeyEngine.Commands.Parsing;

/// <summary>
/// Tokenizes command input.
/// </summary>
public sealed class CommandLexer
{
    /// <summary>
    /// Tokenizes command input.
    /// </summary>
    /// <param name="input">
    /// The command text.
    /// </param>
    /// <returns>
    /// The parsed tokens.
    /// </returns>
    public IReadOnlyList<string> Tokenize(
    string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        List<string> tokens = [];

        StringBuilder builder = new();

        bool inQuotes = false;

        foreach (char c in input)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;

                continue;
            }

            if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (builder.Length > 0)
                {
                    tokens.Add(builder.ToString());

                    builder.Clear();
                }

                continue;
            }

            builder.Append(c);
        }

        if (builder.Length > 0)
        {
            tokens.Add(builder.ToString());
        }

        if (inQuotes)
        {
            throw new FormatException(
                "The command contains an unterminated quoted string.");
        }

        return tokens;
    }
}
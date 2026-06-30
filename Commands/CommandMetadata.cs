using KeyEngine.Metadata;

namespace KeyEngine.Commands
{
    public sealed class CommandMetadata
    {
        public required string Name { get; init; }

        public IReadOnlyList<string> Aliases { get; init; }
            = [];

        internal MethodMetadata Method { get; }

        internal CommandMetadata(MethodMetadata method)
        {
            ArgumentNullException.ThrowIfNull(method);

            Method = method;
        }

        /// <summary>
        /// Gets the command description.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets the command usage string.
        /// </summary>
        public string Usage { get; init; } = string.Empty;

        /// <summary>
        /// Gets the command category.
        /// </summary>
        public string Category { get; init; } = string.Empty;
    }
}

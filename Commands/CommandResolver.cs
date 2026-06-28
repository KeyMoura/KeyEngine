namespace KeyEngine.Commands
{
    public sealed class CommandResolver
    {
        public CommandMetadata Resolve(
            IReadOnlyList<CommandMetadata> overloads,
            object?[] arguments)
        {
            List<CommandMetadata> matches = [];

            foreach (CommandMetadata command in overloads)
            {
                IReadOnlyList<Type> parameterTypes =
                    command.Method.ParameterTypes;

                if (parameterTypes.Count != arguments.Length)
                {
                    continue;
                }

                bool valid = true;

                for (int i = 0; i < parameterTypes.Count; i++)
                {
                    object? argument = arguments[i];

                    // We'll support null properly later.
                    if (argument is null)
                    {
                        valid = false;
                        break;
                    }

                    if (!parameterTypes[i].IsAssignableFrom(argument.GetType()))
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    matches.Add(command);
                }
            }

            return matches.Count switch
            {
                0 => throw new InvalidOperationException(
                    "No matching command overload found."),

                1 => matches[0],

                _ => throw new InvalidOperationException(
                    "Command invocation is ambiguous.")
            };
        }
    }
}

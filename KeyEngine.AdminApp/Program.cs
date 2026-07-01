using KeyEngine.AdminApp;

AdminAppOptions options = ParseOptions(args);
using HttpClient httpClient = new()
{
    BaseAddress = options.BaseUri
};

AdminApiClient client = new(httpClient)
{
    AdminToken = options.AdminToken
};
AdminShell shell = new(
    options.BaseUri,
    client,
    Console.In,
    Console.Out,
    Console.Error);

await shell.RunAsync();

static AdminAppOptions ParseOptions(string[] args)
{
    string baseUrl = "http://localhost:5000";
    string? token = null;

    for (int i = 0; i < args.Length; i++)
    {
        if (args[i] == "--token")
        {
            if (i + 1 >= args.Length)
            {
                throw new ArgumentException(
                    "The --token option requires a value.");
            }

            token = args[++i];
            continue;
        }

        baseUrl = args[i];
    }

    return new AdminAppOptions(
        CreateBaseUri(baseUrl),
        token);
}

static Uri CreateBaseUri(string value)
{
    if (!Uri.TryCreate(
            value,
            UriKind.Absolute,
            out Uri? uri))
    {
        throw new ArgumentException(
            $"'{value}' is not a valid absolute URL.");
    }

    return uri;
}

internal sealed record AdminAppOptions(
    Uri BaseUri,
    string? AdminToken);

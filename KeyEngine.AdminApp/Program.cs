using KeyEngine.AdminApp;

Uri baseUri = GetBaseUri(args);
using HttpClient httpClient = new()
{
    BaseAddress = baseUri
};

AdminApiClient client = new(httpClient);
AdminShell shell = new(
    baseUri,
    client,
    Console.In,
    Console.Out,
    Console.Error);

await shell.RunAsync();

static Uri GetBaseUri(string[] args)
{
    string value = args.Length > 0
        ? args[0]
        : "http://localhost:5000";

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

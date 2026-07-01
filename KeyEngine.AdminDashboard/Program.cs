using Avalonia;

namespace KeyEngine.AdminDashboard;

internal static class Program
{
    private const string DefaultServerUrl = "http://localhost:5000";

    [STAThread]
    public static void Main(string[] args)
    {
        Uri serverUri = CreateServerUri(
            args.Length > 0
                ? args[0]
                : DefaultServerUrl);

        BuildAvaloniaApp(serverUri)
            .StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp(Uri serverUri)
    {
        return AppBuilder
            .Configure(() => new App(serverUri))
            .UsePlatformDetect()
            .LogToTrace();
    }

    private static Uri CreateServerUri(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException(
                $"'{value}' is not a valid HTTP server URL.");
        }

        return uri;
    }
}

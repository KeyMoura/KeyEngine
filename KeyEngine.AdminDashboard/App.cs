using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;
using KeyEngine.AdminClient;

namespace KeyEngine.AdminDashboard;

internal sealed class App
    : Application
{
    private readonly Uri _serverUri;

    public App(Uri serverUri)
    {
        _serverUri = serverUri;
    }

    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            HttpClient httpClient = new()
            {
                BaseAddress = _serverUri
            };

            AdminApiClient client = new(httpClient);
            MainWindow window = new(
                _serverUri,
                client);

            window.Closed += (_, _) => httpClient.Dispose();
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}

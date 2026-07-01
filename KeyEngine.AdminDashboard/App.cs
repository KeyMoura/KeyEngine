using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;

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
            desktop.MainWindow = new MainWindow(_serverUri);
        }

        base.OnFrameworkInitializationCompleted();
    }
}

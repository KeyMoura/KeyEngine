using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using KeyEngine.AdminClient;
using System.Text.Json;

namespace KeyEngine.AdminDashboard;

internal sealed class MainWindow
    : Window
{
    private readonly IAdminApiClient _client;
    private readonly TextBlock _connectionText;
    private readonly TextBlock _applicationText;
    private readonly TextBlock _engineStateText;
    private readonly TextBlock _pluginCountText;
    private readonly TextBlock _parameterCountText;
    private readonly TextBlock _logCountText;
    private readonly TextBlock _detailText;

    public MainWindow(
        Uri serverUri,
        IAdminApiClient client)
    {
        _client = client;

        Title = "KeyEngine Admin Dashboard";
        Width = 820;
        Height = 560;
        MinWidth = 640;
        MinHeight = 420;

        _connectionText = CreateValue("Not connected");
        _applicationText = CreateValue("-");
        _engineStateText = CreateValue("-");
        _pluginCountText = CreateValue("-");
        _parameterCountText = CreateValue("-");
        _logCountText = CreateValue("-");
        _detailText = new TextBlock
        {
            Text = "Select Refresh to query the server.",
            TextWrapping = TextWrapping.Wrap
        };

        Content = CreateLayout(serverUri);
        Opened += async (_, _) => await RefreshStatusAsync();
    }

    private Control CreateLayout(Uri serverUri)
    {
        StackPanel root = new()
        {
            Margin = new Thickness(24),
            Spacing = 16
        };

        root.Children.Add(new TextBlock
        {
            Text = "KeyEngine Admin Dashboard",
            FontSize = 24,
            FontWeight = FontWeight.SemiBold
        });
        root.Children.Add(CreateRow("Server URL", serverUri.ToString()));
        root.Children.Add(CreateRow("Connection", _connectionText));
        root.Children.Add(CreateRow("Application", _applicationText));
        root.Children.Add(CreateRow("Engine state", _engineStateText));
        root.Children.Add(CreateRow("Plugins", _pluginCountText));
        root.Children.Add(CreateRow("Parameters", _parameterCountText));
        root.Children.Add(CreateRow("Runtime logs", _logCountText));

        WrapPanel buttons = new()
        {
            Orientation = Orientation.Horizontal
        };
        buttons.Children.Add(CreateButton("Refresh", RefreshStatusAsync));
        buttons.Children.Add(CreateButton("View plugins", ShowPluginsAsync));
        buttons.Children.Add(CreateButton("View parameters", ShowParametersAsync));
        buttons.Children.Add(CreateButton("View logs", ShowLogsAsync));
        buttons.Children.Add(CreateButton("View routes", ShowRoutesAsync));
        root.Children.Add(buttons);

        root.Children.Add(new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(12),
            Child = new ScrollViewer
            {
                Content = _detailText,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
            }
        });

        return root;
    }

    private async Task RefreshStatusAsync()
    {
        await RunRequestAsync(async () =>
        {
            _connectionText.Text = "Connecting...";
            AdminStatus? status = await _client.GetStatusAsync();

            if (status is null)
            {
                throw new HttpRequestException("The server returned no status data.");
            }

            _connectionText.Text = "Connected";
            _applicationText.Text = $"{status.ApplicationName} {status.ApplicationVersion}".Trim();
            _engineStateText.Text = status.State ?? "Unknown";
            _pluginCountText.Text = status.PluginCount.ToString();
            _parameterCountText.Text = status.ParameterCount.ToString();
            _logCountText.Text = status.RuntimeLogCount.ToString();
            _detailText.Text = "Server status refreshed.";
        });
    }

    private async Task ShowPluginsAsync()
    {
        await RunRequestAsync(async () =>
        {
            IReadOnlyList<AdminPlugin> plugins = await _client.GetPluginsAsync();
            _detailText.Text = plugins.Count == 0
                ? "No plugins reported."
                : $"Plugins ({plugins.Count}){Environment.NewLine}{Environment.NewLine}" +
                  string.Join(
                      Environment.NewLine,
                      plugins.Select(FormatPlugin));
        });
    }

    private async Task ShowParametersAsync()
    {
        await RunRequestAsync(async () =>
        {
            IReadOnlyList<AdminParameter> parameters = await _client.GetParametersAsync();
            _detailText.Text = parameters.Count == 0
                ? "No parameters reported."
                : $"Parameters ({parameters.Count}){Environment.NewLine}{Environment.NewLine}" +
                  string.Join(
                      Environment.NewLine,
                      parameters.Select(FormatParameter));
        });
    }

    private async Task ShowLogsAsync()
    {
        await RunRequestAsync(async () =>
        {
            IReadOnlyList<AdminLogEntry> logs = await _client.GetLogsAsync();
            _detailText.Text = logs.Count == 0
                ? "No runtime logs reported."
                : $"Runtime logs ({logs.Count}){Environment.NewLine}{Environment.NewLine}" +
                  string.Join(
                      Environment.NewLine,
                      logs.Select(FormatLogEntry));
        });
    }

    private async Task ShowRoutesAsync()
    {
        await RunRequestAsync(async () =>
        {
            IReadOnlyList<AdminRoute> routes = await _client.GetRoutesAsync();
            _detailText.Text = routes.Count == 0
                ? "No routes reported."
                : $"Routes ({routes.Count}){Environment.NewLine}{Environment.NewLine}" +
                  string.Join(
                      Environment.NewLine,
                      routes.Select(FormatRoute));
        });
    }

    private async Task RunRequestAsync(Func<Task> request)
    {
        try
        {
            await request();
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            _connectionText.Text = "Unavailable";
            _detailText.Text = $"Unable to reach the server: {exception.Message}";
        }
    }

    private static Button CreateButton(
        string text,
        Func<Task> action)
    {
        Button button = new()
        {
            Content = text,
            Margin = new Thickness(0, 0, 8, 8)
        };
        button.Click += async (_, _) => await action();
        return button;
    }

    private static Control CreateRow(
        string label,
        string value)
    {
        return CreateRow(
            label,
            CreateValue(value));
    }

    private static Control CreateRow(
        string label,
        TextBlock value)
    {
        StackPanel row = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };
        row.Children.Add(new TextBlock
        {
            Text = $"{label}:",
            FontWeight = FontWeight.SemiBold,
            Width = 120
        });
        row.Children.Add(value);
        return row;
    }

    private static TextBlock CreateValue(string text)
    {
        return new TextBlock
        {
            Text = text
        };
    }

    private static string FormatPlugin(AdminPlugin plugin)
    {
        string identity = string.IsNullOrWhiteSpace(plugin.Name)
            ? plugin.Id ?? "Unknown plugin"
            : $"{plugin.Name} ({plugin.Id})";

        return $"{identity} | Version: {plugin.Version ?? "Unknown"} | State: {plugin.State ?? "Unknown"}";
    }

    private static string FormatParameter(AdminParameter parameter)
    {
        string category = string.IsNullOrWhiteSpace(parameter.Category)
            ? string.Empty
            : $" [{parameter.Category}]";
        string readOnly = parameter.IsReadOnly
            ? " | Read-only"
            : string.Empty;

        return $"{parameter.Key}{category} = {parameter.Value} | Type: {parameter.ValueType ?? "Unknown"}{readOnly}";
    }

    private static string FormatLogEntry(AdminLogEntry entry)
    {
        string context = string.IsNullOrWhiteSpace(entry.Category) &&
                         string.IsNullOrWhiteSpace(entry.Source)
            ? string.Empty
            : $" ({entry.Category ?? entry.Source})";

        return $"{entry.Timestamp:u} [{entry.Level ?? "Unknown"}]{context} {entry.Message}";
    }

    private static string FormatRoute(AdminRoute route)
    {
        string protection = route.RequiresAdminToken
            ? " | Admin token required"
            : string.Empty;
        string description = string.IsNullOrWhiteSpace(route.Description)
            ? string.Empty
            : $" | {route.Description}";

        return $"{route.Method} {route.Path}{protection}{description}";
    }
}

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
    private readonly TextBlock _adminTokenStatusText;
    private readonly TextBox _adminTokenTextBox;
    private readonly TextBox _parameterKeyTextBox;
    private readonly TextBox _parameterValueTextBox;
    private readonly TextBox _parameterDescriptionTextBox;
    private readonly TextBox _parameterCategoryTextBox;
    private readonly TextBox _parameterPersistencePathTextBox;

    public MainWindow(
        Uri serverUri,
        IAdminApiClient client)
    {
        _client = client;

        Title = "KeyEngine Admin Dashboard";
        Width = 820;
        Height = 700;
        MinWidth = 640;
        MinHeight = 420;

        _connectionText = CreateValue("Not connected");
        _applicationText = CreateValue("-");
        _engineStateText = CreateValue("-");
        _pluginCountText = CreateValue("-");
        _parameterCountText = CreateValue("-");
        _logCountText = CreateValue("-");
        _adminTokenStatusText = CreateValue("Not set");
        _adminTokenTextBox = CreateInput("Admin token");
        _adminTokenTextBox.PasswordChar = '*';
        _parameterKeyTextBox = CreateInput("Parameter key");
        _parameterValueTextBox = CreateInput("Parameter value");
        _parameterDescriptionTextBox = CreateInput("Description (optional)");
        _parameterCategoryTextBox = CreateInput("Category (optional)");
        _parameterPersistencePathTextBox = CreateInput("Parameter file path");
        _detailText = new TextBlock
        {
            Text = "Select Refresh to query the server.",
            TextWrapping = TextWrapping.Wrap
        };

        Content = new ScrollViewer
        {
            Content = CreateLayout(serverUri),
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
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

        root.Children.Add(new TextBlock
        {
            Text = "Admin token",
            FontSize = 18,
            FontWeight = FontWeight.SemiBold
        });
        root.Children.Add(CreateRow("Token status", _adminTokenStatusText));
        root.Children.Add(_adminTokenTextBox);
        root.Children.Add(CreateButton("Apply token", ApplyAdminTokenAsync));

        root.Children.Add(new TextBlock
        {
            Text = "Parameter editor",
            FontSize = 18,
            FontWeight = FontWeight.SemiBold
        });
        root.Children.Add(_parameterKeyTextBox);
        root.Children.Add(_parameterValueTextBox);
        root.Children.Add(_parameterDescriptionTextBox);
        root.Children.Add(_parameterCategoryTextBox);

        WrapPanel parameterButtons = new();
        parameterButtons.Children.Add(CreateButton("Set parameter", SetParameterAsync));
        parameterButtons.Children.Add(CreateButton("Delete parameter", DeleteParameterAsync));
        parameterButtons.Children.Add(CreateButton("Refresh parameters", ShowParametersAsync));
        root.Children.Add(parameterButtons);

        root.Children.Add(new TextBlock
        {
            Text = "Parameter persistence",
            FontSize = 18,
            FontWeight = FontWeight.SemiBold
        });
        root.Children.Add(_parameterPersistencePathTextBox);

        WrapPanel persistenceButtons = new();
        persistenceButtons.Children.Add(CreateButton("Save parameters", SaveParametersAsync));
        persistenceButtons.Children.Add(CreateButton("Load parameters", LoadParametersAsync));
        root.Children.Add(persistenceButtons);

        root.Children.Add(new TextBlock
        {
            Text = "Runtime logs",
            FontSize = 18,
            FontWeight = FontWeight.SemiBold
        });

        WrapPanel logButtons = new();
        logButtons.Children.Add(CreateButton("Refresh logs", ShowLogsAsync));
        logButtons.Children.Add(CreateButton("Clear logs", ClearLogsAsync));
        root.Children.Add(logButtons);

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

            UpdateStatus(status);
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
            _detailText.Text = FormatParameterList(parameters);
        });
    }

    private async Task ShowLogsAsync()
    {
        await RunRequestAsync(async () =>
        {
            IReadOnlyList<AdminLogEntry> logs = await _client.GetLogsAsync();
            _detailText.Text = FormatLogList(logs);
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

    private async Task SetParameterAsync()
    {
        string key = _parameterKeyTextBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(key))
        {
            _detailText.Text = "A parameter key is required.";
            return;
        }

        string value = _parameterValueTextBox.Text ?? string.Empty;
        string? description = NullIfWhiteSpace(_parameterDescriptionTextBox.Text);
        string? category = NullIfWhiteSpace(_parameterCategoryTextBox.Text);

        await RunRequestAsync(async () =>
        {
            await _client.SetParameterAsync(
                key,
                value,
                description,
                category);
            _detailText.Text = $"Parameter '{key}' was set successfully.";
        });
    }

    private Task ApplyAdminTokenAsync()
    {
        string? token = NullIfWhiteSpace(_adminTokenTextBox.Text);
        _client.AdminToken = token;
        _adminTokenTextBox.Text = string.Empty;

        if (token is null)
        {
            _adminTokenStatusText.Text = "Not set";
            _detailText.Text = "Admin token cleared.";
        }
        else
        {
            _adminTokenStatusText.Text = "Set for this session";
            _detailText.Text = "Admin token set for protected actions.";
        }

        return Task.CompletedTask;
    }

    private async Task DeleteParameterAsync()
    {
        string key = _parameterKeyTextBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(key))
        {
            _detailText.Text = "A parameter key is required.";
            return;
        }

        await RunRequestAsync(async () =>
        {
            await _client.DeleteParameterAsync(key);
            _detailText.Text = $"Parameter '{key}' was deleted successfully.";
        });
    }

    private async Task SaveParametersAsync()
    {
        string path = _parameterPersistencePathTextBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            _detailText.Text = "A parameter file path is required.";
            return;
        }

        await RunRequestAsync(async () =>
        {
            await _client.SaveParametersAsync(path);
            _detailText.Text = $"Parameters were saved successfully to '{path}'.";
        });
    }

    private async Task LoadParametersAsync()
    {
        string path = _parameterPersistencePathTextBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            _detailText.Text = "A parameter file path is required.";
            return;
        }

        await RunRequestAsync(async () =>
        {
            await _client.LoadParametersAsync(path);

            AdminStatus? status = await _client.GetStatusAsync();
            if (status is null)
            {
                throw new HttpRequestException("The server returned no status data.");
            }

            IReadOnlyList<AdminParameter> parameters = await _client.GetParametersAsync();
            UpdateStatus(status);
            _detailText.Text =
                $"Parameters were loaded successfully from '{path}'." +
                Environment.NewLine +
                Environment.NewLine +
                FormatParameterList(parameters);
        });
    }

    private async Task ClearLogsAsync()
    {
        await RunRequestAsync(async () =>
        {
            await _client.ClearLogsAsync();

            AdminStatus? status = await _client.GetStatusAsync();
            if (status is null)
            {
                throw new HttpRequestException("The server returned no status data.");
            }

            IReadOnlyList<AdminLogEntry> logs = await _client.GetLogsAsync();
            UpdateStatus(status);
            _detailText.Text =
                "Runtime logs were cleared successfully." +
                Environment.NewLine +
                Environment.NewLine +
                FormatLogList(logs);
        });
    }

    private async Task RunRequestAsync(Func<Task> request)
    {
        try
        {
            await request();
        }
        catch (HttpRequestException exception) when (
            exception.StatusCode is System.Net.HttpStatusCode.Unauthorized or
            System.Net.HttpStatusCode.Forbidden)
        {
            _detailText.Text =
                "Admin token required or invalid. Enter the configured token and select Apply token.";
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

    private static TextBox CreateInput(string watermark)
    {
        return new TextBox
        {
            PlaceholderText = watermark
        };
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private void UpdateStatus(AdminStatus status)
    {
        _connectionText.Text = "Connected";
        _applicationText.Text = $"{status.ApplicationName} {status.ApplicationVersion}".Trim();
        _engineStateText.Text = status.State ?? "Unknown";
        _pluginCountText.Text = status.PluginCount.ToString();
        _parameterCountText.Text = status.ParameterCount.ToString();
        _logCountText.Text = status.RuntimeLogCount.ToString();
    }

    private static string FormatParameterList(IReadOnlyList<AdminParameter> parameters)
    {
        return parameters.Count == 0
            ? "No parameters reported."
            : $"Parameters ({parameters.Count}){Environment.NewLine}{Environment.NewLine}" +
              string.Join(
                  Environment.NewLine,
                  parameters.Select(FormatParameter));
    }

    private static string FormatLogList(IReadOnlyList<AdminLogEntry> logs)
    {
        return logs.Count == 0
            ? "No runtime logs reported."
            : $"Runtime logs ({logs.Count}){Environment.NewLine}{Environment.NewLine}" +
              string.Join(
                  Environment.NewLine,
                  logs.Select(FormatLogEntry));
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

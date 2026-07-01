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
    private IAdminApiClient _client;
    private HttpClient _httpClient;
    private readonly TextBlock _connectionText;
    private readonly TextBlock _applicationText;
    private readonly TextBlock _engineStateText;
    private readonly TextBlock _pluginCountText;
    private readonly TextBlock _parameterCountText;
    private readonly TextBlock _logCountText;
    private readonly TextBlock _detailText;
    private readonly TextBlock _adminTokenStatusText;
    private readonly TextBlock _staticRootStatusText;
    private readonly TextBox _serverUrlTextBox;
    private readonly TextBox _adminTokenTextBox;
    private readonly TextBox _parameterKeyTextBox;
    private readonly TextBox _parameterValueTextBox;
    private readonly TextBox _parameterDescriptionTextBox;
    private readonly TextBox _parameterCategoryTextBox;
    private readonly TextBox _parameterPersistencePathTextBox;
    private readonly TextBox _staticRootTextBox;

    public MainWindow(Uri serverUri)
    {
        _httpClient = CreateHttpClient(serverUri);
        _client = new AdminApiClient(_httpClient);

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
        _staticRootStatusText = CreateValue("Using default sample root");
        _serverUrlTextBox = CreateInput("Server URL");
        _serverUrlTextBox.Text = serverUri.ToString();
        _adminTokenTextBox = CreateInput("Admin token");
        _adminTokenTextBox.PasswordChar = '*';
        _parameterKeyTextBox = CreateInput("Parameter key");
        _parameterValueTextBox = CreateInput("Parameter value");
        _parameterDescriptionTextBox = CreateInput("Description (optional)");
        _parameterCategoryTextBox = CreateInput("Category (optional)");
        _parameterPersistencePathTextBox = CreateInput("Parameter file path");
        _parameterPersistencePathTextBox.Text = "parameters.json";
        _staticRootTextBox = CreateInput("Static website root path");
        _detailText = new TextBlock
        {
            Text = "Select Refresh to query the server.",
            TextWrapping = TextWrapping.Wrap
        };

        Content = new ScrollViewer
        {
            Content = CreateLayout(),
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        Opened += async (_, _) => await RefreshStatusAsync();
        Closed += (_, _) => _httpClient.Dispose();
    }

    private Control CreateLayout()
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

        root.Children.Add(CreateActivityPanel());

        root.Children.Add(CreateSection(
            "Connection",
            _serverUrlTextBox,
            CreateButtonRow(CreateButton("Connect", ApplyServerAsync)),
            CreateRow("Connection", _connectionText),
            CreateRow("Token status", _adminTokenStatusText),
            _adminTokenTextBox,
            CreateButtonRow(CreateButton("Apply token", ApplyAdminTokenAsync))));

        root.Children.Add(CreateSection(
            "Status",
            CreateRow("Application", _applicationText),
            CreateRow("Engine state", _engineStateText),
            CreateRow("Plugins", _pluginCountText),
            CreateRow("Parameters", _parameterCountText),
            CreateRow("Runtime logs", _logCountText),
            CreateButtonRow(CreateButton("Refresh", RefreshStatusAsync))));

        root.Children.Add(CreateSection(
            "Website Hosting",
            CreateRow("Current root", _staticRootStatusText),
            _staticRootTextBox,
            CreateButtonRow(CreateButton("Set static root", SetStaticRootAsync))));

        root.Children.Add(CreateSection(
            "Parameters",
            _parameterKeyTextBox,
            _parameterValueTextBox,
            _parameterDescriptionTextBox,
            _parameterCategoryTextBox,
            CreateButtonRow(
                CreateButton("Set parameter", SetParameterAsync),
                CreateButton("Delete parameter", DeleteParameterAsync),
                CreateButton("View parameters", ShowParametersAsync),
                CreateButton("Refresh parameters", ShowParametersAsync)),
            new TextBlock
            {
                Text = "Persistence",
                FontWeight = FontWeight.SemiBold
            },
            new TextBlock
            {
                Text = "The sample server loads parameters.json beside the server executable during startup. Startup-only settings such as web.static.root may require saving parameters and restarting the server.",
                TextWrapping = TextWrapping.Wrap
            },
            _parameterPersistencePathTextBox,
            CreateButtonRow(
                CreateButton("Save parameters", SaveParametersAsync),
                CreateButton("Load parameters", LoadParametersAsync))));

        root.Children.Add(CreateSection(
            "Logs",
            CreateButtonRow(
                CreateButton("View logs", ShowLogsAsync),
                CreateButton("Refresh logs", ShowLogsAsync),
                CreateButton("Clear logs", ClearLogsAsync))));

        root.Children.Add(CreateSection(
            "Plugins",
            CreateButtonRow(CreateButton("View plugins", ShowPluginsAsync))));

        root.Children.Add(CreateSection(
            "Routes",
            CreateButtonRow(CreateButton("View routes", ShowRoutesAsync))));

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

            IReadOnlyList<AdminParameter> parameters =
                await _client.GetParametersAsync();
            UpdateStatus(status);
            UpdateStaticRootStatus(parameters);
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
            UpdateStaticRootStatus(parameters);
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

    private async Task ApplyServerAsync()
    {
        string value = _serverUrlTextBox.Text?.Trim() ?? string.Empty;
        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? serverUri) ||
            (serverUri.Scheme != Uri.UriSchemeHttp &&
             serverUri.Scheme != Uri.UriSchemeHttps))
        {
            _detailText.Text =
                "Enter a valid absolute HTTP or HTTPS server URL.";
            return;
        }

        string? adminToken = _client.AdminToken;
        HttpClient previousHttpClient = _httpClient;
        _httpClient = CreateHttpClient(serverUri);
        _client = new AdminApiClient(_httpClient)
        {
            AdminToken = adminToken
        };
        previousHttpClient.Dispose();

        _serverUrlTextBox.Text = serverUri.ToString();
        ResetStatus();

        await RunRequestAsync(async () =>
        {
            _connectionText.Text = "Connecting...";
            AdminStatus? status = await _client.GetStatusAsync();
            if (status is null)
            {
                throw new HttpRequestException("The server returned no status data.");
            }

            IReadOnlyList<AdminParameter> parameters =
                await _client.GetParametersAsync();
            UpdateStatus(status);
            UpdateStaticRootStatus(parameters);
            _detailText.Text = $"Connected successfully to {serverUri}.";
        });
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

    private async Task SetStaticRootAsync()
    {
        string path = _staticRootTextBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            _detailText.Text = "A static website root path is required.";
            return;
        }

        await RunRequestAsync(async () =>
        {
            await _client.SetParameterAsync(
                "web.static.root",
                path,
                "Static website root directory",
                "Web");

            _staticRootStatusText.Text = path;
            _detailText.Text =
                "Static website root was set successfully. The sample server applies this setting when static hosting next starts.";
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
            UpdateStaticRootStatus(parameters);
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

    private Control CreateActivityPanel()
    {
        StackPanel content = new()
        {
            Spacing = 8
        };
        content.Children.Add(new TextBlock
        {
            Text = "Activity",
            FontSize = 18,
            FontWeight = FontWeight.SemiBold
        });
        content.Children.Add(_detailText);

        return new Border
        {
            BorderBrush = Brushes.SteelBlue,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            MinHeight = 90,
            Child = content
        };
    }

    private static Control CreateSection(
        string title,
        params Control[] controls)
    {
        StackPanel content = new()
        {
            Spacing = 10
        };
        content.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 18,
            FontWeight = FontWeight.SemiBold
        });

        foreach (Control control in controls)
        {
            content.Children.Add(control);
        }

        return new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14),
            Child = content
        };
    }

    private static Control CreateButtonRow(params Button[] buttons)
    {
        WrapPanel row = new()
        {
            Orientation = Orientation.Horizontal
        };

        foreach (Button button in buttons)
        {
            row.Children.Add(button);
        }

        return row;
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

    private static HttpClient CreateHttpClient(Uri serverUri)
    {
        return new HttpClient
        {
            BaseAddress = serverUri
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

    private void ResetStatus()
    {
        _connectionText.Text = "Not connected";
        _applicationText.Text = "-";
        _engineStateText.Text = "-";
        _pluginCountText.Text = "-";
        _parameterCountText.Text = "-";
        _logCountText.Text = "-";
        _staticRootStatusText.Text = "Unknown";
    }

    private void UpdateStaticRootStatus(IReadOnlyList<AdminParameter> parameters)
    {
        AdminParameter? staticRoot = parameters.FirstOrDefault(parameter =>
            StringComparer.Ordinal.Equals(
                parameter.Key,
                "web.static.root"));

        if (staticRoot is null ||
            staticRoot.Value.ValueKind == JsonValueKind.Null ||
            string.IsNullOrWhiteSpace(staticRoot.Value.ToString()))
        {
            _staticRootStatusText.Text = "Using default sample root";
            return;
        }

        _staticRootStatusText.Text = staticRoot.Value.ValueKind == JsonValueKind.String
            ? staticRoot.Value.GetString() ?? "Using default sample root"
            : staticRoot.Value.ToString();
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

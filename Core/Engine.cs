using KeyEngine.Commands;
using KeyEngine.Configuration;
using KeyEngine.Diagnostics;
using KeyEngine.Events;
using KeyEngine.Events.Models;
using KeyEngine.IO;
using KeyEngine.Input;
using KeyEngine.Logging;
using KeyEngine.Metadata;
using KeyEngine.Networking;
using KeyEngine.Parameters;
using KeyEngine.Plugins;
using KeyEngine.Reflection;
using KeyEngine.Resources;
using KeyEngine.Scheduler;
using KeyEngine.Serialization;
using KeyEngine.Services;
using KeyEngine.Systems;
using KeyEngine.Timers;
using System.Reflection;

namespace KeyEngine.Core;

/// <summary>
/// Represents the core runtime of KeyEngine.
/// </summary>
/// <remarks>
/// The <see cref="Engine"/> is responsible for managing the engine lifecycle,
/// including initialization, execution, and shutdown. Engine instances should
/// be created through the <see cref="EngineBuilder"/>.
/// </remarks>
public sealed class Engine
{
    private readonly TypeScanner _typeScanner = new();
    private readonly SystemRegistry _systemRegistry = new();
    private readonly ServiceCollection _services = new();
    private IServiceResolver? _serviceProvider;
    private readonly ScanResult _scanResult = new();
    private readonly EngineOptions _options;
    private readonly Scheduler.Scheduler _scheduler;
    private readonly EventBus _eventBus;
    private readonly CommandManager _commandManager;
    private readonly PluginManager _pluginManager;
    private readonly TimerManager _timerManager;
    private readonly ResourceManager _resourceManager;
    private readonly ISerializer _serializer;
    private readonly NetworkManager _networkManager;
    private readonly InputManager _inputManager;
    private readonly ParameterManager _parameterManager;
    private readonly PluginManifestLoader _manifestLoader = new();
    private readonly PluginContextFactory _contextFactory = new();

    /// <summary>
    /// Gets the current execution state of the engine.
    /// </summary>
    public EngineState State { get; private set; } = EngineState.Stopped;

    /// <summary>
    /// Gets the engine event bus.
    /// </summary>
    public EventBus Events => _eventBus;

    /// <summary>
    /// Gets the engine command manager.
    /// </summary>
    public CommandManager Commands => _commandManager;

    /// <summary>
    /// Gets the engine resource manager.
    /// </summary>
    public ResourceManager Resources => _resourceManager;

    /// <summary>
    /// Gets the engine serializer.
    /// </summary>
    public ISerializer Serializer => _serializer;

    /// <summary>
    /// Gets the engine network manager.
    /// </summary>
    public NetworkManager Networking => _networkManager;

    /// <summary>
    /// Gets the engine input manager.
    /// </summary>
    public InputManager Input => _inputManager;

    /// <summary>
    /// Gets the engine runtime parameter manager.
    /// </summary>
    public ParameterManager Parameters => _parameterManager;

    public EngineDiagnostics Diagnostics { get; }

    /// <summary>
    /// Gets the engine metadata.
    /// </summary>
    public ApplicationInfo Info => _options.Info;

    internal Scheduler.Scheduler Scheduler =>
    _scheduler;

    internal PluginManager PluginManager =>
        _pluginManager;

    internal ScanResult ScanResult =>
        _scanResult;

    internal TimerManager TimerManager =>
        _timerManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="Engine"/> class.
    /// </summary>
    /// <param name="options">
    /// The engine configuration.
    /// </param>
    internal Engine(EngineOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;

        _eventBus = new EventBus(_systemRegistry);

        _pluginManager = new PluginManager(_eventBus);

        _scheduler = new Scheduler.Scheduler(options.Scheduler);

        _commandManager = new CommandManager(
            _systemRegistry,
            _eventBus);

        _timerManager = new TimerManager();

        _resourceManager = new ResourceManager();

        _serializer = new JsonSerializerAdapter();

        _networkManager = new NetworkManager();

        _inputManager = new InputManager();

        _parameterManager = new ParameterManager(_eventBus);

        Diagnostics = new EngineDiagnostics(this);

        _services.AddSingleton<Engine>(this);

        _services.AddSingleton(_eventBus);

        _services.AddSingleton(_commandManager);
        _services.AddSingleton(_timerManager);
        _services.AddSingleton(_resourceManager);
        _services.AddSingleton(_serializer);
        _services.AddSingleton(_networkManager);
        _services.AddSingleton(_inputManager);
        _services.AddSingleton(_parameterManager);
        _services.AddSingleton<IFileSystem, PhysicalFileSystem>();
    }

    /// <summary>
    /// Starts the engine lifecycle.
    /// </summary>
    public void Run()
    {
        try
        {
            Initialize();

            while (State == EngineState.Running)
            {
                Tick();
            }
        }
        finally
        {
            Shutdown();
        }
    }

    /// <summary>
    /// Initializes the engine and its registered systems.
    /// </summary>
    public void Initialize()
    {
        if (State != EngineState.Stopped)
        {
            throw new InvalidOperationException(
                "The engine has already been initialized.");
        }

        State = EngineState.Initializing;

        Log.Info("Initializing...");

        _pluginManager.Load(
            _options.PluginDirectory);

        foreach (LoadedPlugin plugin in
            _pluginManager.Plugins)
        {
            Log.Info(
                $"Loaded plugin '{plugin.Manifest.Name}' v{plugin.Manifest.Version}");

            PluginBuilder builder =
                _pluginManager.GetBuilder(plugin);

            foreach (Type system in builder.Systems)
            {
                Log.Info(
                    $"Plugin '{plugin.Instance.GetType().Name}' registered system '{system.Name}'.");

                ScanResult result =
                    _typeScanner.Scan(system);

                builder.ScanResult.AddRange(result);
                _scanResult.AddRange(result);
            }


            foreach (ServiceDescriptor service
                in builder.Services.Services)
            {
                _services.Add(service);
            }
        }

        foreach (Assembly assembly in _options.Assemblies)
        {
            ScanResult result =
                _typeScanner.Scan(assembly);

            _scanResult.AddRange(result);

            _services.AddSingletonRange(
                result.Systems);
        }

        foreach (EventListenerMetadata listener in _scanResult.EventListeners)
        {
            _eventBus.Register(listener);
        }

        foreach (CommandMetadata command in _scanResult.Commands)
        {
            _commandManager.Register(command);
        }

        _serviceProvider = _services.Build();

        _systemRegistry.SetServices(
            _serviceProvider);

        InvokeMethods(MethodKind.Startup);

        _scheduler.Start();

        State = EngineState.Running;

        Log.Info("Running...");
    }

    /// <summary>
    /// Executes the engine's primary runtime loop.
    /// </summary>
    public void Tick()
    {
        if (State != EngineState.Running)
        {
            throw new InvalidOperationException(
                "The engine must be running before Tick() can be called.");
        }

        _scheduler.BeginFrame();

        _inputManager.Update();

        _timerManager.Update(
            _scheduler.DeltaTime);

        InvokeMethods(MethodKind.Update);

        while (_scheduler.ShouldRunFixedUpdate())
        {
            InvokeMethods(MethodKind.FixedUpdate);
        }

        _scheduler.EndFrame();
    }

    /// <summary>
    /// Requests that the engine stop running.
    /// </summary>
    public void Stop()
    {
        if (State == EngineState.Running)
        {
            State = EngineState.ShuttingDown;
        }
    }

    /// <summary>
    /// Shuts down the engine and performs cleanup.
    /// </summary>
    public void Shutdown()
    {
        if (State == EngineState.Stopped)
        {
            return;
        }

        State = EngineState.ShuttingDown;

        Log.Info("Shutting down...");

        try
        {
            InvokeMethods(MethodKind.Shutdown);

            _networkManager.CloseAll();
        }
        finally
        {
            _resourceManager.Dispose();
        }

        State = EngineState.Stopped;
    }

    /// <summary>
    /// Invokes all methods of the specified kind.
    /// </summary>
    /// <param name="kind">
    /// The method kind.
    /// </param>
    private void InvokeMethods(MethodKind kind)
    {
        foreach (MethodMetadata method in _scanResult.GetMethods(kind))
        {
            object instance =
                _systemRegistry.GetOrCreate(method.DeclaringType);

            if (method.ParameterTypes.Count == 0)
            {
                method.Invoker.Invoke(instance);
                continue;
            }

            object? argument = CreateArgument(method);

            method.Invoker.Invoke(instance, argument);
        }
    }

    /// <summary>
    /// Creates the argument supplied to an engine method.
    /// </summary>
    /// <param name="method">
    /// The method metadata.
    /// </param>
    /// <returns>
    /// The created argument.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the parameter type is not supported.
    /// </exception>
    private object CreateArgument(MethodMetadata method)
    {
        if (method.ParameterTypes.Count == 1 &&
            method.ParameterTypes[0] == typeof(UpdateContext))
        {
            return new UpdateContext
            {
                FrameNumber = _scheduler.FrameNumber,
                DeltaTime = _scheduler.DeltaTime,
                ElapsedTime = _scheduler.ElapsedTime
            };
        }

        throw new NotSupportedException(
            $"Unsupported parameter signature.");
    }
}

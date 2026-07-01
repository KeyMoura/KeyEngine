using KeyEngine.Abstractions;
using KeyEngine.Annotations;
using KeyEngine.Core;
using KeyEngine.Parameters;
using KeyEngine.Parameters.Events;

namespace KeyEngine.Tests.Parameters;

public sealed class ParameterManagerTests
{
    [Fact]
    public void SetAndGet_ReturnTypedValueAndMetadata()
    {
        ParameterManager parameters = TestEngineFactory.Create().Parameters;

        parameters.Set(
            "server.port",
            5000,
            "HTTP port",
            "Server");

        Assert.Equal(5000, parameters.Get<int>("server.port"));

        Parameter parameter = Assert.Single(parameters.GetAll());
        Assert.Equal("server.port", parameter.Key);
        Assert.Equal(5000, parameter.Value);
        Assert.Equal(typeof(int), parameter.ValueType);
        Assert.Equal("HTTP port", parameter.Description);
        Assert.Equal("Server", parameter.Category);
        Assert.False(parameter.IsReadOnly);
    }

    [Fact]
    public void TryGet_MissingValue_ReturnsFalse()
    {
        ParameterManager parameters = TestEngineFactory.Create().Parameters;

        bool found = parameters.TryGet<int>(
            "missing",
            out int value);

        Assert.False(found);
        Assert.Equal(0, value);
    }

    [Fact]
    public void ContainsAndRemove_ReflectRegistration()
    {
        ParameterManager parameters = TestEngineFactory.Create().Parameters;
        parameters.Set("server.port", 5000);

        Assert.True(parameters.Contains("server.port"));
        Assert.True(parameters.Remove("server.port"));
        Assert.False(parameters.Contains("server.port"));
        Assert.False(parameters.Remove("server.port"));
    }

    [Fact]
    public void Set_BlankKey_ThrowsArgumentException()
    {
        ParameterManager parameters = TestEngineFactory.Create().Parameters;

        Assert.Throws<ArgumentException>(() =>
            parameters.Set(" ", 5000));
    }

    [Fact]
    public void Set_ReadOnlyParameter_ThrowsClearException()
    {
        ParameterManager parameters = TestEngineFactory.Create().Parameters;
        parameters.Set(
            "server.port",
            5000,
            isReadOnly: true);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => parameters.Set("server.port", 6000));

        Assert.Contains("server.port", exception.Message);
        Assert.Contains("read-only", exception.Message);
    }

    [Fact]
    public void Remove_ReadOnlyParameter_ThrowsClearException()
    {
        ParameterManager parameters = TestEngineFactory.Create().Parameters;
        parameters.Set(
            "server.port",
            5000,
            isReadOnly: true);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => parameters.Remove("server.port"));

        Assert.Contains("server.port", exception.Message);
        Assert.Contains("read-only", exception.Message);
    }

    [Fact]
    public void Set_NewAndChangedValue_PublishesParameterChangedEvents()
    {
        ParameterChangedListener.Reset();
        Engine engine = TestEngineFactory.Create();

        try
        {
            engine.Initialize();
            engine.Parameters.Set("server.port", 5000);
            engine.Parameters.Set("server.port", 6000);

            Assert.Collection(
                ParameterChangedListener.Events,
                created =>
                {
                    Assert.Equal("server.port", created.Key);
                    Assert.Null(created.OldValue);
                    Assert.Equal(5000, created.NewValue);
                },
                changed =>
                {
                    Assert.Equal("server.port", changed.Key);
                    Assert.Equal(5000, changed.OldValue);
                    Assert.Equal(6000, changed.NewValue);
                });
        }
        finally
        {
            engine.Shutdown();
        }
    }

    [Fact]
    public void Engine_RegistersParameterManagerAsSameSingletonInDependencyInjection()
    {
        ParameterAccessSystem.Reset();
        Engine engine = TestEngineFactory.Create();

        try
        {
            engine.Initialize();

            Assert.Same(
                engine.Parameters,
                ParameterAccessSystem.Parameters);
        }
        finally
        {
            engine.Shutdown();
        }
    }
}

internal sealed class ParameterChangedListener
    : IEngineSystem
{
    public static List<ParameterChangedEvent> Events { get; } = [];

    public static void Reset()
    {
        Events.Clear();
    }

    [EventListener]
    private void OnParameterChanged(ParameterChangedEvent @event)
    {
        Events.Add(@event);
    }
}

internal sealed class ParameterAccessSystem
    : IEngineSystem
{
    private readonly ParameterManager _parameters;

    public static ParameterManager? Parameters { get; private set; }

    public ParameterAccessSystem(ParameterManager parameters)
    {
        _parameters = parameters;
    }

    public static void Reset()
    {
        Parameters = null;
    }

    [OnStart]
    private void Start()
    {
        Parameters = _parameters;
    }
}

# KeyEngine Architecture

## Overview

KeyEngine is a lightweight, plugin-driven runtime host. Systems are the main
execution unit: the engine discovers system types, builds their metadata,
resolves their instances through dependency injection, and invokes lifecycle,
command, and event methods through cached invokers.

The high-level flow is:

```text
EngineBuilder
  -> EngineOptions
  -> Engine.Initialize
       -> load and configure plugins
       -> scan registered systems and assemblies
       -> register commands and event listeners
       -> build the service resolver
       -> invoke startup methods
  -> Engine.Tick
       -> update input sources and aggregate input state
       -> update timers
       -> invoke update methods
       -> invoke due fixed updates
  -> Engine.Shutdown
       -> invoke shutdown methods
```

## Engine lifecycle

`EngineBuilder` collects assemblies, scheduler options, application metadata,
and the plugin directory. `Build` creates the engine and its core services.

`Initialize` performs discovery and registration, builds the service resolver,
invokes startup methods, starts the scheduler, and transitions the engine to
running. `Tick` advances one frame. `Stop` requests graceful shutdown by moving
the engine into its shutting-down state. `Shutdown` invokes shutdown methods and
marks the engine stopped.

Lifecycle methods are declared with `OnStart`, `OnUpdate`, `OnFixedUpdate`, and
`OnShutdown`. Supported methods return `void` and accept either no parameters or
one `UpdateContext` parameter.

## Plugin model

Disk-based plugins implement `IPlugin` and provide an embedded `plugin.json`
manifest. The plugin loader discovers assemblies in the configured plugin
directory, reads manifests, constructs plugin contexts, and invokes each
plugin's `Configure` method.

`IPluginBuilder` is the plugin registration boundary. Plugins can register
systems, configuration objects, and singleton or transient services. Plugin
contexts provide manifest information and plugin-specific configuration, data,
cache, and log locations.

Plugin loading currently occurs for the lifetime of the process. Hot reload,
unloading, and dependency ordering are not implemented.

## System discovery

Systems implement `IEngineSystem`. `TypeScanner` examines registered system
types for lifecycle, command, and event-listener attributes. It validates
supported signatures and produces cached metadata and method invokers.

The engine scans explicitly registered application assemblies and systems
declared by plugins. Discovery occurs during initialization, not on every frame.

## Dependency injection

KeyEngine includes a small constructor-injection container. Services are
registered as singleton instances, singleton types, or transient types. The
resolver chooses a public constructor and recursively resolves its parameters.

Engine systems are resolved through the same service infrastructure, which
ensures registered singleton systems are shared by lifecycle, command, and
event dispatch. Core services include the engine, event bus, command manager,
timer manager, resource manager, serializer, network manager, input manager,
and filesystem implementation.

The container is intentionally limited and is not intended to reproduce a
full-featured external DI framework.

## Commands and events

Commands are methods marked with `CommandAttribute`. Discovery records names,
aliases, descriptions, usage, categories, and signatures. The current text
command contract supports methods with no parameters or one `string` parameter.
`CommandManager` resolves overloads and invokes methods on DI-resolved systems.
Command invocation publishes cancellable before-events and completion events.

Events implement `IEvent`. Listener methods use `EventListenerAttribute` and
accept one event parameter. `EventBus` dispatches listeners by exact event type,
priority, and explicit order. For `CancellableEvent` instances, listeners with
`IgnoreCancelled` are skipped after cancellation.

Commands and events currently use synchronous dispatch.

## Supporting subsystems

- **Scheduler:** tracks frame time, elapsed time, and fixed-update accumulation.
- **Timers:** tracks one-shot and repeating timers advanced by engine ticks.
- **Configuration:** caches typed configuration objects and persists them through
  `IFileSystem` and `ISerializer`.
- **Resources:** maps resource types to loaders and caches loaded resources by
  type and provider-neutral `ResourceLocation`.
- **Serialization:** converts values to and from text through `ISerializer`; the
  default implementation uses indented JSON.
- **Networking:** manages synchronous TCP clients, servers, and connections
  using built-in .NET socket APIs.
- **Input:** aggregates snapshot state from application-provided keyboard and
  pointer sources once per frame without depending on a windowing platform.
- **Numerics:** contains vector types, interpolation helpers, and two-dimensional
  graph data.
- **Drawing:** currently provides an immutable color value and ANSI text color
  translation; it is not a rendering system.

## Diagnostics philosophy

`EngineDiagnostics` is a read-only façade over authoritative engine state. It
reports lifecycle, scheduler, registration, timer, and plugin information
without exposing `PluginManager` to consumers such as the Console plugin.

Diagnostics should remain derived from subsystem state. KeyEngine should avoid
creating a second mutable diagnostics registry that can drift from the runtime.

## Optional extensions

UI, Audio, Windowing, Rendering, and Physics are future optional packages or
plugins, not required core dependencies. Integrations should feed existing
boundaries, such as input sources and resource loaders, without coupling the
core runtime to a specific platform stack.

## Future editor and exported runtime

A future game editor may be built as a KeyEngine application rather than as a
core runtime feature. It would compose editor plugins, game-framework plugins,
runtime plugins, and project plugins through the same model used by other
KeyEngine hosts.

An exported game would also be a KeyEngine application. Its build would omit
editor-only plugins and retain only the runtime, game-framework, and project
plugins required by the game. This keeps editor execution and exported runtime
execution on the same lifecycle, dependency-injection, and plugin foundations.

This model is future planning only. User-created game plugins should eventually
have clear packaging and distribution conventions, while UI, Audio, Windowing,
Rendering, Physics, and game-specific systems remain outside the core package.

## Testing

The initial xUnit contract suite exercises engine lifecycle transitions, event
cancellation, resource location validation, resource dispatch and caching,
multi-source input behavior, and timer completion cleanup. It is a focused
alpha baseline rather than comprehensive production-readiness coverage.

## Design principles

- Prefer explicit registration and APIs over hidden behavior.
- Keep the engine responsible for sequencing, not every subsystem detail.
- Cache reflection metadata during initialization.
- Resolve one system instance consistently across dispatch paths.
- Keep public contracts small and implementation types internal where possible.
- Preserve synchronous, deterministic behavior until concurrency is designed
  explicitly.
- Add abstractions only at real ownership or integration boundaries.
- Favor clear failure messages during startup over deferred runtime failures.

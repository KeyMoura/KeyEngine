# KeyEngine Roadmap

## Project vision

KeyEngine is a lightweight, general-purpose, plugin-driven runtime framework
for .NET applications. It provides a small host for composing systems,
services, plugins, commands, events, scheduling, and supporting infrastructure
without assuming a specific application type. Intended hosts include servers,
desktop tools, simulations, games, and other modular software.

The project is under active development. Until 1.0, public APIs may change as
runtime contracts are clarified and tested.

## Completed foundations

The repository currently contains initial implementations of:

- Engine construction, initialization, ticking, stopping, and shutdown.
- Reflection-based system and lifecycle-method discovery.
- Plugin discovery from assemblies with embedded manifests.
- Plugin system and service registration through `IPluginBuilder`.
- A lightweight dependency-injection container with singleton and transient
  registrations.
- Attribute-discovered commands with parsing, aliases, and invocation events.
- Typed events with listener priority, ordering, and cancellation behavior.
- Variable and fixed update scheduling.
- One-shot and repeating timers.
- Typed JSON-backed plugin configuration.
- Provider-neutral resource locations, typed loaders, handles, and caching.
- A text serialization abstraction with a `System.Text.Json` adapter.
- Synchronous TCP clients, servers, and managed connections.
- Multi-source keyboard and pointer input state without a platform dependency.
- Engine and per-plugin diagnostics.
- Basic numerics, graph interpolation, color values, and ANSI color translation.
- A separate Console plugin with engine and plugin diagnostic commands.
- Runtime parameters, change events, and JSON file persistence.
- A small HTTP server with exact and parameterized routes, route discovery,
  static file hosting, and path traversal protection.
- A development/admin API for runtime status, plugins, parameters, logs, and
  route metadata. Mutation routes support a minimal admin token check.
- `KeyEngine.AdminClient` as the reusable HTTP API boundary.
- `KeyEngine.AdminApp` as an interactive console admin shell.
- `KeyEngine.AdminDashboard` as an early Avalonia desktop client for viewing
  runtime data, editing and persisting parameters, configuring the sample
  static root, and supplying an in-memory admin token.
- A bounded in-memory runtime activity log exposed through the admin API.
- An initial xUnit contract suite covering lifecycle, event cancellation,
  resources, input aggregation, timers, plugins, parameters, and web behavior.

These are foundations, not a declaration that every subsystem is feature
complete or API-stable.

## Pre-1.0 priorities

1. Define and test engine lifecycle, initialization-failure, and ownership
   contracts.
2. Expand automated unit and integration coverage as runtime contracts stabilize.
3. Stabilize plugin packaging, directory layout, dependency handling, and load
   failure behavior.
4. Finalize supported command signatures and document parsing rules.
5. Define thread-affinity expectations for commands, events, timers, services,
   and diagnostics.
6. Clarify timer cleanup and scheduler catch-up behavior.
7. Review the public API and internalize implementation-only metadata,
   reflection, validation, and invocation types.
8. Complete user documentation and establish public API compatibility checks.

## Planned future subsystems

The following areas are planned directions and are not currently implemented:

- Asynchronous resource loading and provider-specific resource sources.
- Resource lifetime policies and hot reloading.
- Plugin assembly isolation and version constraints beyond current dependency
  and ordering-hint support.
- Production hardening for the HTTP/admin foundations, including an intentional
  authentication and transport-security design.
- Structured logging integration suitable for non-console hosts.
- Deterministic time sources and expanded scheduling policies.
- Broader rendering-neutral drawing primitives, if required by real consumers.
- Additional serialization formats only when justified by concrete use cases.
- Optional UI, Audio, Windowing, Rendering, and Physics packages or plugins.
  These are not planned as required core dependencies.

Future work should extend existing subsystem boundaries rather than introducing
parallel managers or duplicate registration models.

## Future game editor and runtime model

KeyEngine may eventually serve as the foundation for a separate game-engine
and editor product. This direction is not implemented and is not a pre-1.0
commitment for the core runtime.

The editor would be a KeyEngine application that loads editor,
game-framework, runtime, and project plugins. An exported game would use the
same KeyEngine application model while excluding editor-only plugins and
shipping only the runtime, game-framework, and project plugins it needs. Game
plugins should be straightforward for users to create, package, and distribute.

UI, Audio, Windowing, Rendering, Physics, and game-specific systems would remain
optional packages or plugins rather than required core dependencies.

## Non-goals for now

- Making UI, Audio, Windowing, Rendering, or Physics mandatory core systems.
- Building a full game engine, scene graph, or editor before the runtime and
  plugin contracts are stable.
- Built-in image, texture, font, audio, or media loaders.
- YAML, XML, binary, or network serialization formats.
- Distributed dependency injection or a general application container.
- Transparent multithreading across every runtime subsystem.
- Speculative abstractions without an existing framework use case.

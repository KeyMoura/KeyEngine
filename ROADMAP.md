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
- An initial xUnit contract suite covering lifecycle, event cancellation,
  resources, input aggregation, and timer cleanup.

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
- Plugin dependency ordering and optional assembly isolation.
- Structured logging integration suitable for non-console hosts.
- Deterministic time sources and expanded scheduling policies.
- Broader rendering-neutral drawing primitives, if required by real consumers.
- Additional serialization formats only when justified by concrete use cases.
- Optional UI, Audio, Windowing, Rendering, and Physics packages or plugins.
  These are not planned as required core dependencies.

Future work should extend existing subsystem boundaries rather than introducing
parallel managers or duplicate registration models.

## Non-goals for now

- Making UI, Audio, Windowing, Rendering, or Physics mandatory core systems.
- A full game engine, scene graph, or editor.
- Built-in image, texture, font, audio, or media loaders.
- YAML, XML, binary, or network serialization formats.
- Distributed dependency injection or a general application container.
- Transparent multithreading across every runtime subsystem.
- Speculative abstractions without an existing framework use case.

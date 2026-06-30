# KeyEngine

KeyEngine is a lightweight, plugin-driven runtime framework for .NET
applications. It provides a small synchronous host for modular servers, tools,
simulations, desktop applications, games, and other plugin-based software.

## Current foundation

The v0.1 alpha foundation includes:

- Engine lifecycle and frame scheduling
- Assembly and plugin loading
- Dependency injection
- Commands and events
- Scheduler and timers
- Configuration and JSON serialization
- Typed resources and loader dispatch
- Synchronous TCP networking
- Provider-neutral input state
- Numerics and drawing foundations
- Engine and plugin diagnostics

Optional systems such as UI, Audio, Windowing, Rendering, and Physics are
future packages or plugins. They are not required core dependencies.

## Status

KeyEngine is an early alpha under active development. APIs and runtime
contracts may change before 1.0, and the framework should not yet be considered
production-ready.

The initial xUnit contract suite covers core lifecycle transitions, event
cancellation, resource dispatch and caching, multi-source input behavior, and
timer cleanup. Coverage is intentionally focused and will expand with stable
contracts.

## License

MIT

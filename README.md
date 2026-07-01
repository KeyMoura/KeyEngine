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
- Runtime parameters with JSON file persistence
- Simple HTTP routing and static file hosting through KeyEngine.Web
- Admin API status, diagnostics, parameter, log, and route endpoints
- Reusable AdminClient, console admin shell, and early Avalonia dashboard
- Provider-neutral input state
- Numerics and drawing foundations
- Engine and plugin diagnostics

Optional systems such as UI, Audio, Windowing, Rendering, and Physics are
future packages or plugins. They are not required core dependencies.

## Status

KeyEngine is an early alpha under active development. APIs and runtime
contracts may change before 1.0, and the framework should not yet be considered
production-ready.

The xUnit contract suite covers core lifecycle transitions, event cancellation,
resource dispatch and caching, multi-source input behavior, timer cleanup,
plugin ordering, parameters, web/admin routes, and static file security.
Coverage remains an alpha baseline and is not comprehensive.

## License

MIT

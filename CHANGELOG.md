# Changelog

## v0.1 alpha

- Established the engine lifecycle, scheduler, systems, plugins, and dependency
  injection foundations.
- Added synchronous commands, events, timers, configuration, and diagnostics.
- Added JSON serialization and typed resource loading with caching.
- Added synchronous TCP networking and provider-neutral keyboard and pointer
  input foundations.
- Added runtime parameters, change events, and explicit JSON persistence.
- Added KeyEngine.Web routing, route metadata, static file hosting, and a
  development/admin API with token-protected mutation routes.
- Added bounded runtime logs exposed through the admin API.
- Added the reusable KeyEngine.AdminClient HTTP boundary, KeyEngine.AdminApp
  console shell, and an early Avalonia KeyEngine.AdminDashboard.
- Added dashboard support for runtime inspection, parameter editing and
  persistence, static-root configuration, and session-only admin tokens.
- Added optional TestApp startup loading from `parameters.json`.
- Added initial numerics and drawing primitives.
- Added focused xUnit contract coverage for lifecycle, event cancellation,
  resources, input aggregation, timers, plugins, parameters, web routes, and
  static-file security.

KeyEngine remains an early alpha. Public APIs and runtime contracts may change
before 1.0, and production readiness is not implied.

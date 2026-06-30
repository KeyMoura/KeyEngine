# KeyEngine Package Plan

## Purpose

This document describes the intended long-term package and plugin architecture
for KeyEngine. It is a planning direction, not the current physical project
layout. No package split should occur until public contracts and dependency
boundaries are stable enough to justify it.

## Distribution models

### Core package

The core package is the minimum runtime host required by every KeyEngine
application. It owns engine sequencing and the contracts needed to discover,
validate, register, and run systems and plugins. It must remain lightweight and
must not depend on application-domain systems or platform integrations.

### Optional package

An optional package is a compile-time library referenced by applications or
plugins that need a specific framework subsystem. Optional packages expose
reusable contracts and platform-neutral implementations without becoming
required core dependencies.

### Runtime plugin

A runtime plugin is discovered or registered when an application runs. It uses
the plugin lifecycle and dependency-injection boundaries to contribute systems,
services, commands, or integrations. Plugins are appropriate for independently
deployable behavior, not foundational value types such as numerics.

### Provider or implementation package

A provider package supplies a concrete implementation of a platform-neutral
optional package or plugin contract. Applications choose providers explicitly
at compile or deployment time. Provider-specific dependencies must not leak
into the core package.

## Proposed structure

### Core

- `KeyEngine`
  - Engine and lifecycle
  - Systems and plugin contracts
  - Dependency injection and services
  - Scheduler
  - Events
  - Validation
  - Diagnostics foundation

### Likely optional packages

- `KeyEngine.Commands`
- `KeyEngine.Configuration`
- `KeyEngine.Serialization`
- `KeyEngine.Resources`
- `KeyEngine.Input`
- `KeyEngine.Networking`
- `KeyEngine.Numerics`
- `KeyEngine.Drawing`
- `KeyEngine.IO`
- `KeyEngine.Timers`

These packages are referenced at compile time. Numerics and math types belong
in packages rather than runtime plugins because they are foundational APIs used
directly by consumer code and other packages.

### Likely runtime plugins

- `KeyEngine.Console`
- `KeyEngine.Windowing`
- `KeyEngine.Audio`
- `KeyEngine.UI`
- `KeyEngine.Rendering`
- `KeyEngine.Physics`

UI, Audio, Windowing, Rendering, and Physics should remain optional and should
not live in the core engine package. Their runtime plugin form allows hosts to
select only the systems they need.

### Provider packages

- `KeyEngine.Windowing.Sdl`
- `KeyEngine.Windowing.Glfw`
- `KeyEngine.Audio.OpenAL`
- `KeyEngine.Audio.MiniAudio`
- `KeyEngine.Rendering.OpenGL`
- `KeyEngine.Rendering.Vulkan`
- `KeyEngine.Rendering.DirectX`

Provider packages should implement stable contracts from their corresponding
optional package or runtime plugin while containing all provider-specific
dependencies and setup.

## Dependency principles

- Core remains small and depends only on infrastructure required by all hosts.
- Optional packages never become implicit dependencies of core.
- Packages are selected through compile-time references; plugins contribute
  behavior through runtime loading and registration.
- Runtime plugins may depend on core and relevant optional packages.
- Provider packages depend on their abstraction package or plugin, never the
  reverse.
- Cross-package contracts should be explicit and minimal; avoid circular
  dependencies and duplicate managers.
- A subsystem should be split only when ownership, public API, and versioning
  boundaries are clear.

## Current status

KeyEngine currently ships most foundations from the main project. The proposed
package and plugin separation is deferred. It should be implemented
incrementally before 1.0 only where the split reduces coupling without
destabilizing the runtime architecture.

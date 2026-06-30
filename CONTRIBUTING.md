# Contributing to KeyEngine

KeyEngine welcomes focused fixes, documentation improvements, tests, and
well-motivated framework changes. The project is approaching API stabilization,
so changes should prioritize clear contracts and maintainability over breadth.

## Development setup

Requirements:

- A .NET SDK compatible with the target framework in `KeyEngine.csproj`.
- Git.
- An editor or IDE with C# and nullable-reference-type support.

The solution currently references the core library plus sibling Console,
TestApp, and TestPlugin projects. Keep these projects in the expected repository
layout when building the complete solution.

## Building

From the `KeyEngine` repository directory:

```powershell
dotnet restore KeyEngine.slnx
dotnet build KeyEngine.slnx
```

Build the core library alone with:

```powershell
dotnet build KeyEngine.csproj
```

Changes should build without new warnings. Run relevant automated tests when a
test project is available, and manually exercise affected sample/plugin paths
when integration behavior changes.

## Coding style

- Follow the existing folder, namespace, formatting, and naming conventions.
- Prefer correctness, readability, and explicit APIs over cleverness.
- Keep classes and methods focused on one responsibility.
- Reuse existing systems before introducing new managers or abstractions.
- Preserve behavior unless a breaking change is intentional and documented.
- Prefer immutable data and read-only members where practical.
- Validate public inputs and express nullability invariants in the type system.
- Add XML documentation for new public APIs.
- Avoid dead code, speculative placeholders, and comments that restate code.

## Branches and commits

- Create a focused branch for each change. The preferred automated branch prefix
  is `codex/`; human contributors may use a similarly descriptive convention.
- Keep commits limited to one logical change.
- Use concise imperative commit messages, for example:
  `Validate lifecycle method signatures`.
- Do not mix formatting, refactoring, and behavior changes unless they cannot be
  reviewed independently.

## Proposing features

Open a discussion or issue before implementing a substantial subsystem or
public API. Include:

- The concrete application or plugin use case.
- Why existing extension points are insufficient.
- The smallest viable public contract.
- Ownership, lifecycle, failure, and thread-safety expectations.
- Compatibility implications and alternatives considered.

Future roadmap items are not commitments to a particular design. A planned
area should still be justified by current framework needs.

## Reporting bugs

Include:

- KeyEngine version or commit.
- .NET SDK and operating system.
- Minimal reproduction code or repository.
- Expected and actual behavior.
- Full exception details and relevant logs.
- Whether the issue involves the core library, Console plugin, or a custom
  plugin.

Do not include secrets, private configuration, or proprietary plugin binaries.

## Architecture expectations

- `Engine` coordinates lifecycle and subsystem ordering.
- Systems remain the shared unit for lifecycle, command, and event execution.
- Plugins register through `IPluginBuilder`; do not introduce parallel plugin
  registration models.
- Console command modules consume public engine façades such as
  `Engine.Diagnostics`, not internal managers.
- Reflection and validation happen during discovery whenever possible.
- Commands, events, timers, resources, serialization, configuration, and
  diagnostics should retain clear ownership boundaries.
- Production code should not be made public solely to simplify tests.
- New concurrency behavior requires an explicit thread-safety contract.

Breaking changes may be appropriate before 1.0, but they must remove ambiguity
or improve the long-term API rather than shift complexity to consumers.


# Copilot Instructions for SpectreConsoleDemo

## Project Overview
- This repository contains a .NET 10 terminal utility app built with Spectre.Console.
- Main project path: `SpectreConsoleApp/SpectreConsoleApp.csproj`.
- App style: interactive menu-based console workflows.

## Tech Stack
- Language: C#
- Framework: .NET 10 (`net10.0`)
- UI library: Spectre.Console (`Spectre.Console`, `Spectre.Console.Json`)

## Build and Run
- Restore: `dotnet restore SpectreConsoleApp/SpectreConsoleApp.csproj`
- Build: `dotnet build SpectreConsoleApp/SpectreConsoleApp.csproj`
- Run: `dotnet run --project SpectreConsoleApp/SpectreConsoleApp.csproj`
- Prefer VS Code tasks (`build`, `publish`, `watch`) when available.

## Code Organization
- Keep feature code in focused files under `SpectreConsoleApp/`.
- `Program.cs` is responsible for menu wiring and dispatch.
- Add new menu options in one coherent change:
  1. Add the display label in the selection prompt.
  2. Add matching routing logic in the switch.
  3. Implement behavior in a dedicated method or feature class.

## Coding Conventions
- Preserve existing C# style and naming.
- Keep methods small and readable.
- Avoid introducing new dependencies unless clearly needed.
- Use Spectre.Console primitives for output consistency (panels, tables, prompts, progress, status).
- Prefer deterministic, testable logic for data transforms; keep terminal rendering concerns separate when practical.

## Safety and Scope
- Do not remove existing demo features unless explicitly requested.
- Do not make destructive file system operations in utility features without clear confirmation.
- Do not modify `.devcontainer`, CI, or solution structure unless asked.

## When Updating README
- Keep commands aligned with the real project path (`SpectreConsoleApp/SpectreConsoleApp.csproj`).
- Keep descriptions concise and terminal-focused.

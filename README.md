the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
# Spectre Console Demo

Interactive .NET 10 console app that showcases common [Spectre.Console](https://spectreconsole.net/) UI components.

## What this project does

The app starts with a searchable menu and lets you run small demos for:

- Panel & markup
- Table
- Tree
- Progress
- Prompts
- Bar chart
- Calendar
- Status spinner

Each demo renders directly in the terminal and returns to the menu when you press a key.

## Requirements

- .NET SDK 10.0+
- macOS / Linux / Windows terminal

Optional:

- VS Code + C# extension
- Docker + Dev Containers extension (the repo includes a .NET 10 devcontainer)

## Run locally

```bash
dotnet restore
dotnet run --project ./SpectreConsoleApp.csproj
```

## Build and publish

```bash
dotnet build ./SpectreConsoleApp.csproj
dotnet publish ./SpectreConsoleApp.csproj -c Release
```

## Run in VS Code

- Use the `build`, `publish`, and `watch` tasks from `.vscode/tasks.json`.
- Press `F5` with the `.NET Launch (web)` configuration in `.vscode/launch.json`.

## Dev Container

This repository includes `.devcontainer/devcontainer.json` using:

- `mcr.microsoft.com/devcontainers/dotnet:1-10.0`

If you update the devcontainer config, rebuild it via:

- **Dev Containers: Rebuild Container**

## Project layout

- `Program.cs` – interactive Spectre.Console demos
- `appsettings.json` – default app settings template
- `SpectreConsoleApp.csproj` – project file targeting `net10.0`

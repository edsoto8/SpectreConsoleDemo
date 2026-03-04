# Spectre Console Demo

Interactive .NET 9 console app that showcases common [Spectre.Console](https://spectreconsole.net/) UI components.

## What this project does

The app starts with a searchable menu and lets you run small demos for:

- Panel & Markup
- Colors & Styles
- Table
- Tree
- Progress
- Prompts
- Bar Chart
- Calendar Viewer
- Status Spinner
- Hero CSV Explorer
- Timer

Each demo renders directly in the terminal and returns to the menu when you press a key.

## Requirements

- .NET SDK 9.0+
- macOS / Linux / Windows terminal

Optional:

- VS Code + C# extension
- Docker + Dev Containers extension (the repo includes a .NET 9 devcontainer)

## Run locally

```bash
dotnet restore
dotnet run --project ./SpectreConsoleApp/SpectreConsoleApp.csproj
```

## Build and publish

```bash
dotnet build ./SpectreConsoleApp/SpectreConsoleApp.csproj
dotnet publish ./SpectreConsoleApp/SpectreConsoleApp.csproj -c Release
```

## Run in VS Code

- Use the `build`, `publish`, and `watch` tasks from `.vscode/tasks.json`.
- Press `F5` with the `.NET Launch (web)` configuration in `.vscode/launch.json`.

## Dev Container

This repository includes `.devcontainer/devcontainer.json` using:

- `mcr.microsoft.com/devcontainers/dotnet:1-9.0`

If you update the devcontainer config, rebuild it via:

- **Dev Containers: Rebuild Container**

## Project layout

- `SpectreConsoleApp/Program.cs` – interactive menu and built-in demos (Panel, Colors & Styles, Table, Tree, Progress, Prompts, Bar Chart, Status Spinner)
- `SpectreConsoleApp/CalendarViewer.cs` – calendar viewer with event management, month/year navigation
- `SpectreConsoleApp/HeroDataExplorer.cs` – CSV-backed hero data browser with filtering and sorting
- `SpectreConsoleApp/Timer.cs` – countdown timer with preset and custom durations
- `SpectreConsoleApp/sample-data/heroes.csv` – sample hero dataset for the Hero CSV Explorer
- `SpectreConsoleApp/appsettings.json` – default app settings template
- `SpectreConsoleApp/SpectreConsoleApp.csproj` – project file targeting `net9.0`
- `SpectreConsoleDemo.sln` – solution file

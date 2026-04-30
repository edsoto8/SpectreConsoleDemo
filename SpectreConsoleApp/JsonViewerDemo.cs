using Spectre.Console;
using Spectre.Console.Json;

internal static class JsonViewerDemo
{
    internal static void Run()
    {
        AnsiConsole.MarkupLine("[bold]Spectre.Console's [cyan]JsonText[/] renders JSON with syntax coloring:[/]");
        AnsiConsole.WriteLine();

        const string json = """
            {
                "app": "SpectreConsoleDemo",
                "version": "1.0.0",
                "features": ["Panel", "Table", "Tree", "Progress", "Layout", "JSON"],
                "heroes": {
                    "total": 50,
                    "universes": ["Marvel", "DC"],
                    "topHero": {
                        "name": "Scarlet Witch",
                        "powerLevel": 99,
                        "status": "Active"
                    }
                },
                "settings": {
                    "theme": "dark",
                    "pageSize": 12,
                    "enableSearch": true
                }
            }
            """;

        AnsiConsole.Write(
            new Panel(new JsonText(json))
                .Header("[cyan]Sample JSON[/]", Justify.Center)
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.CadetBlue)
                .Expand());
    }
}

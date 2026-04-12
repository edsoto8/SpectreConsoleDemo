using Spectre.Console;

internal static class LayoutDemo
{
    public static void Run()
    {
        AnsiConsole.Write(new Rule("[bold cyan]Layout Demo[/]").RuleStyle("grey").Centered());
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Spectre.Console's [bold white]Layout[/] widget divides the terminal into resizable named panes.[/]");
        AnsiConsole.WriteLine();

        var layout = new Layout("Root")
            .SplitColumns(
                new Layout("Left").Ratio(1),
                new Layout("Right").Ratio(2)
                    .SplitRows(
                        new Layout("Top").Ratio(1),
                        new Layout("Bottom").Ratio(1)
                    )
            );

        // ── Left pane: feature index ─────────────────────────────────
        var featureList = new Table()
            .NoBorder()
            .HideHeaders()
            .AddColumn("")
            .AddColumn("");

        featureList.AddRow("[cyan]Panel & Markup[/]", "Styled bordered blocks");
        featureList.AddRow("[cyan]Table[/]", "Structured data rows");
        featureList.AddRow("[cyan]Tree[/]", "Hierarchical display");
        featureList.AddRow("[cyan]Progress[/]", "Multi-task bars");
        featureList.AddRow("[cyan]Prompts[/]", "Text / select / multi");
        featureList.AddRow("[cyan]Bar Chart[/]", "Horizontal bar chart");
        featureList.AddRow("[cyan]Calendar[/]", "Date & event viewer");
        featureList.AddRow("[cyan]Status Spinner[/]", "Async simulation");
        featureList.AddRow("[cyan]Hero Explorer[/]", "CSV data explorer");
        featureList.AddRow("[cyan]Timer[/]", "Pausable countdown");
        featureList.AddRow("[cyan]File Explorer[/]", "Directory navigator");
        featureList.AddRow("[cyan bold]Layout[/]", "[bold yellow]← This demo![/]");
        featureList.AddRow("[cyan]Exception[/]", "Rich stack traces");
        featureList.AddRow("[cyan]JSON Viewer[/]", "Syntax-colored JSON");

        layout["Left"].Update(
            new Panel(featureList)
                .Header("[bold]Demo Index[/]", Justify.Center)
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.CadetBlue)
                .Expand());

        // ── Top-right pane: bar chart ────────────────────────────────
        var chart = new BarChart()
            .Width(50)
            .Label("[bold]Spectre.Console Feature Popularity[/]")
            .CenterLabel()
            .AddItem("Markup", 95, Color.Gold1)
            .AddItem("Prompts", 91, Color.MediumPurple)
            .AddItem("Tables", 87, Color.CadetBlue)
            .AddItem("Progress", 78, Color.SeaGreen2)
            .AddItem("Layout", 65, Color.SteelBlue1)
            .AddItem("Live", 60, Color.DarkOrange);

        layout["Top"].Update(
            new Panel(chart)
                .Header("[bold]Usage Stats[/]", Justify.Center)
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.MediumPurple)
                .Expand());

        // ── Bottom-right pane: API reference ─────────────────────────
        var api = new Table()
            .Border(TableBorder.Simple)
            .AddColumn(new TableColumn("[bold]Concept[/]"))
            .AddColumn(new TableColumn("[bold]Code[/]"));

        api.AddRow("Create layout",        "[grey]new Layout(\"Root\")[/]");
        api.AddRow("Split columns",        "[grey].SplitColumns(left, right)[/]");
        api.AddRow("Split rows",           "[grey].SplitRows(top, bottom)[/]");
        api.AddRow("Set ratio",            "[grey].Ratio(2)[/]");
        api.AddRow("Minimum height",       "[grey].MinimumSize(10)[/]");
        api.AddRow("Update pane content",  "[grey]layout[\"Name\"].Update(renderable)[/]");
        api.AddRow("Render once",          "[grey]AnsiConsole.Write(layout)[/]");
        api.AddRow("Animate / live",       "[grey]AnsiConsole.Live(layout).Start(ctx => ...)[/]");

        layout["Bottom"].Update(
            new Panel(api)
                .Header("[bold]Layout API Quick Reference[/]", Justify.Center)
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.DarkOrange)
                .Expand());

        AnsiConsole.Write(layout);
    }
}

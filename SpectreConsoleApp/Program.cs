using Spectre.Console;

AnsiConsole.Clear();
AnsiConsole.Write(
    new FigletText("Spectre Application")
    .Centered()
    .Color(Color.CornflowerBlue));

while (true)
{
    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[bold]Choose a demo[/]")
            .PageSize(20)
            .MoreChoicesText("[grey](Move up and down to see more options)[/]")
            .AddChoiceGroup("── Basics ──", [
                "Panel & Markup",
                "Table",
                "Tree",
                "Bar Chart",
                "JSON Viewer",
            ])
            .AddChoiceGroup("── Interactive ──", [
                "Prompts",
                "Progress",
                "Status Spinner",
                "Countdown Timer",
                "Calendar",
                "Hero CSV Explorer",
                "File Explorer",
                "Layout",
                "Exception Display",
            ])
            .AddChoiceGroup("── System ──", [
                "Exit",
            ])
        .EnableSearch()
        .SearchPlaceholderText("[grey]Type to search demos...[/]")
        .HighlightStyle(Style.Parse("cyan")));

    AnsiConsole.Clear();

    if (choice == "Exit")
    {
        AnsiConsole.MarkupLine("[green]Goodbye![/]");
        break;
    }

    switch (choice)
    {
        case "Panel & Markup":
            PanelAndMarkupDemo.Run();
            break;
        case "Table":
            TableDemo.Run();
            break;
        case "Tree":
            TreeDemo.Run();
            break;
        case "Progress":
            ProgressDemo.Run();
            break;
        case "Prompts":
            PromptsDemo.Run();
            break;
        case "Bar Chart":
            BarChartDemo.Run();
            break;
        case "Calendar":
            CalendarViewer.Run();
            break;
        case "Status Spinner":
            StatusSpinnerDemo.Run();
            break;
        case "Hero CSV Explorer":
            HeroDataExplorer.Run();
            break;
        case "Countdown Timer":
            CountdownTimerDemo.Run();
            break;
        case "File Explorer":
            FileExplorer.Run();
            break;
        case "Layout":
            LayoutDemo.Run();
            break;
        case "Exception Display":
            ExceptionDisplayDemo.Run();
            break;
        case "JSON Viewer":
            JsonViewerDemo.Run();
            break;
    }

    Pause();
    AnsiConsole.Clear();
}

static void Pause()
{
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[grey]Press any key to return to the menu...[/]");
    Console.ReadKey(intercept: true);
}

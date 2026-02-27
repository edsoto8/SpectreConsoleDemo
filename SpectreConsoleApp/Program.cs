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
            .PageSize(12)
        .MoreChoicesText("[grey](Move up and down to see more options)[/]")
        .AddChoices(
        [
            "Panel & Markup",
            "Table",
            "Tree",
            "Progress",
            "Prompts",
            "Bar Chart",
            "Calendar",
            "Status Spinner",
            "Hero CSV Explorer",
            "Timer",
            "Exit"
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
            ShowPanelAndMarkup();
            break;
        case "Table":
            ShowTable();
            break;
        case "Tree":
            ShowTree();
            break;
        case "Progress":
            ShowProgress();
            break;
        case "Prompts":
            ShowPrompts();
            break;
        case "Bar Chart":
            ShowBarChart();
            break;
        case "Calendar":
            CalendarViewer.Run();
            break;
        case "Status Spinner":
            ShowStatus();
            break;
        case "Hero CSV Explorer":
            HeroDataExplorer.Run();
            break;
        case "Timer":
            Timer.Run();
            break;
    }

    Pause();
    AnsiConsole.Clear();
}

static void ShowPanelAndMarkup()
{
    var panel = new Panel(new Markup("[bold yellow]Welcome to Spectre.Console![/]\n[grey]Build rich terminal apps with ease.[/]"))
        .Border(BoxBorder.Rounded)
        .Header("[aqua]Panel Demo[/]", Justify.Center)
        .Expand();

    AnsiConsole.Write(panel);
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[green]Styles[/], [underline]underline[/], and [bold]bold[/] can be combined.");
}

static void ShowTable()
{
    var table = new Table()
        .Border(TableBorder.Rounded)
        .Title("[bold]Feature Matrix[/]")
        .AddColumn("Control")
        .AddColumn("Use Case")
        .AddColumn("Complexity");

    table.AddRow("Panel", "Emphasize key content", "Low");
    table.AddRow("Table", "Structured datasets", "Low");
    table.AddRow("Tree", "Hierarchical data", "Medium");
    table.AddRow("Progress", "Long-running tasks", "Medium");
    table.AddRow("Live", "Realtime dashboards", "High");

    AnsiConsole.Write(table);
}

static void ShowTree()
{
    var tree = new Tree("[bold]Project[/]")
        .Guide(TreeGuide.Line);

    var src = tree.AddNode("[blue]src[/]");
    src.AddNode("[grey]Program.cs[/]");
    src.AddNode("[grey]Widgets/[/]");

    var docs = tree.AddNode("[green]docs[/]");
    docs.AddNode("[grey]README.md[/]");
    docs.AddNode("[grey]CONTRIBUTING.md[/]");

    tree.AddNode("[yellow]SpectreConsoleApp.csproj[/]");

    AnsiConsole.Write(tree);
}

static void ShowProgress()
{
    AnsiConsole.Progress()
        .Columns(
        [
            new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new ElapsedTimeColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn()
        ])
        .Start(ctx =>
        {
            var restore = ctx.AddTask("[green]Restore packages[/]", maxValue: 100);
            var build = ctx.AddTask("[yellow]Build app[/]", maxValue: 100);
            var test = ctx.AddTask("[blue]Run tests[/]", maxValue: 100);

            while (!ctx.IsFinished)
            {
                restore.Increment(2.0);
                build.Increment(1.5);
                test.Increment(1.0);
                Thread.Sleep(45);
            }
        });
}

static void ShowPrompts()
{
    var name = AnsiConsole.Prompt(
        new TextPrompt<string>("What is your [green]name[/]?")
            .PromptStyle("cyan")
            .ValidationErrorMessage("[red]Please enter a valid name[/]")
            .Validate(n => !string.IsNullOrWhiteSpace(n)));

    var favorite = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Pick your [bold]favorite[/] control")
            .AddChoices("Table", "Tree", "Progress", "Prompt", "Chart"));

    var addOns = AnsiConsole.Prompt(
        new MultiSelectionPrompt<string>()
            .Title("Select optional features")
            .NotRequired()
            .InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to accept)[/]")
            .AddChoices("Markup", "Live updates", "JSON output", "Themes", "Paging"));

    var summary = new Table().Border(TableBorder.MinimalHeavyHead);
    summary.AddColumn("Field");
    summary.AddColumn("Value");
    summary.AddRow("Name", name);
    summary.AddRow("Favorite", favorite);
    summary.AddRow("Add-ons", addOns.Count == 0 ? "None" : string.Join(", ", addOns));

    AnsiConsole.Write(new Panel(summary).Header("Prompt Results"));
}

static void ShowBarChart()
{
    var chart = new BarChart()
        .Width(60)
        .Label("[green bold underline]Monthly Usage[/]")
        .CenterLabel();

    chart.AddItem("Panels", 42, Color.CadetBlue);
    chart.AddItem("Tables", 87, Color.MediumPurple);
    chart.AddItem("Trees", 55, Color.SeaGreen2);
    chart.AddItem("Progress", 73, Color.Yellow);

    AnsiConsole.Write(chart);
}

static void ShowStatus()
{
    AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .Start("Preparing terminal UI sample...", _ =>
        {
            Thread.Sleep(1200);
        });

    AnsiConsole.MarkupLine("[green]Completed sample status workflow.[/]");
}

static void Pause()
{
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[grey]Press any key to return to the menu...[/]");
    System.Console.ReadKey(intercept: true);
}
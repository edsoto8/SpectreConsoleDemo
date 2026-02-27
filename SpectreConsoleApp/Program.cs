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
            "Colors & Styles",
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
        case "Colors & Styles":
            ShowColorsAndStyles();
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

static void ShowColorsAndStyles()
{
    // ── Text Decorations ──────────────────────────────────────────────────
    AnsiConsole.Write(new Rule("[bold]Text Decorations[/]").RuleStyle("grey"));
    AnsiConsole.WriteLine();

    var decorTable = new Table()
        .Border(TableBorder.Rounded)
        .AddColumn(new TableColumn("[grey]Decoration[/]").Width(18))
        .AddColumn("[grey]Markup Syntax[/]")
        .AddColumn("[grey]Example[/]");

    decorTable.AddRow(new Markup("Bold"),          new Markup("[grey][[bold]]..[[/bold]][/]"),         new Markup("[bold]The quick brown fox[/]"));
    decorTable.AddRow(new Markup("Italic"),        new Markup("[grey][[italic]]..[[/italic]][/]"),       new Markup("[italic]The quick brown fox[/]"));
    decorTable.AddRow(new Markup("Underline"),     new Markup("[grey][[underline]]..[[/]][/]"),         new Markup("[underline]The quick brown fox[/]"));
    decorTable.AddRow(new Markup("Strikethrough"), new Markup("[grey][[strikethrough]]..[[/]][/]"),     new Markup("[strikethrough]The quick brown fox[/]"));
    decorTable.AddRow(new Markup("Dim"),           new Markup("[grey][[dim]]..[[/dim]][/]"),             new Markup("[dim]The quick brown fox[/]"));
    decorTable.AddRow(new Markup("Reverse"),       new Markup("[grey][[reverse]]..[[/reverse]][/]"),     new Markup("[reverse]The quick brown fox[/]"));
    decorTable.AddRow(new Markup("Invert"),        new Markup("[grey][[black on white]][/]"),           new Markup("[black on white] The quick brown fox [/]"));
    decorTable.AddRow(new Markup("Bold + Italic"), new Markup("[grey][[bold italic]]..[[/]][/]"),       new Markup("[bold italic]The quick brown fox[/]"));
    decorTable.AddRow(new Markup("Bold + Under."), new Markup("[grey][[bold underline]]..[[/]][/]"),    new Markup("[bold underline]The quick brown fox[/]"));

    AnsiConsole.Write(decorTable);
    AnsiConsole.WriteLine();

    // ── Standard Colours ─────────────────────────────────────────────────
    AnsiConsole.Write(new Rule("[bold]Standard Colours[/]").RuleStyle("grey"));
    AnsiConsole.WriteLine();

    var colorGroups = new (string Label, string Markup)[]
    {
        ("Red",          "[red]████ Red[/]"),
        ("Green",        "[green]████ Green[/]"),
        ("Blue",         "[blue]████ Blue[/]"),
        ("Yellow",       "[yellow]████ Yellow[/]"),
        ("Magenta",      "[magenta]████ Magenta[/]"),
        ("Cyan",         "[cyan]████ Cyan[/]"),
        ("White",        "[white]████ White[/]"),
        ("Grey",         "[grey]████ Grey[/]"),
        ("Aqua",         "[aqua]████ Aqua[/]"),
        ("Lime",         "[lime]████ Lime[/]"),
        ("Orange",       "[orange3]████ Orange[/]"),
        ("Purple",       "[mediumpurple]████ Purple[/]"),
        ("Teal",         "[teal]████ Teal[/]"),
        ("Gold",         "[gold1]████ Gold[/]"),
        ("Hot Pink",     "[hotpink]████ Hot Pink[/]"),
        ("Cornflower",   "[cornflowerblue]████ Cornflower[/]"),
    };

    var colorGrid = new Columns(colorGroups.Select(c => new Markup(c.Markup)).ToArray())
    {
        Expand = false,
    };
    AnsiConsole.Write(colorGrid);
    AnsiConsole.WriteLine();

    // ── Foreground on Background ──────────────────────────────────────────
    AnsiConsole.Write(new Rule("[bold]Foreground on Background[/]").RuleStyle("grey"));
    AnsiConsole.WriteLine();

    var bgTable = new Table()
        .Border(TableBorder.Rounded)
        .AddColumn(new TableColumn("[grey]Name[/]").Width(14))
        .AddColumn("[grey]Markup Syntax[/]")
        .AddColumn("[grey]Example[/]");

    bgTable.AddRow(new Markup("Success"),  new Markup("[grey][[bold green on black]][/]"),        new Markup("[bold green on black]  ✔  Operation succeeded  [/]"));
    bgTable.AddRow(new Markup("Warning"),  new Markup("[grey][[bold black on yellow]][/]"),       new Markup("[bold black on yellow]  ⚠  Proceed with caution  [/]"));
    bgTable.AddRow(new Markup("Error"),    new Markup("[grey][[bold white on red]][/]"),          new Markup("[bold white on red]  ✘  Something went wrong  [/]"));
    bgTable.AddRow(new Markup("Info"),     new Markup("[grey][[bold white on blue]][/]"),         new Markup("[bold white on blue]  ℹ  Here is some info  [/]"));
    bgTable.AddRow(new Markup("Muted"),    new Markup("[grey][[grey on black]][/]"),              new Markup("[grey on black]  ·  Secondary content  [/]"));
    bgTable.AddRow(new Markup("Inverted"), new Markup("[grey][[black on white]][/]"),             new Markup("[black on white]  ◆  High contrast block  [/]"));
    bgTable.AddRow(new Markup("Accent"),   new Markup("[grey][[bold black on aqua]][/]"),         new Markup("[bold black on aqua]  ★  Featured item  [/]"));

    AnsiConsole.Write(bgTable);
    AnsiConsole.WriteLine();

    // ── Programmatic Style Object ─────────────────────────────────────────
    AnsiConsole.Write(new Rule("[bold]Programmatic Style Object[/]").RuleStyle("grey"));
    AnsiConsole.WriteLine();

    Style[] styles =
    [
        new(foreground: Color.Red,    background: Color.White,   decoration: Decoration.Bold | Decoration.Underline),
        new(foreground: Color.Black,  background: Color.Yellow,  decoration: Decoration.Bold),
        new(foreground: Color.White,  background: Color.Blue,    decoration: Decoration.Italic),
        new(foreground: Color.Lime,   background: Color.Black,   decoration: Decoration.Dim),
        new(foreground: Color.White,  background: Color.HotPink, decoration: Decoration.Bold | Decoration.Strikethrough),
    ];

    string[] labels = ["Danger", "Warning", "Informational", "Subtle", "Deprecated"];

    var styleTable = new Table()
        .Border(TableBorder.Rounded)
        .AddColumn(new TableColumn("[grey]Name[/]").Width(16))
        .AddColumn("[grey]fg / bg / decoration[/]")
        .AddColumn("[grey]Result[/]");

    for (int i = 0; i < styles.Length; i++)
    {
        var s = styles[i];
        var desc = $"[grey]{s.Foreground} on {s.Background} | {s.Decoration}[/]";
        styleTable.AddRow(new Markup(labels[i]), new Markup(desc), new Markup($" {labels[i]} ", s));
    }

    AnsiConsole.Write(styleTable);
    AnsiConsole.WriteLine();

    // ── Colour Spectrum Rule ──────────────────────────────────────────────
    AnsiConsole.Write(new Rule("[bold]True-Colour Spectrum[/]").RuleStyle("grey"));
    AnsiConsole.WriteLine();

    int termWidth = Math.Min(AnsiConsole.Profile.Width, 80);
    var spectrumMarkup = string.Concat(Enumerable.Range(0, termWidth).Select(i =>
    {
        int r = (int)(Math.Sin(i * Math.PI / termWidth) * 255);
        int g = (int)(Math.Sin(i * Math.PI / termWidth + 2 * Math.PI / 3) * 255);
        int b = (int)(Math.Sin(i * Math.PI / termWidth + 4 * Math.PI / 3) * 255);
        r = Math.Clamp(r, 0, 255);
        g = Math.Clamp(g, 0, 255);
        b = Math.Clamp(b, 0, 255);
        return $"[#{r:X2}{g:X2}{b:X2}]█[/]";
    }));
    AnsiConsole.MarkupLine(spectrumMarkup);
    AnsiConsole.WriteLine();
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
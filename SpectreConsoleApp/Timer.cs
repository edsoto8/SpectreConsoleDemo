using Spectre.Console;

internal static class CountdownTimerDemo
{
    private static readonly (string Label, TimeSpan Duration)[] QuickDefaults =
    [
        ("30 seconds",   TimeSpan.FromSeconds(30)),
        ("1 minute",     TimeSpan.FromMinutes(1)),
        ("2 minutes",    TimeSpan.FromMinutes(2)),
        ("5 minutes",    TimeSpan.FromMinutes(5)),
        ("10 minutes",   TimeSpan.FromMinutes(10)),
        ("25 minutes  [grey](Pomodoro)[/]",  TimeSpan.FromMinutes(25)),
        ("30 minutes",   TimeSpan.FromMinutes(30)),
        ("1 hour",       TimeSpan.FromHours(1)),
        ("Custom",       TimeSpan.Zero),
    ];

    public static void Run()
    {
        AnsiConsole.Write(
            new Rule("[bold cyan]Timer[/]").RuleStyle("grey").Centered());
        AnsiConsole.WriteLine();

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Select a duration or choose [cyan]Custom[/]:[/]")
                .PageSize(10)
                .HighlightStyle(Style.Parse("cyan"))
                .AddChoices(QuickDefaults.Select(d => d.Label)));

        TimeSpan total;

        if (choice.StartsWith("Custom"))
        {
            total = PromptCustomDuration();
        }
        else
        {
            total = QuickDefaults.First(d => d.Label == choice).Duration;
        }

        if (total <= TimeSpan.Zero)
        {
            AnsiConsole.MarkupLine("[red]Duration must be greater than zero.[/]");
            return;
        }

        RunCountdown(total);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static TimeSpan PromptCustomDuration()
    {
        AnsiConsole.WriteLine();

        var hours = AnsiConsole.Prompt(
            new TextPrompt<int>("[bold]Hours[/]   [grey](0–23):[/]")
                .PromptStyle("cyan")
                .DefaultValue(0)
                .ValidationErrorMessage("[red]Enter a whole number between 0 and 23[/]")
                .Validate(n => n is >= 0 and <= 23));

        var minutes = AnsiConsole.Prompt(
            new TextPrompt<int>("[bold]Minutes[/] [grey](0–59):[/]")
                .PromptStyle("cyan")
                .DefaultValue(0)
                .ValidationErrorMessage("[red]Enter a whole number between 0 and 59[/]")
                .Validate(n => n is >= 0 and <= 59));

        var seconds = AnsiConsole.Prompt(
            new TextPrompt<int>("[bold]Seconds[/] [grey](0–59):[/]")
                .PromptStyle("cyan")
                .DefaultValue(0)
                .ValidationErrorMessage("[red]Enter a whole number between 0 and 59[/]")
                .Validate(n => n is >= 0 and <= 59));

        return new TimeSpan(hours, minutes, seconds);
    }

    private static void RunCountdown(TimeSpan total)
    {
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .Centered()
            .AddColumn(new TableColumn(string.Empty).Centered());

        AnsiConsole.Live(table)
            .AutoClear(false)
            .Start(ctx =>
            {
                var remaining = total;

                while (remaining >= TimeSpan.Zero)
                {
                    var color = remaining.TotalSeconds switch
                    {
                        0               => Color.Red,
                        <= 10           => Color.OrangeRed1,
                        <= 60           => Color.Yellow,
                        _               => Color.Green,
                    };

                    var timeLabel = FormatTime(remaining);
                    var isFinished = remaining == TimeSpan.Zero;

                    table.Rows.Clear();
                    table.AddRow(
                        new FigletText(isFinished ? "TIME'S UP" : timeLabel)
                            .Centered()
                            .Color(color));

                    table.AddRow(
                        new Markup(isFinished
                            ? "[bold red]  Done!  [/]"
                            : $"[grey]  {DescribeRemaining(remaining)}  [/]")
                            .Centered());

                    ctx.Refresh();

                    if (isFinished) break;

                    Thread.Sleep(1000);
                    remaining = remaining.Subtract(TimeSpan.FromSeconds(1));
                }
            });
    }

    private static string FormatTime(TimeSpan ts)
    {
        // Show hours only when there are hours remaining
        return ts.Hours > 0
            ? $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}"
            : $"{ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    private static string DescribeRemaining(TimeSpan ts)
    {
        var parts = new List<string>();

        if (ts.Hours > 0)
            parts.Add($"{ts.Hours} hour{(ts.Hours == 1 ? "" : "s")}");
        if (ts.Minutes > 0)
            parts.Add($"{ts.Minutes} minute{(ts.Minutes == 1 ? "" : "s")}");
        if (ts.Seconds > 0 || parts.Count == 0)
            parts.Add($"{ts.Seconds} second{(ts.Seconds == 1 ? "" : "s")}");

        return string.Join(", ", parts) + " remaining";
    }
}

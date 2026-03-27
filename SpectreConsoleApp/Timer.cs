using System.Diagnostics;
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
        AnsiConsole.MarkupLine("[grey]  [bold white]Space[/] Pause/Resume   [bold white]Q / Esc[/] Cancel[/]");
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .Centered()
            .AddColumn(new TableColumn(string.Empty).Centered());

        bool paused = false;
        bool cancelled = false;
        var displayRemaining = total;
        TimeSpan accumulated = TimeSpan.Zero;
        var sw = Stopwatch.StartNew();

        AnsiConsole.Live(table)
            .AutoClear(false)
            .Start(ctx =>
            {
                // Force initial render
                RefreshFrame(table, ctx, displayRemaining, paused, isFinished: false);

                while (!cancelled)
                {
                    // Drain any queued key presses
                    while (Console.KeyAvailable)
                    {
                        var k = Console.ReadKey(intercept: true);
                        if (k.Key == ConsoleKey.Spacebar)
                        {
                            if (paused)
                            {
                                sw.Restart();
                                paused = false;
                            }
                            else
                            {
                                accumulated += sw.Elapsed;
                                sw.Reset();
                                paused = true;
                            }
                        }
                        else if (k.Key is ConsoleKey.Q or ConsoleKey.Escape)
                        {
                            cancelled = true;
                            break;
                        }
                    }

                    if (cancelled) break;

                    var totalElapsed = accumulated + (paused ? TimeSpan.Zero : sw.Elapsed);
                    var raw = total - totalElapsed;
                    if (raw < TimeSpan.Zero) raw = TimeSpan.Zero;

                    // Quantise to whole seconds so FigletText doesn't flicker every frame
                    var newDisplay = TimeSpan.FromSeconds(Math.Floor(raw.TotalSeconds));
                    bool changed = newDisplay != displayRemaining;
                    displayRemaining = newDisplay;

                    if (changed || paused)
                    {
                        bool isFinished = displayRemaining == TimeSpan.Zero;
                        RefreshFrame(table, ctx, displayRemaining, paused, isFinished);
                        if (isFinished) break;
                    }

                    Thread.Sleep(50);
                }
            });

        if (cancelled)
            AnsiConsole.MarkupLine("[red]Timer cancelled.[/]");
    }

    private static void RefreshFrame(Table table, LiveDisplayContext ctx,
        TimeSpan remaining, bool paused, bool isFinished)
    {
        var color = remaining.TotalSeconds switch
        {
            0    => Color.Red,
            <= 10 => Color.OrangeRed1,
            <= 60 => Color.Yellow,
            _    => Color.Green,
        };

        var timeLabel = FormatTime(remaining);

        table.Rows.Clear();
        table.AddRow(
            new FigletText(isFinished ? "TIME'S UP" : timeLabel)
                .Centered()
                .Color(paused ? Color.Yellow : color));

        table.AddRow(
            new Markup(isFinished
                ? "[bold red]  Done!  [/]"
                : paused
                    ? "[bold yellow]  PAUSED — press Space to resume  [/]"
                    : $"[grey]  {DescribeRemaining(remaining)}  [/]")
                .Centered());

        ctx.Refresh();
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

using Spectre.Console;

internal static class CalendarViewer
{
    // Simple in-memory events: key = date, value = list of event labels
    private static readonly Dictionary<DateOnly, List<string>> _events = new()
    {
        [new DateOnly(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day)] = ["Today's task"],
    };

    public static void Run()
    {
        var viewDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);

        while (true)
        {
            AnsiConsole.Clear();
            RenderHeader(viewDate);
            RenderMonthCalendar(viewDate);
            RenderEventList(viewDate);
            RenderNavBar();

            var key = Console.ReadKey(intercept: true);

            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                    viewDate = viewDate.AddMonths(-1);
                    break;

                case ConsoleKey.RightArrow:
                    viewDate = viewDate.AddMonths(1);
                    break;

                case ConsoleKey.UpArrow:
                    viewDate = viewDate.AddYears(-1);
                    break;

                case ConsoleKey.DownArrow:
                    viewDate = viewDate.AddYears(1);
                    break;

                case ConsoleKey.T:
                    viewDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
                    break;

                case ConsoleKey.Y:
                    RenderYearOverview(viewDate.Year);
                    AnsiConsole.MarkupLine("\n[grey]Press any key to return to the month view...[/]");
                    Console.ReadKey(intercept: true);
                    break;

                case ConsoleKey.J:
                    var jumped = PromptJumpToDate();
                    if (jumped.HasValue)
                        viewDate = new DateOnly(jumped.Value.Year, jumped.Value.Month, 1);
                    break;

                case ConsoleKey.E:
                    PromptAddEvent(viewDate);
                    break;

                case ConsoleKey.D:
                    PromptDeleteEvent(viewDate);
                    break;

                case ConsoleKey.Escape:
                case ConsoleKey.Q:
                    return;
            }
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Rendering
    // ──────────────────────────────────────────────────────────────

    private static void RenderHeader(DateOnly viewDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var weekOfYear = System.Globalization.ISOWeek.GetWeekOfYear(viewDate.ToDateTime(TimeOnly.MinValue));

        AnsiConsole.Write(
            new FigletText(viewDate.ToString("MMMM yyyy"))
                .Centered()
                .Color(Color.MediumPurple));

        var infoRow = new Table().NoBorder().Expand();
        infoRow.AddColumn(new TableColumn("").Centered());
        infoRow.AddColumn(new TableColumn("").Centered());
        infoRow.AddColumn(new TableColumn("").Centered());
        infoRow.AddRow(
            $"[grey]Today: [bold white]{today:ddd, MMM d yyyy}[/][/]",
            $"[grey]Week [bold white]{weekOfYear}[/] of year[/]",
            $"[grey]Quarter [bold white]Q{((viewDate.Month - 1) / 3) + 1}[/][/]"
        );
        AnsiConsole.Write(infoRow);
        AnsiConsole.WriteLine();
    }

    private static void RenderMonthCalendar(DateOnly viewDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        int daysInMonth = DateTime.DaysInMonth(viewDate.Year, viewDate.Month);
        var firstDay = new DateOnly(viewDate.Year, viewDate.Month, 1);
        int startDow = (int)firstDay.DayOfWeek; // 0 = Sun

        var calendar = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.MediumPurple1)
            .Title($"[bold mediumpurple]{viewDate:MMMM yyyy}[/]")
            .AddColumn(new TableColumn("[bold red]Sun[/]").Centered())
            .AddColumn(new TableColumn("[bold yellow]Mon[/]").Centered())
            .AddColumn(new TableColumn("[bold yellow]Tue[/]").Centered())
            .AddColumn(new TableColumn("[bold yellow]Wed[/]").Centered())
            .AddColumn(new TableColumn("[bold yellow]Thu[/]").Centered())
            .AddColumn(new TableColumn("[bold yellow]Fri[/]").Centered())
            .AddColumn(new TableColumn("[bold cyan]Sat[/]").Centered());

        var cells = new List<string>();

        // Leading empty cells
        for (int i = 0; i < startDow; i++)
            cells.Add("[grey]  [/]");

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(viewDate.Year, viewDate.Month, day);
            bool isToday = date == today;
            bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
            bool hasEvent = _events.ContainsKey(date);

            string label = isToday
                ? $"[bold green on black]{day,2}[/]"
                : isWeekend
                    ? $"[bold blue]{day,2}[/]"
                    : $"{day,2}";

            if (hasEvent)
                label += "[yellow]•[/]";

            cells.Add(label);
        }

        // Chunk into rows of 7
        for (int i = 0; i < cells.Count; i += 7)
        {
            var week = cells.Skip(i).Take(7).ToList();
            while (week.Count < 7) week.Add("[grey]  [/]");
            calendar.AddRow(week.ToArray());
        }

        AnsiConsole.Write(calendar);
        AnsiConsole.MarkupLine("[grey]  [green]■[/] Today  [blue]■[/] Weekend  [yellow]•[/] Has event[/]");
        AnsiConsole.WriteLine();
    }

    private static void RenderEventList(DateOnly viewDate)
    {
        int daysInMonth = DateTime.DaysInMonth(viewDate.Year, viewDate.Month);

        var monthEvents = _events
            .Where(e => e.Key.Year == viewDate.Year && e.Key.Month == viewDate.Month)
            .OrderBy(e => e.Key)
            .ToList();

        if (monthEvents.Count == 0)
            return;

        var table = new Table()
            .Border(TableBorder.SimpleHeavy)
            .BorderColor(Color.Yellow)
            .Title("[bold yellow]Events this month[/]")
            .AddColumn(new TableColumn("[bold]Date[/]"))
            .AddColumn(new TableColumn("[bold]Events[/]"));

        foreach (var (date, evts) in monthEvents)
        {
            bool isToday = date == DateOnly.FromDateTime(DateTime.Today);
            string dateFmt = isToday
                ? $"[bold green]{date:ddd, MMM d}[/]"
                : $"[white]{date:ddd, MMM d}[/]";
            table.AddRow(dateFmt, string.Join(", ", evts.Select(e => $"[yellow]{e}[/]")));
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private static void RenderNavBar()
    {
        var rule = new Rule("[grey]Navigation[/]").RuleStyle("grey").LeftJustified();
        AnsiConsole.Write(rule);
        AnsiConsole.MarkupLine(
            "[grey]" +
            "  [[[white]← →[/]]] Prev / Next month  " +
            "[[[white]↑ ↓[/]]] Prev / Next year  " +
            "[[[white]T[/]]] Today  " +
            "[[[white]J[/]]] Jump to date  " +
            "[[[white]Y[/]]] Year overview  " +
            "[[[white]E[/]]] Add event  " +
            "[[[white]D[/]]] Delete event  " +
            "[[[white]Q / Esc[/]]] Back" +
            "[/]");
    }

    private static void RenderYearOverview(int year)
    {
        AnsiConsole.Clear();

        AnsiConsole.Write(
            new FigletText(year.ToString())
                .Centered()
                .Color(Color.CadetBlue));

        var today = DateOnly.FromDateTime(DateTime.Today);
        var grid = new Grid();

        // 4 columns of mini-months
        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();

        var panels = new List<Panel>();

        for (int month = 1; month <= 12; month++)
        {
            int daysInMonth = DateTime.DaysInMonth(year, month);
            var firstDay = new DateOnly(year, month, 1);
            int startDow = (int)firstDay.DayOfWeek;

            var tbl = new Table()
                .NoBorder()
                .AddColumn(new TableColumn("[red]Su[/]").Centered())
                .AddColumn(new TableColumn("[white]Mo[/]").Centered())
                .AddColumn(new TableColumn("[white]Tu[/]").Centered())
                .AddColumn(new TableColumn("[white]We[/]").Centered())
                .AddColumn(new TableColumn("[white]Th[/]").Centered())
                .AddColumn(new TableColumn("[white]Fr[/]").Centered())
                .AddColumn(new TableColumn("[cyan]Sa[/]").Centered());

            var cells = Enumerable.Repeat("[grey] [/]", startDow).ToList();

            for (int day = 1; day <= daysInMonth; day++)
            {
                var d = new DateOnly(year, month, day);
                bool isToday = d == today;
                bool isWeekend = d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday;
                bool hasEvent = _events.ContainsKey(d);

                string cell = isToday
                    ? $"[bold green on black]{day,2}[/]"
                    : isWeekend ? $"[blue]{day,2}[/]" : $"{day,2}";

                if (hasEvent) cell += "[yellow]•[/]";
                cells.Add(cell);
            }

            for (int i = 0; i < cells.Count; i += 7)
            {
                var week = cells.Skip(i).Take(7).ToList();
                while (week.Count < 7) week.Add("[grey] [/]");
                tbl.AddRow(week.ToArray());
            }

            bool isCurrent = year == today.Year && month == today.Month;
            var panel = new Panel(tbl)
                .Header($"[bold {(isCurrent ? "mediumpurple" : "white")}]{firstDay:MMMM}[/]")
                .Border(isCurrent ? BoxBorder.Double : BoxBorder.Rounded)
                .BorderColor(isCurrent ? Color.MediumPurple : Color.Grey23);

            panels.Add(panel);
        }

        // Lay out in rows of 4
        for (int row = 0; row < 3; row++)
        {
            grid.AddRow(panels[row * 4], panels[row * 4 + 1], panels[row * 4 + 2], panels[row * 4 + 3]);
        }

        AnsiConsole.Write(grid);
    }

    // ──────────────────────────────────────────────────────────────
    // Prompts
    // ──────────────────────────────────────────────────────────────

    private static DateTime? PromptJumpToDate()
    {
        AnsiConsole.WriteLine();
        var input = AnsiConsole.Prompt(
            new TextPrompt<string>("[bold]Jump to date[/] [grey](e.g. 2025-03 or 2025-03-15, empty to cancel):[/]")
                .AllowEmpty());

        if (string.IsNullOrWhiteSpace(input)) return null;

        // Try yyyy-MM-dd then yyyy-MM
        if (DateTime.TryParseExact(input.Trim(), ["yyyy-MM-dd", "yyyy-MM", "MM/yyyy", "M/yyyy"],
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var dt))
            return dt;

        AnsiConsole.MarkupLine("[red]Could not parse date. Using current view.[/]");
        Thread.Sleep(900);
        return null;
    }

    private static void PromptAddEvent(DateOnly viewDate)
    {
        AnsiConsole.WriteLine();
        int daysInMonth = DateTime.DaysInMonth(viewDate.Year, viewDate.Month);

        var day = AnsiConsole.Prompt(
            new TextPrompt<int>($"[bold]Add event — day[/] [grey](1-{daysInMonth}):[/]")
                .Validate(d => d >= 1 && d <= daysInMonth
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"Enter a day between 1 and {daysInMonth}.")));

        var label = AnsiConsole.Prompt(
            new TextPrompt<string>("[bold]Event label:[/]")
                .PromptStyle("yellow")
                .Validate(s => !string.IsNullOrWhiteSpace(s)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Label cannot be empty.")));

        var key = new DateOnly(viewDate.Year, viewDate.Month, day);
        if (!_events.ContainsKey(key))
            _events[key] = [];

        _events[key].Add(label.Trim());
        AnsiConsole.MarkupLine($"[green]Event added to {key:ddd, MMM d}.[/]");
        Thread.Sleep(700);
    }

    private static void PromptDeleteEvent(DateOnly viewDate)
    {
        AnsiConsole.WriteLine();

        var monthEvents = _events
            .Where(e => e.Key.Year == viewDate.Year && e.Key.Month == viewDate.Month)
            .OrderBy(e => e.Key)
            .ToList();

        if (monthEvents.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No events this month.[/]");
            Thread.Sleep(800);
            return;
        }

        // Build flat list of "date – label" choices
        var choices = monthEvents
            .SelectMany(kv => kv.Value.Select(label => (Key: kv.Key, Label: label)))
            .Select(x => $"{x.Key:MMM d} – {x.Label}")
            .ToList();

        choices.Add("Cancel");

        var picked = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Select event to delete:[/]")
                .AddChoices(choices));

        if (picked == "Cancel") return;

        foreach (var (key, evts) in monthEvents)
        {
            string target = $"{key:MMM d} – ";
            var match = evts.FirstOrDefault(e => picked == $"{key:MMM d} – {e}");
            if (match != null)
            {
                evts.Remove(match);
                if (evts.Count == 0) _events.Remove(key);
                AnsiConsole.MarkupLine("[red]Event removed.[/]");
                Thread.Sleep(700);
                return;
            }
        }
    }
}

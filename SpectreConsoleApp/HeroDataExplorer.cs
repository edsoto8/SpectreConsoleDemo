using Spectre.Console;

internal static class HeroDataExplorer
{
    public static void Run()
    {
        var heroes = HeroCsvRepository.LoadHeroes();

        if (heroes.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No heroes were loaded from sample-data/heroes.csv.[/]");
            return;
        }

        var activeFilters = new List<HeroFilter>();
        var topCount = 20;
        var currentResults = GetDefaultTop(heroes, topCount);
        var totalMatchCount = heroes.Count;
        string? currentSortColumn = null;
        var sortDescending = false;

        while (true)
        {
            AnsiConsole.Clear();
            ShowStatusBar(activeFilters, currentSortColumn, sortDescending, currentResults.Count, totalMatchCount, heroes.Count);
            AnsiConsole.WriteLine();
            ShowTable(currentResults, totalMatchCount, currentResults.Count);
            AnsiConsole.WriteLine();

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Choose an action[/]")
                    .AddChoices("🦸 Select hero", "🔍 Filter data", "🔀 Sort data", "📊 Show statistics", "✏️ Manage heroes", "🎯 Select Top Count", "🚪 Exit")
                    .EnableSearch()
                    .SearchPlaceholderText("[grey]Type to search demos...[/]")
                    .HighlightStyle(Style.Parse("cyan")));

            if (action == "🚪 Exit")
            {
                return;
            }

            if (action == "🎯 Select Top Count")
            {
                topCount = AnsiConsole.Prompt(
                    new TextPrompt<int>("Show top [green]how many[/] heroes?")
                        .DefaultValue(topCount)
                        .Validate(n => n is >= 1 and <= 200
                            ? ValidationResult.Success()
                            : ValidationResult.Error("Enter a value between 1 and 200.")));
                activeFilters.Clear();
                currentResults = GetDefaultTop(heroes, topCount);
                totalMatchCount = heroes.Count;
                currentSortColumn = null;
                sortDescending = false;
                continue;
            }

            if (action == "🦸 Select hero")
            {
                SelectAndShowHero(currentResults, heroes);
                continue;
            }

            if (action == "📊 Show statistics")
            {
                AnsiConsole.Clear();
                ShowSummary(currentResults, heroes.Count);
                AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
                System.Console.ReadKey(intercept: true);
                continue;
            }

            if (action == "✏️ Manage heroes")
            {
                var dataChanged = ManageHeroes(heroes);
                if (dataChanged)
                {
                    activeFilters.Clear();
                    currentResults = GetDefaultTop(heroes, topCount);
                    totalMatchCount = heroes.Count;
                    currentSortColumn = null;
                    sortDescending = false;
                }
                continue;
            }

            if (action == "🔍 Filter data")
            {
                var (filtered, filteredTotal) = BuildFilteredResults(heroes, activeFilters, topCount);
                if (filtered.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No heroes matched your filters. Showing default top 20 again.[/]");
                    Thread.Sleep(1200);
                    activeFilters.Clear();
                    currentResults = GetDefaultTop(heroes, topCount);
                    totalMatchCount = heroes.Count;
                }
                else
                {
                    currentResults = filtered;
                    totalMatchCount = filteredTotal;
                }

                continue;
            }

            var (sortedResults, chosenColumn, chosenDesc) = BuildSortedResults(currentResults, topCount);
            currentResults = sortedResults;
            currentSortColumn = chosenColumn;
            sortDescending = chosenDesc;
        }
    }

    private static (List<Hero> Results, int TotalMatches) BuildFilteredResults(IReadOnlyList<Hero> heroes, List<HeroFilter> activeFilters, int topCount)
    {
        var filterAction = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Filter options[/]")
                .AddChoices("Add one filter", "Clear filters", "Back")
                .EnableSearch()
                .SearchPlaceholderText("[grey]Type to search demos...[/]")
                .HighlightStyle(Style.Parse("cyan")));

        if (filterAction == "Clear filters")
        {
            activeFilters.Clear();
            AnsiConsole.MarkupLine("[green]All filters were cleared.[/]");
            Thread.Sleep(700);
            return (GetDefaultTop(heroes, topCount), heroes.Count);
        }

        if (filterAction == "Back")
        {
            if (activeFilters.Count == 0)
                return (GetDefaultTop(heroes, topCount), heroes.Count);

            var backFiltered = ApplyActiveFilters(heroes, activeFilters).ToList();
            return (backFiltered.Take(topCount).ToList(), backFiltered.Count);
        }

        activeFilters.Add(PromptForSingleFilter(heroes));

        var filtered = ApplyActiveFilters(heroes, activeFilters).ToList();

        return (filtered.Take(topCount).ToList(), filtered.Count);
    }

    private static void SelectAndShowHero(IReadOnlyList<Hero> currentResults, IReadOnlyList<Hero> allHeroes)
    {
        if (allHeroes.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No heroes available to select.[/]");
            Thread.Sleep(700);
            return;
        }

        var selectedHero = PromptToPickHero(allHeroes, "[bold]Select a hero[/]");

        AnsiConsole.WriteLine();
        DisplayHeroDetails(selectedHero);
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        System.Console.ReadKey(intercept: true);
    }

    private static Hero PromptToPickHero(IReadOnlyList<Hero> matches, string title)
    {
        var options = matches
            .Select(hero => $"{hero.Id} - {Markup.Escape(hero.HeroName)} ({Markup.Escape(hero.Universe)})")
            .ToList();

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(title)
                .PageSize(20)
                .AddChoices(options)
                .EnableSearch()
                .SearchPlaceholderText("[grey]Type ID or name to jump...[/]")
                .HighlightStyle(Style.Parse("cyan")));

        return matches[options.IndexOf(selected)];
    }

    private static HeroFilter PromptForSingleFilter(IReadOnlyList<Hero> heroes)
    {
        var column = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a column to filter")
                .PageSize(12)
                .AddChoices("Hero Name", "Real Name", "Powers", "Universe", "Team", "Power Level", "Status", "Type"));

        return column switch
        {
            "Hero Name" => BuildTextContainsFilter("Hero Name", hero => hero.HeroName),
            "Real Name" => BuildTextContainsFilter("Real Name", hero => hero.RealName),
            "Powers" => BuildTextContainsFilter("Powers", hero => hero.Powers),
            "Universe" => BuildValueSelectionFilter("Universe", heroes.Select(h => h.Universe), hero => hero.Universe),
            "Team" => BuildValueSelectionFilter("Team", heroes.Select(h => h.Team), hero => hero.Team),
            "Power Level" => BuildNumericFilter("Power Level", hero => hero.PowerLevel),
            "Status" => BuildValueSelectionFilter("Status", heroes.Select(h => h.Status), hero => hero.Status),
            "Type" => BuildValueSelectionFilter("Type", heroes.Select(h => h.Type), hero => hero.Type),
            _ => BuildTextContainsFilter("Hero Name", hero => hero.HeroName)
        };
    }

    private static HeroFilter BuildTextContainsFilter(string column, Func<Hero, string> selector)
    {
        var value = AnsiConsole.Prompt(
            new TextPrompt<string>($"Enter text for [green]{column}[/] contains")
                .ValidationErrorMessage("[red]Please enter a value[/]")
                .Validate(input => string.IsNullOrWhiteSpace(input)
                    ? ValidationResult.Error("Value cannot be empty.")
                    : ValidationResult.Success()));

        return new HeroFilter(
            column,
            "contains",
            value,
            hero => selector(hero).Contains(value, StringComparison.OrdinalIgnoreCase));
    }

    private static HeroFilter BuildValueSelectionFilter(string column, IEnumerable<string> values, Func<Hero, string> selector)
    {
        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"Select [green]{column}[/] value")
                .PageSize(10)
                .AddChoices(values.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(v => v).ToList()));

        return new HeroFilter(
            column,
            "equals",
            selected,
            hero => selector(hero).Equals(selected, StringComparison.OrdinalIgnoreCase));
    }

    private static HeroFilter BuildNumericFilter(string column, Func<Hero, int> selector)
    {
        var operation = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"Choose operator for [green]{column}[/]")
                .AddChoices("=", ">=", "<="));

        var value = AnsiConsole.Prompt(
            new TextPrompt<int>($"Enter [green]{column}[/] value"));

        Func<Hero, bool> predicate = operation switch
        {
            "=" => hero => selector(hero) == value,
            ">=" => hero => selector(hero) >= value,
            "<=" => hero => selector(hero) <= value,
            _ => hero => selector(hero) == value
        };

        return new HeroFilter(column, operation, value.ToString(), predicate);
    }

    private static bool ManageHeroes(List<Hero> heroes)
    {
        var subAction = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Manage Heroes[/]")
                .AddChoices("➕ Add hero", "✏️ Update hero", "🗑️ Delete hero", "↩ Back")
                .HighlightStyle(Style.Parse("cyan")));

        return subAction switch
        {
            "➕ Add hero"   => AddHero(heroes),
            "✏️ Update hero" => UpdateHero(heroes),
            "🗑️ Delete hero" => DeleteHero(heroes),
            _               => false
        };
    }

    private static bool AddHero(List<Hero> heroes)
    {
        AnsiConsole.Write(new Rule("[bold green]Add New Hero[/]").RuleStyle("green dim"));
        AnsiConsole.WriteLine();

        var heroName = AnsiConsole.Prompt(
            new TextPrompt<string>("[yellow]Hero Name:[/]")
                .Validate(s => !string.IsNullOrWhiteSpace(s)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Required.")));

        var realName = AnsiConsole.Prompt(
            new TextPrompt<string>("[yellow]Real Name:[/]")
                .Validate(s => !string.IsNullOrWhiteSpace(s)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Required.")));

        var powers = AnsiConsole.Prompt(
            new TextPrompt<string>("[yellow]Powers[/] [grey](avoid commas):[/]")
                .Validate(s => !string.IsNullOrWhiteSpace(s)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Required.")));

        var universe = PromptPickOrCustom("Universe", heroes.Select(h => h.Universe), string.Empty);
        var team     = PromptPickOrCustom("Team",     heroes.Select(h => h.Team),     string.Empty);

        var powerLevel = AnsiConsole.Prompt(
            new TextPrompt<int>("[yellow]Power Level[/] [grey](1-100):[/]")
                .Validate(n => n is >= 1 and <= 100
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Enter a value between 1 and 100.")));

        var status = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Status:[/]")
                .AddChoices("Active", "Retired", "Deceased")
                .HighlightStyle(Style.Parse("cyan")));

        var type = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Type:[/]")
                .AddChoices("Hero", "Villain", "Anti-Hero")
                .HighlightStyle(Style.Parse("cyan")));

        var nextId  = heroes.Count == 0 ? 1 : heroes.Max(h => h.Id) + 1;
        var newHero = new Hero(nextId, heroName, realName, powers, universe, team, powerLevel, status, type);

        heroes.Add(newHero);
        HeroCsvRepository.SaveHeroes(heroes);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]Hero [bold]{Markup.Escape(heroName)}[/] added with ID {nextId}.[/]");
        Thread.Sleep(1000);
        return true;
    }

    private static bool UpdateHero(List<Hero> heroes)
    {
        if (heroes.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No heroes to update.[/]");
            Thread.Sleep(700);
            return false;
        }

        var hero = PromptToPickHero(heroes, "[bold]Select hero to update[/]");

        AnsiConsole.Write(new Rule($"[bold cyan]Updating: {Markup.Escape(hero.HeroName)}[/]").RuleStyle("cyan dim"));
        AnsiConsole.WriteLine();

        var field = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Which field to update?")
                .PageSize(12)
                .AddChoices("Hero Name", "Real Name", "Powers", "Universe", "Team", "Power Level", "Status", "Type")
                .HighlightStyle(Style.Parse("cyan")));

        Hero updated = field switch
        {
            "Hero Name"   => hero with { HeroName   = AnsiConsole.Prompt(new TextPrompt<string>("[yellow]Hero Name:[/]").DefaultValue(hero.HeroName)) },
            "Real Name"   => hero with { RealName   = AnsiConsole.Prompt(new TextPrompt<string>("[yellow]Real Name:[/]").DefaultValue(hero.RealName)) },
            "Powers"      => hero with { Powers     = AnsiConsole.Prompt(new TextPrompt<string>("[yellow]Powers[/] [grey](avoid commas):[/]").DefaultValue(hero.Powers)) },
            "Universe"    => hero with { Universe   = PromptPickOrCustom("Universe", heroes.Select(h => h.Universe), hero.Universe) },
            "Team"        => hero with { Team       = PromptPickOrCustom("Team",     heroes.Select(h => h.Team),     hero.Team) },
            "Power Level" => hero with { PowerLevel = AnsiConsole.Prompt(new TextPrompt<int>("[yellow]Power Level[/] [grey](1-100):[/]").DefaultValue(hero.PowerLevel).Validate(n => n is >= 1 and <= 100 ? ValidationResult.Success() : ValidationResult.Error("Enter a value between 1 and 100."))) },
            "Status"      => hero with { Status     = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("[yellow]Status:[/]").AddChoices("Active", "Retired", "Deceased").HighlightStyle(Style.Parse("cyan"))) },
            "Type"        => hero with { Type       = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("[yellow]Type:[/]").AddChoices("Hero", "Villain", "Anti-Hero").HighlightStyle(Style.Parse("cyan"))) },
            _             => hero
        };

        var index = heroes.IndexOf(hero);
        heroes[index] = updated;
        HeroCsvRepository.SaveHeroes(heroes);

        AnsiConsole.MarkupLine($"[green]{Markup.Escape(field)} updated.[/]");
        Thread.Sleep(800);
        return true;
    }

    private static bool DeleteHero(List<Hero> heroes)
    {
        if (heroes.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No heroes to delete.[/]");
            Thread.Sleep(700);
            return false;
        }

        var hero = PromptToPickHero(heroes, "[bold red]Select hero to delete[/]");

        AnsiConsole.WriteLine();
        DisplayHeroDetails(hero);
        AnsiConsole.WriteLine();

        var confirmed = AnsiConsole.Prompt(
            new ConfirmationPrompt($"[red]Delete [bold]{Markup.Escape(hero.HeroName)}[/]? This cannot be undone.[/]"));

        if (!confirmed)
        {
            AnsiConsole.MarkupLine("[grey]Delete cancelled.[/]");
            Thread.Sleep(600);
            return false;
        }

        heroes.Remove(hero);
        HeroCsvRepository.SaveHeroes(heroes);

        AnsiConsole.MarkupLine($"[red]{Markup.Escape(hero.HeroName)} deleted.[/]");
        Thread.Sleep(800);
        return true;
    }

    private static string PromptPickOrCustom(string label, IEnumerable<string> existingValues, string defaultValue)
    {
        var options = existingValues
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(v => v)
            .Append("✏️ Custom...")
            .ToList();

        var pick = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[yellow]{label}:[/]")
                .PageSize(12)
                .AddChoices(options)
                .HighlightStyle(Style.Parse("cyan")));

        return pick == "✏️ Custom..."
            ? AnsiConsole.Prompt(
                new TextPrompt<string>($"[yellow]Enter {label}:[/]")
                    .DefaultValue(string.IsNullOrWhiteSpace(defaultValue) ? "" : defaultValue))
            : pick;
    }

    private static IEnumerable<Hero> ApplyActiveFilters(IEnumerable<Hero> heroes, IReadOnlyCollection<HeroFilter> activeFilters)
    {
        return activeFilters.Count == 0
            ? heroes
            : heroes.Where(hero => activeFilters.All(filter => filter.Predicate(hero)));
    }

    private static List<Hero> GetDefaultTop(IEnumerable<Hero> heroes, int count)
    {
        return heroes
            .OrderByDescending(h => h.PowerLevel)
            .Take(count)
            .ToList();
    }

    private static void ShowStatusBar(
        IReadOnlyCollection<HeroFilter> activeFilters,
        string? sortColumn,
        bool sortDescending,
        int displayedCount,
        int matchedCount,
        int totalInDataset)
    {
        var parts = new List<string>();

        // Filter section
        if (activeFilters.Count == 0)
        {
            parts.Add("[grey]Filter:[/] [dim]none[/]");
        }
        else
        {
            var filterText = string.Join(", ", activeFilters.Select(f => $"[yellow]{Markup.Escape(f.Column)}[/] [grey]{Markup.Escape(f.Operator)}[/] [cyan]'{Markup.Escape(f.Value)}'[/]"));
            parts.Add($"[grey]Filter:[/] {filterText}");
        }

        // Sort section
        if (string.IsNullOrEmpty(sortColumn))
        {
            parts.Add("[grey]Sorted by:[/] [dim]Power Level (↓ default)[/]");
        }
        else
        {
            var arrow = sortDescending ? "↓" : "↑";
            var direction = sortDescending ? "Descending" : "Ascending";
            parts.Add($"[grey]Sorted by:[/] [cyan]{Markup.Escape(sortColumn)}[/] [grey]({arrow} {direction})[/]");
        }

        // Showing section
        var showingColor = displayedCount < matchedCount ? "yellow" : "green";
        parts.Add($"[grey]Showing[/] [{showingColor}]{displayedCount}[/] [grey]of[/] [cyan]{matchedCount}[/] [grey]matched[/] [dim]({totalInDataset} total)[/]");

        AnsiConsole.MarkupLine(" " + string.Join("  [grey]|[/]  ", parts));
    }

    private static (List<Hero> Results, string SortColumn, bool Descending) BuildSortedResults(IReadOnlyList<Hero> heroes, int topCount)
    {
        if (heroes.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No rows available to sort.[/]");
            Thread.Sleep(800);
            return ([], string.Empty, false);
        }

        var sortColumn = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Sort by")
                .AddChoices("Hero Name", "Power Level", "Universe", "Team", "Status", "Type"));

        var sortDirection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Sort direction")
                .AddChoices("Ascending", "Descending"));

        var descending = sortDirection == "Descending";

        return (ApplySort(heroes, sortColumn, descending).Take(topCount).ToList(), sortColumn, descending);
    }

    private static IEnumerable<Hero> ApplySort(IEnumerable<Hero> heroes, string sortColumn, bool descending)
    {
        return (sortColumn, descending) switch
        {
            ("Hero Name", false) => heroes.OrderBy(h => h.HeroName),
            ("Hero Name", true) => heroes.OrderByDescending(h => h.HeroName),
            ("Power Level", false) => heroes.OrderBy(h => h.PowerLevel),
            ("Power Level", true) => heroes.OrderByDescending(h => h.PowerLevel),
            ("Universe", false) => heroes.OrderBy(h => h.Universe),
            ("Universe", true) => heroes.OrderByDescending(h => h.Universe),
            ("Team", false) => heroes.OrderBy(h => h.Team),
            ("Team", true) => heroes.OrderByDescending(h => h.Team),
            ("Status", false) => heroes.OrderBy(h => h.Status),
            ("Status", true) => heroes.OrderByDescending(h => h.Status),
            ("Type", false) => heroes.OrderBy(h => h.Type),
            ("Type", true) => heroes.OrderByDescending(h => h.Type),
            _ => heroes.OrderBy(h => h.HeroName)
        };
    }

    private static void ShowSummary(IReadOnlyCollection<Hero> heroes, int totalInDataset)
    {
        // ── Pre-compute stats ─────────────────────────────────────────────────
        var avgPowerLevel = heroes.Average(h => h.PowerLevel);
        var activeCount   = heroes.Count(h => h.Status.Equals("Active",    StringComparison.OrdinalIgnoreCase));
        var retiredCount  = heroes.Count(h => h.Status.Equals("Retired",   StringComparison.OrdinalIgnoreCase));
        var deceasedCount = heroes.Count(h => h.Status.Equals("Deceased",  StringComparison.OrdinalIgnoreCase));
        var heroCount     = heroes.Count(h => h.Type.Equals("Hero",        StringComparison.OrdinalIgnoreCase));
        var villainCount  = heroes.Count(h => h.Type.Equals("Villain",     StringComparison.OrdinalIgnoreCase));
        var antiHeroCount = heroes.Count(h => h.Type.Equals("Anti-Hero",   StringComparison.OrdinalIgnoreCase));
        var topHero       = heroes.OrderByDescending(h => h.PowerLevel).First();
        var topVillain    = heroes.Where(h => h.Type.Equals("Villain", StringComparison.OrdinalIgnoreCase))
                                  .OrderByDescending(h => h.PowerLevel).FirstOrDefault();

        var byUniverse = heroes
            .GroupBy(h => h.Universe)
            .Select(g => (Universe: g.Key, Count: g.Count(), AvgPower: g.Average(h => h.PowerLevel)))
            .OrderByDescending(g => g.Count)
            .ToList();

        var byTeam = heroes
            .GroupBy(h => h.Team)
            .Select(g => (Team: g.Key, Count: g.Count()))
            .OrderByDescending(g => g.Count)
            .Take(8)
            .ToList();

        // ── Section title ─────────────────────────────────────────────────────
        AnsiConsole.Write(new Rule("[bold yellow]Hero Dataset Statistics[/]").RuleStyle("yellow dim"));
        AnsiConsole.WriteLine();

        // ── Row 1: three stat panels side by side ─────────────────────────────
        var statGrid = new Grid().Expand();
        statGrid.AddColumn();
        statGrid.AddColumn();
        statGrid.AddColumn();

        var overviewGrid = new Grid();
        overviewGrid.AddColumn().AddColumn();
        overviewGrid.AddRow("[bold]Showing[/]",         $"[cyan bold]{heroes.Count}[/] [grey]of {totalInDataset} total[/]");
        overviewGrid.AddRow("[bold]Avg power level[/]", $"[cyan bold]{avgPowerLevel:F1}[/]");
        overviewGrid.AddRow("[bold]Universes[/]",        $"[cyan bold]{byUniverse.Count}[/]");
        overviewGrid.AddRow("[bold]Teams[/]",            $"[cyan bold]{heroes.Select(h => h.Team).Distinct().Count()}[/]");

        var statusGrid = new Grid();
        statusGrid.AddColumn().AddColumn();
        statusGrid.AddRow("[green bold]Active[/]",   $"[green]{activeCount}[/]");
        statusGrid.AddRow("[yellow bold]Retired[/]", $"[yellow]{retiredCount}[/]");
        statusGrid.AddRow("[red bold]Deceased[/]",   $"[red]{deceasedCount}[/]");

        var typeGrid = new Grid();
        typeGrid.AddColumn().AddColumn();
        typeGrid.AddRow("[cyan bold]Heroes[/]",      $"[cyan]{heroCount}[/]");
        typeGrid.AddRow("[red bold]Villains[/]",     $"[red]{villainCount}[/]");
        typeGrid.AddRow("[yellow bold]Anti-Heroes[/]",$"[yellow]{antiHeroCount}[/]");

        statGrid.AddRow(
            new Panel(overviewGrid).Header("[bold]Overview[/]").Expand().BorderColor(Color.Cyan1),
            new Panel(statusGrid).Header("[bold]Status[/]").Expand().BorderColor(Color.Green),
            new Panel(typeGrid).Header("[bold]Type Breakdown[/]").Expand().BorderColor(Color.Red));

        AnsiConsole.Write(statGrid);
        AnsiConsole.WriteLine();

        // ── Row 2: spotlight panels (top hero / top villain) ──────────────────
        var spotlightGrid = new Grid().Expand();
        spotlightGrid.AddColumn().AddColumn();

        var topHeroContent = new Markup(
            $"[cyan bold]{Markup.Escape(topHero.HeroName)}[/]\n" +
            $"[grey]{Markup.Escape(topHero.RealName)}[/]\n" +
            $"[yellow]Power:[/] {PowerBar(topHero.PowerLevel, 20)}\n" +
            $"[yellow]Universe:[/] {Markup.Escape(topHero.Universe)}  [yellow]Team:[/] {Markup.Escape(topHero.Team)}");

        var topVillainContent = topVillain is null
            ? new Markup("[grey]No villains in current set.[/]")
            : new Markup(
                $"[red bold]{Markup.Escape(topVillain.HeroName)}[/]\n" +
                $"[grey]{Markup.Escape(topVillain.RealName)}[/]\n" +
                $"[yellow]Power:[/] {PowerBar(topVillain.PowerLevel, 20)}\n" +
                $"[yellow]Universe:[/] {Markup.Escape(topVillain.Universe)}  [yellow]Team:[/] {Markup.Escape(topVillain.Team)}");

        spotlightGrid.AddRow(
            new Panel(topHeroContent).Header("[bold green]★ Highest Power Hero[/]").Expand().BorderColor(Color.Green).Padding(1, 0),
            new Panel(topVillainContent).Header("[bold red]★ Highest Power Villain[/]").Expand().BorderColor(Color.Red).Padding(1, 0));

        AnsiConsole.Write(spotlightGrid);
        AnsiConsole.WriteLine();

        // ── Bar chart: count by Universe ──────────────────────────────────────
        var universeChart = new BarChart()
            .Width(60)
            .Label("[bold]Entries by Universe[/]")
            .CenterLabel();

        var universeColors = new[] { Color.CornflowerBlue, Color.MediumPurple, Color.Teal, Color.Orange1, Color.HotPink };
        for (var i = 0; i < byUniverse.Count; i++)
        {
            universeChart.AddItem(byUniverse[i].Universe, byUniverse[i].Count, universeColors[i % universeColors.Length]);
        }

        AnsiConsole.Write(new Panel(universeChart).Header("[bold]Universe Distribution[/]").Expand().BorderColor(Color.CornflowerBlue));
        AnsiConsole.WriteLine();

        // ── Bar chart: type breakdown ─────────────────────────────────────────
        var typeChart = new BarChart()
            .Width(60)
            .Label("[bold]Entries by Type[/]")
            .CenterLabel()
            .AddItem("Heroes",      heroCount,     Color.Cyan1)
            .AddItem("Villains",    villainCount,  Color.Red)
            .AddItem("Anti-Heroes", antiHeroCount, Color.Yellow);

        AnsiConsole.Write(new Panel(typeChart).Header("[bold]Type Breakdown[/]").Expand().BorderColor(Color.Yellow));
        AnsiConsole.WriteLine();

        // ── Table: Top 10 by Power Level ──────────────────────────────────────
        var top10 = heroes.OrderByDescending(h => h.PowerLevel).Take(10).ToList();
        var top10Table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Gold1)
            .AddColumn(new TableColumn("[bold]#[/]").Centered())
            .AddColumn("[bold]Hero Name[/]")
            .AddColumn("[bold]Universe[/]")
            .AddColumn("[bold]Type[/]")
            .AddColumn("[bold]Power Level[/]");

        for (var i = 0; i < top10.Count; i++)
        {
            var h = top10[i];
            var rank = i switch { 0 => "[gold1]🥇[/]", 1 => "[silver]🥈[/]", 2 => "[darkorange]🥉[/]", _ => $"[grey]{i + 1}[/]" };
            top10Table.AddRow(rank, Markup.Escape(h.HeroName), Markup.Escape(h.Universe), ColorizeType(h.Type), PowerBar(h.PowerLevel, 20));
        }

        AnsiConsole.Write(new Panel(top10Table).Header("[bold gold1]Top 10 by Power Level[/]").Expand().BorderColor(Color.Gold1));
        AnsiConsole.WriteLine();

        // ── Table: Universe averages ──────────────────────────────────────────
        var univTable = new Table()
            .Border(TableBorder.Simple)
            .BorderColor(Color.CornflowerBlue)
            .AddColumn("[bold]Universe[/]")
            .AddColumn(new TableColumn("[bold]Count[/]").Centered())
            .AddColumn(new TableColumn("[bold]Avg Power[/]").Centered())
            .AddColumn(new TableColumn("[bold]Max Power[/]").Centered());

        foreach (var (univ, count, avgPow) in byUniverse)
        {
            var max = heroes.Where(h => h.Universe == univ).Max(h => h.PowerLevel);
            univTable.AddRow(
                Markup.Escape(univ),
                $"[cyan]{count}[/]",
                $"[yellow]{avgPow:F1}[/]",
                $"[green]{max}[/]");
        }

        AnsiConsole.Write(new Panel(univTable).Header("[bold]Universe Power Stats[/]").Expand().BorderColor(Color.CornflowerBlue));
        AnsiConsole.WriteLine();
    }

    private static void ShowTable(IReadOnlyList<Hero> heroes, int totalResults, int maxRows)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("ID")
            .AddColumn("Hero Name")
            .AddColumn("Real Name")
            .AddColumn("Powers")
            .AddColumn("Universe")
            .AddColumn("Team")
            .AddColumn("Power Level")
            .AddColumn("Status")
            .AddColumn("Type");

        foreach (var hero in heroes)
        {
            table.AddRow(
                hero.Id.ToString(),
                Markup.Escape(hero.HeroName),
                Markup.Escape(hero.RealName),
                Markup.Escape(hero.Powers),
                Markup.Escape(hero.Universe),
                Markup.Escape(hero.Team),
                hero.PowerLevel.ToString(),
                ColorizeStatus(hero.Status),
                ColorizeType(hero.Type));
        }

        AnsiConsole.Write(table);
    }

    private static string PowerBar(int level, int barWidth = 30)
    {
        var filled = (int)Math.Round(level / 100.0 * barWidth);
        var empty = barWidth - filled;
        return $"[green]{new string('█', filled)}[/][grey]{new string('░', empty)}[/] [green]{level}[/][grey]/100[/]";
    }

    private static void DisplayHeroDetails(Hero hero)
    {
        var panel = new Panel(
            new Markup($"""
                [yellow]ID:[/] {hero.Id}
                [yellow]Hero Name:[/] [cyan bold]{Markup.Escape(hero.HeroName)}[/]
                [yellow]Real Name:[/] {Markup.Escape(hero.RealName)}
                [yellow]Powers:[/] {Markup.Escape(hero.Powers)}
                [yellow]Universe:[/] {Markup.Escape(hero.Universe)}
                [yellow]Team:[/] {Markup.Escape(hero.Team)}
                [yellow]Power Level:[/] {PowerBar(hero.PowerLevel)}
                [yellow]Status:[/] {ColorizeStatus(hero.Status)}
                [yellow]Type:[/] {ColorizeType(hero.Type)}
                """))
            .Header("[bold blue]Hero Details[/]")
            .BorderColor(Color.Cyan1)
            .Padding(1, 1);

        AnsiConsole.Write(panel);
    }

    private static string ColorizeType(string type)
    {
        var escaped = Markup.Escape(type);

        return type.ToLowerInvariant() switch
        {
            "hero" => $"[cyan]{escaped}[/]",
            "villain" => $"[red]{escaped}[/]",
            "anti-hero" => $"[yellow]{escaped}[/]",
            _ => escaped
        };
    }

    private static string ColorizeStatus(string status)
    {
        var escaped = Markup.Escape(status);

        return status.ToLowerInvariant() switch
        {
            "active" => $"[green]{escaped}[/]",
            "retired" => $"[yellow]{escaped}[/]",
            "deceased" => $"[red]{escaped}[/]",
            _ => escaped
        };
    }
}

internal static class HeroCsvRepository
{
    public static void SaveHeroes(List<Hero> heroes)
    {
        var path = ResolveCsvPath();

        if (path is null)
        {
            AnsiConsole.MarkupLine("[red]Could not resolve CSV path — changes were not saved.[/]");
            return;
        }

        var lines = new List<string>(heroes.Count + 1)
        {
            "ID,Hero Name,Real Name,Powers,Universe,Team,Power Level,Status,Type"
        };

        lines.AddRange(heroes.Select(h =>
            string.Join(",", new[]
            {
                h.Id.ToString(),
                QuoteCsvField(h.HeroName),
                QuoteCsvField(h.RealName),
                QuoteCsvField(h.Powers),
                QuoteCsvField(h.Universe),
                QuoteCsvField(h.Team),
                h.PowerLevel.ToString(),
                QuoteCsvField(h.Status),
                QuoteCsvField(h.Type),
            })));

        File.WriteAllLines(path, lines);
    }

    public static List<Hero> LoadHeroes()
    {
        var path = ResolveCsvPath();

        if (path is null || !File.Exists(path))
        {
            return [];
        }

        var lines = File.ReadAllLines(path);

        return lines
            .Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(ParseHero)
            .ToList();
    }

    private static string? ResolveCsvPath()
    {
        var currentDirPath = Path.Combine(Directory.GetCurrentDirectory(), "sample-data", "heroes.csv");
        if (File.Exists(currentDirPath))
        {
            return currentDirPath;
        }

        var candidate = AppContext.BaseDirectory;
        for (var i = 0; i < 6; i++)
        {
            var filePath = Path.Combine(candidate, "sample-data", "heroes.csv");
            if (File.Exists(filePath))
            {
                return filePath;
            }

            var parent = Directory.GetParent(candidate);
            if (parent is null)
            {
                break;
            }

            candidate = parent.FullName;
        }

        return null;
    }

    private static Hero ParseHero(string line)
    {
        var fields = ParseCsvLine(line);
        if (fields.Length < 9)
        {
            throw new InvalidOperationException($"Invalid hero CSV row: {line}");
        }

        return new Hero(
            int.Parse(fields[0]),
            fields[1],
            fields[2],
            fields[3],
            fields[4],
            fields[5],
            int.Parse(fields[6]),
            fields[7],
            fields[8]);
    }

    // RFC 4180-compliant CSV line parser — handles quoted fields with embedded commas and "" escapes
    private static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var sb = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    // Escaped quote ("") inside a quoted field
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    fields.Add(sb.ToString().Trim());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
        }

        fields.Add(sb.ToString().Trim());
        return [.. fields];
    }

    private static string QuoteCsvField(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}

internal sealed record Hero(
    int Id,
    string HeroName,
    string RealName,
    string Powers,
    string Universe,
    string Team,
    int PowerLevel,
    string Status,
    string Type);

internal sealed record HeroFilter(
    string Column,
    string Operator,
    string Value,
    Func<Hero, bool> Predicate);

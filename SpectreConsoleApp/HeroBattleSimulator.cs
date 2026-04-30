using Spectre.Console;

internal static class HeroBattleSimulator
{
    private static readonly Random Rng = new();

    public static void Run()
    {
        var heroes = HeroCsvRepository.LoadHeroes();

        if (heroes.Count < 2)
        {
            AnsiConsole.MarkupLine("[red]Need at least 2 heroes in the dataset to run a battle.[/]");
            return;
        }

        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("BATTLE!").Centered().Color(Color.Red));
        AnsiConsole.Write(new Rule("[bold red]Hero Battle Simulator[/]").RuleStyle("red dim"));
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold cyan]Player 1 — choose your fighter![/]");
        AnsiConsole.WriteLine();
        var hero1 = PickHero(heroes, "Select [bold cyan]Fighter 1[/]");

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold yellow]Player 2 — choose your fighter![/]");
        AnsiConsole.WriteLine();
        var hero2 = PickHero(heroes.Where(h => h.Id != hero1.Id).ToList(), "Select [bold yellow]Fighter 2[/]");

        AnsiConsole.Clear();
        RunBattle(hero1, hero2);
    }

    private static Hero PickHero(IReadOnlyList<Hero> choices, string title)
    {
        var sorted = choices.OrderBy(h => h.HeroName).ToList();
        var options = sorted
            .Select(h => $"{h.Id,3} - {Markup.Escape(h.HeroName),-22} [[{Markup.Escape(h.Universe),-6}]] PWR:{h.PowerLevel,3}")
            .ToList();

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(title)
                .PageSize(20)
                .AddChoices(options)
                .EnableSearch()
                .SearchPlaceholderText("[grey]Type to search heroes...[/]")
                .HighlightStyle(Style.Parse("cyan")));

        return sorted[options.IndexOf(selected)];
    }

    private static void RunBattle(Hero h1, Hero h2)
    {
        var f1 = new BattleHero(h1);
        var f2 = new BattleHero(h2);
        var log = new List<string>();
        int round = 1;
        bool p1Attacking = f1.Hero.PowerLevel >= f2.Hero.PowerLevel;
        BattleHero? winner = null;

        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .Centered()
            .AddColumn(new TableColumn(string.Empty));

        AnsiConsole.Live(table)
            .AutoClear(false)
            .Start(ctx =>
            {
                RenderBattle(table, f1, f2, log, round, p1Attacking,
                    "[grey]Press any key to begin the battle...[/]");
                ctx.Refresh();
                Console.ReadKey(intercept: true);

                while (f1.CurrentHp > 0 && f2.CurrentHp > 0)
                {
                    var attacker = p1Attacking ? f1 : f2;
                    var defender = p1Attacking ? f2 : f1;

                    RenderBattle(table, f1, f2, log, round, p1Attacking,
                        $"[grey]Press any key —[/] [bold]{Markup.Escape(attacker.Hero.HeroName)}[/] [grey]attacks![/]");
                    ctx.Refresh();
                    Console.ReadKey(intercept: true);

                    var (damage, message) = CalculateAttack(attacker, defender);
                    defender.CurrentHp = Math.Max(0, defender.CurrentHp - damage);
                    log.Add(message);
                    if (log.Count > 5) log.RemoveAt(0);

                    if (defender.CurrentHp <= 0)
                    {
                        winner = attacker;
                        RenderBattle(table, f1, f2, log, round, p1Attacking,
                            $"[bold gold1]{Markup.Escape(attacker.Hero.HeroName)} WINS![/]  [grey]Press any key to continue...[/]");
                        ctx.Refresh();
                        Console.ReadKey(intercept: true);
                        break;
                    }

                    p1Attacking = !p1Attacking;
                    round++;
                }
            });

        AnsiConsole.Clear();
        ShowVictory(winner!);
    }

    private static (int Damage, string Message) CalculateAttack(BattleHero attacker, BattleHero defender)
    {
        var roll = Rng.NextDouble();

        if (roll < 0.08)
            return (0, $"[grey]{Markup.Escape(attacker.Hero.HeroName)}[/] attacks but [yellow]misses![/]");

        var baseDamage = Rng.Next(
            (int)(attacker.MaxHp * 0.10),
            (int)(attacker.MaxHp * 0.20) + 1);

        bool crit = roll > 0.85;
        if (crit) baseDamage = (int)(baseDamage * 1.75);

        var absorbed = Rng.Next(0, defender.Hero.PowerLevel / 8 + 1);
        var finalDamage = Math.Max(1, baseDamage - absorbed);

        var powers = attacker.Hero.Powers.Split('/');
        var power = powers[Rng.Next(powers.Length)].Trim();

        var nameColor = AttackerColor(attacker);
        var critTag = crit ? " [bold red]** CRITICAL **[/]" : "";
        var msg = $"[{nameColor}]{Markup.Escape(attacker.Hero.HeroName)}[/] uses [italic]{Markup.Escape(power)}[/]! " +
                  $"[red]-{finalDamage} HP[/]{critTag}";

        return (finalDamage, msg);
    }

    private static void RenderBattle(
        Table table,
        BattleHero f1,
        BattleHero f2,
        IReadOnlyList<string> log,
        int round,
        bool p1Attacking,
        string prompt)
    {
        table.Rows.Clear();

        table.AddRow(new Markup($"[bold red]-- HERO BATTLE --[/]  [grey]Round {round}[/]").Centered());
        table.AddRow(new Text(string.Empty));

        var grid = new Grid().Expand();
        grid.AddColumn(new GridColumn());
        grid.AddColumn(new GridColumn());
        grid.AddRow(
            BuildFighterPanel(f1, isAttacking: p1Attacking),
            BuildFighterPanel(f2, isAttacking: !p1Attacking));
        table.AddRow(grid);
        table.AddRow(new Text(string.Empty));

        if (log.Count > 0)
        {
            table.AddRow(
                new Panel(new Markup(string.Join("\n", log)))
                    .Header("[bold]Battle Log[/]")
                    .BorderColor(Color.Grey23)
                    .Expand());
            table.AddRow(new Text(string.Empty));
        }

        table.AddRow(new Markup(prompt).Centered());
    }

    private static Panel BuildFighterPanel(BattleHero fighter, bool isAttacking)
    {
        var pct = (double)fighter.CurrentHp / fighter.MaxHp;
        const int barWidth = 22;
        var filled = (int)Math.Round(pct * barWidth);
        var empty = barWidth - filled;
        var barColor = pct > 0.60 ? "green" : pct > 0.30 ? "yellow" : "red";
        var hpBar = $"[{barColor}]{new string('█', filled)}[/][grey]{new string('░', empty)}[/]";

        var nameColor = AttackerColor(fighter);
        var borderColor = isAttacking
            ? (fighter.Hero.Type.Equals("Villain", StringComparison.OrdinalIgnoreCase) ? Color.Red : Color.Cyan1)
            : Color.Grey23;

        var powers = fighter.Hero.Powers.Length > 38
            ? fighter.Hero.Powers[..38] + "..."
            : fighter.Hero.Powers;

        var statusLine = fighter.CurrentHp <= 0
            ? "[bold red]DEFEATED[/]"
            : isAttacking
                ? $"[bold {nameColor}]>> ATTACKING[/]"
                : "[grey]standing by[/]";

        var content = new Markup(
            $"[bold {nameColor}]{Markup.Escape(fighter.Hero.HeroName)}[/]\n" +
            $"[grey]{Markup.Escape(fighter.Hero.RealName)}[/]\n" +
            $"[dim]{Markup.Escape(fighter.Hero.Universe)} · {Markup.Escape(fighter.Hero.Team)}[/]\n" +
            $"\n" +
            $"[grey dim]{Markup.Escape(powers)}[/]\n" +
            $"\n" +
            $"[yellow]HP[/] [bold]{fighter.CurrentHp}[/][grey]/{fighter.MaxHp}[/]\n" +
            $"{hpBar}\n" +
            $"\n" +
            $"{statusLine}");

        return new Panel(content)
            .Header($"[bold]PWR {fighter.Hero.PowerLevel}[/]")
            .BorderColor(borderColor)
            .Padding(1, 0)
            .Expand();
    }

    private static void ShowVictory(BattleHero winner)
    {
        AnsiConsole.Write(new FigletText("VICTORY!").Centered().Color(Color.Gold1));
        AnsiConsole.WriteLine();

        var pct = (double)winner.CurrentHp / winner.MaxHp;
        const int barWidth = 30;
        var filled = (int)Math.Round(pct * barWidth);
        var hpBar = $"[green]{new string('█', filled)}[/][grey]{new string('░', barWidth - filled)}[/]";
        var nameColor = AttackerColor(winner);

        var panel = new Panel(
            new Markup(
                $"[bold {nameColor}]{Markup.Escape(winner.Hero.HeroName)}[/] [white]is victorious![/]\n\n" +
                $"[grey]Real Name:[/]   {Markup.Escape(winner.Hero.RealName)}\n" +
                $"[grey]Universe:[/]    {Markup.Escape(winner.Hero.Universe)}\n" +
                $"[grey]Team:[/]        {Markup.Escape(winner.Hero.Team)}\n" +
                $"[grey]Power Level:[/] [bold]{winner.Hero.PowerLevel}[/]\n\n" +
                $"[yellow]Remaining HP[/]  [bold]{winner.CurrentHp}[/][grey]/{winner.MaxHp}[/]\n" +
                $"{hpBar}"))
            .Header("[bold gold1]Champion[/]", Justify.Center)
            .BorderColor(Color.Gold1)
            .Padding(2, 1);

        AnsiConsole.Write(panel);
    }

    private static string AttackerColor(BattleHero fighter) =>
        fighter.Hero.Type.ToLowerInvariant() switch
        {
            "villain" => "red",
            "anti-hero" => "yellow",
            _ => "cyan"
        };

    private sealed class BattleHero(Hero hero)
    {
        public Hero Hero { get; } = hero;
        public int MaxHp { get; } = hero.PowerLevel * 10;
        public int CurrentHp { get; set; } = hero.PowerLevel * 10;
    }
}

using Spectre.Console;

internal static class PanelAndMarkupDemo
{
    internal static void Run()
    {
        var panel = new Panel(new Markup("[bold yellow]Welcome to Spectre.Console![/]\n[grey]Build rich terminal apps with ease.[/]"))
            .Border(BoxBorder.Rounded)
            .Header("[aqua]Panel Demo[/]", Justify.Center)
            .Expand();

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]Styles[/], [underline]underline[/], and [bold]bold[/] can be combined.");
    }
}

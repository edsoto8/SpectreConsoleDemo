using Spectre.Console;

internal static class StatusSpinnerDemo
{
    internal static void Run()
    {
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start("Preparing terminal UI sample...", _ =>
            {
                Thread.Sleep(1200);
            });

        AnsiConsole.MarkupLine("[green]Completed sample status workflow.[/]");
    }
}

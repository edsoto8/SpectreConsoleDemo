using Spectre.Console;

internal static class ProgressDemo
{
    internal static void Run()
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
}

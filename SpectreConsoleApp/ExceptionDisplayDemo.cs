using Spectre.Console;

internal static class ExceptionDisplayDemo
{
    internal static void Run()
    {
        AnsiConsole.MarkupLine("[bold]Spectre.Console renders exceptions with rich formatting and colorized stack traces.[/]");
        AnsiConsole.WriteLine();

        var examples = new (string Label, Exception Ex)[]
        {
            ("ArgumentNullException",       new ArgumentNullException("heroName", "Hero name cannot be null.")),
            ("InvalidOperationException",   CaptureInvalidOp()),
            ("ApplicationException",        new ApplicationException("Demo pipeline failed: unexpected state in renderer.")),
        };

        foreach (var (label, ex) in examples)
        {
            AnsiConsole.Write(new Rule($"[grey]{Markup.Escape(label)}[/]").RuleStyle("grey").LeftJustified());
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes);
            AnsiConsole.WriteLine();
        }
    }

    private static Exception CaptureInvalidOp()
    {
        try { throw new InvalidOperationException("Cannot process hero: power level out of valid range (1–100)."); }
        catch (Exception ex) { return ex; }
    }
}

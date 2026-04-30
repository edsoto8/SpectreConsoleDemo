using Spectre.Console;

internal static class PromptsDemo
{
    internal static void Run()
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
}

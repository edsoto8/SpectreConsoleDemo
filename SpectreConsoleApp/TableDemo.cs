using Spectre.Console;

internal static class TableDemo
{
    internal static void Run()
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Feature Matrix[/]")
            .AddColumn("Control")
            .AddColumn("Use Case")
            .AddColumn("Complexity");

        table.AddRow("Panel", "Emphasize key content", "Low");
        table.AddRow("Table", "Structured datasets", "Low");
        table.AddRow("Tree", "Hierarchical data", "Medium");
        table.AddRow("Progress", "Long-running tasks", "Medium");
        table.AddRow("Live", "Realtime dashboards", "High");

        AnsiConsole.Write(table);
    }
}

using Spectre.Console;

internal static class TreeDemo
{
    internal static void Run()
    {
        var tree = new Tree("[bold]Project[/]")
            .Guide(TreeGuide.Line);

        var src = tree.AddNode("[blue]src[/]");
        src.AddNode("[grey]Program.cs[/]");
        src.AddNode("[grey]Widgets/[/]");

        var docs = tree.AddNode("[green]docs[/]");
        docs.AddNode("[grey]README.md[/]");
        docs.AddNode("[grey]CONTRIBUTING.md[/]");

        tree.AddNode("[yellow]SpectreConsoleApp.csproj[/]");

        AnsiConsole.Write(tree);
    }
}

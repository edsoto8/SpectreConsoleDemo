using Spectre.Console;

internal static class BarChartDemo
{
    internal static void Run()
    {
        var chart = new BarChart()
            .Width(60)
            .Label("[green bold underline]Monthly Usage[/]")
            .CenterLabel();

        chart.AddItem("Panels", 42, Color.CadetBlue);
        chart.AddItem("Tables", 87, Color.MediumPurple);
        chart.AddItem("Trees", 55, Color.SeaGreen2);
        chart.AddItem("Progress", 73, Color.Yellow);

        AnsiConsole.Write(chart);
    }
}

using Matrix.Domain.Entities;
using Spectre.Console;

namespace Matrix.Infrastructure.Reports;

public static class WorldConsoleReport
{
    public static void Print(World world)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[cyan]Resultado final[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();

        PrintWorldSummary(world);

        PrintHumans(world);
    }

    private static void PrintWorldSummary(World world)
    {
        int countriesCount = world.Humans.Select(x => x.Location.BirthCountry).Distinct().Count();
        // string title = $"Mundo {world.Name} · ano {world.CurrentYear}";
        string title = world.CurrentYear == 1 ? "Resumo" : $"Resumo dos {world.CurrentYear} anos";

        Panel panel = new(
            $"""
            Humanos: {world.Humans.Count}
            Países: {countriesCount}
            """
        )
        {
            Header = new PanelHeader(title),
            Border = BoxBorder.Rounded,
            Width = 30
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    private static void PrintHumans(World world)
    {
        Table table = new()
        {
            Expand = true
        };

        table.Border(TableBorder.Rounded);

        table.AddColumn("Data de nasc.");
        table.AddColumn("Nome");
        table.AddColumn("Idade");
        table.AddColumn("Status");
        table.AddColumn("País de origem");
        table.AddColumn("Saúde");
        table.AddColumn("Filhos");
        table.AddColumn("Doenças");

        IEnumerable<Human> humans = world.Humans.OrderBy(x => x.Identity.BirthDate);

        foreach (var human in humans)
        {
            table.AddRow(
                human.Identity.BirthDate.ToString(),
                human.Identity.FullName,
                human.Life.Age.ToString(),
                human.Life.IsAlive ? "[cyan]Vivo[/]" : "[red]Morto[/]",
                human.Location.BirthCountryDescription,
                $"{human.Health.Health}%",
                human.Family.ChildrenIds.Count.ToString(),
                human.Health.Diseases.Count.ToString()
            );
        }

        AnsiConsole.Write(table);
    }
}
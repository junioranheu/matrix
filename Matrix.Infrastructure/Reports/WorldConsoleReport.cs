using Matrix.Domain.Entities;
using Matrix.Shared.Extensions;
using Spectre.Console;

namespace Matrix.Infrastructure.Reports;

public static class WorldConsoleReport
{
    public static void Print(World world)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[cyan]Resultado final do mundo {world.Name}[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();

        PrintHumans(world);

        PrintWorldSummary(world);
    }

    private static void PrintWorldSummary(World world)
    {
        int countriesCount = world.Humans.Select(x => x.Location.BirthCountry).Distinct().Count();
        string title = world.CurrentYear == 1 ? "Resumo" : $"Resumo dos {world.CurrentYear} anos";
        DateOnly? lastDeathDate = world.Humans.Where(x => x.Life.DateOfDeath.HasValue).Max(x => x.Life.DateOfDeath);

        Panel panel = new(
            $"""
            Humanos: {world.Humans.Count}
            Países: {countriesCount}
            Ultima morte: {(lastDeathDate is null ? "—" : $"ano {lastDeathDate.Value:yyyy}")}
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
        table.AddColumn("Causa da morte");
        table.AddColumn("País de origem");
        table.AddColumn("Filhos");

        IEnumerable<Human> humans = world.Humans.OrderBy(x => x.Identity.BirthDate);

        foreach (var human in humans)
        {
            table.AddRow(
                human.Identity.BirthDate.ToString(),
                human.Identity.FullName,
                human.Life.Age.ToString(),
                human.Life.IsAlive ? "[cyan]Vivo[/]" : $"[red]Morto em {human.Life.DateOfDeathString}[/]",
                human.Life.CauseOfDeath is null ? "—" : human.Life.CauseOfDeath.GetDescription(),
                human.Location.BirthCountryDescription,
                human.Family.ChildrenIds.Count.ToString()
            );
        }

        AnsiConsole.Write(table);
    }
}
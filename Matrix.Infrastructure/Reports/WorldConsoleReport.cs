using Matrix.Domain.Entities;
using Matrix.Shared.Extensions;
using Spectre.Console;

namespace Matrix.Infrastructure.Reports;

public static class WorldConsoleReport
{
    public static void Print(InitialSettings settings, World world)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[cyan]Resultado final do mundo {world.Name}[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();

        PrintHumans(world);

        WorldYearConsoleReport.Print(settings, world, isFinalReport: true);
    }

    #region methods
    private static void PrintHumans(World world)
    {
        Table table = new()
        {
            Expand = true,
            ShowRowSeparators = true,
            Border = TableBorder.Rounded
        };

        table.AddColumn("Data de nasc.");
        table.AddColumn("Nome");
        table.AddColumn("Gênero");
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
                human.Identity.Gender.GetDescription(),
                human.Life.Age.ToString(),
                human.Life.IsAlive ? "[cyan]Vivo[/]" : $"[red]Morto em {human.Life.DateOfDeathString}[/]",
                human.Life.CauseOfDeath is null ? "—" : human.Life.CauseOfDeath.GetDescription(),
                human.Location.BirthCountryDescription,
                human.Family.ChildrenIds.Count.ToString()
            );
        }

        AnsiConsole.Write(table);
    }
    #endregion
}
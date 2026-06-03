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

        PrintHumans(settings, world);

        WorldYearConsoleReport.Print(settings, world, isFinalReport: true);
    }

    #region methods
    /// <summary>
    /// Exibe a listagem de humanos da simulação e,
    /// opcionalmente, o histórico de eventos de vida.
    /// </summary>
    /// <param name="settings">
    /// Configurações utilizadas para definir quais
    /// informações devem ser exibidas.
    /// </param>
    /// <param name="world">
    /// Mundo contendo os humanos que serão exibidos.
    /// </param>
    private static void PrintHumans(InitialSettings settings, World world)
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
        table.AddColumn("País atual");
        table.AddColumn("Filhos");
        table.AddColumn("Eventos");

        IEnumerable<Human> humans = world.Humans.OrderBy(x => x.Identity.BirthDate);

        foreach (Human human in humans)
        {
            AddHumanRow(table, human);
        }

        AnsiConsole.Write(table);

        if (settings.ShowEvents)
        {
            PrintLifeEventsHistory(humans);
        }
    }

    /// <summary>
    /// Adiciona uma linha representando um humano na tabela.
    /// </summary>
    /// <param name="table">
    /// Tabela utilizada para exibição.
    /// </param>
    /// <param name="human">
    /// Humano que será exibido.
    /// </param>
    private static void AddHumanRow(Table table, Human human)
    {
        table.AddRow(
            human.Identity.BirthDate.ToString(),
            human.Identity.FullName,
            human.Identity.Gender.GetDescription(),
            human.Life.Age.ToString(),
            human.Life.IsAlive ? "[cyan]Vivo[/]" : $"[red]Morto em {human.Life.DateOfDeathString}[/]",
            human.Life.CauseOfDeath is null ? "—" : human.Life.CauseOfDeath.GetDescription(),
            human.Location.BirthCountryDescription,
            human.Location.CurrentCountry.GetDescription(),
            human.Family.ChildrenIds.Count.ToString(),
            $"{human.Life.LifeEvents.Count} eventos"
        );
    }

    /// <summary>
    /// Exibe o histórico de vida dos humanos.
    /// </summary>
    /// <param name="humans">
    /// Humanos que terão seus eventos exibidos.
    /// </param>
    private static void PrintLifeEventsHistory(IEnumerable<Human> humans)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[cyan]Histórico de vida[/]").RuleStyle("grey"));

        foreach (Human human in humans)
        {
            if (human.Life.LifeEvents.Count == 0)
            {
                continue;
            }

            Tree tree = new($"[cyan]{human.Identity.FullName}[/]");

            List<string> lifeEvents = [.. human.Life.LifeEvents];

            int lastAgeEventIndex = lifeEvents.FindLastIndex(x =>
                x.Contains("COMPLETOU", StringComparison.OrdinalIgnoreCase) &&
                x.Contains("ANOS", StringComparison.OrdinalIgnoreCase));

            for (int i = 0; i < lifeEvents.Count; i++)
            {
                string lifeEvent = lifeEvents[i];

                bool isAgeEvent =
                    lifeEvent.Contains("COMPLETOU", StringComparison.OrdinalIgnoreCase) &&
                    lifeEvent.Contains("ANOS", StringComparison.OrdinalIgnoreCase);

                if (isAgeEvent && i != lastAgeEventIndex)
                {
                    continue;
                }

                tree.AddNode(lifeEvent);
            }

            AnsiConsole.Write(tree);
            AnsiConsole.WriteLine();
        }
    }
    #endregion
}
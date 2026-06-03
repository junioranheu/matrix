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
        table.AddColumn("Pais");
        table.AddColumn("Filhos");
        table.AddColumn("Eventos");

        IEnumerable<Human> humans = world.Humans.OrderBy(x => x.Identity.BirthDate);

        Dictionary<Guid, Human> humansByIdDictionary = humans.ToDictionary(x => x.Id);

        foreach (Human human in humans)
        {
            AddHumanRow(table, humansByIdDictionary, human);
        }

        AnsiConsole.Write(table);

        if (settings.ShowEvents)
        {
            PrintLifeEventsHistory(humans);
        }
    }

    /// <summary>
    /// Adiciona uma linha representando um humano na tabela.
    /// Exibe informações pessoais, localização, pais, filhos e eventos de vida.
    /// </summary>
    /// <param name="table">
    /// Tabela utilizada para exibição.
    /// </param>
    /// <param name="humansById">
    /// Dicionário contendo os humanos indexados por identificador para consultas rápidas de parentesco.
    /// </param>
    /// <param name="human">
    /// Humano que será exibido.
    /// </param>
    private static void AddHumanRow(Table table, Dictionary<Guid, Human> humansById, Human human)
    {
        // Pais;
        string parents = string.Join(", ", new[]
        {
            human.Family.FatherId is not null && humansById.TryGetValue(human.Family.FatherId.Value, out Human? fatherHuman) ? fatherHuman.Identity.FullName : null,
            human.Family.MotherId is not null && humansById.TryGetValue(human.Family.MotherId.Value, out Human? motherHuman) ? motherHuman.Identity.FullName : null
        }.Where(x => !string.IsNullOrWhiteSpace(x)));

        // Filhos;
        List<string> childrenNames = [.. human.Family.ChildrenIds.Where(humansById.ContainsKey).Select(id => humansById[id].Identity.FullName)];
        string children = string.Join(", ", childrenNames);
        string childrenDisplay = childrenNames.Count == 0 ? "—" : $"({childrenNames.Count}) {children}";

        // Tabela;
        table.AddRow(
            $"{human.Identity.BirthDate:yyyy}",
            human.Identity.FullName,
            human.Identity.Gender.GetDescription(),
            human.Life.Age.ToString(),
            human.Life.IsAlive ? "[cyan]Vivo[/]" : $"[red]Morto em {human.Life.DateOfDeathString}[/]",
            human.Life.CauseOfDeath is null ? "—" : human.Life.CauseOfDeath.GetDescription(),
            human.Location.BirthCountryDescription,
            human.Location.CurrentCountryDescription,
            string.IsNullOrWhiteSpace(parents) ? "—" : parents,
            string.IsNullOrWhiteSpace(children) ? "—" : $"({childrenNames.Count}) {children}",
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

            int lastBirthdayIndex = human.Life.LifeEvents.FindLastIndex(IsBirthdayEvent);

            for (int i = 0; i < human.Life.LifeEvents.Count; i++)
            {
                string lifeEvent = human.Life.LifeEvents[i];

                if (IsBirthdayEvent(lifeEvent) && i != lastBirthdayIndex)
                {
                    continue;
                }

                tree.AddNode(lifeEvent);
            }

            AnsiConsole.Write(tree);
            AnsiConsole.WriteLine();
        }
    }

    /// <summary>
    /// Indica se o evento representa um aniversário.
    /// </summary>
    /// <param name="lifeEvent">
    /// Evento a ser analisado.
    /// </param>
    /// <returns>
    /// True quando o evento representa um aniversário.
    /// </returns>
    private static bool IsBirthdayEvent(string lifeEvent)
    {
        return lifeEvent.Contains("COMPLETOU", StringComparison.OrdinalIgnoreCase) &&
               lifeEvent.Contains("ANOS", StringComparison.OrdinalIgnoreCase);
    }
    #endregion
}
using Matrix.Domain.Entities;
using Matrix.Domain.Enums;
using Matrix.Shared.Helpers;
using Spectre.Console;

namespace Matrix.Domain.Factories;

/// <summary>
/// Responsável por executar o ciclo principal da simulação,
/// processando a evolução anual dos habitantes e do mundo.
/// </summary>
public sealed class SimulationFactory
{
    public static void Run(InitialSettings settings, World world)
    {
        for (int year = 1; year <= settings.SimulationYears; year++)
        {
            StartYear(world, settings);

            ProcessPopulation(world, settings);

            EndYear(world);
        }
    }

    #region methods
    /// <summary>
    /// Executa as ações de preparação do ano atual.
    /// </summary>
    private static void StartYear(World world, InitialSettings settings)
    {
        ConsoleConfigurationHelpers.SetTitle(worldName: world.Name, currentyYear: world.CurrentYear);

        if (!settings.ShowEvents)
        {
            return;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[cyan]Ano {world.CurrentDateString}[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Processa todos os habitantes vivos do mundo.
    /// </summary>
    private static void ProcessPopulation(World world, InitialSettings settings)
    {
        List<Human> humans = [.. world.Humans.Where(x => x.Life.IsAlive)];

        int births = 0;
        int deaths = 0;

        foreach (Human human in humans)
        {
            ProcessHumanYear(human);

            if (TryNaturalDeath(human))
            {
                deaths++;
            }
        }

        if (settings.ShowEvents)
        {
            AnsiConsole.MarkupLine($"[white]População:[/] {world.Humans.Where(x => x.Life.IsAlive == true).Count()}");
            AnsiConsole.MarkupLine($"[green]Nascimentos:[/] {births}");
            AnsiConsole.MarkupLine($"[red]Mortes:[/] {deaths}");
        }
    }

    /// <summary>
    /// Processa a evolução anual de um habitante.
    /// </summary>
    private static void ProcessHumanYear(Human human)
    {
        human.Life.AgeOneYear(needs: human.Needs, health: human.Health);

        human.Needs.IncreaseHappiness(RandomHelpers.RandomBetween(-2, 3));

        human.Health.Heal(life: human.Life, RandomHelpers.RandomBetween(-3, 1));
    }

    /// <summary>
    /// Determina se o habitante morre por causas naturais.
    /// </summary>
    private static bool TryNaturalDeath(Human human)
    {
        if (!human.Life.IsAlive)
        {
            return false;
        }

        int chance = human.Life.Age switch
        {
            >= 100 => 80,
            >= 90 => 45,
            >= 80 => 20,
            >= 70 => 8,
            _ => 0
        };

        if (chance == 0)
        {
            return false;
        }

        bool died = RandomHelpers.RandomBetween(1, 100) <= chance;

        if (!died)
        {
            return false;
        }

        human.Life.Die(needs: human.Needs, health: human.Health, cause: CauseOfDeathEnum.NaturalCauses);

        return true;
    }

    /// <summary>
    /// Finaliza o ano atual e avança o calendário.
    /// </summary>
    private static void EndYear(World world)
    {
        world.AdvanceYear();
    }
    #endregion
}
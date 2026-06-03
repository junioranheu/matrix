using Matrix.Domain.Entities;
using Matrix.Domain.Enums;
using Matrix.Domain.Helpers;
using Matrix.Shared.Helpers;
using Spectre.Console;

namespace Matrix.Domain.Factories;

/// <summary>
/// Responsável por executar o ciclo principal da simulação,
/// processando a evolução anual dos habitantes e do mundo.
/// </summary>
public sealed class SimulationFactory
{
    private const int MIN_REPRODUCTION_AGE = 15;
    private const int MAX_REPRODUCTION_AGE = 50;

    private const int REPRODUCTION_CHANCE_AGE_MIN_TO_25 = 18;
    private const int REPRODUCTION_CHANCE_AGE_MIN_TO_26_TO_35 = 12;
    private const int REPRODUCTION_CHANCE_AGE_36_TO_MAX = 5;

    private const int NATURAL_DEATH_AGE_70 = 10;
    private const int NATURAL_DEATH_AGE_80 = 30;
    private const int NATURAL_DEATH_AGE_90 = 77;
    private const int NATURAL_DEATH_AGE_100 = 97;

    public static void Run(InitialSettings settings, World world)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(world);

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
            ProcessHumanYear(human, currentDate: world.CurrentDate);

            bool hasGivenBirth = TryProcreate(world, human, currentDate: world.CurrentDate);

            if (hasGivenBirth)
            {
                births++;
            }

            bool hasDiedFromNaturalDeath = TryNaturalDeath(human, currentDate: world.CurrentDate);

            if (hasDiedFromNaturalDeath)
            {
                deaths++;
            }
        }

        if (!settings.ShowEvents)
        {
            return;
        }

        int alivePopulation = world.Humans.Count(x => x.Life.IsAlive);

        AnsiConsole.MarkupLine($"[white]População:[/] {alivePopulation}");
        AnsiConsole.MarkupLine($"[green]Nascimentos:[/] {births}");
        AnsiConsole.MarkupLine($"[red]Mortes:[/] {deaths}");
    }

    /// <summary>
    /// Processa a evolução anual de um habitante.
    /// </summary>
    private static void ProcessHumanYear(Human human, DateOnly currentDate)
    {
        human.Life.AgeOneYear(needs: human.Needs, health: human.Health, currentDate);

        human.Needs.IncreaseHappiness(RandomHelpers.RandomBetween(-2, 3));

        human.Health.Heal(life: human.Life, RandomHelpers.RandomBetween(-3, 1));
    }

    /// <summary>
    /// Tenta realizar uma procriação com base no habitante informado.
    /// </summary>
    private static bool TryProcreate(World world, Human human, DateOnly currentDate)
    {
        if (!human.Life.IsAlive)
        {
            return false;
        }

        if (!CanReproduce(human))
        {
            return false;
        }

        if (human.Identity.Gender != GenderEnum.Female)
        {
            return false;
        }

        Human? partner = GetReproductionPartner(world, human);

        if (partner is null)
        {
            return false;
        }

        int chance = GetReproductionChance(human.Life.Age);

        if (chance == 0)
        {
            return false;
        }

        bool hasChild = RandomHelpers.RandomBetween(1, 100) <= chance;

        if (!hasChild)
        {
            return false;
        }

        Human newborn = CreateNewborn(father: partner, mother: human, currentDate);

        world.Humans.Add(newborn);

        return true;
    }

    /// <summary>
    /// Verifica se o habitante está dentro da faixa fértil.
    /// </summary>
    private static bool CanReproduce(Human human)
    {
        return human.Life.Age >= MIN_REPRODUCTION_AGE &&
               human.Life.Age <= MAX_REPRODUCTION_AGE &&
               human.Life.IsAlive;
    }

    /// <summary>
    /// Seleciona aleatoriamente um parceiro compatível para reprodução.
    /// </summary>
    private static Human? GetReproductionPartner(World world, Human human)
    {
        List<Human> candidates = [.. world.Humans.Where(x =>
            x.Life.IsAlive &&
            x.Identity.Gender != human.Identity.Gender &&
            x.Life.Age >= MIN_REPRODUCTION_AGE &&
            x.Life.Age <= MAX_REPRODUCTION_AGE)];

        if (candidates.Count == 0)
        {
            return null;
        }

        int index = RandomHelpers.RandomBetween(0, candidates.Count - 1);

        return candidates[index];
    }

    /// <summary>
    /// Cria um novo habitante recém-nascido.
    /// </summary>
    private static Human CreateNewborn(Human father, Human mother, DateOnly currentDate)
    {
        GenderEnum gender = RandomHelpers.RandomBetween(0, 1) == 0 ? GenderEnum.Male : GenderEnum.Female;

        (string firstName, string _) = SimulationHelper.GenerateRandomName(country: mother.Location.BirthCountry, gender: gender);

        Human child = HumanFactory.CreateChild(
            gender: gender,
            father,
            mother,
            firstName: firstName,
            currentDate);

        return child;
    }

    /// <summary>
    /// Determina a chance de reprodução conforme a idade.
    /// </summary>
    private static int GetReproductionChance(int age)
    {
        return age switch
        {
            >= 18 and <= 25 => REPRODUCTION_CHANCE_AGE_MIN_TO_25,
            >= 26 and <= 35 => REPRODUCTION_CHANCE_AGE_MIN_TO_26_TO_35,
            >= 36 and <= 45 => REPRODUCTION_CHANCE_AGE_36_TO_MAX,
            _ => 0
        };
    }

    /// <summary>
    /// Determina se o habitante morre por causas naturais.
    /// </summary>
    private static bool TryNaturalDeath(Human human, DateOnly currentDate)
    {
        if (!human.Life.IsAlive)
        {
            return false;
        }

        int chance = human.Life.Age switch
        {
            >= 100 => NATURAL_DEATH_AGE_100,
            >= 90 => NATURAL_DEATH_AGE_90,
            >= 80 => NATURAL_DEATH_AGE_80,
            >= 70 => NATURAL_DEATH_AGE_70,
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

        human.Life.Die(needs: human.Needs, cause: CauseOfDeathEnum.NaturalCauses, currentDate);

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
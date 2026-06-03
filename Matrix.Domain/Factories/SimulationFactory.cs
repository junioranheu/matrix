using Matrix.Domain.Entities;
using Matrix.Domain.Enums;
using Matrix.Domain.Helpers;
using Matrix.Shared.Extensions;
using Matrix.Shared.Helpers;

namespace Matrix.Domain.Factories;

/// <summary>
/// Responsável por executar o ciclo principal da simulação,
/// processando a evolução anual dos habitantes e do mundo.
/// </summary>
public sealed class SimulationFactory
{
    #region consts
    private const int MIN_REPRODUCTION_AGE = 15;
    private const int MAX_REPRODUCTION_AGE = 50;

    private const int REPRODUCTION_CHANCE_AGE_MIN_TO_25 = 18;
    private const int REPRODUCTION_CHANCE_AGE_MIN_TO_26_TO_35 = 12;
    private const int REPRODUCTION_CHANCE_AGE_36_TO_MAX = 5;

    private const int NATURAL_DEATH_AGE_70 = 10;
    private const int NATURAL_DEATH_AGE_80 = 30;
    private const int NATURAL_DEATH_AGE_90 = 77;
    private const int NATURAL_DEATH_AGE_100 = 97;
    #endregion

    /// <summary>
    /// Executa a simulação completa.
    /// </summary>
    public static void Run(InitialSettings settings, World world, bool isFinalReport, Action<InitialSettings, World, bool> yearReport)
    {
        for (int year = 0; year < settings.SimulationYears; year++)
        {
            StartYear(world);

            ProcessPopulation(world);

            yearReport.Invoke(settings, world, isFinalReport);

            EndYear(world);
        }

        ConsoleConfigurationHelpers.SetTitle(world.Name, world.CurrentYear);
    }

    #region methods
    /// <summary>
    /// Executa as ações de preparação do ano atual.
    /// </summary>
    private static void StartYear(World world)
    {
        ConsoleConfigurationHelpers.SetTitle(world.Name, world.CurrentYear);
    }

    /// <summary>
    /// Processa todos os habitantes vivos do mundo,
    /// executando eventos sociais, familiares,
    /// econômicos e biológicos.
    /// </summary>
    private static void ProcessPopulation(World world)
    {
        List<Human> humans = [.. world.Humans.Where(x => x.Life.IsAlive)];

        foreach (Human human in humans)
        {
            ProcessHumanYear(human, world.CurrentDate);

            TryCreateRelationship(world, human);

            TryFindLover(world, human);

            TryBreakRelationship(world, human);

            TryGainHappiness(human);

            TryLoseHappiness(human);

            TryBecomeRich(human);

            TryBecomePoor(human);

            TryMoveCountry(human);

            TryAccident(human, world.CurrentDate);

            TryProcreate(world, human, world.CurrentDate);

            TryNaturalDeath(world, human, world.CurrentDate);
        }
    }

    /// <summary>
    /// Executa a evolução natural do habitante,
    /// incluindo envelhecimento, felicidade e saúde.
    /// </summary>
    private static void ProcessHumanYear(Human human, DateOnly currentDate)
    {
        human.Life.AgeOneYear(needs: human.Needs, health: human.Health, currentDate);

        human.Needs.IncreaseHappiness(RandomHelpers.RandomBetween(-2, 3));

        human.Health.Heal(life: human.Life, RandomHelpers.RandomBetween(-3, 1));
    }

    /// <summary>
    /// Tenta criar um relacionamento estável
    /// entre dois habitantes solteiros.
    /// </summary>
    private static void TryCreateRelationship(World world, Human human)
    {
        if (!human.Life.IsAlive)
        {
            return;
        }

        if (human.Relationships.PartnerId is not null)
        {
            return;
        }

        if (human.Life.Age < 18)
        {
            return;
        }

        if (RandomHelpers.RandomBetween(1, 100) > 3)
        {
            return;
        }

        List<Human> candidates =
        [
            .. world.Humans.Where(x =>
                x.Id != human.Id &&
                x.Life.IsAlive &&
                x.Relationships.PartnerId is null &&
                x.Identity.Gender != human.Identity.Gender &&
                Math.Abs(x.Life.Age - human.Life.Age) <= 15)
        ];

        if (candidates.Count == 0)
        {
            return;
        }

        Human partner = candidates[RandomHelpers.RandomBetween(0, candidates.Count - 1)];

        human.Relationships.SetPartner(life: human.Life, partnerId: partner.Id);
        partner.Relationships.SetPartner(life: partner.Life, partnerId: human.Id);
    }

    /// <summary>
    /// Tenta criar um relacionamento extraconjugal.
    /// </summary>
    private static void TryFindLover(World world, Human human)
    {
        if (human.Relationships.PartnerId is null)
        {
            return;
        }

        if (RandomHelpers.RandomBetween(1, 1000) > 5)
        {
            return;
        }

        List<Human> candidates =
        [
            .. world.Humans.Where(x =>
                x.Id != human.Id &&
                x.Life.IsAlive &&
                x.Identity.Gender != human.Identity.Gender)
        ];

        if (candidates.Count == 0)
        {
            return;
        }

        Human lover = candidates[RandomHelpers.RandomBetween(0, candidates.Count - 1)];

        human.Relationships.AddLover(life: human.Life, loverId: lover.Id);
    }

    /// <summary>
    /// Tenta encerrar um relacionamento existente.
    /// </summary>
    private static void TryBreakRelationship(World world, Human human)
    {
        if (human.Relationships.PartnerId is null)
        {
            return;
        }

        int chance = 1;

        if (human.Needs.Happiness < 30)
        {
            chance += 4;
        }

        if (RandomHelpers.RandomBetween(1, 100) > chance)
        {
            return;
        }

        Human? partner = world.Humans.FirstOrDefault(x => x.Id == human.Relationships.PartnerId);

        human.Relationships.RemovePartner();
        partner?.Relationships.RemovePartner();
    }

    /// <summary>
    /// Tenta gerar um novo filho utilizando parceiro
    /// oficial ou amante.
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

        chance += GetChildrenModifier(human);

        if (chance <= 0)
        {
            return false;
        }

        bool hasChild = RandomHelpers.RandomBetween(1, 100) <= chance;

        if (!hasChild)
        {
            return false;
        }

        Human newborn = CreateNewborn(partner, human, currentDate);

        world.Humans.Add(newborn);

        return true;
    }

    /// <summary>
    /// Obtém o parceiro mais provável para reprodução.
    /// </summary>
    private static Human? GetReproductionPartner(World world, Human human)
    {
        if (human.Relationships.PartnerId is not null)
        {
            Human? partner = world.Humans.FirstOrDefault(x => x.Id == human.Relationships.PartnerId);

            if (partner is not null)
            {
                return partner;
            }
        }

        if (human.Relationships.LoversIds.Count > 0)
        {
            Guid loverId = human.Relationships.LoversIds[RandomHelpers.RandomBetween(0, human.Relationships.LoversIds.Count - 1)];

            Human? lover = world.Humans.FirstOrDefault(x => x.Id == loverId);

            if (lover is not null)
            {
                return lover;
            }
        }

        return null;
    }

    /// <summary>
    /// Reduz a fertilidade conforme a quantidade
    /// de filhos já existentes.
    /// </summary>
    private static int GetChildrenModifier(Human human)
    {
        int children = human.Family.ChildrenIds.Count;

        return children switch
        {
            0 => 0,
            1 => -2,
            2 => -5,
            3 => -10,
            4 => -15,
            _ => -25
        };
    }

    /// <summary>
    /// Verifica se o habitante está em idade fértil.
    /// </summary>
    private static bool CanReproduce(Human human)
    {
        return human.Life.Age >= MIN_REPRODUCTION_AGE &&
               human.Life.Age <= MAX_REPRODUCTION_AGE &&
               human.Life.IsAlive;
    }

    /// <summary>
    /// Cria um novo habitante e registra os pais.
    /// </summary>
    private static Human CreateNewborn(Human father, Human mother, DateOnly currentDate)
    {
        GenderEnum gender = RandomHelpers.RandomBetween(0, 1) == 0 ? GenderEnum.Male : GenderEnum.Female;

        (string firstName, string _) = SimulationHelper.GenerateRandomName(mother.Location.BirthCountry, gender);

        Human child = HumanFactory.CreateChild(gender, father, mother, firstName, currentDate);

        father.Family.AddChild(life: father.Life, needs: father.Needs, childId: child.Id);

        mother.Family.AddChild(life: mother.Life, needs: mother.Needs, childId: child.Id);

        return child;
    }

    /// <summary>
    /// Determina a chance de reprodução pela idade.
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
    /// Tenta aplicar um acidente fatal aleatório.
    /// </summary>
    private static void TryAccident(Human human, DateOnly currentDate)
    {
        if (!human.Life.IsAlive)
        {
            return;
        }

        if (RandomHelpers.RandomBetween(1, 1000) > 2)
        {
            return;
        }

        human.Life.Die(needs: human.Needs, cause: CauseOfDeathEnum.Accident, dateOfDeath: currentDate);
    }

    /// <summary>
    /// Evento raro de enriquecimento.
    /// Pode representar herança, negócio bem sucedido,
    /// prêmio ou investimento lucrativo.
    /// </summary>
    private static void TryBecomeRich(Human human)
    {
        if (!human.Life.IsAlive)
        {
            return;
        }

        if (RandomHelpers.RandomBetween(1, 1000) > 2)
        {
            return;
        }

        decimal amount = RandomHelpers.RandomBetween(min: 5_000, max: 100_000);

        human.Finance.EarnMoney(life: human.Life, amount);

        human.Needs.IncreaseHappiness(15);
    }

    /// <summary>
    /// Evento raro de perda financeira.
    /// Pode representar golpes, dívidas,
    /// investimentos ruins ou acidentes.
    /// </summary>
    private static void TryBecomePoor(Human human)
    {
        if (!human.Life.IsAlive)
        {
            return;
        }

        if (RandomHelpers.RandomBetween(1, 1000) > 5)
        {
            return;
        }

        decimal amount = RandomHelpers.RandomBetween(min: 1_000, max: 50_000);

        human.Finance.SpendMoney(life: human.Life, amount);

        human.Needs.IncreaseHappiness(-10);
    }

    /// <summary>
    /// Evento raro de migração.
    /// Representa mudança de país por trabalho,
    /// relacionamento ou busca por melhores condições.
    /// </summary>
    private static void TryMoveCountry(Human human)
    {
        if (!human.Life.IsAlive)
        {
            return;
        }

        if (human.Life.Age < 18)
        {
            return;
        }

        if (RandomHelpers.RandomBetween(1, 1000) > 3)
        {
            return;
        }

        CountryEnum destination = EnumExtensions.GetRandom<CountryEnum>();

        if (destination == human.Location.CurrentCountry)
        {
            return;
        }

        human.Location.MoveToCountry(life: human.Life, destination);

        human.Needs.IncreaseHappiness(RandomHelpers.RandomBetween(-5, 20));
    }

    /// <summary>
    /// Evento positivo que aumenta felicidade.
    /// </summary>
    private static void TryGainHappiness(Human human)
    {
        if (RandomHelpers.RandomBetween(1, 100) <= 5)
        {
            human.Needs.IncreaseHappiness(10);
        }
    }

    /// <summary>
    /// Evento negativo que reduz felicidade.
    /// </summary>
    private static void TryLoseHappiness(Human human)
    {
        if (RandomHelpers.RandomBetween(1, 100) <= 5)
        {
            human.Needs.IncreaseHappiness(-10);
        }
    }

    /// <summary>
    /// Determina se o habitante morre por causas naturais.
    /// </summary>
    private static bool TryNaturalDeath(World world, Human human, DateOnly currentDate)
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

        human.Life.Die(needs: human.Needs, cause: CauseOfDeathEnum.NaturalCauses, dateOfDeath: currentDate);

        Human? partner = world.Humans.FirstOrDefault(x => x.Relationships.PartnerId == human.Id);

        partner?.Needs.IncreaseHappiness(-25);

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
using Matrix.Domain.Entities;
using Matrix.Domain.Enums;
using Matrix.Domain.Helpers;
using Matrix.Shared.Extensions;
using Matrix.Shared.Helpers;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;

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

            if (!HasAliveHumans(world))
            {
                return;
            }

            ProcessPopulation(world);

            yearReport.Invoke(settings, world, isFinalReport);

            EndYear(world);
        }

        ConsoleConfigurationHelpers.SetTitle(world.Name, world.CurrentYear);
    }

    #region methods
    /// <summary>
    /// Verifica se ainda existe ao menos um humano vivo no mundo.
    /// </summary>
    /// <param name="world">
    /// Mundo utilizado para consulta.
    /// </param>
    /// <returns>
    /// Verdadeiro quando existe ao menos um humano vivo; caso contrário, falso.
    /// </returns>
    private static bool HasAliveHumans(World world)
    {
        return world.Humans.Any(x => x.Life.IsAlive);
    }

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
                Math.Abs(x.Life.Age - human.Life.Age) <= 15 &&
                !AreCloseRelatives(world, human, x))
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
                x.Identity.Gender != human.Identity.Gender &&
                !AreCloseRelatives(world, human, x))
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
    /// Obtém o parceiro mais provável para reprodução.
    /// </summary>
    private static Human? GetReproductionPartner(World world, Human human)
    {
        if (human.Relationships.PartnerId is not null)
        {
            Human? partner = world.Humans.FirstOrDefault(x => x.Id == human.Relationships.PartnerId);

            if (CanReproduceWith(world, human, partner))
            {
                return partner;
            }
        }

        if (human.Relationships.LoversIds.Count > 0)
        {
            List<Human> validLovers = [.. world.Humans.Where(x => human.Relationships.LoversIds.Contains(x.Id)).Where(x => CanReproduceWith(world, human, x))];

            if (validLovers.Count > 0)
            {
                return validLovers[
                    RandomHelpers.RandomBetween(0, validLovers.Count - 1)
                ];
            }
        }

        return null;
    }

    /// <summary>
    /// Tenta gerar um novo filho utilizando parceiro oficial ou amante.
    /// </summary>
    private static bool TryProcreate(World world, Human human, DateOnly currentDate)
    {
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

        Human newborn = CreateNewborn(father: partner, mother: human, currentDate);

        world.Humans.Add(newborn);

        return true;
    }

    /// <summary>
    /// Valida se dois humanos podem reproduzir.
    /// </summary>
    private static bool CanReproduceWith(World world, Human human, Human? candidate)
    {
        // O parceiro precisa existir;
        if (candidate is null)
        {
            return false;
        }

        // Não pode reproduzir consigo mesmo;
        if (candidate.Id == human.Id)
        {
            return false;
        }

        // Ambos precisam estar vivos;
        if (!candidate.Life.IsAlive)
        {
            return false;
        }

        // Apenas sexos opostos;
        if (candidate.Identity.Gender == human.Identity.Gender)
        {
            return false;
        }

        // Ambos precisam estar em idade fértil;
        if (!CanReproduce(candidate))
        {
            return false;
        }

        // Permite reprodução quando existe alguma conexão territorial
        // entre os dois humanos, seja pelo país atual ou pelo país de origem.
        // Exemplos válidos:
        // - Atual == Atual;
        // - Atual == Origem;
        // - Origem == Atual;
        // - Origem == Origem;
        bool sameCurrentCountry = candidate.Location.CurrentCountry == human.Location.CurrentCountry;
        bool candidateCurrentAndHumanBirth = candidate.Location.CurrentCountry == human.Location.BirthCountry;
        bool candidateBirthAndHumanCurrent = candidate.Location.BirthCountry == human.Location.CurrentCountry;
        bool sameBirthCountry = candidate.Location.BirthCountry == human.Location.BirthCountry;

        if (!sameCurrentCountry && !candidateCurrentAndHumanBirth && !candidateBirthAndHumanCurrent && !sameBirthCountry)
        {
            return false;
        }

        // Impede qualquer parentesco próximo;
        if (AreCloseRelatives(world, human, candidate))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Verifica se duas pessoas possuem parentesco próximo.
    /// </summary>
    private static bool AreCloseRelatives(World world, Human human, Human candidate)
    {
        return IsParentOf(human, candidate) ||
            IsParentOf(candidate, human) ||
            AreSiblings(human, candidate) ||
            IsGrandparentOf(world, human, candidate) ||
            IsGrandparentOf(world, candidate, human) ||
            IsUncleOrAuntOf(world, human, candidate) ||
            IsUncleOrAuntOf(world, candidate, human);
    }

    /// <summary>
    /// Verifica se parent é pai ou mãe de child.
    /// </summary>
    private static bool IsParentOf(Human parent, Human child)
    {
        return child.Family.FatherId == parent.Id ||
               child.Family.MotherId == parent.Id;
    }

    /// <summary>
    /// Verifica se duas pessoas são irmãs ou meio-irmãs.
    /// </summary>
    private static bool AreSiblings(Human first, Human second)
    {
        Guid? firstFatherId = first.Family.FatherId;
        Guid? secondFatherId = second.Family.FatherId;

        Guid? firstMotherId = first.Family.MotherId;
        Guid? secondMotherId = second.Family.MotherId;

        bool sameFather =
            firstFatherId is not null &&
            secondFatherId is not null &&
            firstFatherId == secondFatherId;

        bool sameMother =
            firstMotherId is not null &&
            secondMotherId is not null &&
            firstMotherId == secondMotherId;

        return sameFather || sameMother;
    }

    /// <summary>
    /// Verifica se grandparent é avô ou avó de grandchild.
    /// </summary>
    private static bool IsGrandparentOf(World world, Human grandparent, Human grandchild)
    {
        List<Guid> parentsIds =
        [
            grandchild.Family.FatherId ?? Guid.Empty,
            grandchild.Family.MotherId ?? Guid.Empty
        ];

        foreach (Guid parentId in parentsIds.Where(x => x != Guid.Empty))
        {
            Human? parent = world.Humans.FirstOrDefault(x => x.Id == parentId);

            if (parent is null)
            {
                continue;
            }

            if (IsParentOf(grandparent, parent))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Verifica se uncle é tio ou tia de nephew.
    /// </summary>
    private static bool IsUncleOrAuntOf(World world, Human uncle, Human nephew)
    {
        List<Guid> parentsIds =
        [
            nephew.Family.FatherId ?? Guid.Empty,
            nephew.Family.MotherId ?? Guid.Empty
        ];

        foreach (Guid parentId in parentsIds.Where(x => x != Guid.Empty))
        {
            Human? parent = world.Humans.FirstOrDefault(x => x.Id == parentId);

            if (parent is null)
            {
                continue;
            }

            if (AreSiblings(uncle, parent))
            {
                return true;
            }
        }

        return false;
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

        (string firstName, string _) = SimulationHelper.GenerateRandomName(father.Location.BirthCountry, gender);

        Human child = HumanFactory.CreateChild(gender, father, mother, firstName, currentDate);

        father.Family.AddChild(life: father.Life, needs: father.Needs, childId: child.Id);

        mother.Family.AddChild(life: mother.Life, needs: mother.Needs, childId: child.Id);

        child.Life.AddLifeEvent($"Nasceu em {currentDate:yyyy}, {mother.Location.CurrentCountry.GetDescription()}.");

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
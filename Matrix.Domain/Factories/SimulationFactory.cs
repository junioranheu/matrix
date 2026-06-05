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

    private const int REPRODUCTION_CHANCE_AGE_MIN_TO_25 = 25;
    private const int REPRODUCTION_CHANCE_AGE_MIN_TO_26_TO_35 = 20;
    private const int REPRODUCTION_CHANCE_AGE_36_TO_MAX = 10;

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
    /// Executa as ações de preparação do ano atual.
    /// </summary>
    private static void StartYear(World world)
    {
        ConsoleConfigurationHelpers.SetTitle(world.Name, world.CurrentYear);
    }

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
        int alive = world.Humans.Count(x => x.Life.IsAlive);

        if (alive <= 50)
        {
            PrintExtinctionDiagnostics(world);
        }

        return alive > 0;
    }

    /// <summary>
    /// Exibe um relatório de diagnóstico quando a civilização
    /// está próxima da extinção, permitindo identificar possíveis
    /// causas para o colapso populacional, como envelhecimento,
    /// falta de casais férteis ou desequilíbrio entre nascimentos e mortes.
    /// </summary>
    private static void PrintExtinctionDiagnostics(World world)
    {
        List<Human> alive = [.. world.Humans.Where(x => x.Life.IsAlive)];

        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;

        Console.WriteLine($"População: {alive.Count}");

        Console.WriteLine($"Homens: {alive.Count(x => x.Identity.Gender == GenderEnum.Male)}");
        Console.WriteLine($"Mulheres: {alive.Count(x => x.Identity.Gender == GenderEnum.Female)}");

        Console.WriteLine($"0-17: {alive.Count(x => x.Life.Age <= 17)}");
        Console.WriteLine($"18-30: {alive.Count(x => x.Life.Age >= 18 && x.Life.Age <= 30)}");
        Console.WriteLine($"31-45: {alive.Count(x => x.Life.Age >= 31 && x.Life.Age <= 45)}");
        Console.WriteLine($"46-60: {alive.Count(x => x.Life.Age >= 46 && x.Life.Age <= 60)}");
        Console.WriteLine($"61+: {alive.Count(x => x.Life.Age >= 61)}");

        Console.WriteLine($"Mulheres férteis: {alive.Count(x => x.Identity.Gender == GenderEnum.Female && x.Life.Age >= 18 && x.Life.Age <= 45)}");

        Console.WriteLine($"Casados: {alive.Count(x => x.Relationships.PartnerId != null)}");

        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// Armazena todos os pares de humanos que possuem
    /// parentesco próximo e não podem se relacionar.
    /// </summary>
    private static HashSet<(Guid first, Guid second)> BuildCloseRelativesCache(World world)
    {
        HashSet<(Guid, Guid)> cache = [];

        foreach (Human human in world.Humans)
        {
            AddParentRelationships(cache, human);
        }

        AddSiblingRelationships(world, cache);

        return cache;
    }

    /// <summary>
    /// Registra uma relação de parentesco nos dois sentidos.
    /// Exemplo:
    /// João -> Maria
    /// Maria -> João
    /// </summary>
    private static void AddRelationship(HashSet<(Guid first, Guid second)> cache, Guid first, Guid second)
    {
        cache.Add((first, second));
        cache.Add((second, first));
    }

    /// <summary>
    /// Registra relações entre pais e filhos.
    /// </summary>
    private static void AddParentRelationships(HashSet<(Guid first, Guid second)> cache, Human child)
    {
        if (child.Family.FatherId is not null)
        {
            AddRelationship(cache, first: child.Id, second: child.Family.FatherId.Value);
        }

        if (child.Family.MotherId is not null)
        {
            AddRelationship(cache, first: child.Id, second: child.Family.MotherId.Value);
        }
    }

    /// <summary>
    /// Agrupa humanos por pai e mãe e registra
    /// relações de irmandade entre todos os membros
    /// de cada família encontrada.
    /// </summary>
    private static void AddSiblingRelationships(World world, HashSet<(Guid first, Guid second)> cache)
    {
        // Agrupa os humanos por combinação de pai e mãe.
        // O tamanho inicial evita realocações internas
        // quando a população é muito grande.
        Dictionary<(Guid? fatherId, Guid? motherId), List<Guid>> families = new(world.Humans.Count);

        foreach (Human human in world.Humans)
        {
            Guid? fatherId = human.Family.FatherId;
            Guid? motherId = human.Family.MotherId;

            // Humanos sem pai e mãe conhecidos não podem
            // ser agrupados como irmãos.
            //
            // Isso evita falsos positivos e impede a criação
            // de uma família gigantesca contendo todos os
            // habitantes sem ascendência registrada.
            if (fatherId is null && motherId is null)
            {
                continue;
            }

            // Utiliza a combinação de pai e mãe como chave
            // para identificar uma família.
            (Guid?, Guid?) key = (fatherId, motherId);

            // Cria a família caso ela ainda não exista.
            if (!families.TryGetValue(key, out List<Guid>? family))
            {
                family = [];

                families[key] = family;
            }

            // Armazena apenas o identificador do humano,
            // reduzindo consumo de memória e melhorando
            // a localidade de cache durante o processamento.
            family.Add(human.Id);
        }

        // Percorre todas as famílias encontradas.
        foreach (List<Guid> family in families.Values)
        {
            int count = family.Count;

            // Famílias com apenas um membro não possuem
            // irmãos para relacionar.
            if (count < 2)
            {
                continue;
            }

            // Gera todas as combinações únicas de irmãos.
            //
            // Exemplo:
            // A B C D
            //
            // A-B A-C A-D
            // B-C B-D
            // C-D
            //
            // Evita comparações duplicadas como B-A.
            for (int i = 0; i < count - 1; i++)
            {
                Guid first = family[i];

                for (int j = i + 1; j < count; j++)
                {
                    AddRelationship(cache, first: first, second: family[j]);
                }
            }
        }
    }

    /// <summary>
    /// Processa todos os habitantes vivos do mundo,
    /// executando eventos sociais, familiares,
    /// econômicos e biológicos.
    /// </summary>
    private static void ProcessPopulation(World world)
    {
        HashSet<(Guid first, Guid second)> closeRelatives = BuildCloseRelativesCache(world);

        List<Human> humans = [.. world.Humans.Where(x => x.Life.IsAlive)];

        DateOnly currentDate = world.CurrentDate;

        foreach (Human human in humans)
        {
            ProcessHumanYear(human, currentDate);

            TryCreateRelationship(world, human, closeRelatives);

            TryFindLover(world, human, closeRelatives);

            TryDivorce(world, human);

            TryGainHappiness(human);

            TryLoseHappiness(human);

            TryBecomeRich(human);

            TryBecomePoor(human);

            TryMoveCountry(human, currentDate);

            TryAccident(human, currentDate);

            TryProcreate(world, human, currentDate, closeRelatives);

            TryNaturalDeath(world, human, currentDate);

            TryToGetDepression(world, human, currentDate);
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
    /// Tenta criar um relacionamento estável entre dois
    /// habitantes solteiros e compatíveis.
    /// </summary>
    private static void TryCreateRelationship(World world, Human human, HashSet<(Guid first, Guid second)> closeRelatives)
    {
        // Apenas pessoas vivas podem iniciar relacionamentos;
        if (!human.Life.IsAlive)
        {
            return;
        }

        // Ignora quem já possui parceiro;
        if (human.Relationships.PartnerId is not null)
        {
            return;
        }

        // Impede relacionamentos antes da maioridade;
        if (human.Life.Age < 18)
        {
            return;
        }

        // Chance anual de encontrar um parceiro.
        if (RandomHelpers.RandomBetween(1, 100) > 25)
        {
            return;
        }

        // Procura possíveis parceiros compatíveis:
        // - não pode ser a própria pessoa;
        // - precisa estar vivo;
        // - precisa estar solteiro;
        // - deve ser do sexo oposto;
        // - diferença máxima de idade de X anos;
        // - não pode possuir parentesco próximo.
        List<Human> candidates =
        [
            ..world.Humans.Where(x =>
                x.Id != human.Id &&
                x.Life.IsAlive &&
                x.Relationships.PartnerId is null &&
                x.Identity.Gender != human.Identity.Gender &&
                Math.Abs(x.Life.Age - human.Life.Age) <= 25 &&
                !closeRelatives.Contains((human.Id, x.Id))
            )
        ];

        // Não encontrou ninguém compatível;
        if (candidates.Count == 0)
        {
            return;
        }

        // Escolhe aleatoriamente um dos candidatos válidos;
        Human partner = candidates[RandomHelpers.RandomBetween(0, candidates.Count - 1)];

        // Registra o relacionamento para ambos os lados;
        human.Relationships.SetPartner(life: human.Life, partnerId: partner.Id);
        partner.Relationships.SetPartner(life: partner.Life, partnerId: human.Id);
    }

    /// <summary>
    /// Tenta criar um relacionamento extraconjugal.
    /// </summary>
    private static void TryFindLover(World world, Human human, HashSet<(Guid first, Guid second)> closeRelatives)
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
                !closeRelatives.Contains((human.Id, x.Id))
            )
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
    private static void TryDivorce(World world, Human human)
    {
        if (human.Relationships.PartnerId is null)
        {
            return;
        }

        int chance = human.Needs.Happiness switch
        {
            < 10 => 15,
            < 20 => 10,
            < 30 => 1,
            _ => 0
        };

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
    private static Human? GetReproductionPartner(World world, Human human, HashSet<(Guid first, Guid second)> closeRelatives)
    {
        // Primeiro tenta utilizar o parceiro oficial do relacionamento;
        if (human.Relationships.PartnerId is not null)
        {
            Human? partner = world.Humans.FirstOrDefault(x => x.Id == human.Relationships.PartnerId);

            // O parceiro precisa atender todas as regras
            // de reprodução (idade, sexo, parentesco etc);
            if (CanReproduceWith(human, partner, closeRelatives))
            {
                return partner;
            }
        }

        // Caso não consiga reproduzir com o parceiro oficial,
        // tenta utilizar algum amante válido;
        if (human.Relationships.LoversIds.Count > 0)
        {
            // Filtra apenas amantes que podem reproduzir;
            List<Human> validLovers =
            [
                ..world.Humans.Where(x =>
                    human.Relationships.LoversIds.Contains(x.Id) &&
                    CanReproduceWith(human, x, closeRelatives))
            ];

            // Se existir ao menos um amante válido,
            // escolhe um aleatoriamente;
            if (validLovers.Count > 0)
            {
                return validLovers[RandomHelpers.RandomBetween(0, validLovers.Count - 1)];
            }
        }

        return null;
    }

    /// <summary>
    /// Tenta gerar um novo filho utilizando parceiro oficial ou amante.
    /// </summary>
    private static bool TryProcreate(World world, Human human, DateOnly currentDate, HashSet<(Guid first, Guid second)> closeRelatives)
    {
        // Verifica se o humano atende aos requisitos básicos para reprodução;
        if (!CanReproduce(human))
        {
            return false;
        }

        // Apenas mulheres podem engravidar e dar origem ao recém-nascido;
        if (human.Identity.Gender != GenderEnum.Female)
        {
            return false;
        }

        // Obtém um parceiro elegível para reprodução;
        Human? partner = GetReproductionPartner(world, human, closeRelatives);

        // Não é possível reproduzir sem um parceiro;
        if (partner is null)
        {
            return false;
        }

        // Obtém a chance base de reprodução de acordo com a idade;
        int chance = GetReproductionChance(human.Life.Age);

        // Ajusta a chance conforme a quantidade de filhos já existentes;
        chance += GetChildrenModifier(human);

        // Caso a chance final seja nula ou negativa, cancela a tentativa;
        if (chance <= 0)
        {
            return false;
        }

        // Realiza um sorteio percentual entre 1 e 100;
        int reproductionRoll = RandomHelpers.RandomBetween(1, 100);

        // Determina se a reprodução foi bem-sucedida;
        bool reproductionSucceeded = reproductionRoll <= chance;

        // Se o sorteio falhou, não haverá nascimento;
        if (!reproductionSucceeded)
        {
            return false;
        }

        // Cria o recém-nascido utilizando o parceiro como pai e a humana como mãe;
        Human newborn = CreateNewborn(father: partner, mother: human, currentDate);

        // Adiciona o novo humano à população mundial;
        world.AddHuman(human: newborn, currentDate, country: newborn.Location.BirthCountry, isInitialSpawn: false, age: 0);

        // Informa que houve reprodução com sucesso;
        return true;
    }

    /// <summary>
    /// Valida se dois humanos podem reproduzir.
    /// </summary>
    private static bool CanReproduceWith(Human human, Human? candidate, HashSet<(Guid first, Guid second)> closeRelatives)
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
        if (closeRelatives.Contains((human.Id, candidate.Id)))
        {
            return false;
        }

        return true;
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
    /// Reduz a fertilidade conforme a quantidade
    /// de filhos já existentes.
    /// </summary>
    private static int GetChildrenModifier(Human human)
    {
        int children = human.Family.ChildrenIds.Count;

        return children switch
        {
            0 => 0,
            1 => -1,
            2 => -3,
            3 => -6,
            4 => -10,
            _ => -15
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

        father.Family.AddChild(life: father.Life, needs: father.Needs, childId: child.Id, currentDate);

        mother.Family.AddChild(life: mother.Life, needs: mother.Needs, childId: child.Id, currentDate);

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

        if (RandomHelpers.RandomBetween(1, 10000) > 5)
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

        human.Needs.DecreaseHappiness(15);
    }

    /// <summary>
    /// Evento raro de migração.
    /// Representa mudança de país por trabalho,
    /// relacionamento ou busca por melhores condições.
    /// </summary>
    private static void TryMoveCountry(Human human, DateOnly currentDate)
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

        human.Location.MoveToCountry(life: human.Life, destination, currentDate);

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

        partner?.Needs.DecreaseHappiness(25);

        return true;
    }

    /// <summary>
    /// Determina se o habitante obtém/morre por depressão.
    /// </summary>
    private static bool TryToGetDepression(World world, Human human, DateOnly currentDate)
    {
        if (!human.Life.IsAlive)
        {
            return false;
        }

        if (human.Needs.Happiness >= 15)
        {
            return false;
        }

        bool died = RandomHelpers.RandomBetween(1, 100) <= 5;

        if (!died)
        {
            return false;
        }

        human.Life.Die(needs: human.Needs, cause: CauseOfDeathEnum.Depression, dateOfDeath: currentDate);

        Human? partner = world.Humans.FirstOrDefault(x => x.Relationships.PartnerId == human.Id);

        partner?.Needs.DecreaseHappiness(25);

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
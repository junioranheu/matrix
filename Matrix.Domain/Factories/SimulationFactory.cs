using Matrix.Domain.Entities;
using Matrix.Domain.Enums;
using Matrix.Domain.Helpers;
using Matrix.Shared.Extensions;
using Matrix.Shared.Helpers;
using Spectre.Console;
using System.Collections.Concurrent;

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

    private const int YEAR_MAX_DIFFERENCE_FOR_REPRODUCTION = 35;
    private const int YEAR_SIBLING_RELATIONSHIPS_BANNED = 500;
    #endregion

    /// <summary>
    /// Executa a simulação completa.
    /// </summary>
    public static async Task Run(InitialSettings settings, World world, bool isFinalReport, Action<InitialSettings, World, bool> yearReport)
    {
        for (int year = 0; year < settings.SimulationYears; year++)
        {
            StartYear(world);

            if (!HasAliveHumans(world))
            {
                return;
            }

            await ProcessHumanLifecycle(world);

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
        // Para populações grandes, a construção do cache pode ser custosa.
        // Foi implementada uma versão paralela usando coleções concorrentes
        // para reduzir tempo de parede ao agrupar parentes próximos.

        // É usado um ConcurrentDictionary como um HashSet thread-safe (valor dummy);
        ConcurrentDictionary<(Guid, Guid), byte> pairSet = new(concurrencyLevel: Environment.ProcessorCount, capacity: Math.Max(16, world.Humans.Count * 4));

        // Adiciona relações pai-filho em paralelo;
        Parallel.ForEach(world.Humans, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, human =>
        {
            if (human.Family.FatherId is not null)
            {
                (Guid Id, Guid Value) a = (human.Id, human.Family.FatherId.Value);
                (Guid Value, Guid Id) b = (human.Family.FatherId.Value, human.Id);

                pairSet.TryAdd(a, 0);
                pairSet.TryAdd(b, 0);
            }

            if (human.Family.MotherId is not null)
            {
                (Guid Id, Guid Value) a = (human.Id, human.Family.MotherId.Value);
                (Guid Value, Guid Id) b = (human.Family.MotherId.Value, human.Id);

                pairSet.TryAdd(a, 0);
                pairSet.TryAdd(b, 0);
            }
        });

        // Durante os primeiros séculos da civilização,
        // relacionamentos entre irmãos ainda eram socialmente aceitos.
        // A partir deste período, a sociedade passa a reconhecer e evitar
        // esse tipo de relação; é necessário identificar os vínculos de irmandade.
        if (world.CurrentYear >= YEAR_SIBLING_RELATIONSHIPS_BANNED)
        {
            // Agrupa humanos por combinação (pai, mãe) em um dicionário concorrente;
            var families = new ConcurrentDictionary<(Guid? fatherId, Guid? motherId), ConcurrentBag<Guid>>(Environment.ProcessorCount, Math.Max(16, world.Humans.Count));

            Parallel.ForEach(world.Humans, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, human =>
            {
                Guid? fatherId = human.Family.FatherId;
                Guid? motherId = human.Family.MotherId;

                if (fatherId is null && motherId is null)
                {
                    return;
                }

                var key = (fatherId, motherId);
                var bag = families.GetOrAdd(key, _ => []);

                bag.Add(human.Id);
            });

            // Para cada família, gera combinações únicas de irmãos e registra pares;
            foreach (var bag in families.Values)
            {
                Guid[] members = [.. bag];
                int count = members.Length;

                if (count < 2)
                {
                    continue;
                }

                for (int i = 0; i < count - 1; i++)
                {
                    Guid first = members[i];

                    for (int j = i + 1; j < count; j++)
                    {
                        var a = (first, members[j]);
                        var b = (members[j], first);
                        pairSet.TryAdd(a, 0);
                        pairSet.TryAdd(b, 0);
                    }
                }
            }
        }

        // Converte o conjunto concorrente para HashSet de retorno
        return [.. pairSet.Keys];
    }

    /// <summary>
    /// Processa todas as ações dos habitantes vivos do mundo,
    /// executando eventos sociais, familiares, econômicos e biológicos.
    /// </summary>
    private static async Task ProcessHumanLifecycle(World world)
    {
        // Dados globais da simulação;
        DateOnly currentDate = world.CurrentDate;

        // Habitantes vivos;
        List<Human> humans = [.. world.Humans.Where(x => x.Life.IsAlive)];
        List<Human> men = [.. humans.Where(x => x.Identity.Gender == GenderEnum.Male)];
        List<Human> women = [.. humans.Where(x => x.Identity.Gender == GenderEnum.Female)];

        // Índices de busca;
        Dictionary<Guid, Human> humansById = humans.ToDictionary(x => x.Id);

        // Cache de relacionamentos;
        // BuildCloseRelativesCache pode se tornar uma operação pesada em populações grandes.
        // Por isso, o processamento é paralelizado para reduzir o tempo gasto na construção
        // dos relacionamentos de parentesco próximos (pais, filhos e irmãos).
        HashSet<(Guid first, Guid second)> closeRelatives = BuildCloseRelativesCache(world);

        // Eventos globais anteriores ao processamento individual (ex.: desastres naturais);
        TryNaturalDisaster(world, currentDate, humans, humansById);

        // Fase A: processa em paralelo apenas mudanças individuais de cada humano.
        // Tudo que depende de outros humanos ou altera o estado global da simulação
        // fica para a Fase B, executada de forma sequencial.
        Parallel.ForEach(humans, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, human =>
        {
            List<Human> potentialPartners = GetPotentialPartners(human, men, women);

            ProcessHumanYear(human, currentDate);

            TryFindLover(human, potentialPartners, closeRelatives);

            TryGainHappiness(human);

            TryLoseHappiness(human);

            TryBecomeRich(human);

            TryBecomePoor(human);

            TryMoveCountry(human, currentDate);

            TrySleepEatExercise(human, currentDate);

            TryStudyAndGraduate(human, currentDate);

            TryWorkAndCareer(human, currentDate);

            TrySicknessAndRecovery(human, currentDate);

            TryDonateOrLoan(human, currentDate);

            TryMakeFriendsEnemies(human, potentialPartners);
        });

        // Fase B: processa de forma sequencial eventos que envolvem múltiplos humanos
        // ou alteram o estado global da simulação.
        await AnsiConsole.Progress().StartAsync(ctx =>
        {
            ProgressTask task = ctx.AddTask("Processando...", maxValue: humans.Count);

            foreach (Human human in humans)
            {
                List<Human> potentialPartners = GetPotentialPartners(human, men, women);

                TryCreateRelationship(human, potentialPartners, closeRelatives);

                TryDivorce(humansById, human, currentDate);

                TryMarryCouple(humansById, human, currentDate);

                TryAttemptTheft(humansById, human);

                TryWorkplaceAccident(humansById, human, currentDate, world);

                TryMedicalMalpractice(humansById, human, currentDate, world);

                TryAccident(human, currentDate);

                TryProcreate(humansById, world, human, currentDate, closeRelatives);

                TryNaturalDeath(humansById, human, currentDate);

                TryToGetDepression(humansById, human, currentDate);

                TryDistributeInheritance(human, world, currentDate, humansById);

                // Incrementa o progresso após processar cada habitante;
                task.Increment(1);
            }

            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Define o conjunto de possíveis parceiros compatíveis
    /// com o sexo do habitante atual, evitando percorrer
    /// pessoas que jamais poderiam formar um relacionamento.
    /// </summary>
    private static List<Human> GetPotentialPartners(Human human, List<Human> men, List<Human> women)
    {
        List<Human> potentialPartners = human.Identity.Gender == GenderEnum.Male ? women : men;

        return potentialPartners;
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
    private static void TryCreateRelationship(Human human, List<Human> potentialPartners, HashSet<(Guid first, Guid second)> closeRelatives)
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
        if (human.Life.Age < MIN_REPRODUCTION_AGE)
        {
            return;
        }

        // Chance anual de encontrar um parceiro.
        if (RandomHelpers.RandomBetween(1, 100) > 25)
        {
            return;
        }

        // Escolhe aleatoriamente um candidato compatível sem criar listas temporárias;
        Human? selected = null;
        int validCount = 0;

        foreach (Human x in potentialPartners)
        {
            if (x.Id == human.Id || !x.Life.IsAlive || x.Relationships.PartnerId is not null || x.Identity.Gender == human.Identity.Gender)
            {
                continue;
            }

            if (Math.Abs(x.Life.Age - human.Life.Age) > YEAR_MAX_DIFFERENCE_FOR_REPRODUCTION)
            {
                continue;
            }

            if (closeRelatives.Contains((human.Id, x.Id)))
            {
                continue;
            }

            validCount++;

            if (RandomHelpers.RandomBetween(1, validCount) == 1)
            {
                selected = x;
            }
        }

        if (selected is null)
        {
            return;
        }

        // Registra o relacionamento para ambos os lados;
        human.Relationships.SetPartner(life: human.Life, partnerId: selected.Id);
        selected.Relationships.SetPartner(life: selected.Life, partnerId: human.Id);
    }

    /// <summary>
    /// Tenta criar um relacionamento extraconjugal.
    /// </summary>
    private static void TryFindLover(Human human, List<Human> potentialPartners, HashSet<(Guid first, Guid second)> closeRelatives)
    {
        if (human.Relationships.PartnerId is null)
        {
            return;
        }

        if (RandomHelpers.RandomBetween(1, 1000) > 5)
        {
            return;
        }

        // Escolhe um amante aleatório sem alocar lista temporária (reservoir sampling);
        Human? selected = null;
        int validCount = 0;

        foreach (Human x in potentialPartners)
        {
            if (x.Id == human.Id || !x.Life.IsAlive || x.Identity.Gender == human.Identity.Gender)
            {
                continue;
            }

            if (closeRelatives.Contains((human.Id, x.Id)))
            {
                continue;
            }

            validCount++;

            if (RandomHelpers.RandomBetween(1, validCount) == 1)
            {
                selected = x;
            }
        }

        if (selected is null)
        {
            return;
        }

        human.Relationships.AddLover(life: human.Life, loverId: selected.Id);
    }

    /// <summary>
    /// Tenta encerrar um relacionamento existente.
    /// </summary>
    private static void TryDivorce(Dictionary<Guid, Human> humansById, Human human, DateOnly currentDate)
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

        Human? partner = null;

        if (human.Relationships.PartnerId is not null)
        {
            humansById.TryGetValue(human.Relationships.PartnerId.Value, out partner);
        }

        // Use the full Divorce method to ensure emotional effects and married flag are updated.
        human.Relationships.Divorce(human.Life, human.Needs, human.Emotions, currentDate);
        partner?.Relationships.Divorce(partner.Life, partner.Needs, partner.Emotions, currentDate);
    }

    /// <summary>
    /// Obtém o parceiro mais provável para reprodução.
    /// </summary>
    private static Human? GetReproductionPartner(Dictionary<Guid, Human> humansById, Human human, HashSet<(Guid first, Guid second)> closeRelatives)
    {
        // Primeiro tenta utilizar o parceiro oficial do relacionamento;
        if (human.Relationships.PartnerId is not null)
        {
            Human? partner = null;

            if (human.Relationships.PartnerId is not null)
            {
                humansById.TryGetValue(human.Relationships.PartnerId.Value, out partner);
            }

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
            // Armazena o amante selecionado para reprodução.
            Human? selectedLover = null;

            // Quantidade de amantes válidos encontrados.
            int validCount = 0;

            // Percorre apenas os amantes registrados.
            foreach (Guid loverId in human.Relationships.LoversIds)
            {
                if (!humansById.TryGetValue(loverId, out Human? lover))
                {
                    continue;
                }

                if (!CanReproduceWith(human, lover, closeRelatives))
                {
                    continue;
                }

                validCount++;

                // Seleciona aleatoriamente um amante válido
                // sem criar listas temporárias.
                if (RandomHelpers.RandomBetween(1, validCount) == 1)
                {
                    selectedLover = lover;
                }
            }

            if (selectedLover is not null)
            {
                return selectedLover;
            }
        }

        return null;
    }

    /// <summary>
    /// Tenta gerar um novo filho utilizando parceiro oficial ou amante.
    /// </summary>
    private static bool TryProcreate(Dictionary<Guid, Human> humansById, World world, Human human, DateOnly currentDate, HashSet<(Guid first, Guid second)> closeRelatives)
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
        Human? partner = GetReproductionPartner(humansById, human, closeRelatives);

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
    /// Processa efeitos após uma morte: atualiza parceiro e distribui herança.
    /// </summary>
    private static void ProcessDeathEffects(Human deceased, World world, DateOnly currentDate, Dictionary<Guid, Human> humansById)
    {
        // Notifica e limpa o parceiro;
        if (deceased.Relationships.PartnerId is not null)
        {
            if (humansById.TryGetValue(deceased.Relationships.PartnerId.Value, out Human? partner) && partner.Life.IsAlive)
            {
                // Remove partner reference and married flag;
                partner.Relationships.RemovePartner();

                // Ajusta felicidade e registra evento;
                partner.Needs.DecreaseHappiness(30);
                partner.Life.AddLifeEvent(description: "Ficou viúvo.", currentDate);
            }
        }

        // Distribui herança simples;
        TryDistributeInheritance(deceased, world, currentDate, humansById);
    }

    /// <summary>
    /// Tenta um acidente de trabalho dependendo da profissão.
    /// </summary>
    private static void TryWorkplaceAccident(Dictionary<Guid, Human> humansById, Human human, DateOnly currentDate, World world)
    {
        if (!human.Life.IsAlive)
        {
            return;
        }

        if (human.Career.CareerStatus != CareerStatusEnum.Employed)
        {
            return;
        }

        // Taxas por 10.000 (ex.: 50 => 0.5% ao ano);
        int rate = human.Career.JobType switch
        {
            JobTypeEnum.Miner => 50,
            JobTypeEnum.Builder => 40,
            JobTypeEnum.Electrician => 60,
            JobTypeEnum.Plumber => 30,
            JobTypeEnum.Mechanic => 40,
            JobTypeEnum.Farmer => 30,
            JobTypeEnum.Fisherman => 30,
            JobTypeEnum.Firefighter => 80,
            JobTypeEnum.PoliceOfficer => 50,
            JobTypeEnum.Soldier => 60,
            JobTypeEnum.Driver => 20,
            _ => 0
        };

        if (rate == 0)
        {
            return;
        }

        if (RandomHelpers.RandomBetween(1, 10000) > rate)
        {
            return;
        }

        human.Life.Die(needs: human.Needs, cause: CauseOfDeathEnum.WorkplaceAccident, dateOfDeath: currentDate);

        ProcessDeathEffects(human, world, currentDate, humansById);
    }

    /// <summary>
    /// Tenta erro médico grave quando o humano possui doenças.
    /// </summary>
    private static void TryMedicalMalpractice(Dictionary<Guid, Human> humansById, Human human, DateOnly currentDate, World world)
    {
        if (!human.Life.IsAlive)
        {
            return;
        }

        if (human.Health.Diseases.Count == 0)
        {
            return;
        }

        // Probabilidade baixa de erro médico
        if (RandomHelpers.RandomBetween(1, 10000) > 5)
        {
            return;
        }

        human.Life.Die(needs: human.Needs, cause: CauseOfDeathEnum.MedicalMalpractice, dateOfDeath: currentDate);

        ProcessDeathEffects(human, world, currentDate, humansById);
    }

    /// <summary>
    /// Evento raro: desastre natural que afeta um país inteiro.
    /// </summary>
    private static void TryNaturalDisaster(World world, DateOnly currentDate, List<Human> humans, Dictionary<Guid, Human> humansById)
    {
        // Baixa probabilidade anual de um desastre;
        if (RandomHelpers.RandomBetween(1, 1000) > 5)
        {
            return;
        }

        CountryEnum affected = EnumExtensions.GetRandom<CountryEnum>();

        // Seleciona habitantes vivos no país afetado;
        List<Human> victims = [.. humans.Where(x => x.Location.CurrentCountry == affected)];

        if (victims.Count == 0)
        {
            return;
        }

        // Morte depende de vulnerabilidade;
        foreach (Human human in victims)
        {
            int chance = human.Life.Age switch
            {
                >= 60 => 15,
                < 12 => 10,
                _ => 4
            };

            if (RandomHelpers.RandomBetween(1, 100) <= chance)
            {
                human.Life.Die(needs: human.Needs, cause: CauseOfDeathEnum.NaturalDisaster, dateOfDeath: currentDate);

                ProcessDeathEffects(human, world, currentDate, humansById);
            }
        }
    }

    /// <summary>
    /// Pequenos eventos diários: sono, alimentação e exercício.
    /// </summary>
    private static void TrySleepEatExercise(Human human, DateOnly currentDate)
    {
        if (!human.Life.IsAlive)
        {
            return;
        }

        // Sono ocasional;
        if (RandomHelpers.RandomBetween(1, 100) <= 10)
        {
            int hours = RandomHelpers.RandomBetween(1, 8);
            HumanHealth.Sleep(human.Life, human.Needs, hours);
        }

        // Alimentação;
        if (RandomHelpers.RandomBetween(1, 100) <= 20)
        {
            int nutrition = RandomHelpers.RandomBetween(1, 20);
            HumanHealth.Eat(human.Life, human.Needs, nutrition);
        }

        // Exercício físico;
        if (RandomHelpers.RandomBetween(1, 100) <= 5)
        {
            int hours = RandomHelpers.RandomBetween(1, 3);
            HumanHealth.Exercise(human.Life, human.Needs, hours, currentDate);
        }
    }

    /// <summary>
    /// Simula estudo e possibilidade de conclusão de formação.
    /// </summary>
    private static void TryStudyAndGraduate(Human human, DateOnly currentDate)
    {
        if (!human.Life.IsAlive)
        {
            return;
        }

        // Crianças e jovens estudam com mais frequência;
        if (human.Life.Age >= 6 && human.Life.Age <= 25)
        {
            if (RandomHelpers.RandomBetween(1, 100) <= 20)
            {
                int hours = RandomHelpers.RandomBetween(1, 5);
                human.Education.Study(human.Life, human.Needs, hours);
            }

            // Pequena chance anual de concluir um nível;
            if (RandomHelpers.RandomBetween(1, 100) <= 3)
            {
                human.Education.Graduate(human.Life, human.Needs, currentDate);
            }
        }
    }

    /// <summary>
    /// Tenta inserir o humano no mercado de trabalho, trabalhar, ser promovido, demitido ou aposentar.
    /// </summary>
    private static void TryWorkAndCareer(Human human, DateOnly currentDate)
    {
        if (!human.Life.IsAlive)
        {
            return;
        }

        // Apenas adultos podem trabalhar;
        if (!human.Life.IsAdult)
        {
            return;
        }

        // Aposentadoria por idade;
        if (human.Life.Age >= 65)
        {
            if (RandomHelpers.RandomBetween(1, 100) <= 50 && human.Career.CareerStatus != CareerStatusEnum.Retired)
            {
                human.Career.Retire(human.Life, human.Needs, currentDate);
            }

            return;
        }

        // Conseguir um emprego quando desempregado;
        if (human.Career.CareerStatus != CareerStatusEnum.Employed)
        {
            if (RandomHelpers.RandomBetween(1, 100) <= 20)
            {
                JobTypeEnum job = EnumExtensions.GetRandom<JobTypeEnum>();

                if (job != JobTypeEnum.None)
                {
                    human.Career.SetJob(human.Life, job, currentDate);
                }
            }

            return;
        }

        // Trabalhar e receber salário;
        decimal salary = RandomHelpers.RandomBetween(min: 500, max: 10_000);
        human.Career.Work(human.Life, human.Finance, human.Needs, salary);

        // Promoção ocasional;
        if (RandomHelpers.RandomBetween(1, 100) <= 5)
        {
            human.Career.Promote(human.Life, human.Needs, currentDate);
        }

        // Demissão ocasional;
        if (RandomHelpers.RandomBetween(1, 1000) <= 2)
        {
            human.Career.Fire(human.Life, human.Needs, wantedToBeFired: false, currentDate);
        }
    }

    /// <summary>
    /// Contrai doenças e tenta se curar.
    /// </summary>
    private static void TrySicknessAndRecovery(Human human, DateOnly currentDate)
    {
        if (!human.Life.IsAlive)
        {
            return;
        }

        // Contrair uma doença;
        if (human.Health.Diseases.Count == 0 && RandomHelpers.RandomBetween(1, 100) <= 5)
        {
            DiseaseEnum disease = EnumExtensions.GetRandom<DiseaseEnum>();
            human.Health.ContractDisease(human.Life, human.Needs, human.Emotions, disease, currentDate);
        }

        // Tentar curar uma doença existente;
        if (human.Health.Diseases.Count > 0 && RandomHelpers.RandomBetween(1, 100) <= 20)
        {
            DiseaseEnum disease = human.Health.Diseases[0];
            human.Health.CureDisease(human.Life, human.Needs, human.Health, disease, currentDate);
        }
    }

    /// <summary>
    /// Eventos financeiros: doações e empréstimos.
    /// </summary>
    private static void TryDonateOrLoan(Human human, DateOnly currentDate)
    {
        if (!human.Life.IsAlive)
        {
            return;
        }

        // Doação por ricos
        if (human.Finance.IsRich && RandomHelpers.RandomBetween(1, 1000) <= 5)
        {
            decimal amount = Math.Max(1, human.Finance.Money / 10);
            human.Finance.Donate(human.Life, human.Needs, amount, currentDate);
        }

        // Tomar empréstimo quando pobre
        if (human.Finance.IsPoor && RandomHelpers.RandomBetween(1, 1000) <= 10)
        {
            decimal amount = RandomHelpers.RandomBetween(min: 100, max: 10_000);
            human.Finance.TakeLoan(human.Life, amount, currentDate);
        }
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
    private static bool TryNaturalDeath(Dictionary<Guid, Human> humansById, Human human, DateOnly currentDate)
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

        Human? partner = null;

        if (human.Relationships.PartnerId is not null)
        {
            humansById.TryGetValue(human.Relationships.PartnerId.Value, out partner);
        }

        partner?.Needs.DecreaseHappiness(25);

        return true;
    }

    /// <summary>
    /// Tenta criar amizades ou inimizades aleatórias.
    /// </summary>
    private static void TryMakeFriendsEnemies(Human human, List<Human> potentialPartners)
    {
        if (!human.Life.IsAlive)
        {
            return;
        }

        // Chance pequena de fazer um amigo;
        if (RandomHelpers.RandomBetween(1, 100) <= 8)
        {
            // Seleciona um amigo aleatório sem alocar lista temporária;
            Human? selectedFriend = null;
            int validCount = 0;

            foreach (Human x in potentialPartners)
            {
                if (x.Id == human.Id || !x.Life.IsAlive)
                {
                    continue;
                }

                validCount++;

                if (RandomHelpers.RandomBetween(1, validCount) == 1)
                {
                    selectedFriend = x;
                }
            }

            if (selectedFriend is not null)
            {
                human.Social.AddFriend(human.Life, human.Needs, selectedFriend.Id);
            }
        }

        // Chance menor de arrumar um inimigo;
        if (RandomHelpers.RandomBetween(1, 100) <= 3)
        {
            // Seleciona um inimigo aleatório sem alocar lista temporária;
            Human? selectedEnemy = null;
            int validCountEnemy = 0;

            foreach (Human x in potentialPartners)
            {
                if (x.Id == human.Id || !x.Life.IsAlive)
                {
                    continue;
                }

                validCountEnemy++;
                if (RandomHelpers.RandomBetween(1, validCountEnemy) == 1)
                {
                    selectedEnemy = x;
                }
            }

            if (selectedEnemy is not null)
            {
                human.Social.AddEnemy(human.Life, human.Needs, selectedEnemy.Id);
            }
        }
    }

    /// <summary>
    /// Converte parceiros em cônjuges (casamento) ocasionalmente.
    /// </summary>
    private static void TryMarryCouple(Dictionary<Guid, Human> humansById, Human human, DateOnly currentDate)
    {
        if (!human.Life.IsAlive)
        {
            return;
        }

        if (human.Relationships.PartnerId is null)
        {
            return;
        }

        if (RandomHelpers.RandomBetween(1, 100) > 5)
        {
            return;
        }

        if (!humansById.TryGetValue(human.Relationships.PartnerId.Value, out Human? partner))
        {
            return;
        }

        // Ambos devem ser adultos e vivos
        if (!human.Life.IsAdult || !partner.Life.IsAdult)
        {
            return;
        }

        // Registrar casamento para ambos
        human.Relationships.Marry(human.Life, human.Needs, human.Emotions, partner.Id, currentDate);
        partner.Relationships.Marry(partner.Life, partner.Needs, partner.Emotions, human.Id, currentDate);
    }

    /// <summary>
    /// Tenta roubar dinheiro de outro humano.
    /// </summary>
    private static void TryAttemptTheft(Dictionary<Guid, Human> humansById, Human human)
    {
        if (!human.Life.IsAlive)
        {
            return;
        }

        // Humanos muito pobres têm mais chance de roubar
        if (!human.Finance.IsPoor || RandomHelpers.RandomBetween(1, 1000) > 5)
        {
            return;
        }

        // Escolhe uma vítima aleatória que tenha dinheiro (reservoir sampling);
        Human? victim = null;
        int validCount = 0;

        foreach (Human candidate in humansById.Values)
        {
            if (candidate.Id == human.Id || !candidate.Life.IsAlive || candidate.Finance.Money <= 0)
            {
                continue;
            }

            validCount++;

            if (RandomHelpers.RandomBetween(1, validCount) == 1)
            {
                victim = candidate;
            }
        }

        if (victim is null)
        {
            return;
        }

        decimal amount = Math.Min(victim.Finance.Money, RandomHelpers.RandomBetween(min: 10, max: 1_000));

        if (amount <= 0)
        {
            return;
        }

        // Transferência simples: recebe o ladrão, vítima perde dinheiro (tentativa sem validação de vida da vítima);
        victim.Finance.SpendMoney(victim.Life, amount);
        human.Finance.EarnMoney(human.Life, amount);
    }

    /// <summary>
    /// Distribui herança simples quando um humano morre.
    /// </summary>
    private static void TryDistributeInheritance(Human human, World world, DateOnly currentDate, Dictionary<Guid, Human> humansById)
    {
        // Se o humano não morreu, não há herança para distribuir;
        if (!human.Life.IsDead)
        {
            return;
        }

        // Liquidar o espólio (paga dívidas e retorna o montante disponível)
        decimal estate = human.Finance.SettleEstate(human.Life, currentDate);

        if (estate <= 0)
        {
            return;
        }

        List<Human> heirs = [];

        // Preferir parceiro vivo; 
        if (human.Relationships.PartnerId is not null)
        {
            if (humansById.TryGetValue(human.Relationships.PartnerId.Value, out Human? partner) && partner.Life.IsAlive)
            {
                heirs.Add(partner);
            }
        }

        // Filhos vivos;
        foreach (Guid childId in human.Family.ChildrenIds)
        {
            if (humansById.TryGetValue(childId, out Human? child) && child.Life.IsAlive)
            {
                heirs.Add(child);
            }
        }

        // Se não houver herdeiros válidos, dá parte para amigos;
        if (heirs.Count == 0)
        {
            foreach (Guid friendId in human.Social.FriendsIds)
            {
                if (humansById.TryGetValue(friendId, out Human? friend) && friend.Life.IsAlive)
                {
                    heirs.Add(friend);
                }
            }
        }

        if (heirs.Count == 0)
        {
            return;
        }

        decimal portion = Math.Max(1, Math.Floor(estate / heirs.Count));

        foreach (Human heir in heirs)
        {
            string from = $"{human.Identity.FirstName} {human.Identity.LastName}";
            heir.Finance.ReceiveInheritance(heir.Life, heir.Needs, portion, currentDate, from);
        }
    }

    /// <summary>
    /// Determina se o habitante obtém/morre por depressão.
    /// </summary>
    private static bool TryToGetDepression(Dictionary<Guid, Human> humansById, Human human, DateOnly currentDate)
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

        Human? partner = null;

        if (human.Relationships.PartnerId is not null)
        {
            humansById.TryGetValue(human.Relationships.PartnerId.Value, out partner);
        }

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
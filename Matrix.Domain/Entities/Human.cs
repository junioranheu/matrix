using Matrix.Domain.Enums;

namespace Matrix.Domain.Entities;

public sealed class Human(
    string firstName,
    string lastName,
    CountryEnum country,
    int health,
    int intelligence,
    int charisma,
    int strength,
    int hunger,
    int happiness,
    int stress,
    Guid? fatherId = null,
    Guid? motherId = null)
{
    #region Identity
    /// <summary>
    /// Identificador único do humano.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Primeiro nome.
    /// </summary>
    public string FirstName { get; private set; } = firstName;

    /// <summary>
    /// Sobrenome.
    /// </summary>
    public string LastName { get; private set; } = lastName;

    /// <summary>
    /// Nome completo.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
    #endregion

    #region Family
    /// <summary>
    /// Identificador do pai.
    /// </summary>
    public Guid? FatherId { get; private set; } = fatherId;

    /// <summary>
    /// Identificador da mãe.
    /// </summary>
    public Guid? MotherId { get; private set; } = motherId;

    /// <summary>
    /// Filhos.
    /// </summary>
    public List<Guid> ChildrenIds { get; private set; } = [];
    #endregion

    #region Relationship
    /// <summary>
    /// Parceiro atual.
    /// </summary>
    public Guid? PartnerId { get; private set; }

    /// <summary>
    /// Histórico de amantes.
    /// </summary>
    public List<Guid> LoversIds { get; private set; } = [];
    #endregion

    #region Social
    /// <summary>
    /// Amigos.
    /// </summary>
    public List<Guid> FriendsIds { get; private set; } = [];

    /// <summary>
    /// Inimigos.
    /// </summary>
    public List<Guid> EnemiesIds { get; private set; } = [];
    #endregion

    #region Location
    /// <summary>
    /// País de nascimento.
    /// </summary>
    public CountryEnum CountryBorn { get; private set; } = country;

    /// <summary>
    /// País atual.
    /// </summary>
    public CountryEnum CountryCurrent { get; private set; } = country;
    #endregion

    #region Life
    /// <summary>
    /// Idade atual.
    /// </summary>
    public int Age { get; private set; }

    /// <summary>
    /// Indica se está vivo.
    /// </summary>
    public bool IsAlive { get; private set; } = true;

    /// <summary>
    /// Causa da morte.
    /// </summary>
    public CauseOfDeathEnum? CauseOfDeath { get; private set; }
    #endregion

    #region Finance
    /// <summary>
    /// Dinheiro disponível.
    /// </summary>
    public decimal Money { get; private set; }

    /// <summary>
    /// Dívidas.
    /// </summary>
    public decimal Debt { get; private set; }

    /// <summary>
    /// Status financeiro.
    /// </summary>
    public FinancialStatus FinancialStatus { get; private set; } = FinancialStatus.Unknown;
    #endregion

    #region Attributes
    /// <summary>
    /// Saúde física.
    /// </summary>
    public int Health { get; private set; } = health;

    /// <summary>
    /// Inteligência.
    /// </summary>
    public int Intelligence { get; private set; } = intelligence;

    /// <summary>
    /// Carisma.
    /// </summary>
    public int Charisma { get; private set; } = charisma;

    /// <summary>
    /// Força física.
    /// </summary>
    public int Strength { get; private set; } = strength;
    #endregion

    #region Needs
    /// <summary>
    /// Fome.
    /// </summary>
    public int Hunger { get; private set; } = hunger;

    /// <summary>
    /// Felicidade.
    /// </summary>
    public int Happiness { get; private set; } = happiness;

    /// <summary>
    /// Estresse.
    /// </summary>
    public int Stress { get; private set; } = stress;

    /// <summary>
    /// Energia.
    /// </summary>
    public int Energy { get; private set; } = 100;
    #endregion

    #region Health
    /// <summary>
    /// Imunidade.
    /// </summary>
    public int Immunity { get; private set; } = 50;

    /// <summary>
    /// Fertilidade.
    /// </summary>
    public int Fertility { get; private set; } = 100;

    /// <summary>
    /// Doenças atuais.
    /// </summary>
    public List<DiseaseEnum> Diseases { get; private set; } = [];
    #endregion

    #region Education
    /// <summary>
    /// Nível educacional.
    /// </summary>
    public int EducationLevel { get; private set; }

    /// <summary>
    /// Conhecimento acumulado.
    /// </summary>
    public int Knowledge { get; private set; }
    #endregion

    #region Career
    /// <summary>
    /// Profissão atual.
    /// </summary>
    public JobTypeEnum? JobType { get; private set; } = JobTypeEnum.None;

    /// <summary>
    /// Experiência profissional.
    /// </summary>
    public int WorkExperience { get; private set; }

    /// <summary>
    /// Reputação.
    /// </summary>
    public int Reputation { get; private set; }

    /// <summary>
    /// Anos trabalhados.
    /// </summary>
    public int YearsWorked { get; private set; }
    #endregion

    #region Emotions
    /// <summary>
    /// Humor atual.
    /// </summary>
    public MoodEnum Mood { get; private set; } = MoodEnum.Neutral;
    #endregion

    #region Statistics
    /// <summary>
    /// Quantidade total de relacionamentos.
    /// </summary>
    public int RelationshipsCount { get; private set; }

    /// <summary>
    /// Eventos importantes da vida.
    /// </summary>
    public List<string> LifeEvents { get; private set; } = [];
    #endregion

    #region Computed properties
    public bool HasPartner => PartnerId.HasValue;

    public bool HasChildren => ChildrenIds.Count > 0;

    public bool IsChild => Age < 12;

    public bool IsTeenager => Age >= 12 && Age < 18;

    public bool IsAdult => Age >= 18;

    public bool IsElderly => Age >= 60;

    public bool IsRich => Money >= 1_000_000m;

    public bool IsPoor => Money <= 1_000m;

    public bool IsHungry => Hunger >= 80;

    public bool IsStressed => Stress >= 80;

    public bool IsHealthy => Health >= 80 && Diseases.Count == 0;
    #endregion

    #region Helper methods
    private bool CannotAct()
    {
        return !IsAlive;
    }

    private static bool IsInvalid(Guid id)
    {
        return id == Guid.Empty;
    }

    private static int Clamp(int value)
    {
        return Math.Clamp(value, 0, 100);
    }
    #endregion

    #region Life methods
    /// <summary>
    /// Incrementa a idade em um ano.
    /// </summary>
    public void AgeOneYear()
    {
        if (CannotAct())
        {
            return;
        }

        Age++;

        IncreaseHunger(5);
        IncreaseStress(2);

        Energy = Clamp(Energy - 3);

        if (Age >= 65)
        {
            Damage(Random.Shared.Next(0, 3));
        }

        if (Diseases.Count > 0)
        {
            Damage(Diseases.Count);
        }

        AddLifeEvent($"Completou {Age} anos.");
    }

    /// <summary>
    /// Mata o humano.
    /// </summary>
    public void Die(CauseOfDeathEnum cause = CauseOfDeathEnum.NaturalCauses)
    {
        IsAlive = false;
        Health = 0;
        Energy = 0;
        CauseOfDeath = cause;

        AddLifeEvent($"Faleceu por {cause}.");
    }

    /// <summary>
    /// Adiciona um evento ao histórico.
    /// </summary>
    public void AddLifeEvent(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return;
        }

        LifeEvents.Add(description);
    }
    #endregion

    #region Health methods
    /// <summary>
    /// Recupera saúde.
    /// </summary>
    public void Heal(int amount)
    {
        if (CannotAct() || amount <= 0)
        {
            return;
        }

        Health = Clamp(Health + amount);
    }

    /// <summary>
    /// Aplica dano.
    /// </summary>
    public void Damage(int amount)
    {
        if (CannotAct() || amount <= 0)
        {
            return;
        }

        Health -= amount;

        if (Health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Recupera energia através do sono.
    /// </summary>
    public void Sleep(int hours)
    {
        if (CannotAct() || hours <= 0)
        {
            return;
        }

        Energy = Clamp(Energy + (hours * 10));

        DecreaseStress(hours * 2);

        IncreaseHunger(hours);

        AddLifeEvent($"Dormiu por {hours} horas.");
    }

    /// <summary>
    /// Alimenta o humano.
    /// </summary>
    public void Eat(int nutrition)
    {
        if (CannotAct() || nutrition <= 0)
        {
            return;
        }

        DecreaseHunger(nutrition);

        IncreaseHappiness(2);

        Energy = Clamp(Energy + (nutrition / 2));
    }

    /// <summary>
    /// Realiza atividade física.
    /// </summary>
    public void Exercise(int hours)
    {
        if (CannotAct() || hours <= 0)
        {
            return;
        }

        Strength = Clamp(Strength + hours);

        Energy = Clamp(Energy - (hours * 5));

        IncreaseHunger(hours * 3);

        IncreaseHappiness(hours);

        IncreaseStress(hours);

        AddLifeEvent($"Praticou exercícios por {hours} horas.");
    }

    /// <summary>
    /// Aumenta imunidade.
    /// </summary>
    public void IncreaseImmunity(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Immunity = Clamp(Immunity + amount);
    }

    /// <summary>
    /// Reduz imunidade.
    /// </summary>
    public void DecreaseImmunity(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Immunity = Clamp(Immunity - amount);
    }

    /// <summary>
    /// Contrai uma doença.
    /// </summary>
    public void ContractDisease(DiseaseEnum disease)
    {
        if (Diseases.Contains(disease))
        {
            return;
        }

        Diseases.Add(disease);

        Damage(5);

        DecreaseHappiness(5);

        ChangeMood(MoodEnum.Sad);

        AddLifeEvent($"Contraiu {disease}.");
    }

    /// <summary>
    /// Cura uma doença.
    /// </summary>
    public void CureDisease(DiseaseEnum disease)
    {
        if (!Diseases.Contains(disease))
        {
            return;
        }

        Diseases.Remove(disease);

        Heal(5);

        IncreaseHappiness(5);

        AddLifeEvent($"Curou-se de {disease}.");
    }

    #endregion

    #region Needs methods
    /// <summary>
    /// Aumenta a fome.
    /// </summary>
    public void IncreaseHunger(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Hunger = Clamp(Hunger + amount);
    }

    /// <summary>
    /// Reduz a fome.
    /// </summary>
    public void DecreaseHunger(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Hunger = Clamp(Hunger - amount);
    }

    /// <summary>
    /// Aumenta o estresse.
    /// </summary>
    public void IncreaseStress(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Stress = Clamp(Stress + amount);
    }

    /// <summary>
    /// Reduz o estresse.
    /// </summary>
    public void DecreaseStress(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Stress = Clamp(Stress - amount);
    }

    /// <summary>
    /// Aumenta felicidade.
    /// </summary>
    public void IncreaseHappiness(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Happiness = Clamp(Happiness + amount);
    }

    /// <summary>
    /// Reduz felicidade.
    /// </summary>
    public void DecreaseHappiness(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Happiness = Clamp(Happiness - amount);
    }
    #endregion

    #region Emotion methods
    /// <summary>
    /// Altera o humor atual.
    /// </summary>
    public void ChangeMood(MoodEnum mood)
    {
        Mood = mood;
    }
    #endregion

    #region Finance Methods
    /// <summary>
    /// Adiciona dinheiro ao patrimônio.
    /// </summary>
    public void EarnMoney(decimal amount)
    {
        if (CannotAct() || amount <= 0)
        {
            return;
        }

        Money += amount;
    }

    /// <summary>
    /// Remove dinheiro do patrimônio.
    /// </summary>
    public void SpendMoney(decimal amount)
    {
        if (CannotAct() || amount <= 0)
        {
            return;
        }

        Money -= amount;

        if (Money < 0)
        {
            Money = 0;
        }
    }

    /// <summary>
    /// Obtém um empréstimo.
    /// </summary>
    public void TakeLoan(decimal amount)
    {
        if (CannotAct() || amount <= 0)
        {
            return;
        }

        Debt += amount;
        Money += amount;

        AddLifeEvent($"Pegou um empréstimo de {amount:C}.");
    }

    /// <summary>
    /// Paga uma dívida.
    /// </summary>
    public void PayDebt(decimal amount)
    {
        if (CannotAct() || amount <= 0)
        {
            return;
        }

        if (!HasMoney(amount))
        {
            return;
        }

        SpendMoney(amount);

        Debt -= amount;

        if (Debt < 0)
        {
            Debt = 0;
        }
    }

    /// <summary>
    /// Verifica se possui dinheiro suficiente.
    /// </summary>
    public bool HasMoney(decimal amount)
    {
        return Money >= amount;
    }

    /// <summary>
    /// Doa dinheiro.
    /// </summary>
    public void Donate(decimal amount)
    {
        if (CannotAct() || amount <= 0)
        {
            return;
        }

        if (!HasMoney(amount))
        {
            return;
        }

        SpendMoney(amount);

        IncreaseHappiness(5);

        AddLifeEvent($"Doou {amount:C}.");
    }

    /// <summary>
    /// Recebe herança.
    /// </summary>
    public void ReceiveInheritance(decimal amount)
    {
        if (CannotAct() || amount <= 0)
        {
            return;
        }

        EarnMoney(amount);

        IncreaseHappiness(10);

        AddLifeEvent($"Recebeu uma herança de {amount:C}.");
    }
    #endregion

    #region Education methods
    /// <summary>
    /// Estuda.
    /// </summary>
    public void Study(int hours)
    {
        if (CannotAct() || hours <= 0)
        {
            return;
        }

        Knowledge += hours;

        Intelligence = Clamp(Intelligence + (hours / 20));

        Energy = Clamp(Energy - hours);

        IncreaseStress(hours / 2);

        AddLifeEvent($"Estudou por {hours} horas.");
    }

    /// <summary>
    /// Aumenta conhecimento.
    /// </summary>
    public void IncreaseKnowledge(int amount)
    {
        if (CannotAct() || amount <= 0)
        {
            return;
        }

        Knowledge += amount;
    }

    /// <summary>
    /// Conclui um nível educacional.
    /// </summary>
    public void Graduate()
    {
        if (CannotAct())
        {
            return;
        }

        EducationLevel++;

        IncreaseHappiness(10);

        IncreaseKnowledge(25);

        AddLifeEvent("Concluiu uma formação.");
    }

    #endregion

    #region Career methods
    /// <summary>
    /// Define uma profissão.
    /// </summary>
    public void SetJob(JobTypeEnum jobType)
    {
        if (CannotAct())
        {
            return;
        }

        JobType = jobType;

        AddLifeEvent($"Começou a trabalhar como {jobType}.");
    }

    /// <summary>
    /// Trabalha e recebe salário.
    /// </summary>
    public void Work(decimal salary)
    {
        if (CannotAct() || salary <= 0)
        {
            return;
        }

        if (JobType is null ||
            JobType == JobTypeEnum.None)
        {
            return;
        }

        EarnMoney(salary);

        WorkExperience++;

        IncreaseStress(10);

        Energy = Clamp(Energy - 15);

        IncreaseHunger(5);

        AddLifeEvent($"Recebeu salário de {salary:C}.");
    }

    /// <summary>
    /// Recebe promoção.
    /// </summary>
    public void Promote()
    {
        if (CannotAct())
        {
            return;
        }

        Reputation = Clamp(Reputation + 10);

        IncreaseHappiness(15);

        AddLifeEvent("Recebeu uma promoção.");
    }

    /// <summary>
    /// É demitido.
    /// </summary>
    public void Fire()
    {
        if (CannotAct())
        {
            return;
        }

        JobType = JobTypeEnum.None;

        IncreaseStress(20);

        DecreaseHappiness(20);

        AddLifeEvent("Foi demitido.");
    }

    /// <summary>
    /// Aumenta reputação.
    /// </summary>
    public void IncreaseReputation(int amount)
    {
        if (CannotAct() || amount <= 0)
        {
            return;
        }

        Reputation = Clamp(Reputation + amount);
    }

    /// <summary>
    /// Reduz reputação.
    /// </summary>
    public void DecreaseReputation(int amount)
    {
        if (CannotAct() || amount <= 0)
        {
            return;
        }

        Reputation = Clamp(Reputation - amount);
    }

    /// <summary>
    /// Aposenta-se.
    /// </summary>
    public void Retire()
    {
        if (CannotAct())
        {
            return;
        }

        JobType = JobTypeEnum.None;

        IncreaseHappiness(20);

        DecreaseStress(20);

        AddLifeEvent("Aposentou-se.");
    }
    #endregion

    #region Relationship methods
    /// <summary>
    /// Define um parceiro.
    /// </summary>
    public void SetPartner(Guid partnerId)
    {
        if (CannotAct())
        {
            return;
        }

        if (IsInvalid(partnerId))
        {
            return;
        }

        PartnerId = partnerId;
    }

    /// <summary>
    /// Remove o parceiro atual.
    /// </summary>
    public void RemovePartner()
    {
        PartnerId = null;
    }

    /// <summary>
    /// Casa-se com outro humano.
    /// </summary>
    public void Marry(Guid partnerId)
    {
        if (CannotAct())
        {
            return;
        }

        if (!IsAdult)
        {
            return;
        }

        SetPartner(partnerId);

        RelationshipsCount++;

        IncreaseHappiness(15);

        ChangeMood(MoodEnum.Happy);

        AddLifeEvent("Casou-se.");
    }

    /// <summary>
    /// Realiza um divórcio.
    /// </summary>
    public void Divorce()
    {
        if (CannotAct())
        {
            return;
        }

        if (!HasPartner)
        {
            return;
        }

        RemovePartner();

        IncreaseStress(20);

        DecreaseHappiness(20);

        ChangeMood(MoodEnum.Heartbroken);

        AddLifeEvent("Divorciou-se.");
    }

    /// <summary>
    /// Adiciona um amante.
    /// </summary>
    public void AddLover(Guid loverId)
    {
        if (CannotAct())
        {
            return;
        }

        if (IsInvalid(loverId))
        {
            return;
        }

        if (LoversIds.Contains(loverId))
        {
            return;
        }

        LoversIds.Add(loverId);
    }

    /// <summary>
    /// Remove um amante.
    /// </summary>
    public void RemoveLover(Guid loverId)
    {
        LoversIds.Remove(loverId);
    }
    #endregion

    #region Family methods
    /// <summary>
    /// Adiciona um filho.
    /// </summary>
    public void AddChild(Guid childId)
    {
        if (CannotAct())
        {
            return;
        }

        if (IsInvalid(childId))
        {
            return;
        }

        if (ChildrenIds.Contains(childId))
        {
            return;
        }

        ChildrenIds.Add(childId);

        IncreaseHappiness(20);

        AddLifeEvent("Teve um filho.");
    }

    /// <summary>
    /// Remove um filho.
    /// </summary>
    public void RemoveChild(Guid childId)
    {
        ChildrenIds.Remove(childId);
    }

    /// <summary>
    /// Verifica se pode se reproduzir.
    /// </summary>
    public bool CanReproduce()
    {
        return IsAlive &&
               Age >= 18 &&
               Age <= 50 &&
               Health > 30 &&
               Fertility > 0;
    }

    #endregion

    #region Social Methods

    /// <summary>
    /// Adiciona um amigo.
    /// </summary>
    public void AddFriend(Guid friendId)
    {
        if (CannotAct())
        {
            return;
        }

        if (IsInvalid(friendId))
        {
            return;
        }

        if (FriendsIds.Contains(friendId))
        {
            return;
        }

        FriendsIds.Add(friendId);

        IncreaseHappiness(5);
    }

    /// <summary>
    /// Remove um amigo.
    /// </summary>
    public void RemoveFriend(Guid friendId)
    {
        FriendsIds.Remove(friendId);
    }

    /// <summary>
    /// Adiciona um inimigo.
    /// </summary>
    public void AddEnemy(Guid enemyId)
    {
        if (CannotAct())
        {
            return;
        }

        if (IsInvalid(enemyId))
        {
            return;
        }

        if (EnemiesIds.Contains(enemyId))
        {
            return;
        }

        EnemiesIds.Add(enemyId);

        IncreaseStress(5);
    }

    /// <summary>
    /// Remove um inimigo.
    /// </summary>
    public void RemoveEnemy(Guid enemyId)
    {
        EnemiesIds.Remove(enemyId);
    }
    #endregion

    #region Location methods
    /// <summary>
    /// Muda o país de residência.
    /// </summary>
    public void MoveToCountry(CountryEnum country)
    {
        if (CannotAct())
        {
            return;
        }

        CountryCurrent = country;

        AddLifeEvent($"Mudou-se para {country}.");
    }
    #endregion

    #region Validation methods
    /// <summary>
    /// Verifica se é adulto.
    /// </summary>
    public bool IsAdultHuman()
    {
        return Age >= 18;
    }

    /// <summary>
    /// Verifica se é idoso.
    /// </summary>
    public bool IsElderlyHuman()
    {
        return Age >= 60;
    }
    #endregion
}
using Matrix.Domain.Enums;

namespace Matrix.Domain.Entities;

public sealed class Human(string firstName,
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
    #region props
    public Guid Id { get; } = Guid.NewGuid();

    // Identidade;
    public string FirstName { get; private set; } = firstName;
    public string LastName { get; private set; } = lastName;

    // Família;
    public Guid? FatherId { get; private set; } = fatherId;
    public Guid? MotherId { get; private set; } = motherId;

    // Relacionamentos;
    public Guid? PartnerId { get; private set; } = null;
    public List<Guid> LoversIds { get; private set; } = [];

    // Origem;
    public CountryEnum CountryBorn { get; private set; } = country;
    public CountryEnum CountryCurrent { get; private set; } = country;

    // Vida;
    public int Age { get; private set; } = 0;
    public bool IsAlive { get; private set; } = true;

    // Dinheiro;
    public decimal Money { get; private set; } = 0;
    public FinancialStatus FinancialStatus { get; private set; } = FinancialStatus.Unknown;

    // Características;
    public int Health { get; private set; } = health;
    public int Intelligence { get; private set; } = intelligence;
    public int Charisma { get; private set; } = charisma;
    public int Strength { get; private set; } = strength;

    // Necessidades;
    public int Hunger { get; private set; } = hunger;
    public int Happiness { get; private set; } = happiness;
    public int Stress { get; private set; } = stress;

    // Trabalho;
    public JobTypeEnum? JobType { get; private set; } = JobTypeEnum.None;

    // Saúde
    public int Energy { get; private set; } = 100;
    public int Immunity { get; private set; } = 50;

    // Educação
    public int EducationLevel { get; private set; }
    public int Knowledge { get; private set; }

    // Trabalho
    public int WorkExperience { get; private set; }
    public int Reputation { get; private set; }

    // Família
    public List<Guid> ChildrenIds { get; private set; } = [];

    // Doenças
    public List<DiseaseEnum> Diseases { get; private set; } = [];

    // Emoções
    public MoodEnum Mood { get; private set; } = MoodEnum.Neutral;

    // Fertilidade
    public int Fertility { get; private set; } = 100;

    // Patrimônio
    public decimal Debt { get; private set; }

    // Estatísticas
    public int YearsWorked { get; private set; }
    public int RelationshipsCount { get; private set; }

    // Histórico
    public List<string> LifeEvents { get; private set; } = [];
    #endregion

    #region methods
    /// <summary>
    /// Incrementa a idade do humano em um ano.
    /// Não possui efeito caso o humano esteja morto.
    /// </summary>
    public void AgeOneYear()
    {
        if (!IsAlive)
        {
            return;
        }

        Age++;
    }

    /// <summary>
    /// Adiciona dinheiro ao patrimônio do humano.
    /// Não possui efeito caso o humano esteja morto.
    /// </summary>
    public void EarnMoney(decimal amount)
    {
        if (!IsAlive)
        {
            return;
        }

        Money += amount;
    }

    /// <summary>
    /// Remove dinheiro do patrimônio do humano.
    /// O saldo nunca será inferior a zero.
    /// </summary>
    public void SpendMoney(decimal amount)
    {
        if (!IsAlive)
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
    /// Recupera pontos de saúde.
    /// A saúde máxima é limitada a 100.
    /// </summary>
    public void Heal(int amount)
    {
        if (!IsAlive)
        {
            return;
        }

        Health += amount;

        if (Health > 100)
        {
            Health = 100;
        }
    }

    /// <summary>
    /// Aplica dano ao humano.
    /// Caso a saúde chegue a zero ou menos, o humano morre.
    /// </summary>
    public void Damage(int amount)
    {
        if (!IsAlive)
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
    /// Define um parceiro para o humano.
    /// </summary>
    public void SetPartner(Guid partnerId)
    {
        if (!IsAlive)
        {
            return;
        }

        PartnerId = partnerId;
    }

    /// <summary>
    /// Remove o relacionamento atual do humano.
    /// </summary>
    public void RemovePartner()
    {
        PartnerId = null;
    }

    /// <summary>
    /// Finaliza a vida do humano.
    /// </summary>
    public void Die()
    {
        IsAlive = false;
        Health = 0;
    }

    /// <summary>
    /// Registra um amante para o humano.
    /// </summary>
    public void AddLover(Guid loverId)
    {
        if (!LoversIds.Contains(loverId))
        {
            LoversIds.Add(loverId);
        }
    }

    /// <summary>
    /// Remove um amante do histórico do humano.
    /// </summary>
    public void RemoveLover(Guid loverId)
    {
        LoversIds.Remove(loverId);
    }

    /// <summary>
    /// Verifica se o humano possui saldo suficiente.
    /// </summary>
    /// <returns>True quando possui saldo suficiente.</returns>
    public bool HasMoney(decimal amount)
    {
        return Money >= amount;
    }

    /// <summary>
    /// Verifica se o humano atingiu a maioridade.
    /// </summary>
    /// <returns>True quando possui 18 anos ou mais.</returns>
    public bool IsAdult()
    {
        return Age >= 18;
    }

    /// <summary>
    /// Verifica se o humano é idoso.
    /// </summary>
    /// <returns>True quando possui 60 anos ou mais.</returns>
    public bool IsElderly()
    {
        return Age >= 60;
    }

    public void IncreaseHunger(int amount)
    {
        Hunger = Math.Clamp(Hunger + amount, 0, 100);
    }

    public void DecreaseHunger(int amount)
    {
        Hunger = Math.Clamp(Hunger - amount, 0, 100);
    }

    public void IncreaseStress(int amount)
    {
        Stress = Math.Clamp(Stress + amount, 0, 100);
    }

    public void DecreaseStress(int amount)
    {
        Stress = Math.Clamp(Stress - amount, 0, 100);
    }

    public void IncreaseHappiness(int amount)
    {
        Happiness = Math.Clamp(Happiness + amount, 0, 100);
    }

    public void DecreaseHappiness(int amount)
    {
        Happiness = Math.Clamp(Happiness - amount, 0, 100);
    }

    /// <summary>
    /// Verifica se o humano pode se reproduzir.
    /// </summary>
    /// <returns>True quando atende aos critérios mínimos.</returns>
    public bool CanReproduce()
    {
        return IsAlive && Age >= 18 && Age <= 50 && Health > 30;
    }
    #endregion
}
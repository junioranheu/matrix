using Matrix.Domain.Enums;
using Matrix.Shared.Helpers;
using static Matrix.Shared.Helpers.NumericHelpers;

namespace Matrix.Domain.Entities;

public sealed class HumanCareer()
{
    #region props
    /// <summary>
    /// Profissão atual.
    /// </summary>
    public JobTypeEnum JobType { get; private set; } = JobTypeEnum.None;

    /// <summary>
    /// Situação profissional.
    /// </summary>
    public CareerStatusEnum CareerStatus { get; private set; } = CareerStatusEnum.Unemployed;

    /// <summary>
    /// Nível de carreira.
    /// </summary>
    public CareerLevelEnum CareerLevel { get; private set; } = CareerLevelEnum.None;

    /// <summary>
    /// Experiência profissional.
    /// </summary>
    public int Experience { get; private set; } = 0;

    /// <summary>
    /// Reputação profissional.
    /// </summary>
    public int Reputation { get; private set; } = 0;

    /// <summary>
    /// Satisfação com o trabalho.
    /// </summary>
    public int JobSatisfaction { get; private set; } = 50;

    /// <summary>
    /// Total de anos trabalhados.
    /// </summary>
    public int YearsWorked { get; private set; } = 0;
    #endregion

    #region methods
    /// <summary>
    /// Inicia uma carreira.
    /// </summary>
    public void SetJob(HumanLife life, JobTypeEnum jobType, DateOnly currentDate)
    {
        if (life.CannotAct())
        {
            return;
        }

        JobType = jobType;
        CareerStatus = CareerStatusEnum.Employed;

        if (CareerLevel == CareerLevelEnum.None)
        {
            CareerLevel = CareerLevelEnum.Junior;
        }

        life.AddLifeEvent(description: $"Começou a trabalhar como {jobType}.", currentDate);
    }

    /// <summary>
    /// Trabalha e recebe salário.
    /// </summary>
    public void Work(HumanLife life, HumanFinance finance, HumanNeeds needs, decimal salary)
    {
        if (life.CannotAct() || salary <= 0)
        {
            return;
        }

        if (CareerStatus != CareerStatusEnum.Employed)
        {
            return;
        }

        finance.EarnMoney(life, salary);

        GainExperience(life, 1);

        needs.IncreaseStress(10);

        needs.DecreaseEnergy(15);

        needs.IncreaseHunger(5);
    }

    /// <summary>
    /// Recebe uma promoção.
    /// </summary>
    public void Promote(HumanLife life, HumanNeeds needs, DateOnly currentDate)
    {
        if (life.CannotAct())
        {
            return;
        }

        IncreaseReputation(life, 10);

        CareerLevel = CareerLevel switch
        {
            CareerLevelEnum.None => CareerLevelEnum.Junior,
            CareerLevelEnum.Junior => CareerLevelEnum.MidLevel,
            CareerLevelEnum.MidLevel => CareerLevelEnum.Senior,
            CareerLevelEnum.Senior => CareerLevelEnum.Specialist,
            CareerLevelEnum.Specialist => CareerLevelEnum.Manager,
            CareerLevelEnum.Manager => CareerLevelEnum.Director,
            CareerLevelEnum.Director => CareerLevelEnum.Executive,
            _ => CareerLevelEnum.Executive
        };

        needs.IncreaseHappiness(15);

        life.AddLifeEvent(description: $"Foi promovido para {CareerLevel}.", currentDate);
    }

    /// <summary>
    /// É demitido.
    /// </summary>
    public void Fire(HumanLife life, HumanNeeds needs, bool wantedToBeFired, DateOnly currentDate)
    {
        if (life.CannotAct())
        {
            return;
        }

        JobType = JobTypeEnum.None;
        CareerStatus = CareerStatusEnum.Unemployed;

        if (wantedToBeFired)
        {
            needs.DecreaseHappiness(10);
            needs.IncreaseStress(20);
        }
        else
        {
            needs.DecreaseHappiness(RandomHelpers.RandomBetween(25, 50));
            needs.IncreaseStress(RandomHelpers.RandomBetween(25, 50));
        }

        life.AddLifeEvent(description: "Foi demitido.", currentDate);
    }

    /// <summary>
    /// Pede demissão.
    /// </summary>
    public void QuitJob(HumanLife life, HumanNeeds needs, DateOnly currentDate)
    {
        if (life.CannotAct())
        {
            return;
        }

        JobType = JobTypeEnum.None;
        CareerStatus = CareerStatusEnum.Unemployed;

        needs.IncreaseStress(5);

        life.AddLifeEvent(description: "Pediu demissão.", currentDate);
    }

    /// <summary>
    /// Aumenta a experiência profissional.
    /// </summary>
    public void GainExperience(HumanLife life, int amount)
    {
        if (life.CannotAct() || amount <= 0)
        {
            return;
        }

        Experience += amount;
    }

    /// <summary>
    /// Registra mais um ano de experiência profissional.
    /// </summary>
    public void CompleteWorkingYear(HumanLife life)
    {
        if (life.CannotAct())
        {
            return;
        }

        YearsWorked++;
    }

    /// <summary>
    /// Aumenta a reputação profissional.
    /// </summary>
    public void IncreaseReputation(HumanLife life, int amount)
    {
        if (life.CannotAct() || amount <= 0)
        {
            return;
        }

        Reputation = Clamp(Reputation + amount);
    }

    /// <summary>
    /// Reduz a reputação profissional.
    /// </summary>
    public void DecreaseReputation(HumanLife life, int amount)
    {
        if (life.CannotAct() || amount <= 0)
        {
            return;
        }

        Reputation = Clamp(Reputation - amount);
    }

    /// <summary>
    /// Aumenta a satisfação profissional.
    /// </summary>
    public void IncreaseJobSatisfaction(HumanLife life, int amount)
    {
        if (life.CannotAct() || amount <= 0)
        {
            return;
        }

        JobSatisfaction = Clamp(JobSatisfaction + amount);
    }

    /// <summary>
    /// Reduz a satisfação profissional.
    /// </summary>
    public void DecreaseJobSatisfaction(HumanLife life, int amount)
    {
        if (life.CannotAct() || amount <= 0)
        {
            return;
        }

        JobSatisfaction = Clamp(JobSatisfaction - amount);
    }

    /// <summary>
    /// Aposenta-se.
    /// </summary>
    public void Retire(HumanLife life, HumanNeeds needs, DateOnly currentDate)
    {
        if (life.CannotAct())
        {
            return;
        }

        JobType = JobTypeEnum.None;
        CareerStatus = CareerStatusEnum.Retired;

        needs.IncreaseHappiness(20);

        needs.DecreaseStress(20);

        life.AddLifeEvent(description: "Aposentou-se.", currentDate);
    }
    #endregion
}
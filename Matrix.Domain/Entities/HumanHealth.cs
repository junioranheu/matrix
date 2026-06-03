using Matrix.Domain.Enums;
using Matrix.Shared.Helpers;
using static Matrix.Shared.Helpers.NumericHelpers;

namespace Matrix.Domain.Entities;

public sealed class HumanHealth(int health, int immunity, int fertility, List<DiseaseEnum> diseases)
{
    #region props
    /// <summary>
    /// Saúde.
    /// </summary>
    public int Health { get; private set; } = health;

    /// <summary>
    /// Imunidade.
    /// </summary>
    public int Immunity { get; private set; } = immunity;

    /// <summary>
    /// Fertilidade.
    /// </summary>
    public int Fertility { get; private set; } = fertility;

    /// <summary>
    /// Doenças atuais.
    /// </summary>
    public List<DiseaseEnum> Diseases { get; private set; } = diseases;
    #endregion

    #region computed props
    public bool IsHealthy => Diseases.Count == 0;
    #endregion

    #region methods
    /// <summary>
    /// Recupera saúde.
    /// </summary>
    public void Heal(HumanLife life, int amount)
    {
        if (life.CannotAct() || amount <= 0)
        {
            return;
        }

        Health = Clamp(Health + amount);
    }

    /// <summary>
    /// Aplica dano.
    /// </summary>
    public void Damage(HumanLife life, HumanNeeds needs, int amount)
    {
        if (life.CannotAct() || amount <= 0)
        {
            return;
        }

        Health -= amount;

        if (Health <= 0)
        {
            life.Die(needs, health: this, cause: CauseOfDeathEnum.Unknown);
        }
    }

    /// <summary>
    /// Recupera energia através do sono.
    /// </summary>
    public static void Sleep(HumanLife life, HumanNeeds needs, int hours)
    {
        if (life.CannotAct() || hours <= 0)
        {
            return;
        }

        needs.IncreaseEnergy(hours * 10);

        needs.DecreaseStress(hours * 2);

        needs.IncreaseHunger(hours);

        life.AddLifeEvent($"Dormiu por {hours} horas.");
    }

    /// <summary>
    /// Alimenta o humano.
    /// </summary>
    public static void Eat(HumanLife life, HumanNeeds needs, int nutrition)
    {
        if (life.CannotAct() || nutrition <= 0)
        {
            return;
        }

        needs.DecreaseHunger(nutrition);

        needs.IncreaseHappiness(2);

        needs.IncreaseEnergy(nutrition / 2);
    }

    /// <summary>
    /// Realiza atividade física.
    /// </summary>
    public static void Exercise(HumanLife life, HumanNeeds needs, int hours)
    {
        if (life.CannotAct() || hours <= 0)
        {
            return;
        }

        needs.DecreaseEnergy(hours * 5);

        needs.IncreaseHunger(hours * 3);

        needs.IncreaseHappiness(hours);

        needs.IncreaseStress(hours);

        life.AddLifeEvent($"Praticou exercícios por {hours} horas.");
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
    public void ContractDisease(HumanLife life, HumanNeeds needs, HumanEmotions emotions, DiseaseEnum disease)
    {
        if (Diseases.Contains(disease))
        {
            return;
        }

        Diseases.Add(disease);

        Damage(life, needs, amount: 5);

        needs.DecreaseHappiness(RandomHelpers.RandomBetween(1, 35));

        emotions.ChangeMood(MoodEnum.Sad);

        life.AddLifeEvent($"Contraiu {disease}.");
    }

    /// <summary>
    /// Cura uma doença.
    /// </summary>
    public void CureDisease(HumanLife life, HumanNeeds needs, HumanHealth health, DiseaseEnum disease)
    {
        if (!Diseases.Contains(disease))
        {
            return;
        }

        Diseases.Remove(disease);

        health.Heal(life, amount: 5);

        needs.IncreaseHappiness(5);

        life.AddLifeEvent($"Curou-se de {disease}.");
    }
    #endregion
}
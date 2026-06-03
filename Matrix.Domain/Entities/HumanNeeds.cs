using static Matrix.Shared.Helpers.NumericHelpers;

namespace Matrix.Domain.Entities;

public sealed class HumanNeeds(int hunger, int happiness, int stress, int energy)
{
    #region props
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
    public int Energy { get; private set; } = energy;
    #endregion

    #region computed props
    public bool IsHungry => Hunger >= 50 && Hunger < 85;

    public bool IsStarving => Hunger >= 85;

    public bool IsStressed => Stress >= 51;
    #endregion

    #region methods
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

    /// <summary>
    /// Aumenta energia.
    /// </summary>
    public void IncreaseEnergy(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Energy = Clamp(Energy + amount);
    }

    /// <summary>
    /// Reduz energia.
    /// </summary>
    public void DecreaseEnergy(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Energy = Clamp(Energy - amount);
    }
    #endregion
}
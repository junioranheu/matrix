namespace Matrix.Domain.Entities;

public sealed class HumanEducation()
{
    #region props
    /// <summary>
    /// Nível educacional.
    /// </summary>
    public int EducationLevel { get; private set; } = 0;

    /// <summary>
    /// Conhecimento acumulado.
    /// </summary>
    public int Knowledge { get; private set; } = 0;
    #endregion

    #region methods
    /// <summary>
    /// Estuda.
    /// </summary>
    public void Study(HumanLife life, HumanNeeds needs, int hours)
    {
        if (life.CannotAct() || hours <= 0)
        {
            return;
        }

        Knowledge += hours;

        needs.DecreaseEnergy(hours / 2);

        needs.IncreaseStress(hours / 2);

        life.AddLifeEvent($"Estudou por {hours} horas.");
    }

    /// <summary>
    /// Aumenta conhecimento.
    /// </summary>
    public void IncreaseKnowledge(HumanLife life, int amount)
    {
        if (life.CannotAct() || amount <= 0)
        {
            return;
        }

        Knowledge += amount;
    }

    /// <summary>
    /// Conclui um nível educacional.
    /// </summary>
    public void Graduate(HumanLife life, HumanNeeds needs)
    {
        if (life.CannotAct())
        {
            return;
        }

        EducationLevel++;

        needs.IncreaseHappiness(10);

        IncreaseKnowledge(life, 10);

        life.AddLifeEvent("Concluiu uma formação.");
    }
    #endregion
}
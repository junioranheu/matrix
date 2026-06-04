using Matrix.Domain.Enums;
using Matrix.Shared.Extensions;
using Matrix.Shared.Helpers;
using System.Diagnostics.Metrics;

namespace Matrix.Domain.Entities;

public sealed class HumanLife(int age = 0)
{
    #region props
    /// <summary>
    /// Idade atual.
    /// </summary>
    public int Age { get; private set; } = age;

    /// <summary>
    /// Indica se está vivo.
    /// </summary>
    public bool IsAlive { get; private set; } = true;

    /// <summary>
    /// Data da morte.
    /// </summary>
    public DateOnly? DateOfDeath { get; private set; } = null;

    /// <summary>
    /// Ano da morte formatado com quatro dígitos.
    /// Exemplo: 0001, 0025, 1250.
    /// </summary>
    public string DateOfDeathString => DateOfDeath.GetValueOrDefault().Year.ToString("D4");

    /// <summary>
    /// Causa da morte.
    /// </summary>
    public CauseOfDeathEnum? CauseOfDeath { get; private set; } = null;

    /// <summary>
    /// Eventos importantes da vida.
    /// </summary>
    public List<string> LifeEvents { get; private set; } = [];
    #endregion

    #region computed props
    public bool IsDead => !IsAlive;

    public bool IsChild => Age < 12;

    public bool IsTeenager => Age >= 12 && Age < 18;

    public bool IsAdult => Age >= 18;

    public bool IsElderly => Age >= 60;
    #endregion

    #region methods
    /// <summary>
    /// Indica se o humano está impossibilitado de executar ações.
    /// </summary>
    public bool CannotAct()
    {
        return IsDead;
    }

    /// <summary>
    /// Incrementa a idade em um ano.
    /// </summary>
    public void AgeOneYear(HumanNeeds needs, HumanHealth health, DateOnly currentDate)
    {
        if (CannotAct())
        {
            return;
        }

        Age++;

        needs.IncreaseHunger(5);
        needs.IncreaseStress(2);
        needs.DecreaseEnergy(3);

        if (Age >= 65)
        {
            health.Damage(life: this, needs, amount: RandomHelpers.RandomBetween(0, 3), currentDate);
        }

        if (health.Diseases.Count > 0)
        {
            health.Damage(life: this, needs, amount: health.Diseases.Count, currentDate);
        }

        AddLifeEvent(description: $"Completou {Age} anos.", currentDate);
    }

    /// <summary>
    /// Mata o humano.
    /// </summary>
    public void Die(HumanNeeds needs, CauseOfDeathEnum cause, DateOnly dateOfDeath)
    {
        IsAlive = false;
        needs.DecreaseEnergy(999);
        DateOfDeath = dateOfDeath;
        CauseOfDeath = cause;

        AddLifeEvent(description: cause.GetDescription(), currentDate: dateOfDeath);
    }

    /// <summary>
    /// Adiciona um evento ao histórico de um humano.
    /// </summary>
    public void AddLifeEvent(string description, DateOnly currentDate)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return;
        }

        description = description.Trim().TrimEnd('.');
        description = $"{description}, no ano de {currentDate:yyyy}.";

        LifeEvents.Add(description);
    }
    #endregion
}
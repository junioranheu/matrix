using Matrix.Domain.Enums;

namespace Matrix.Domain.Entities;

public sealed class HumanFinance()
{
    #region props
    /// <summary>
    /// Dinheiro disponível.
    /// </summary>
    public decimal Money { get; private set; } = 0;

    /// <summary>
    /// Dívidas.
    /// </summary>
    public decimal Debt { get; private set; } = 0;

    /// <summary>
    /// Status financeiro.
    /// </summary>
    public FinancialStatusEnum FinancialStatus { get; private set; } = FinancialStatusEnum.Unknown;
    #endregion

    #region computed props
    public bool IsRich => FinancialStatus == FinancialStatusEnum.Rich || FinancialStatus == FinancialStatusEnum.UltraRich;

    public bool IsPoor => FinancialStatus == FinancialStatusEnum.ExtremePoverty || FinancialStatus == FinancialStatusEnum.Poor;
    #endregion

    #region methods
    /// <summary>
    /// Adiciona dinheiro ao patrimônio.
    /// </summary>
    public void EarnMoney(HumanLife life, decimal amount)
    {
        if (life.CannotAct() || amount <= 0)
        {
            return;
        }

        Money += amount;

        UpdateFinancialStatus(life);
    }

    /// <summary>
    /// Remove dinheiro do patrimônio.
    /// </summary>
    public void SpendMoney(HumanLife life, decimal amount)
    {
        if (life.CannotAct() || amount <= 0)
        {
            return;
        }

        Money -= amount;

        if (Money < 0)
        {
            Money = 0;
        }

        UpdateFinancialStatus(life);
    }

    /// <summary>
    /// Obtém um empréstimo.
    /// </summary>
    public void TakeLoan(HumanLife life, decimal amount)
    {
        if (life.CannotAct() || amount <= 0)
        {
            return;
        }

        Debt += amount;
        Money += amount;

        life.AddLifeEvent($"Pegou um empréstimo de {amount:C}.");
    }

    /// <summary>
    /// Paga uma dívida.
    /// </summary>
    public void PayDebt(HumanLife life, decimal amount)
    {
        if (life.CannotAct() || amount <= 0)
        {
            return;
        }

        if (!HasMoney(amount))
        {
            return;
        }

        SpendMoney(life, amount);

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
    public void Donate(HumanLife life, HumanNeeds needs, decimal amount)
    {
        if (life.CannotAct() || amount <= 0)
        {
            return;
        }

        if (!HasMoney(amount))
        {
            return;
        }

        SpendMoney(life, amount);

        needs.IncreaseHappiness(5);

        life.AddLifeEvent($"Doou {amount:C}.");
    }

    /// <summary>
    /// Recebe herança.
    /// </summary>
    public void ReceiveInheritance(HumanLife life, HumanNeeds needs, decimal amount)
    {
        if (life.CannotAct() || amount <= 0)
        {
            return;
        }

        EarnMoney(life, amount);

        needs.IncreaseHappiness(10);

        life.AddLifeEvent($"Recebeu uma herança de {amount:C}.");
    }

    /// <summary>
    /// Reavalia o status financeiro com base no patrimônio atual.
    /// </summary>
    public void UpdateFinancialStatus(HumanLife life)
    {
        if (life.CannotAct())
        {
            return;
        }

        FinancialStatus = Money switch
        {
            < 100 => FinancialStatusEnum.ExtremePoverty,
            < 1_000 => FinancialStatusEnum.Poor,
            < 10_000 => FinancialStatusEnum.LowerMiddleClass,
            < 100_000 => FinancialStatusEnum.MiddleClass,
            < 1_000_000 => FinancialStatusEnum.UpperMiddleClass,
            < 10_000_000 => FinancialStatusEnum.Rich,
            _ => FinancialStatusEnum.UltraRich
        };
    }
    #endregion
}
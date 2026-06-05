using Matrix.Domain.Enums;

namespace Matrix.Domain.Entities;

public sealed class HumanRelationships()
{
    #region props
    /// <summary>
    /// Parceiro atual.
    /// </summary>
    public Guid? PartnerId { get; private set; } = null;

    /// <summary>
    /// Histórico de amantes.
    /// </summary>
    public List<Guid> LoversIds { get; private set; } = [];

    /// <summary>
    /// Indica se o relacionamento foi formalizado em casamento.
    /// </summary>
    public bool IsMarried { get; private set; } = false;
    #endregion

    #region computed props
    public bool HasPartner => PartnerId.HasValue;
    #endregion

    #region methods
    /// <summary>
    /// Define um parceiro.
    /// </summary>
    public void SetPartner(HumanLife life, Guid partnerId)
    {
        if (life.CannotAct())
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
        IsMarried = false;
    }

    /// <summary>
    /// Casa-se com outro humano.
    /// </summary>
    public void Marry(HumanLife life, HumanNeeds needs, HumanEmotions emotions, Guid partnerId, DateOnly currentDate)
    {
        if (life.CannotAct())
        {
            return;
        }

        if (!life.IsAdult)
        {
            return;
        }

        SetPartner(life, partnerId);

        IsMarried = true;

        needs.IncreaseHappiness(15);

        emotions.ChangeMood(MoodEnum.Happy);

        life.AddLifeEvent(description: "Casou-se.", currentDate);
    }

    /// <summary>
    /// Realiza um divórcio.
    /// </summary>
    public void Divorce(HumanLife life, HumanNeeds needs, HumanEmotions emotions, DateOnly currentDate)
    {
        if (life.CannotAct())
        {
            return;
        }

        if (!HasPartner)
        {
            return;
        }

        RemovePartner();

        // Ensure married flag cleared
        IsMarried = false;

        needs.IncreaseStress(50);

        needs.DecreaseHappiness(20);

        emotions.ChangeMood(MoodEnum.Heartbroken);

        life.AddLifeEvent(description: "Divorciou-se.", currentDate);
    }

    /// <summary>
    /// Adiciona um amante.
    /// </summary>
    public void AddLover(HumanLife life, Guid loverId)
    {
        if (life.CannotAct())
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
}
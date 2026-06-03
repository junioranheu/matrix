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
    }

    /// <summary>
    /// Casa-se com outro humano.
    /// </summary>
    public void Marry(HumanLife life, HumanNeeds needs, HumanEmotions emotions, Guid partnerId)
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

        needs.IncreaseHappiness(15);

        emotions.ChangeMood(MoodEnum.Happy);

        life.AddLifeEvent("Casou-se.");
    }

    /// <summary>
    /// Realiza um divórcio.
    /// </summary>
    public void Divorce(HumanLife life, HumanNeeds needs, HumanEmotions emotions)
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

        needs.IncreaseStress(50);

        needs.DecreaseHappiness(20);

        emotions.ChangeMood(MoodEnum.Heartbroken);

        life.AddLifeEvent("Divorciou-se.");
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
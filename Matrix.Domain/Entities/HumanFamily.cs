namespace Matrix.Domain.Entities;

public sealed class HumanFamily(Guid? fatherId, Guid? motherId)
{
    #region props
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

    #region computed props
    public bool HasChildren => ChildrenIds.Count > 0;
    #endregion

    #region methods
    /// <summary>
    /// Adiciona um filho.
    /// </summary>
    public void AddChild(HumanLife life, HumanNeeds needs, Guid childId)
    {
        if (life.CannotAct())
        {
            return;
        }

        if (ChildrenIds.Contains(childId))
        {
            return;
        }

        ChildrenIds.Add(childId);

        needs.IncreaseHappiness(20);

        life.AddLifeEvent("Teve um filho.");
    }

    /// <summary>
    /// Remove um filho.
    /// </summary>
    public void RemoveChild(HumanLife life, Guid childId)
    {
        ChildrenIds.Remove(childId);

        life.AddLifeEvent("Perdeu um filho.");
    }
    #endregion
}
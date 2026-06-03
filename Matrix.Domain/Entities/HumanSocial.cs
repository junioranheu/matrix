namespace Matrix.Domain.Entities;

public sealed class HumanSocial()
{
    #region props
    /// <summary>
    /// Amigos.
    /// </summary>
    public List<Guid> FriendsIds { get; private set; } = [];

    /// <summary>
    /// Inimigos.
    /// </summary>
    public List<Guid> EnemiesIds { get; private set; } = [];
    #endregion

    #region methods
    /// <summary>
    /// Adiciona um amigo.
    /// </summary>
    public void AddFriend(HumanLife life, HumanNeeds needs, Guid friendId)
    {
        if (life.CannotAct())
        {
            return;
        }

        if (FriendsIds.Contains(friendId))
        {
            return;
        }

        FriendsIds.Add(friendId);

        needs.IncreaseHappiness(5);
    }

    /// <summary>
    /// Remove um amigo.
    /// </summary>
    public void RemoveFriend(HumanNeeds needs, Guid friendId)
    {
        FriendsIds.Remove(friendId);

        needs.DecreaseHappiness(20);
    }

    /// <summary>
    /// Adiciona um inimigo.
    /// </summary>
    public void AddEnemy(HumanLife life, HumanNeeds needs, Guid enemyId)
    {
        if (life.CannotAct())
        {
            return;
        }

        if (EnemiesIds.Contains(enemyId))
        {
            return;
        }

        EnemiesIds.Add(enemyId);

        needs.IncreaseStress(25);
    }

    /// <summary>
    /// Remove um inimigo.
    /// </summary>
    public void RemoveEnemy(Guid enemyId)
    {
        EnemiesIds.Remove(enemyId);
    }
    #endregion
}
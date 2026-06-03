namespace Matrix.Domain.Entities;

public sealed class World(string name)
{
    #region props
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; } = name;
    public int CurrentYear { get; private set; } = 1;
    public List<Human> Humans { get; } = [];
    #endregion

    #region methods
    public void AdvanceYear()
    {
        CurrentYear++;
    }

    public void AddHuman(Human human)
    {
        Humans.Add(human);
    }
    #endregion
}
namespace Matrix.Domain.Entities;

public sealed class World(string name, DateOnly initialYear)
{
    #region props
    /// <summary>
    /// Identificador único do planeta.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Nome do planeta.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Ano atual da simulação.
    /// </summary>
    public int CurrentYear { get; private set; } = 0;

    /// <summary>
    /// Data atual da simulação calculada com base no ano inicial e no ano corrente.
    /// </summary>
    public DateOnly CurrentDate => initialYear.AddYears(CurrentYear);

    /// <summary>
    /// Ano atual da simulação formatado com quatro dígitos.
    /// Exemplo: 0001, 0025, 1250.
    /// </summary>
    public string CurrentDateString => initialYear.AddYears(CurrentYear).Year.ToString("D4");

    /// <summary>
    /// Lista de humanos existentes no planeta.
    /// </summary>
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
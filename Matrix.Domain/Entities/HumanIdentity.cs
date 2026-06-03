using Matrix.Domain.Enums;

namespace Matrix.Domain.Entities;

public sealed class HumanIdentity(GenderEnum gender, string firstName, string lastName, DateOnly birthDate)
{
    #region props
    /// <summary>
    /// Identificador único do humano.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Identificador único do humano.
    /// </summary>
    public GenderEnum Gender { get; private set; } = gender;

    /// <summary>
    /// Primeiro nome.
    /// </summary>
    public string FirstName { get; private set; } = firstName;

    /// <summary>
    /// Sobrenome.
    /// </summary>
    public string LastName { get; private set; } = lastName;

    /// <summary>
    /// Nome completo.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Data de nascimento.
    /// </summary>
    public DateOnly BirthDate { get; private set; } = birthDate;
    #endregion
}
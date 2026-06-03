using Matrix.Domain.Enums;
using Matrix.Shared.Extensions;

namespace Matrix.Domain.Entities;

public sealed class HumanLocation(CountryEnum birthCountry)
{
    #region props
    /// <summary>
    /// País de nascimento.
    /// </summary>
    public CountryEnum BirthCountry { get; private set; } = birthCountry;

    /// <summary>
    /// País atual.
    /// </summary>
    public CountryEnum CurrentCountry { get; private set; } = birthCountry;

    /// <summary>
    /// Descrição do país de nascimento;
    /// </summary>
    public string BirthCountryDescription => BirthCountry.GetDescription();
    #endregion

    #region methods
    /// <summary>
    /// Muda o país de residência.
    /// </summary>
    public void MoveToCountry(HumanLife life, CountryEnum country)
    {
        if (life.CannotAct())
        {
            return;
        }

        CurrentCountry = country;

        life.AddLifeEvent($"Mudou-se para {country}.");
    }
    #endregion
}
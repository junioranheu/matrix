using Matrix.Domain.Enums;
using Matrix.Domain.Mappers;
using static Matrix.Domain.Seeds.HumanNamesSeed;

namespace Matrix.Domain.Helpers;

/// <summary>
/// Utilitários para cálculos, probabilidades, genética e geração de valores da simulação.
/// </summary>
public static class SimulationHelper
{
    private static readonly Random Random = new();

    /// <summary>
    /// Gera um nome e sobrenome aleatórios compatíveis com o país e gênero informados.
    /// </summary>
    /// <param name="country">
    /// País utilizado para determinar a categoria de nomes e sobrenomes disponível.
    /// </param>
    /// <param name="gender">
    /// Gênero utilizado para selecionar o conjunto de primeiros nomes.
    /// </param>
    /// <returns>
    /// Uma tupla contendo o nome e sobrenome gerados aleatoriamente.
    /// </returns>
    public static (string firstName, string lastName) GenerateRandomName(CountryEnum country, GenderEnum gender)
    {
        HumanNameNationality nationality = HumanNameNationalityMapper.MapFrom(country);
         
        HumanNamePool pool = HumanNames.Pools[nationality];

        string[] names = gender switch
        {
            GenderEnum.Male => pool.MaleNames,
            GenderEnum.Female => pool.FemaleNames,
            _ => throw new ArgumentOutOfRangeException(nameof(gender))
        };

        string firstName = names[Random.Shared.Next(names.Length)];

        string lastName = pool.LastNames[Random.Shared.Next(pool.LastNames.Length)];

        return (firstName, lastName);
    }

    /// <summary>
    /// Gera um atributo herdado dos pais aplicando uma mutação aleatória.
    /// </summary>
    /// <param name="fatherValue">
    /// Valor do atributo do pai.
    /// </param>
    /// <param name="motherValue">
    /// Valor do atributo da mãe.
    /// </param>
    /// <param name="mutationRange">
    /// Variação máxima positiva ou negativa aplicada ao valor herdado.
    /// </param>
    /// <returns>
    /// Valor final herdado limitado entre 1 e 100.
    /// </returns>
    public static int InheritFromParentsValues(int fatherValue, int motherValue, int mutationRange = 10)
    {
        int average = (fatherValue + motherValue) / 2;
        int mutation = Random.Next(-mutationRange, mutationRange + 1);
        int output = Math.Clamp(average + mutation, 1, 100);

        return output;
    }
}
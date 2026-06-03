using Matrix.Domain.Entities;
using Matrix.Domain.Enums;
using Matrix.Domain.Helpers;
using Matrix.Shared.Extensions;
using Matrix.Shared.Helpers;

namespace Matrix.Domain.Factories;

/// <summary>
/// Responsável pela criação e inicialização do mundo da simulação,
/// incluindo a geração do planeta e dos primeiros habitantes.
/// </summary>
public sealed class WorldFactory()
{
    private const int MIN_AGE = 18;
    private const int MAX_AGE = 30;

    public static World Create(DateOnly initialYear)
    {
        // Cria o mundo;
        World world = new(name: SimulationHelper.GeneratePlanetName(), initialYear);

        // Obtém aleatoriamente um país de origem;
        CountryEnum startingCountry = EnumExtensions.GetRandom<CountryEnum>();

        // Cria o primeiro homem do mundo;
        (int manAge, string manLastName) = CreateFirstManOfTheWorld(world, startingCountry, currentYear: initialYear);

        // Cria a primeira mulher do mundo;
        CreateFirstWomanOfTheWorld(world, startingCountry, currentYear: initialYear, manAge, manLastName);

        return world;
    }

    #region methods
    /// <summary>
    /// Cria o primeiro homem do mundo utilizando uma nacionalidade aleatória,
    /// define sua idade inicial e o adiciona à população da simulação.
    /// </summary>
    /// <param name="world">
    /// Mundo que receberá o novo habitante.
    /// </param>
    /// <param name="startingCountry">
    /// País utilizado para geração do nome e definição da nacionalidade inicial.
    /// </param>
    /// <param name="currentYear">
    /// Data atual da simulação utilizada para calcular a data de nascimento.
    /// </param>
    /// <returns>
    /// Uma tupla contendo a idade e o sobrenome do homem criado.
    /// Essas informações são utilizadas para a criação da primeira mulher do mundo.
    /// </returns>
    private static (int manAge, string manLastName) CreateFirstManOfTheWorld(World world, CountryEnum startingCountry, DateOnly currentYear)
    {
        (string manFirstName, string manLastName) = SimulationHelper.GenerateRandomName(
           country: startingCountry,
           gender: GenderEnum.Male);

        int manAge = RandomHelpers.RandomBetween(min: MIN_AGE, max: MAX_AGE);

        Human man = HumanFactory.CreateInitialHuman(
            firstName: manFirstName,
            lastName: manLastName,
            country: startingCountry,
            age: manAge,
            currentYear);

        world.AddHuman(man);

        return (manAge, manLastName);
    }

    /// <summary>
    /// Cria a primeira mulher do mundo utilizando o mesmo sobrenome do primeiro homem,
    /// garantindo que sua idade não seja superior à dele, e a adiciona à população da simulação.
    /// </summary>
    /// <param name="world">
    /// Mundo que receberá a nova habitante.
    /// </param>
    /// <param name="startingCountry">
    /// País utilizado para geração do nome e definição da nacionalidade inicial.
    /// </param>
    /// <param name="currentYear">
    /// Data atual da simulação utilizada para calcular a data de nascimento.
    /// </param>
    /// <param name="manAge">
    /// Idade do primeiro homem do mundo, utilizada como limite máximo para a idade da mulher.
    /// </param>
    /// <param name="manLastName">
    /// Sobrenome do primeiro homem do mundo, utilizado como sobrenome da mulher.
    /// </param>
    private static void CreateFirstWomanOfTheWorld(World world, CountryEnum startingCountry, DateOnly currentYear, int manAge, string manLastName)
    {
        (string womanFirstName, string _) = SimulationHelper.GenerateRandomName(
            country: startingCountry,
            gender: GenderEnum.Female);

        int womanAge = RandomHelpers.RandomBetween(min: MIN_AGE, max: MAX_AGE);

        if (womanAge > manAge)
        {
            womanAge = manAge;
        }

        Human woman = HumanFactory.CreateInitialHuman(
            firstName: womanFirstName,
            lastName: manLastName,
            country: startingCountry,
            age: womanAge,
            currentYear);

        world.AddHuman(woman);
    }
    #endregion
}
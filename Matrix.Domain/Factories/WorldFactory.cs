using Matrix.Domain.Entities;
using Matrix.Domain.Enums;
using Matrix.Domain.Helpers;
using Matrix.Shared.Extensions;
using Matrix.Shared.Helpers;
using System;

namespace Matrix.Domain.Factories;

/// <summary>
/// Responsável pela criação e inicialização do mundo da simulação,
/// incluindo a geração do planeta e dos primeiros habitantes.
/// </summary>
public sealed class WorldFactory()
{
    private const int MIN_AGE = 18;
    private const int MAX_AGE = 30;

    public static World Create(InitialSettings settings, DateOnly initialYear)
    {
        // Cria o mundo;
        World world = new(name: SimulationHelper.GeneratePlanetName(), initialYear);

        for (int i = 0; i < settings.StartingCouples; i++)
        {
            // Obtém aleatoriamente um país de origem;
            CountryEnum startingCountry = EnumExtensions.GetRandom<CountryEnum>();

            Human man = CreateMan(world, gender: GenderEnum.Male, startingCountry, currentDate: initialYear);

            Human woman = CreateWoman(world, gender: GenderEnum.Female, startingCountry, currentDate: initialYear, man);

            Marry(man, woman);
        }

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
    /// <param name="gender">
    /// Gênero da lenda máxima.
    /// </param>
    /// <param name="startingCountry">
    /// País utilizado para geração do nome e definição da nacionalidade inicial.
    /// </param>
    /// <param name="currentDate">
    /// Data atual da simulação utilizada para calcular a data de nascimento.
    /// </param>
    private static Human CreateMan(World world, GenderEnum gender, CountryEnum startingCountry, DateOnly currentDate)
    {
        (string manFirstName, string manLastName) = SimulationHelper.GenerateRandomName(country: startingCountry, gender);

        int manAge = RandomHelpers.RandomBetween(min: MIN_AGE, max: MAX_AGE);

        Human man = HumanFactory.CreateInitialHuman(
            gender,
            firstName: manFirstName,
            lastName: manLastName,
            country: startingCountry,
            age: manAge,
            currentDate);

        world.AddHuman(human: man, currentDate, country: startingCountry, isInitialSpawn: true, age: manAge);

        return man;
    }

    /// <summary>
    /// Cria a primeira mulher do mundo utilizando o mesmo sobrenome do primeiro homem,
    /// garantindo que sua idade não seja superior à dele, e a adiciona à população da simulação.
    /// </summary>
    /// <param name="world">
    /// Mundo que receberá a nova habitante.
    /// </param>
    /// <param name="gender">
    /// Gênero da lenda máxima.
    /// </param>
    /// <param name="startingCountry">
    /// País utilizado para geração do nome e definição da nacionalidade inicial.
    /// </param>
    /// <param name="currentDate">
    /// Data atual da simulação utilizada para calcular a data de nascimento.
    /// </param>
    /// <param name="man">
    /// Homem, que deverá ajudar a construir a mulher com sua costela -- não, pera. Apenas o sobrenome e a idade mesmo.
    /// </param>
    private static Human CreateWoman(World world, GenderEnum gender, CountryEnum startingCountry, DateOnly currentDate, Human man)
    {
        (string womanFirstName, string _) = SimulationHelper.GenerateRandomName(country: startingCountry, gender);

        Human woman = HumanFactory.CreateInitialHuman(
            gender,
            firstName: womanFirstName,
            lastName: man.Identity.LastName,
            country: startingCountry,
            age: man.Life.Age,
            currentDate);

        world.AddHuman(human: woman, currentDate, country: startingCountry, isInitialSpawn: true, age: man.Life.Age);

        return woman;
    }

    /// <summary>
    /// Realiza o casamento entre dois humanos, registrando o vínculo de parceria
    /// em ambos os indivíduos.
    /// </summary>
    /// <param name="man">
    /// Homem que participará do casamento.
    /// </param>
    /// <param name="woman">
    /// Mulher que participará do casamento.
    /// </param>
    private static void Marry(Human man, Human woman)
    {
        man.Relationships.SetPartner(life: man.Life, partnerId: woman.Id);
        woman.Relationships.SetPartner(life: woman.Life, partnerId: man.Id);
    }
    #endregion
}
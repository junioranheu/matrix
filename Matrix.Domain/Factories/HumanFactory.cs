using Matrix.Domain.Entities;
using Matrix.Domain.Enums;
using Matrix.Domain.Helpers;

namespace Matrix.Domain.Factories;

/// <summary>
/// Responsável pela criação de humanos da simulação,
/// incluindo humanos iniciais e filhos gerados por reprodução.
/// </summary>
public sealed class HumanFactory
{
    /// <summary>
    /// Cria um humano inicial sem pai ou mãe definidos.
    /// </summary>
    public static Human CreateInitialHuman(string firstName, string lastName, CountryEnum country)
    {
        return new Human(
            firstName,
            lastName,
            country,
            health: SimulationHelper.RandomValue(RandomRange.Medium),
            intelligence: SimulationHelper.RandomValue(RandomRange.Medium),
            charisma: SimulationHelper.RandomValue(RandomRange.Medium),
            strength: SimulationHelper.RandomValue(RandomRange.Medium),
            hunger: SimulationHelper.RandomBetween(0, 25),
            happiness: SimulationHelper.RandomBetween(75, 100),
            stress: SimulationHelper.RandomBetween(0, 25),
            fatherId: null,
            motherId: null);
    }

    /// <summary>
    /// Cria um filho herdando características dos pais.
    /// </summary>
    public static Human CreateChild(Human father, Human mother, string firstName)
    {
        return new Human(
            firstName,
            lastName: father.LastName,
            country: mother.CountryCurrent,
            health: SimulationHelper.InheritFromParentsValues(father.Health, mother.Health),
            intelligence: SimulationHelper.InheritFromParentsValues(father.Intelligence, mother.Intelligence),
            charisma: SimulationHelper.InheritFromParentsValues(father.Charisma, mother.Charisma),
            strength: SimulationHelper.InheritFromParentsValues(father.Strength, mother.Strength),
            hunger: SimulationHelper.RandomBetween(0, 50),
            happiness: SimulationHelper.RandomBetween(50, 100),
            stress: SimulationHelper.RandomBetween(0, 75),
            fatherId: father.Id,
            motherId: mother.Id);
    }
}
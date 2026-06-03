using Matrix.Domain.Entities;
using Matrix.Domain.Enums;
using Matrix.Domain.Helpers;
using Matrix.Shared.Enums;
using Matrix.Shared.Helpers;

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
    public static Human CreateInitialHuman(GenderEnum gender, string firstName, string lastName, CountryEnum country, int age, DateOnly currentDate)
    {
        return new Human(
            life: new HumanLife(
                age),

            identity: new HumanIdentity(
                gender,
                firstName,
                lastName,
                birthDate: currentDate),

            family: new HumanFamily(
                fatherId: null,
                motherId: null),

            location: new HumanLocation(
                birthCountry: country),

            health: new HumanHealth(
                health: RandomHelpers.RandomValue(RandomRange.Medium),
                immunity: RandomHelpers.RandomValue(RandomRange.Medium),
                fertility: RandomHelpers.RandomValue(RandomRange.Medium),
                diseases: []),

            needs: new HumanNeeds(
                hunger: RandomHelpers.RandomBetween(0, 25),
                happiness: RandomHelpers.RandomBetween(75, 100),
                stress: RandomHelpers.RandomBetween(0, 25),
                energy: 100));
    }

    /// <summary>
    /// Cria um filho herdando características dos pais.
    /// </summary>
    public static Human CreateChild(GenderEnum gender, Human father, Human mother, string firstName, DateOnly currentDate)
    {
        return new Human(
            life: new HumanLife(
               age: 0),

            identity: new HumanIdentity(
                gender,
                firstName,
                father.Identity.LastName,
                birthDate: currentDate),

            family: new HumanFamily(
                fatherId: father.Id,
                motherId: mother.Id),

            location: new HumanLocation(
                mother.Location.CurrentCountry),

            health: new HumanHealth(
                health: SimulationHelper.InheritFromParentsValues(
                    father.Health.Health,
                    mother.Health.Health),

                immunity: SimulationHelper.InheritFromParentsValues(
                    father.Health.Immunity,
                    mother.Health.Immunity),

                fertility: SimulationHelper.InheritFromParentsValues(
                    father.Health.Fertility,
                    mother.Health.Fertility),

                diseases: []),

            needs: new HumanNeeds(
                hunger: RandomHelpers.RandomBetween(0, 50),
                happiness: RandomHelpers.RandomBetween(50, 100),
                stress: RandomHelpers.RandomBetween(0, 75),
                energy: 100));
    }
}
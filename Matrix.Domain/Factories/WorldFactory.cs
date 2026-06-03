using Matrix.Domain.Entities;
using Matrix.Domain.Enums;
using Matrix.Domain.Helpers;
using Matrix.Shared.Extensions;

namespace Matrix.Domain.Factories;

public sealed class WorldFactory()
{
    public static World Create(DateOnly currentYear)
    {
        World world = new(name: "XDDD");

        CountryEnum startingCountry = EnumExtensions.GetRandom<CountryEnum>();

        (string manFirstName, string manLastName) = SimulationHelper.GenerateRandomName(country: startingCountry, gender: GenderEnum.Male);
        Human man = HumanFactory.CreateInitialHuman(firstName: manFirstName, lastName: manLastName, country: startingCountry, currentYear);
        world.AddHuman(man);

        (string womanFirstName, string _) = SimulationHelper.GenerateRandomName(country: startingCountry, gender: GenderEnum.Female);
        Human woman = HumanFactory.CreateInitialHuman(firstName: womanFirstName, lastName: manLastName, country: startingCountry, currentYear);
        world.AddHuman(woman);

        return world;
    }
}
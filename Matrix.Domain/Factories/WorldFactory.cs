using Matrix.Domain.Entities;
using Matrix.Domain.Enums;
using Matrix.Domain.Helpers;
using Matrix.Shared.Extensions;

namespace Matrix.Domain.Factories;

public sealed class WorldFactory()
{
    public static World Create()
    {
        World world = new(name: "Matrix aaaaaaaaaaaaaaaaa");

        CountryEnum startingCountry = EnumExtensions.GetRandom<CountryEnum>();

        (string manFirstName, string manLastName) = SimulationHelper.GenerateRandomName(country: startingCountry, gender: GenderEnum.Male);
        Human man = HumanFactory.CreateInitialHuman(firstName: manFirstName, lastName: manLastName, country: startingCountry);
        world.AddHuman(man);

        (string womanFirstName, string _) = SimulationHelper.GenerateRandomName(country: startingCountry, gender: GenderEnum.Female);
        Human woman = HumanFactory.CreateInitialHuman(firstName: womanFirstName, lastName: manLastName, country: startingCountry);
        world.AddHuman(woman);

        return world;
    }
}
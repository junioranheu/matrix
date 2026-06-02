using Matrix.Domain.Entities;
using Matrix.Domain.Enums;

namespace Matrix.Domain.Factories;

public sealed class WorldFactory(HumanFactory humanFactory)
{
    private readonly HumanFactory _humanFactory = humanFactory;

    public World Create()
    {
        World world = new(name: "Matrix");

        Human man = HumanFactory.CreateInitialHuman(firstName: "Adão", lastName: "de Souza", country: CountryEnum.Brazil);
        world.AddHuman(man);

        Human woman = HumanFactory.CreateInitialHuman(firstName: "Eva", lastName: "da Silva", country: CountryEnum.Brazil);
        world.AddHuman(woman);

        return world;
    }
}
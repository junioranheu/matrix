using Matrix.Domain.Entities;
using Matrix.Domain.Enums;

namespace Matrix.UnitTests.Tests.Entities;

public sealed class HumanTests
{
    [Fact]
    public void Constructor_ShouldInitializeAllComponents()
    {
        // Arrange;
        var life = new HumanLife(age: 30);
        var identity = new HumanIdentity(GenderEnum.Male, "Junior", "Souza", new DateOnly(1997, 03, 25));
        var family = new HumanFamily(fatherId: null, motherId: null);
        var location = new HumanLocation(CountryEnum.Brazil);
        var health = new HumanHealth(health: 100, immunity: 50, fertility: 50, diseases: []);
        var needs = new HumanNeeds(hunger: 10, happiness: 50, stress: 10, energy: 80);

        // Act;
        var human = new Human(life, identity, family, location, health, needs);

        // Assert;
        Assert.NotEqual(Guid.Empty, human.Id);
        Assert.Equal(30, human.Life.Age);
        Assert.Equal("Junior Souza", human.Identity.FullName);
        Assert.Empty(human.Family.ChildrenIds);
        Assert.Equal(CountryEnum.Brazil, human.Location.BirthCountry);
        Assert.Equal(100, human.Health.Health);
        Assert.Equal(10, human.Needs.Hunger);

        Assert.NotNull(human.Relationships);
        Assert.NotNull(human.Social);
        Assert.NotNull(human.Finance);
        Assert.NotNull(human.Career);
        Assert.NotNull(human.Education);
        Assert.NotNull(human.Emotions);
    }

    [Fact]
    public void Life_AgeOneYear_ShouldIncrementAgeAndUpdateNeedsAndAddLifeEvent()
    {
        // Arrange;
        var life = new HumanLife(age: 20);
        var identity = new HumanIdentity(GenderEnum.Male, "Junior", "Souza", new DateOnly(1997, 03, 25));
        var family = new HumanFamily(null, null);
        var location = new HumanLocation(CountryEnum.Brazil);
        var health = new HumanHealth(health: 100, immunity: 50, fertility: 50, diseases: []);
        var needs = new HumanNeeds(hunger: 10, happiness: 50, stress: 5, energy: 50);

        var human = new Human(life, identity, family, location, health, needs);
        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act;
        human.Life.AgeOneYear(human.Needs, human.Health, currentDate);

        // Assert;
        Assert.Equal(21, human.Life.Age);
        Assert.Equal(15, human.Needs.Hunger);    // +5;
        Assert.Equal(7, human.Needs.Stress);     // +2;
        Assert.Equal(47, human.Needs.Energy);    // -3;
        Assert.NotEmpty(human.Life.LifeEvents);
        Assert.Contains($"Completou {human.Life.Age} anos", human.Life.LifeEvents[0]);
    }

    [Fact]
    public void Life_Die_ShouldMarkAsDeadSetDateAndCauseAndReduceEnergy()
    {
        // Arrange;
        var life = new HumanLife(age: 40);
        var identity = new HumanIdentity(GenderEnum.Female, "Mariana", "Scalzaretto", new DateOnly(1997, 12, 19));
        var family = new HumanFamily(null, null);
        var location = new HumanLocation(CountryEnum.Brazil);
        var health = new HumanHealth(health: 10, immunity: 10, fertility: 10, diseases: []);
        var needs = new HumanNeeds(hunger: 10, happiness: 50, stress: 10, energy: 50);

        var human = new Human(life, identity, family, location, health, needs);
        var deathDate = new DateOnly(1997, 12, 19);

        // Act;
        human.Life.Die(human.Needs, CauseOfDeathEnum.Accident, deathDate);

        // Assert;
        Assert.False(human.Life.IsAlive);
        Assert.True(human.Life.IsDead);
        Assert.Equal(deathDate, human.Life.DateOfDeath);
        Assert.Equal(CauseOfDeathEnum.Accident, human.Life.CauseOfDeath);
        Assert.Equal(0, human.Needs.Energy); 
    }
}
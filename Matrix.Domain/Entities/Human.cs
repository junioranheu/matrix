namespace Matrix.Domain.Entities;

public sealed class Human(
    HumanLife life,
    HumanIdentity identity,
    HumanFamily family,
    HumanLocation location,
    HumanHealth health,
    HumanNeeds needs)
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public HumanLife Life { get; private set; } = new(
        age: life.Age);

    public HumanIdentity Identity { get; private set; } = new(
        gender: identity.Gender,
        firstName: identity.FirstName,
        lastName: identity.LastName,
        birthDate: identity.BirthDate);

    public HumanFamily Family { get; private set; } = new(
        fatherId: family.FatherId,
        motherId: family.MotherId);

    public HumanLocation Location { get; private set; } = new(
        birthCountry: location.BirthCountry);

    public HumanRelationships Relationships { get; private set; } = new();

    public HumanSocial Social { get; private set; } = new();

    public HumanHealth Health { get; private set; } = new(
        health: health.Health,
        immunity: health.Immunity,
        fertility: health.Fertility,
        diseases: health.Diseases);

    public HumanNeeds Needs { get; private set; } = new(
        hunger: needs.Hunger,
        happiness: needs.Happiness,
        stress: needs.Stress,
        energy: needs.Energy);

    public HumanFinance Finance { get; private set; } = new();

    public HumanCareer Career { get; private set; } = new();

    public HumanEducation Education { get; private set; } = new();

    public HumanEmotions Emotions { get; private set; } = new();
}
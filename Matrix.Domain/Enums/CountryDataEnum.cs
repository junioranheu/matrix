namespace Matrix.Domain.Enums;

public sealed class CountryDataEnum
{
    public CountryEnum Country { get; init; }

    public int AverageHealth { get; init; }

    public int AverageEducation { get; init; }

    public decimal AverageIncome { get; init; }
}
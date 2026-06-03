using Matrix.Shared.Enums;

namespace Matrix.Shared.Helpers;

public sealed class RandomHelpers
{
    private static readonly Random Random = new();

    /// <summary>
    /// Gera um valor aleatório baseado em uma faixa pré-definida.
    /// </summary>
    /// <param name="range">
    /// Faixa desejada para geração do valor.
    /// </param>
    /// <returns>
    /// Um número aleatório correspondente à faixa informada.
    /// </returns>
    public static int RandomValue(RandomRange range)
    {
        return range switch
        {
            RandomRange.VeryLow => Random.Next(0, 20),
            RandomRange.Low => Random.Next(20, 40),
            RandomRange.Medium => Random.Next(40, 70),
            RandomRange.High => Random.Next(70, 90),
            RandomRange.VeryHigh => Random.Next(90, 101),
            RandomRange.Any => Random.Next(0, 101),
            _ => 50
        };
    }

    /// <summary>
    /// Gera um valor aleatório entre os limites informados.
    /// </summary>
    /// <param name="min">
    /// Valor mínimo (inclusive).
    /// </param>
    /// <param name="max">
    /// Valor máximo (inclusive).
    /// </param>
    /// <returns>
    /// Um número aleatório entre min e max.
    /// </returns>
    public static int RandomBetween(int min, int max)
    {
        return Random.Next(min, max + 1);
    }

    /// <summary>
    /// Determina se um evento ocorreu com base em uma probabilidade.
    /// </summary>
    /// <param name="percentage">
    /// Probabilidade de sucesso em percentual.
    /// Exemplo: 25 = 25%.
    /// </param>
    /// <returns>
    /// True se o evento ocorrer; caso contrário, false.
    /// </returns>
    public static bool Chance(double percentage)
    {
        bool output = Random.NextDouble() * 100 <= percentage;

        return output;
    }
}
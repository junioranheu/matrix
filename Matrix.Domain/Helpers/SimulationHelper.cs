using Matrix.Domain.Enums;

namespace Matrix.Domain.Helpers;

/// <summary>
/// Utilitários para cálculos, probabilidades, genética e geração de valores da simulação.
/// </summary>
public static class SimulationHelper
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

    /// <summary>
    /// Gera um atributo herdado dos pais aplicando uma mutação aleatória.
    /// </summary>
    /// <param name="fatherValue">
    /// Valor do atributo do pai.
    /// </param>
    /// <param name="motherValue">
    /// Valor do atributo da mãe.
    /// </param>
    /// <param name="mutationRange">
    /// Variação máxima positiva ou negativa aplicada ao valor herdado.
    /// </param>
    /// <returns>
    /// Valor final herdado limitado entre 1 e 100.
    /// </returns>
    public static int InheritFromParentsValues(int fatherValue, int motherValue, int mutationRange = 10)
    {
        int average = (fatherValue + motherValue) / 2;
        int mutation = Random.Next(-mutationRange, mutationRange + 1);
        int output = Math.Clamp(average + mutation, 1, 100);

        return output;
    }
}
namespace Matrix.Domain.Helpers;

/// <summary>
/// Utilitários para cálculos, probabilidades, genética e geração de valores da simulação.
/// </summary>
public static class SimulationHelper
{
    private static readonly Random Random = new();

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
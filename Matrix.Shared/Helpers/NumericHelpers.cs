namespace Matrix.Shared.Helpers;

public static class NumericHelpers
{
    /// <summary>
    /// Restringe o valor atual para que permaneça dentro do intervalo especificado.
    /// </summary>
    /// <param name="value">Valor a ser limitado.</param>
    /// <param name="min">Valor mínimo permitido.</param>
    /// <param name="max">Valor máximo permitido.</param>
    /// <returns>
    /// O valor atual se estiver dentro do intervalo; caso contrário,
    /// <paramref name="min"/> ou <paramref name="max"/>.
    /// </returns>
    public static int Clamp(int value, int min = 0, int max = 100)
    {
        return Math.Clamp(value, min, max);
    }

    /// <summary>
    /// Restringe o valor atual para que permaneça dentro do intervalo especificado.
    /// </summary>
    /// <param name="value">Valor a ser limitado.</param>
    /// <param name="min">Valor mínimo permitido.</param>
    /// <param name="max">Valor máximo permitido.</param>
    /// <returns>
    /// O valor atual se estiver dentro do intervalo; caso contrário,
    /// <paramref name="min"/> ou <paramref name="max"/>.
    /// </returns>
    public static decimal Clamp(decimal value, decimal min = 0, decimal max = 0)
    {
        return Math.Clamp(value, min, max);
    }
}
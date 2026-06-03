using System.ComponentModel;
using System.Reflection;

namespace Matrix.Shared.Extensions;

public static class EnumExtensions
{
    /// <summary>
    /// Obtém a descrição do valor da enumeração.
    /// Retorna o nome da enumeração caso nenhuma descrição esteja definida.
    /// </summary>
    /// <param name="value">
    /// Valor da enumeração.
    /// </param>
    /// <returns>
    /// Descrição do valor da enumeração.
    /// </returns>
    public static string GetDescription(this Enum value)
    {
        FieldInfo? field = value.GetType().GetField(value.ToString());

        if (field is null)
        {
            return value.ToString();
        }

        DescriptionAttribute? attribute = field.GetCustomAttribute<DescriptionAttribute>();

        return attribute?.Description ?? value.ToString();
    }

    /// <summary>
    /// Obtém um valor aleatório da enumeração informada.
    /// </summary>
    /// <typeparam name="TEnum">
    /// Tipo da enumeração.
    /// </typeparam>
    /// <returns>
    /// Um valor aleatório pertencente à enumeração.
    /// </returns>
    public static TEnum GetRandom<TEnum>() where TEnum : struct, Enum
    {
        TEnum[] values = Enum.GetValues<TEnum>();

        return values[Random.Shared.Next(values.Length)];
    }
}
using System.Text;

namespace Matrix.Shared.Helpers;

public static class ConsoleConfigurationHelpers
{
    /// <summary>
    /// Configura o ambiente do console.
    /// </summary>
    public static void Configure()
    {
        SetTitle();
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;
    }

    /// <summary>
    /// Define o título da janela do console.
    /// </summary>
    /// <param name="suffix">
    /// Texto opcional adicionado ao final do título.
    /// </param>
    public static void SetTitle(string? suffix = null)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        Console.Title = string.IsNullOrWhiteSpace(suffix) ? "Bem-vindo à Matrix." : $"Bem-vindo à Matrix. {suffix}";
    }
}
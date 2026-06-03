using System.Text;

namespace Matrix.Shared.Helpers;

public static class ConsoleConfigurationHelpers
{
    /// <summary>
    /// Configura o ambiente do console.
    /// </summary>
    public static void Configure()
    {
        SetTitle(title: "Olá, mundo");
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;
    }

    /// <summary>
    /// Define o título da janela do console.
    /// </summary>
    /// </param>
    public static void SetTitle(string title)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        Console.Title = title;
    }

    /// <summary>
    /// Define o título da janela do console (2 parâmetros).
    /// </summary>
    /// </param>
    public static void SetTitle(string worldName, int currentyYear)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        Console.Title = $"Mundo {worldName} · Ano {currentyYear}";
    }
}
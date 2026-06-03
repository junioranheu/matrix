using Matrix.Domain.Entities;
using Matrix.Shared.Helpers;
using Spectre.Console;

namespace Matrix.Domain.Factories;

public sealed class SimulationFactory
{
    public static void StartSimulation(InitialSettings settings, World world)
    {
        for (int i = 1; i <= settings.SimulationYears; i++)
        {
            ConsoleConfigurationHelpers.SetTitle(worldName: world.Name, currentyYear: world.CurrentYear);

            if (settings.ShowEvents)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.Write(new Rule($"[cyan]Ano {world.CurrentDateString}[/]").RuleStyle("grey"));
                AnsiConsole.WriteLine();

                AnsiConsole.WriteLine($"BLA BLA BLA {world.CurrentYear}");
            }

            world.AdvanceYear();
        }
    }
}
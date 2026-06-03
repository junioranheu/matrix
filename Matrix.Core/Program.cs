using Matrix.Domain.Entities;
using Matrix.Domain.Factories;
using Matrix.Infrastructure.Reports;
using Matrix.Shared.Helpers;
using Spectre.Console;

// Configuração do prompt;
ConsoleConfigurationHelpers.Configure();

// Wizard inicial para configuração da simulação;
InitialSettings settings = new();
await settings.ConfigureAsync();

// Criação do mundo e dos primeiros humanos;
World world = WorldFactory.Create(initialYear: DateOnly.MinValue);

// Simulação;
// TO DO;
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

// Output final;
WorldConsoleReport.Print(world);

Console.ReadKey();
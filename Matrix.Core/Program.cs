using Matrix.Domain.Entities;
using Matrix.Domain.Factories;
using Matrix.Infrastructure.Reports;
using Matrix.Shared.Helpers;
using System.Diagnostics;

bool isDebug = Debugger.IsAttached;

// Configuração do prompt;
ConsoleConfigurationHelpers.Configure();

// Wizard inicial para configuração da simulação;
InitialSettings settings = new();
await settings.ConfigureAsync(isDebug);

// Criação do mundo e dos primeiros humanos;
World world = WorldFactory.Create(settings, initialYear: DateOnly.MinValue);

// Simulação;
await SimulationFactory.Run(
    settings,
    world,
    isFinalReport: false,
    yearReport: WorldYearConsoleReport.Print
);

// Output final;
WorldConsoleReport.Print(settings, world);

if (!isDebug)
{
    WorldHtmlReport.Export(world);
}

Console.ReadKey();
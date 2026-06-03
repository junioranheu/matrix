using Matrix.Domain.Entities;
using Matrix.Domain.Factories;
using Matrix.Infrastructure.Reports;
using Matrix.Shared.Helpers;

// Configuração do prompt;
ConsoleConfigurationHelpers.Configure();

// Wizard inicial para configuração da simulação;
InitialSettings settings = new();
await settings.ConfigureAsync();

// Criação do mundo e dos primeiros humanos;
World world = WorldFactory.Create(settings, initialYear: DateOnly.MinValue);

// Simulação;
SimulationFactory.Run(
    settings,
    world,
    isFinalReport: false,
    yearReport: WorldYearConsoleReport.Print
);

// Output final;
WorldConsoleReport.Print(settings, world);

Console.ReadKey();
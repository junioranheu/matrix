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
World world = WorldFactory.Create(initialYear: DateOnly.MinValue);

// Simulação;
for (int year = 1; year <= settings.SimulationYears; year++)
{
    SimulationFactory.Run(world);

    WorldYearConsoleReport.Print(settings, world);
}

// Output final;
WorldConsoleReport.Print(world);

Console.ReadKey();
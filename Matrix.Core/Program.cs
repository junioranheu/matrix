using Matrix.Domain.Entities;
using Matrix.Domain.Factories;
using Matrix.Infrastructure.Reports;
using Matrix.Shared.Helpers;

ConsoleConfigurationHelpers.Configure();

World world = WorldFactory.Create();

ConsoleConfigurationHelpers.SetTitle(suffix: $"Mundo {world.Name}");

WorldConsoleReport.Print(world);

Console.ReadKey();
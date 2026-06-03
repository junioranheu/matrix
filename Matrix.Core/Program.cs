using Matrix.Domain.Entities;
using Matrix.Domain.Factories;
using Matrix.Infrastructure.Reports;
using Matrix.Shared.Helpers;

ConsoleConfigurationHelpers.Configure();

World world = WorldFactory.Create();

ConsoleConfigurationHelpers.SetTitle(suffix: $"Mundo {world.Name}");

for (int i = 0; i < 10; i++)
{
    ConsoleConfigurationHelpers.SetTitle(suffix: $"Mundo {world.Name} | Ano {world.CurrentYear}");
}

WorldConsoleReport.Print(world);

Console.ReadKey();
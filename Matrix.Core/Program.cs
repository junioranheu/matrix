using Matrix.Domain.Entities;
using Matrix.Domain.Factories;

Console.Title = "Bem-vindo à Matrix.";

World _ = WorldFactory.Create();

Console.ReadKey();
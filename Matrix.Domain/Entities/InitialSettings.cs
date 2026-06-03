using Matrix.Domain.Consts;
using Matrix.Domain.Enums;
using Matrix.Shared.Helpers;
using Spectre.Console;

namespace Matrix.Domain.Entities;

public sealed class InitialSettings
{
    /// <summary>
    /// Quantidade de anos que serão simulados.
    /// </summary>
    public int SimulationYears { get; set; } = DateTime.UtcNow.Year;

    /// <summary>
    /// Quantos países serão inicialmente populados.
    /// </summary>
    public int AmountOfCountries { get; set; } = 1;

    /// <summary>
    /// Civilização inicial. 
    /// Quantidade de casais por país.
    /// </summary>
    public int StartingCouplesByCountry { get; set; } = 1;

    /// <summary>
    /// Exibe os eventos durante a simulação.
    /// </summary>
    public bool ShowEvents { get; set; } = true;

    public async Task ConfigureAsync(bool isDebug)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText(ApplicationConstants.Name).Centered().Color(Color.Cyan1));
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine($"[grey]{ApplicationConstants.Description}[/] [cyan]v{ApplicationConstants.Version}[/]");
        AnsiConsole.WriteLine();

        if (isDebug)
        {
            AnsiConsole.Write(new Rule("[cyan]Modo debug ativado[/]").RuleStyle("grey"));

            SimulationYears = DateTime.UtcNow.Year;
            AmountOfCountries = 10;
            StartingCouplesByCountry = 4;
            ShowEvents = true;
        }
        else
        {
            AnsiConsole.Write(new Rule("[cyan]Configuração inicial[/]").RuleStyle("grey"));
            AnsiConsole.WriteLine();

            // Anos de simulação;
            const int minSimulationYears = 1;
            const int maxSimulationYears = 9999;

            SimulationYears = AnsiConsole.Prompt(
                new TextPrompt<int>("Quantos anos deseja simular?").
                    PromptStyle("cyan").
                    ValidationErrorMessage($"[red]A quantidade de anos da simulação deve estar entre {minSimulationYears} e {maxSimulationYears}.[/]").
                    Validate(x => x >= minSimulationYears && x <= maxSimulationYears));

            // Países;
            const int minAmountOfCountries = 1;
            int maxAmountOfCountries = Enum.GetValues<CountryEnum>().Length;

            StartingCouplesByCountry = AnsiConsole.Prompt(
                new TextPrompt<int>("Quantos casais deseja iniciar na civilização?").
                    PromptStyle("cyan").
                    ValidationErrorMessage($"[red]A quantidade de países deve estar entre {minAmountOfCountries} e {maxAmountOfCountries}.[/]").
                    Validate(x => x >= minAmountOfCountries && x <= maxAmountOfCountries));

            // Casais;
            const int minCouples = 1;
            const int maxCouples = 100;

            StartingCouplesByCountry = AnsiConsole.Prompt(
                new TextPrompt<int>("Quantos casais deseja iniciar na civilização?").
                    PromptStyle("cyan").
                    Validate(x =>
                    {
                        if (x < minCouples || x > maxCouples)
                        {
                            return ValidationResult.Error($"[red]A quantidade inicial de casais deve estar entre {minCouples} e {maxCouples}.[/]");
                        }

                        return ValidationResult.Success();
                    }));

            // Exibir eventos;
            ShowEvents = AnsiConsole.Prompt(
                new SelectionPrompt<bool>().
                    Title("Deseja exibir os eventos durante a simulação?").
                    AddChoices(true, false).
                    DefaultValue(true).
                    UseConverter(x => x ? "[cyan]Sim[/]" : "[cyan]Não[/]"));
        }

        AnsiConsole.WriteLine();

        Table table = new();

        table.Border(TableBorder.Rounded);
        table.AddColumn("[cyan]Configuração[/]");
        table.AddColumn("[cyan]Valor[/]");

        table.AddRow("Anos simulados", SimulationYears.ToString());
        table.AddRow("População inicial", StartingCouplesByCountry.ToString());
        table.AddRow("Exibir eventos", ShowEvents ? "Sim" : "Não");

        AnsiConsole.Write(table);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[cyan]✓ Configuração concluída.[/]");

        await AnsiConsole.Status().
            Spinner(Spinner.Known.Dots).
            StartAsync("[cyan]Iniciando simulação...[/]", async x =>
            {
                int delay = isDebug ? 0 : RandomHelpers.RandomBetween(1000, 2000);
                await Task.Delay(delay);

                AnsiConsole.MarkupLine("[cyan]✓ Simulação iniciada![/]");
            });
    }
}
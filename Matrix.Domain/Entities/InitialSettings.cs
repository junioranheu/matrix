using Matrix.Domain.Consts;
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
    /// Exibe os eventos durante a simulação.
    /// </summary>
    public bool ShowEvents { get; set; } = true;

    public async Task ConfigureAsync()
    {
        bool isDebug = System.Diagnostics.Debugger.IsAttached;

        AnsiConsole.Clear();

        AnsiConsole.Write(new FigletText(ApplicationConstants.Name).Centered().Color(Color.Cyan1));

        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine($"[grey]{ApplicationConstants.Description}[/] [cyan]v{ApplicationConstants.Version}[/]");

        AnsiConsole.WriteLine();

        if (isDebug)
        {
            AnsiConsole.Write(new Rule("[cyan]Modo debug ativado[/]").RuleStyle("grey"));

            SimulationYears = 100;
            ShowEvents = true;
        }
        else
        {
            AnsiConsole.Write(new Rule("[cyan]Configuração inicial[/]").RuleStyle("grey"));

            AnsiConsole.WriteLine();

            const int minSimulationYears = 1;
            const int maxSimulationYears = 9999;

            SimulationYears = AnsiConsole.Prompt(
                new TextPrompt<int>("Quantos anos deseja simular?").
                    PromptStyle("cyan").
                    ValidationErrorMessage($"[red]A quantidade de anos da simulação deve estar entre {minSimulationYears} e {maxSimulationYears}.[/]").
                    Validate(x => x >= minSimulationYears && x <= maxSimulationYears));

            ShowEvents = AnsiConsole.Prompt(
                new SelectionPrompt<bool>().
                    Title("Deseja exibir os eventos durante a simulação?").
                    AddChoices(true, false).
                    UseConverter(x => x ? "[cyan]Sim[/]" : "[cyan]Não[/]"));
        }

        AnsiConsole.WriteLine();

        Table table = new();

        table.Border(TableBorder.Rounded);
        table.AddColumn("[cyan]Configuração[/]");
        table.AddColumn("[cyan]Valor[/]");

        table.AddRow("Anos simulados", SimulationYears.ToString());
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
using Matrix.Domain.Entities;
using Matrix.Domain.Enums;
using Spectre.Console;

namespace Matrix.Infrastructure.Reports;

/// <summary>
/// Responsável por exibir um resumo estatístico do mundo para o ano atual da simulação.
/// </summary>
public static class WorldYearConsoleReport
{
    /// <summary>
    /// Exibe o relatório anual do mundo no console.
    /// </summary>
    /// <param name="settings">
    /// Configurações iniciais da simulação.
    /// </param>
    /// <param name="world">
    /// Mundo contendo os humanos e informações utilizadas para gerar o relatório.
    /// </param>
    public static void Print(InitialSettings settings, World world)
    {
        if (!settings.ShowEvents)
        {
            return;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[cyan]Ano {world.CurrentDateString}[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();

        Panel panel = new(BuildContent(world))
        {
            Header = new PanelHeader($"Resumo do ano {world.CurrentDateString}"),
            Border = BoxBorder.Rounded
        };

        AnsiConsole.Write(panel);
    }

    #region methods

    /// <summary>
    /// Monta o conteúdo textual exibido no painel de resumo anual.
    /// </summary>
    /// <param name="world">
    /// Mundo utilizado como fonte das estatísticas.
    /// </param>
    /// <returns>
    /// Texto formatado contendo os indicadores do ano atual.
    /// </returns>
    private static string BuildContent(World world)
    {
        int alive = GetAliveCount(world);
        int births = GetBirthsThisYear(world);
        int deaths = GetDeathsThisYear(world);

        int men = GetMenCount(world);
        int women = GetWomenCount(world);

        int children = GetChildrenCount(world);
        int adults = GetAdultsCount(world);
        int elders = GetEldersCount(world);

        int countries = GetCountriesCount(world);

        Human? oldestHuman = GetOldestAliveHuman(world);

        return $"""
            🌎 Humanos vivos...... {alive}
            👶 Nascimentos........ {births}
            ⚰️ Mortes............. {deaths}
            📈 Crescimento........ {births - deaths}

            👨 Homens............. {men}
            👩 Mulheres........... {women}

            👶 Crianças........... {children}
            🧑 Adultos............ {adults}
            👴 Idosos............. {elders}

            🏳️ Países............. {countries}

            🥇 Mais velho......... {(oldestHuman is null ? "—" : $"{oldestHuman.Identity.FullName} ({oldestHuman.Life.Age})")}
            """;
    }

    /// <summary>
    /// Obtém a quantidade de humanos vivos.
    /// </summary>
    /// <param name="world">
    /// Mundo utilizado para consulta.
    /// </param>
    /// <returns>
    /// Quantidade de humanos vivos.
    /// </returns>
    private static int GetAliveCount(World world)
    {
        return world.Humans.Count(x => x.Life.IsAlive);
    }

    /// <summary>
    /// Obtém a quantidade de humanos nascidos no ano atual.
    /// </summary>
    /// <param name="world">
    /// Mundo utilizado para consulta.
    /// </param>
    /// <returns>
    /// Quantidade de nascimentos ocorridos no ano atual.
    /// </returns>
    private static int GetBirthsThisYear(World world)
    {
        return world.Humans.Count(x => x.Identity.BirthDate.Year == world.CurrentDate.Year);
    }

    /// <summary>
    /// Obtém a quantidade de humanos que morreram no ano atual.
    /// </summary>
    /// <param name="world">
    /// Mundo utilizado para consulta.
    /// </param>
    /// <returns>
    /// Quantidade de mortes ocorridas no ano atual.
    /// </returns>
    private static int GetDeathsThisYear(World world)
    {
        return world.Humans.Count(x => x.Life.DateOfDeath.HasValue && x.Life.DateOfDeath.Value.Year == world.CurrentDate.Year);
    }

    /// <summary>
    /// Obtém a quantidade de homens vivos.
    /// </summary>
    /// <param name="world">
    /// Mundo utilizado para consulta.
    /// </param>
    /// <returns>
    /// Quantidade de homens vivos.
    /// </returns>
    private static int GetMenCount(World world)
    {
        return world.Humans.Count(x => x.Life.IsAlive == true && x.Identity.Gender == GenderEnum.Male);
    }

    /// <summary>
    /// Obtém a quantidade de mulheres vivas.
    /// </summary>
    /// <param name="world">
    /// Mundo utilizado para consulta.
    /// </param>
    /// <returns>
    /// Quantidade de mulheres vivas.
    /// </returns>
    private static int GetWomenCount(World world)
    {
        return world.Humans.Count(x => x.Life.IsAlive == true && x.Identity.Gender == GenderEnum.Female);
    }

    /// <summary>
    /// Obtém a quantidade de crianças vivas.
    /// </summary>
    /// <param name="world">
    /// Mundo utilizado para consulta.
    /// </param>
    /// <returns>
    /// Quantidade de humanos com menos de dezoito anos.
    /// </returns>
    private static int GetChildrenCount(World world)
    {
        return world.Humans.Count(x => x.Life.IsAlive == true && x.Life.Age < 18);
    }

    /// <summary>
    /// Obtém a quantidade de adultos vivos.
    /// </summary>
    /// <param name="world">
    /// Mundo utilizado para consulta.
    /// </param>
    /// <returns>
    /// Quantidade de humanos entre dezoito e sessenta e quatro anos.
    /// </returns>
    private static int GetAdultsCount(World world)
    {
        return world.Humans.Count(x => x.Life.IsAlive == true && x.Life.Age >= 18 && x.Life.Age < 65);
    }

    /// <summary>
    /// Obtém a quantidade de idosos vivos.
    /// </summary>
    /// <param name="world">
    /// Mundo utilizado para consulta.
    /// </param>
    /// <returns>
    /// Quantidade de humanos com sessenta e cinco anos ou mais.
    /// </returns>
    private static int GetEldersCount(World world)
    {
        return world.Humans.Count(x => x.Life.IsAlive == true && x.Life.Age >= 65);
    }

    /// <summary>
    /// Obtém a quantidade de países distintos presentes no mundo.
    /// </summary>
    /// <param name="world">
    /// Mundo utilizado para consulta.
    /// </param>
    /// <returns>
    /// Quantidade de países distintos.
    /// </returns>
    private static int GetCountriesCount(World world)
    {
        return world.Humans.Select(x => x.Location.BirthCountry).Distinct().Count();
    }

    /// <summary>
    /// Obtém o humano vivo com a maior idade.
    /// </summary>
    /// <param name="world">
    /// Mundo utilizado para consulta.
    /// </param>
    /// <returns>
    /// Humano vivo mais velho ou nulo caso não existam humanos vivos.
    /// </returns>
    private static Human? GetOldestAliveHuman(World world)
    {
        return world.Humans.Where(x => x.Life.IsAlive).MaxBy(x => x.Life.Age);
    }
    #endregion
}
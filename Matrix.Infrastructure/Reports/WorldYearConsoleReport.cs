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
    /// <param name="isFinalReport">
    /// Relatório final. Também indica se as estatísticas devem incluir humanos mortos (ou seja, todos!).
    /// </param>
    public static void Print(InitialSettings settings, World world, bool isFinalReport)
    {
        if (!isFinalReport && !settings.ShowEvents)
        {
            return;
        }

        if (!isFinalReport)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule($"[cyan]Ano {world.CurrentDateString}[/]").RuleStyle("grey"));
            AnsiConsole.WriteLine();
        }

        string title = isFinalReport ? $"Resumo dos {world.CurrentYear} anos" : $"Resumo do ano {world.CurrentDateString}";

        Panel panel = new(BuildContent(world, isFinalReport))
        {
            Header = new PanelHeader(title),
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
    /// <param name="isFinalReport">
    /// Relatório final. Também indica se as estatísticas devem incluir humanos mortos (ou seja, todos!).
    /// </param>
    /// <returns>
    /// Texto formatado contendo os indicadores do ano atual.
    /// </returns>
    private static string BuildContent(World world, bool isFinalReport)
    {
        IEnumerable<Human> humans = GetHumansFromWorld(world, isFinalReport);

        int alive = GetAliveCount(humans, isFinalReport);
        int births = GetBirthsThisYear(humans, currentYear: world.CurrentDate);
        int deaths = GetDeathsThisYear(humans, currentYear: world.CurrentDate, isFinalReport);

        int men = GetMenCount(humans, isFinalReport);
        int women = GetWomenCount(humans, isFinalReport);
        int humansTotal = men + women;

        int children = GetChildrenCount(humans, isFinalReport);
        int adults = GetAdultsCount(humans, isFinalReport);
        int elders = GetEldersCount(humans, isFinalReport);

        int countries = GetCountriesCount(humans);

        Human? oldestHuman = GetOldestAliveHuman(humans);
        DateOnly? lastDeathDate = GetLastDeath(humans);

        string statisticsMode = isFinalReport ? "Geral" : "Apenas vivos";

        return $"""
        📊 Estatísticas....... {statisticsMode}

        🌎 Humanos {(isFinalReport ? "(total)" : "(vivos)")}.... {(isFinalReport ? humansTotal : alive)}
        👶 Nascimentos........ {(isFinalReport ? "-" : births)}
        ⚰️ Mortes............. {deaths}
        📈 Crescimento........ {(isFinalReport ? "-" : births - deaths)}

        👨 Homens............. {men}
        👩 Mulheres........... {women}

        👶 Crianças........... {children}
        🧑 Adultos............ {adults}
        👴 Idosos............. {elders}

        🏳️ Países............. {countries}

        🥇 Mais velho......... {(oldestHuman is null ? "—" : $"{oldestHuman.Identity.FullName} ({oldestHuman.Life.Age})")}
        ⚰️ Última morte....... {(lastDeathDate is null ? "—" : $"ano {lastDeathDate.Value:yyyy}")}
        """;
    }

    /// <summary>
    /// Obtém a coleção de humanos que será utilizada no relatório.
    /// </summary>
    /// <param name="world">
    /// Mundo contendo todos os humanos da simulação.
    /// </param>
    /// <param name="isFinalReport">
    /// Indica se o relatório final está sendo gerado. Quando verdadeiro,
    /// inclui humanos vivos e mortos; caso contrário, apenas humanos vivos.
    /// </param>
    /// <returns>
    /// Coleção de humanos utilizada para cálculo das estatísticas.
    /// </returns>
    private static IEnumerable<Human> GetHumansFromWorld(World world, bool isFinalReport)
    {
        IEnumerable<Human> humans = isFinalReport ? world.Humans : world.Humans.Where(x => x.Life.IsAlive);

        return humans;
    }

    /// <summary>
    /// Obtém a quantidade de humanos vivos.
    /// </summary>
    /// <param name="humans">
    /// Humanos do mundo utilizados para consulta.
    /// </param>
    /// <returns>
    /// Quantidade de humanos vivos.
    /// </returns>
    private static int GetAliveCount(IEnumerable<Human> humans, bool isFinalReport)
    {
        return isFinalReport ? humans.Count() : humans.Count(x => x.Life.IsAlive == true);
    }

    /// <summary>
    /// Obtém a quantidade de humanos nascidos no ano atual.
    /// </summary>
    /// <param name="humans">
    /// Humanos do mundo utilizados para consulta.
    /// </param>
    /// <param name="currentYear">
    /// Ano utilizado para a consulta.
    /// </param>
    /// <returns>
    /// Quantidade de nascimentos ocorridos no ano atual.
    /// </returns>
    private static int GetBirthsThisYear(IEnumerable<Human> humans, DateOnly currentYear)
    {
        return humans.Count(x => x.Identity.BirthDate.Year == currentYear.Year);
    }

    /// <summary>
    /// Obtém a quantidade de humanos que morreram no ano atual.
    /// </summary>
    /// <param name="humans">
    /// Humanos do mundo utilizados para consulta.
    /// </param>
    /// <param name="currentYear">
    /// Ano utilizado para a consulta.
    /// </param>
    /// <returns>
    /// Quantidade de mortes ocorridas no ano atual.
    /// </returns>
    private static int GetDeathsThisYear(IEnumerable<Human> humans, DateOnly currentYear, bool isFinalReport)
    {
        return isFinalReport ? humans.Count(x => x.Life.DateOfDeath.HasValue) : humans.Count(x => x.Life.DateOfDeath.HasValue && x.Life.DateOfDeath.Value.Year == currentYear.Year);
    }

    /// <summary>
    /// Obtém a quantidade de homens vivos.
    /// </summary>
    /// <param name="humans">
    /// Humanos do mundo utilizados para consulta.
    /// </param>
    /// <returns>
    /// Quantidade de homens vivos.
    /// </returns>
    private static int GetMenCount(IEnumerable<Human> humans, bool isFinalReport)
    {
        return isFinalReport ? humans.Count(x => x.Identity.Gender == GenderEnum.Male) : humans.Count(x => x.Life.IsAlive == true && x.Identity.Gender == GenderEnum.Male);
    }

    /// <summary>
    /// Obtém a quantidade de mulheres vivas.
    /// </summary>
    /// <param name="humans">
    /// Humanos do mundo utilizados para consulta.
    /// </param>
    /// <returns>
    /// Quantidade de mulheres vivas.
    /// </returns>
    private static int GetWomenCount(IEnumerable<Human> humans, bool isFinalReport)
    {
        return isFinalReport ? humans.Count(x => x.Identity.Gender == GenderEnum.Female) : humans.Count(x => x.Life.IsAlive == true && x.Identity.Gender == GenderEnum.Female);
    }

    /// <summary>
    /// Obtém a quantidade de crianças vivas.
    /// </summary>
    /// <param name="humans">
    /// Humanos do mundo utilizados para consulta.
    /// </param>
    /// <returns>
    /// Quantidade de humanos com menos de dezoito anos.
    /// </returns>
    private static int GetChildrenCount(IEnumerable<Human> humans, bool isFinalReport)
    {
        return isFinalReport ? humans.Count(x => x.Life.Age < 18) : humans.Count(x => x.Life.IsAlive == true && x.Life.Age < 18);
    }

    /// <summary>
    /// Obtém a quantidade de adultos vivos.
    /// </summary>
    /// <param name="humans">
    /// Humanos do mundo utilizados para consulta.
    /// </param>
    /// <returns>
    /// Quantidade de humanos entre dezoito e sessenta e quatro anos.
    /// </returns>
    private static int GetAdultsCount(IEnumerable<Human> humans, bool isFinalReport)
    {
        return isFinalReport ? humans.Count(x => x.Life.Age >= 18 && x.Life.Age < 65) : humans.Count(x => x.Life.IsAlive == true && x.Life.Age >= 18 && x.Life.Age < 65);
    }

    /// <summary>
    /// Obtém a quantidade de idosos vivos.
    /// </summary>
    /// <param name="humans">
    /// Humanos do mundo utilizados para consulta.
    /// </param>
    /// <returns>
    /// Quantidade de humanos com sessenta e cinco anos ou mais.
    /// </returns>
    private static int GetEldersCount(IEnumerable<Human> humans, bool isFinalReport)
    {
        return isFinalReport ? humans.Count(x => x.Life.Age >= 65) : humans.Count(x => x.Life.IsAlive == true && x.Life.Age >= 65);
    }

    /// <summary>
    /// Obtém a quantidade de países distintos presentes no mundo.
    /// </summary>
    /// <param name="humans">
    /// Humanos do mundo utilizados para consulta.
    /// </param>
    /// <returns>
    /// Quantidade de países distintos.
    /// </returns>
    private static int GetCountriesCount(IEnumerable<Human> humans)
    {
        return humans.Select(x => x.Location.BirthCountry).Distinct().Count();
    }

    /// <summary>
    /// Obtém o humano vivo com a maior idade.
    /// </summary>
    /// <param name="humans">
    /// Humanos do mundo utilizados para consulta.
    /// </param>
    /// <returns>
    /// Humano vivo mais velho ou nulo caso não existam humanos vivos.
    /// </returns>
    private static Human? GetOldestAliveHuman(IEnumerable<Human> humans)
    {
        return humans.Where(x => x.Life.IsAlive == true).MaxBy(x => x.Life.Age);
    }

    /// <summary>
    /// Obtém a data da morte mais recente entre os humanos informados.
    /// </summary>
    /// <param name="humans">
    /// Coleção de humanos utilizada para consulta.
    /// </param>
    /// <returns>
    /// Data da morte mais recente ou nulo caso nenhum humano tenha falecido.
    /// </returns>
    private static DateOnly? GetLastDeath(IEnumerable<Human> humans)
    {
        return humans.Where(x => x.Life.DateOfDeath.HasValue).Max(x => x.Life.DateOfDeath);
    }
    #endregion
}
using Matrix.Domain.Entities;
using System.Diagnostics;
using System.Text;

namespace Matrix.Infrastructure.Reports;

public static class WorldHtmlReport
{
    /// <summary>
    /// Gera e abre o relatório HTML da simulação.
    /// </summary>
    /// <param name="world">
    /// Mundo que será exportado.
    /// </param>
    public static void Export(World world)
    {
        string reportsFolder = Path.Combine(Path.GetTempPath(), nameof(Matrix));

        if (Directory.Exists(reportsFolder))
        {
            Directory.Delete(reportsFolder, true);
        }

        Directory.CreateDirectory(reportsFolder);

        string filePath = Path.Combine(reportsFolder, $"{nameof(WorldHtmlReport)}.html");

        string html = GenerateHtml(world);

        File.WriteAllText(filePath, html);

        Process.Start(new ProcessStartInfo
        {
            FileName = filePath,
            UseShellExecute = true
        });
    }

    #region methods
    /// <summary>
    /// Gera o conteúdo HTML do relatório.
    /// </summary>
    /// <param name="world">
    /// Mundo que será exportado.
    /// </param>
    /// <returns>
    /// Conteúdo HTML gerado.
    /// </returns>
    private static string GenerateHtml(World world)
    {
        StringBuilder html = new();

        html.AppendLine("""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <title>Matrix Report</title>

                <style>
                    body {
                        font-family: Arial, sans-serif;
                        padding: 20px;
                    }

                    table {
                        border-collapse: collapse;
                        width: 100%;
                        margin-bottom: 30px;
                    }

                    th, td {
                        border: 1px solid #ddd;
                        padding: 8px;
                    }

                    th {
                        background: #f5f5f5;
                    }

                    tr:nth-child(even) {
                        background: #fafafa;
                    }

                    details {
                        margin-bottom: 10px;
                        border: 1px solid #ddd;
                        border-radius: 6px;
                        padding: 10px;
                    }

                    summary {
                        cursor: pointer;
                        font-weight: bold;
                    }

                    ul {
                        margin-top: 10px;
                    }
                </style>
            </head>
            <body>
            """);

        html.AppendLine($"<h1>Mundo {world.Name}</h1>");

        AppendHumansTable(html, world);

        AppendLifeEventsHistory(html, world.Humans);

        html.AppendLine("""
            </body>
            </html>
            """);

        return html.ToString();
    }

    /// <summary>
    /// Adiciona a tabela de humanos ao relatório.
    /// </summary>
    /// <param name="html">
    /// HTML em construção.
    /// </param>
    /// <param name="world">
    /// Mundo da simulação.
    /// </param>
    private static void AppendHumansTable(StringBuilder html, World world)
    {
        html.AppendLine("""
            <h2>Humanos</h2>

            <table>
                <tr>
                    <th>Nascimento</th>
                    <th>Nome</th>
                    <th>Idade</th>
                    <th>Status</th>
                    <th>Filhos</th>
                </tr>
            """);

        foreach (Human human in world.Humans.OrderBy(x => x.Identity.BirthDate))
        {
            html.AppendLine($"""
                <tr>
                    <td>{human.Identity.BirthDate:yyyy}</td>
                    <td>{human.Identity.FullName}</td>
                    <td>{human.Life.Age}</td>
                    <td>{(human.Life.IsAlive ? "Vivo" : "Morto")}</td>
                    <td>{human.Family.ChildrenIds.Count}</td>
                </tr>
                """);
        }

        html.AppendLine("</table>");
    }

    /// <summary>
    /// Adiciona o histórico de vida dos humanos ao relatório.
    /// </summary>
    /// <param name="html">
    /// HTML em construção.
    /// </param>
    /// <param name="humans">
    /// Humanos que terão seus eventos exibidos.
    /// </param>
    private static void AppendLifeEventsHistory(StringBuilder html, IEnumerable<Human> humans)
    {
        html.AppendLine("<h2>Histórico de vida</h2>");

        foreach (Human human in humans.OrderBy(x => x.Identity.BirthDate))
        {
            if (human.Life.LifeEvents.Count == 0)
            {
                continue;
            }

            html.AppendLine($"""
                <details>
                    <summary>
                        {human.Identity.FullName}
                        ({human.Life.Age} anos)
                    </summary>

                    <ul>
                """);

            int lastBirthdayIndex = human.Life.LifeEvents.FindLastIndex(IsBirthdayEvent);

            for (int i = 0; i < human.Life.LifeEvents.Count; i++)
            {
                string lifeEvent = human.Life.LifeEvents[i];

                if (IsBirthdayEvent(lifeEvent) &&  i != lastBirthdayIndex)
                {
                    continue;
                }

                html.AppendLine($"<li>{lifeEvent}</li>");
            }

            html.AppendLine("""
                    </ul>
                </details>
                """);
        }
    }

    /// <summary>
    /// Indica se o evento representa um aniversário.
    /// </summary>
    /// <param name="lifeEvent">
    /// Evento a ser analisado.
    /// </param>
    /// <returns>
    /// True quando o evento representa um aniversário.
    /// </returns>
    private static bool IsBirthdayEvent(string lifeEvent)
    {
        return lifeEvent.Contains("COMPLETOU", StringComparison.OrdinalIgnoreCase) &&
               lifeEvent.Contains("ANOS", StringComparison.OrdinalIgnoreCase);
    }
    #endregion
}
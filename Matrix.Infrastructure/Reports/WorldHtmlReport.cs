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
        string reportsFolder = Path.Combine(Path.GetTempPath(), "Matrix");

        if (Directory.Exists(reportsFolder))
        {
            Directory.Delete(reportsFolder, true);
        }

        Directory.CreateDirectory(reportsFolder);

        string filePath = Path.Combine(reportsFolder, "report.html");

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
                </style>
            </head>
            <body>
            """);

        html.AppendLine($"<h1>Mundo {world.Name}</h1>");

        html.AppendLine("""
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

        html.AppendLine("""
            </table>
            </body>
            </html>
            """);

        return html.ToString();
    }
    #endregion
}
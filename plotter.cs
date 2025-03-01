using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScottPlot;
using ScottPlot.Statistics;

/// <summary>
/// Classe utilitaire pour la création et la sauvegarde de différents graphiques 
/// (performance quotidienne, histogramme de rendements, etc.) 
/// ainsi que pour l'écriture de résultats dans un fichier CSV.
/// </summary>
public class Plotter
{
    /// <summary>
    /// Retourne un chemin absolu combinant un répertoire fixe et le nom de fichier spécifié.
    /// Si le répertoire n'existe pas, il est créé.
    /// </summary>
    /// <param name="fileName">Nom du fichier (avec extension) à sauvegarder.</param>
    /// <returns>Le chemin absolu du fichier.</returns>
    /// C:\Users\admin\Desktop\cours dauphine\S1\C#\projet\results\output.png
    public static string GetOutputPath(string fileName)
    {
        string directory = @"C:\Users\admin\Desktop\cours dauphine\S1\C#\projet\results";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        return Path.Combine(directory, fileName);
    }

    /// <summary>
    /// Retourne un chemin absolu combinant un répertoire fixe, un sous-dossier 
    /// (spécifié par <paramref name="folderName"/>) et le nom de fichier spécifié. 
    /// Si le sous-dossier n'existe pas, il est créé.
    /// </summary>
    /// <param name="folderName">Nom du sous-dossier (exemple : "MomentumStrategy_20010401-20011101").</param>
    /// <param name="fileName">Nom du fichier (avec extension) à sauvegarder.</param>
    /// <returns>Le chemin absolu du fichier, incluant le sous-dossier.</returns>
    /// C:\Users\admin\Desktop\cours dauphine\S1\C#\projet\results\MomentumStrategy_20240301\output.png
    /// Desfois on veut créer un folder avec les résults et desfois on veut juste créer le dossier performance metrics
    public static string GetOutputPath(string folderName, string fileName)
    {
        string directory = @"C:\Users\admin\Desktop\cours dauphine\S1\C#\projet\results";
        string folderPath = Path.Combine(directory, folderName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        return Path.Combine(folderPath, fileName);
    }

    /// <summary>
    /// Génère et sauvegarde un graphique de l'évolution quotidienne de la valeur du portefeuille.
    /// </summary>
    /// <param name="portfolioHistory">
    /// Dictionnaire contenant l'historique du portefeuille : 
    /// la clé est la date (<see cref="DateTime"/>) et la valeur est la valeur du portefeuille.
    /// </param>
    /// <param name="folderName">
    /// Nom du sous-dossier dans lequel le graphique sera sauvegardé 
    /// (par exemple "MomentumStrategy_20010401-20011101").
    /// </param>
    public static void PlotDailyPerformance(Dictionary<DateTime, double> portfolioHistory, string folderName)
    {
        if (portfolioHistory == null || portfolioHistory.Count == 0)
        {
            Console.WriteLine("Historique du portefeuille vide.");
            return;
        }

        double[] xs = portfolioHistory.Keys.Select(date => date.ToOADate()).ToArray();
        double[] ys = portfolioHistory.Values.ToArray();

        var myPlot = new ScottPlot.Plot();
        myPlot.Add.Scatter(xs, ys);
        myPlot.Title("Performance Quotidienne du Portefeuille");
        myPlot.XLabel("Date");
        myPlot.YLabel("Valeur du Portefeuille");
        myPlot.Axes.DateTimeTicksBottom();

        string filePath = GetOutputPath(folderName, "DailyPerformance.png");
        myPlot.SavePng(filePath, 800, 500);
        Console.WriteLine($"Graphique de performance quotidienne sauvegardé sous '{filePath}'.");
    }

    /// <summary>
    /// Génère et sauvegarde un histogramme des rendements quotidiens du portefeuille.
    /// </summary>
    /// <param name="portfolioHistory">
    /// Dictionnaire contenant l'historique du portefeuille : 
    /// la clé est la date (<see cref="DateTime"/>) et la valeur est la valeur du portefeuille.
    /// </param>
    /// <param name="folderName">
    /// Nom du sous-dossier dans lequel le graphique sera sauvegardé 
    /// (par exemple "MomentumStrategy_20010401-20011101").
    /// </param>
    public static void PlotReturnHistogram(Dictionary<DateTime, double> portfolioHistory, string folderName)
    {
        var sortedHistory = portfolioHistory.OrderBy(kvp => kvp.Key).ToList();
        if (sortedHistory.Count < 2)
        {
            Console.WriteLine("Pas assez de données pour calculer les rendements.");
            return;
        }

        List<double> dailyReturns = new List<double>();
        for (int i = 1; i < sortedHistory.Count; i++)
        {
            double previousValue = sortedHistory[i - 1].Value;
            double currentValue = sortedHistory[i].Value;
            double dailyReturn = (currentValue / previousValue) - 1;
            dailyReturns.Add(dailyReturn);
        }

        var hist = Histogram.WithBinCount(10, dailyReturns.ToArray());
        var myPlot = new ScottPlot.Plot();
        var barPlot = myPlot.Add.Bars(hist.Bins, hist.Counts);

        // Ajustement de la taille des barres pour un affichage plus lisible
        foreach (var bar in barPlot.Bars)
        {
            bar.Size = hist.FirstBinSize * 0.8;
        }

        myPlot.Axes.Margins(bottom: 0);
        myPlot.XLabel("Rendement Quotidien");
        myPlot.YLabel("Fréquence");
        myPlot.Title("Histogramme des Rendements Quotidiens");

        string filePath = GetOutputPath(folderName, "DailyReturnsHistogram.png");
        myPlot.SavePng(filePath, 800, 500);
        Console.WriteLine($"Histogramme des rendements quotidiens sauvegardé sous '{filePath}'.");
    }

    /// <summary>
    /// Génère et sauvegarde un scatter plot des rendements quotidiens du portefeuille.
    /// </summary>
    /// <param name="portfolioHistory">
    /// Dictionnaire contenant l'historique du portefeuille : 
    /// la clé est la date (<see cref="DateTime"/>) et la valeur est la valeur du portefeuille.
    /// </param>
    /// <param name="folderName">
    /// Nom du sous-dossier dans lequel le graphique sera sauvegardé 
    /// (par exemple "MomentumStrategy_20010401-20011101").
    /// </param>
    public static void PlotDailyReturns(Dictionary<DateTime, double> portfolioHistory, string folderName)
    {
        var sortedHistory = portfolioHistory.OrderBy(kvp => kvp.Key).ToList();
        if (sortedHistory.Count < 2)
        {
            Console.WriteLine("Pas assez de données pour tracer les rendements quotidiens.");
            return;
        }

        List<double> dailyReturns = new List<double>();
        List<DateTime> dates = new List<DateTime>();
        for (int i = 1; i < sortedHistory.Count; i++)
        {
            double previousValue = sortedHistory[i - 1].Value;
            double currentValue = sortedHistory[i].Value;
            double dailyReturn = (currentValue / previousValue) - 1;
            dailyReturns.Add(dailyReturn);
            dates.Add(sortedHistory[i].Key);
        }

        double[] xs = dates.Select(date => date.ToOADate()).ToArray();
        double[] ys = dailyReturns.ToArray();

        var myPlot = new ScottPlot.Plot();
        myPlot.Add.Scatter(xs, ys);
        myPlot.Title("Rendements Quotidiens");
        myPlot.XLabel("Date");
        myPlot.YLabel("Rendement Quotidien");
        myPlot.Axes.DateTimeTicksBottom();

        string filePath = GetOutputPath(folderName, "DailyReturns.png");
        myPlot.SavePng(filePath, 800, 500);
        Console.WriteLine($"Graphique des rendements quotidiens sauvegardé sous '{filePath}'.");
    }

    /// <summary>
    /// Classe imbriquée responsable de la sauvegarde des indicateurs de performance dans un fichier CSV.
    /// Gère la création/suppression du fichier et l'écriture de l'en-tête lors de la première exécution.
    /// </summary>
    public static class CsvPerformanceLogger
    {
        // Indicateur pour savoir si le fichier a déjà été créé lors de cette exécution
        private static bool fileCreated = false;

        /// <summary>
        /// Sauvegarde les métriques de performance (Sharpe, volatilité, rendement, etc.) dans un fichier CSV. 
        /// Si le fichier n'existe pas encore ou n'a pas été créé dans cette exécution, il est (re)créé 
        /// avec l'en-tête, sinon la nouvelle ligne est simplement ajoutée.
        /// </summary>
        /// <param name="results">Objet <see cref="Results"/> contenant les indicateurs de performance à sauvegarder.</param>
        /// <param name="backtestStart">Date de début de la période de backtest.</param>
        /// <param name="backtestEnd">Date de fin de la période de backtest.</param>
        /// <param name="periodLabel">Label décrivant la période (par exemple "Recession US").</param>
        /// <param name="strategyName">Nom de la stratégie utilisée (par exemple "MomentumStrategy").</param>
        public static void SavePerformanceMetrics(
            Results results,
            DateTime backtestStart,
            DateTime backtestEnd,
            string periodLabel,
            string strategyName)
        {
            string filePath = GetOutputPath("PerformanceMetrics.csv");

            // L'en-tête du CSV
            string header = "Période,Label,Stratégie,Sharpe,Annualized Vol,Annualized Return,Total Return,Total Vol";

            // Construction de la ligne de données
            string periodStr = $"{backtestStart:yyyy-MM-dd} - {backtestEnd:yyyy-MM-dd}";
            string dataLine = $"{periodStr},{periodLabel},{strategyName}," +
                              $"{results.SharpeRatio:F2}," +
                              $"{results.AnnualizedVolatility:F2}," +
                              $"{results.AnnualizedReturn:F2}," +
                              $"{results.TotalReturn:F3}," +
                              $"{results.TotalVolatility:F2}";

            // Si le fichier n'a pas encore été créé lors de cette exécution...
            if (!fileCreated)
            {
                // S'il existe déjà sur le disque (d’une exécution précédente), on le supprime
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // On crée un nouveau fichier avec l'en-tête + la première ligne
                File.WriteAllText(filePath, header + Environment.NewLine + dataLine + Environment.NewLine);

                // On marque l'indicateur comme "fichier déjà créé"
                fileCreated = true;
            }
            else
            {
                // Sinon, on ajoute simplement la nouvelle ligne
                File.AppendAllText(filePath, dataLine + Environment.NewLine);
            }

            Console.WriteLine($"Fichier CSV des indicateurs de performance sauvegardé sous '{filePath}'.");
        }
    }
}

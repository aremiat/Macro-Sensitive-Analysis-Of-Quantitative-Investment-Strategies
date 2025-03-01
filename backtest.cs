using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;

/// <summary>
/// Classe dédiée à l'exécution d'un backtest sur différentes périodes et différentes stratégies.
/// Elle orchestre :
/// <list type="bullet">
///   <item>le filtrage des données de log-returns sur la période souhaitée,</item>
///   <item>la simulation d'un portefeuille avec des rebalancements mensuels,</item>
///   <item>le calcul des indicateurs de performance,</item>
///   <item>et la sauvegarde des graphiques et résultats.</item>
/// </list>
/// </summary>
public class Backtest
{
    private readonly Database _database;
    private readonly List<(DateTime start, DateTime end, string label)> _periods;
    private readonly List<StrategyBase> _strategies;

    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="Backtest"/>.
    /// </summary>
    /// <param name="database">Instance de <see cref="Database"/> pour accéder aux données de marché.</param>
    /// <param name="periods">Liste de tuples (date de début, date de fin, label) représentant les périodes de backtest.</param>
    /// <param name="strategies">Liste de stratégies (<see cref="StrategyBase"/>) à tester.</param>
    public Backtest(Database database,
                    List<(DateTime start, DateTime end, string label)> periods,
                    List<StrategyBase> strategies)
    {
        _database = database;
        _periods = periods;
        _strategies = strategies;
    }

    /// <summary>
    /// Lance le backtest en parcourant chaque période et chaque stratégie.
    /// </summary>
    public void Run()
    {
        // Récupère les rendements logaritmiques depuis la base de données
        var logReturns = _database.CalculateLogReturns();
        if (logReturns == null || logReturns.Count == 0)
        {
            Console.WriteLine("Aucune donnée de log return disponible.");
            return;
        }

        // Vérifie l'existence du fichier CSV global pour stocker les performances
        string resultsCsvPath = Plotter.GetOutputPath("PerformanceMetrics.csv");
        if (!File.Exists(resultsCsvPath))
        {
            string header = "Période,Label,Stratégie,Sharpe,Annualized Vol,Annualized Return,Total Return";
            File.WriteAllLines(resultsCsvPath, new[] { header });
        }

        // Pour chaque période
        foreach (var period in _periods)
        {
            // pour chaque stratégie
            foreach (var strategy in _strategies)
            {
                string strategyName = strategy.GetType().Name;
                Console.WriteLine($"\nExécution du backtest pour la période {period.label} avec la stratégie {strategyName}");

                // Filtrer les log-returns sur la période courante
                var filteredLogReturns = logReturns
                    .Where(kvp => kvp.Key >= period.start && kvp.Key <= period.end)
                    .OrderBy(kvp => kvp.Key)
                    .ToList();

                if (filteredLogReturns.Count == 0)
                {
                    Console.WriteLine("Aucune donnée de log return disponible pour cette période.");
                    continue;
                }

                // Déterminer les dates de rebalancement (le premier jour ouvrable de chaque mois)
                var rebalancingDates = filteredLogReturns
                    .Select(kvp => kvp.Key)
                    .GroupBy(date => new { date.Year, date.Month })
                    .Select(g => g.Min())
                    .OrderBy(d => d)
                    .ToList();

                // Simuler le portefeuille
                double portfolioValue = 1.0;
                var portfolioHistory = new Dictionary<DateTime, double>
                {
                    { filteredLogReturns.First().Key, portfolioValue }
                };
                string folderName = $"{strategyName}_{period.start:yyyyMMdd}-{period.end:yyyyMMdd}";

                // Parcourt les intervalles de rebalancement
                for (int i = 0; i < rebalancingDates.Count - 1; i++)
                {
                    DateTime currentRebalance = rebalancingDates[i];
                    DateTime nextRebalance = rebalancingDates[i + 1];

                    // Calcule les poids de la stratégie à la date de rebalancement
                    var rawWeights = strategy.CalculateWeight(currentRebalance);

                    var weights = EnforceWeightConstraints(rawWeights,0.005, 0.20);

                    Console.WriteLine($"\nRebalancement le {currentRebalance:yyyy-MM-dd} avec stratégie {strategyName}");
                    foreach (var w in weights)
                    {
                        Console.WriteLine($"{w.Key}: {w.Value * 100:F2}%");
                    }

                    SaveRebalancingComposition(folderName, currentRebalance, weights);

                    // Parcourt les jours ouvrables entre deux rebalancements pour mettre à jour la valeur du portefeuille
                    var periodDates = filteredLogReturns
                        .Where(kvp => kvp.Key > currentRebalance && kvp.Key <= nextRebalance)
                        .Select(kvp => kvp.Key)
                        .OrderBy(d => d)
                        .ToList();

                    foreach (var day in periodDates)
                    {
                        double dailyPortfolioLogReturn = 0.0;
                        if (logReturns.ContainsKey(day))
                        {
                            var dailyLogReturns = logReturns[day];
                            foreach (var asset in weights)
                            {
                                string ticker = asset.Key;
                                double weight = asset.Value;
                                if (dailyLogReturns.ContainsKey(ticker))
                                {
                                    dailyPortfolioLogReturn += weight * dailyLogReturns[ticker];
                                }
                                else
                                {
                                    Console.WriteLine("Erreur daily return");
                                }
                            }
                        }
                        else
                            {
                                Console.WriteLine("erreur log retun");
                            }
                        // Convertit le log-return cumulé en multiplicateur de valeur
                        portfolioValue *= Math.Exp(dailyPortfolioLogReturn);
                        portfolioHistory[day] = portfolioValue;
                    }
                }

                // 4. Calculer les indicateurs de performance (Sharpe, etc.) pour la période
                Results results = Results.Calculate(portfolioHistory);
                Console.WriteLine($"[{period.label}] Stratégie {strategyName} => Sharpe: {results.SharpeRatio:F2}, Annualized Return: {results.AnnualizedReturn:P2}");

                // 5. Construire un dossier de sauvegarde (ex. : "MomentumStrategy_20010401-20011101") pour les graphiques
                Plotter.PlotDailyPerformance(portfolioHistory, folderName);
                Plotter.PlotReturnHistogram(portfolioHistory, folderName);
                Plotter.PlotDailyReturns(portfolioHistory, folderName);

                // 6. Sauvegarder les résultats dans le CSV global
                Plotter.CsvPerformanceLogger.SavePerformanceMetrics(results, period.start, period.end, period.label, strategyName);
            }
        }

        Console.WriteLine($"\nTous les résultats ont été sauvegardés dans le fichier CSV : {resultsCsvPath}");
    }

    /// <summary>
    /// Sauvegarde dans un CSV, dans le même dossier que les images, 
    /// la composition (poids par actif) à la date de rebalancement spécifiée.
    /// </summary>
    /// <param name="folderName">Nom du dossier de sortie (ex. "MomentumStrategy_20010401-20011101").</param>
    /// <param name="rebalancingDate">Date de rebalancement.</param>
    /// <param name="weights">Dictionnaire (ticker -> poids) calculé par la stratégie.</param>
    private void SaveRebalancingComposition(string folderName, DateTime rebalancingDate, Dictionary<string, double> weights)
    {
        // Nom de fichier basé sur la date de rebalancement, ex: "Rebalancing_20231015.csv"
        string fileName = $"Rebalancing_{rebalancingDate:yyyyMMdd}.csv";

        // On obtient le chemin complet dans le sous-dossier folderName
        string filePath = Plotter.GetOutputPath(folderName, fileName);

        string header = "Ticker,Weight";

        // Construit le contenu du CSV
        // La première ligne sera l'en-tête, puis on ajoute chaque actif.
        List<string> lines = new List<string>();
        lines.Add(header);

        foreach (var w in weights)
        {
            double weightPercent = w.Value * 100.0;
            lines.Add($"{w.Key},{weightPercent:F2}");
        }

        // Écrit toutes les lignes dans le fichier (on écrase s'il existe déjà)
        File.WriteAllLines(filePath, lines);

        Console.WriteLine($"Composition du portefeuille sauvegardée sous '{filePath}'.");
    }

    /// <summary>
    /// Contraint chaque poids à être compris entre un minimum et un maximum absolu, 
    /// puis renormalise la somme des poids absolus à 1.
    /// </summary>
    /// <param name="weights">Dictionnaire (ticker -> poids) renvoyé par la stratégie.</param>
    /// <param name="minAbsWeight">Poids absolu minimal (ex. 0.005 pour 0.5%).</param>
    /// <param name="maxAbsWeight">Poids absolu maximal (ex. 0.10 pour 10%).</param>
    /// <returns>Dictionnaire de poids ajustés et renormalisés.</returns>
    private Dictionary<string, double> EnforceWeightConstraints(
        Dictionary<string, double> weights,
        double minAbsWeight,
        double maxAbsWeight)
    {
        // Séparer longs (>=0) et shorts (<0)
        var longs = new Dictionary<string, double>();
        var shorts = new Dictionary<string, double>(); // on stocke les poids en valeur absolue
        foreach (var kvp in weights)
        {
            if (kvp.Value >= 0)
                longs[kvp.Key] = kvp.Value;
            else
                shorts[kvp.Key] = Math.Abs(kvp.Value);
        }

        // Traitement itératif pour un groupe (les valeurs sont positives)
        Dictionary<string, double> EnforceConstraintsForGroup(Dictionary<string, double> group, double min, double max)
        {
            const int maxIterations = 100;
            for (int iter = 0; iter < maxIterations; iter++)
            {
                bool updated = false;
                // Identifier les actifs deja clipés aux bornes
                var unconstrainedKeys = group.Where(kvp => kvp.Value > min && kvp.Value < max)
                                             .Select(kvp => kvp.Key)
                                             .ToList();

                // S'il ils sont tous clipés alors pas besoin d'optimisation
                if (unconstrainedKeys.Count == group.Count)
                    break;

                double totalAdjustment = 0.0;
                // Appliquer le clipping
                foreach (var key in group.Keys.ToList())
                {
                    double weight = group[key];
                    if (weight > max)
                    {
                        totalAdjustment += (weight - max);
                        group[key] = max;
                        updated = true;
                    }
                    else if (weight < min)
                    {
                        totalAdjustment -= (min - weight);
                        group[key] = min;
                        updated = true;
                    }
                }

                // Redistribution du surplus/déficit parmi les actifs non contraints
                if (Math.Abs(totalAdjustment) > 1e-3)
                {
                    double delta = totalAdjustment / unconstrainedKeys.Count;
                    foreach (var key in unconstrainedKeys)
                    {
                        group[key] += delta;
                    }
                    updated = true;
                }

                if (!updated)
                    break;
            }
            return group;
        }

        // Appliquer sur les deux groupes
        longs = EnforceConstraintsForGroup(longs, minAbsWeight, maxAbsWeight);
        shorts = EnforceConstraintsForGroup(shorts, minAbsWeight, maxAbsWeight);

        // Réassembler en réappliquant le signe pour les shorts
        var adjusted = new Dictionary<string, double>();
        foreach (var kvp in longs)
        {
            adjusted[kvp.Key] = kvp.Value;
        }
        foreach (var kvp in shorts)
        {
            adjusted[kvp.Key] = -kvp.Value;
        }

        // Renormalisation finale : somme des valeurs absolues = 1
        double sumAbs = adjusted.Values.Sum(x => Math.Abs(x));
        if (Math.Abs(sumAbs - 1) > 1e-6)  
        {
            foreach (var key in adjusted.Keys.ToList())
            {
                adjusted[key] /= sumAbs;
            }
        }

        return adjusted;
    }
}


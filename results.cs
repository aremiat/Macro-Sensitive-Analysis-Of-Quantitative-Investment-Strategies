using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Représente les résultats d'un backtest ou d'une simulation de portefeuille, 
/// incluant différents indicateurs de performance (rendement, volatilité, Sharpe, etc.).
/// </summary>
public class Results
{
    /// <summary>
    /// Obtient le rendement annualisé du portefeuille.
    /// </summary>
    public double AnnualizedReturn { get; private set; }

    /// <summary>
    /// Obtient la volatilité annualisée du portefeuille (basée sur 252 jours de bourse).
    /// </summary>
    public double AnnualizedVolatility { get; private set; }

    /// <summary>
    /// Obtient le ratio de Sharpe du portefeuille.
    /// </summary>
    public double SharpeRatio { get; private set; }

    /// <summary>
    /// Obtient le rendement total du portefeuille sur toute la période.
    /// </summary>
    public double TotalReturn { get; private set; }

    /// <summary>
    /// Obtient la volatilité totale du portefeuille sur toute la période (non annualisée).
    /// </summary>
    public double TotalVolatility { get; private set; }

    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="Results"/> avec 
    /// les différents indicateurs de performance calculés.
    /// </summary>
    /// <param name="annualizedReturn">Rendement annualisé.</param>
    /// <param name="annualizedVolatility">Volatilité annualisée (basée sur 252 jours).</param>
    /// <param name="sharpeRatio">Ratio de Sharpe.</param>
    /// <param name="totalReturn">Rendement total sur la période.</param>
    /// <param name="totalVolatility">Volatilité totale sur la période (non annualisée).</param>
    public Results(double annualizedReturn, double annualizedVolatility, double sharpeRatio, double totalReturn, double totalVolatility)
    {
        AnnualizedReturn = annualizedReturn;
        AnnualizedVolatility = annualizedVolatility;
        SharpeRatio = sharpeRatio;
        TotalReturn = totalReturn;
        TotalVolatility = totalVolatility;
    }

    /// <summary>
    /// Calcule et retourne les indicateurs de performance d'un portefeuille à partir de l'historique 
    /// de sa valeur. Le dictionnaire <paramref name="portfolioHistory"/> doit contenir au moins 
    /// deux points (date, valeur). Le taux sans risque <paramref name="riskFreeRate"/> est 
    /// considéré comme un taux annualisé (par défaut 0).
    /// </summary>
    /// <param name="portfolioHistory">
    /// Historique du portefeuille sous forme de dictionnaire 
    /// où la clé est la date (<see cref="DateTime"/>) et la valeur est la valeur du portefeuille à cette date.
    /// </param>
    /// <param name="riskFreeRate">Taux sans risque annualisé (optionnel, par défaut 0).</param>
    /// <returns>
    /// Un objet <see cref="Results"/> contenant les indicateurs de performance calculés 
    /// (rendement et volatilité annualisés, ratio de Sharpe, rendement total, etc.).
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Levée si l'historique du portefeuille contient moins de deux points de données.
    /// </exception>
    public static Results Calculate(Dictionary<DateTime, double> portfolioHistory, double riskFreeRate = 0.0)
    {
        if (portfolioHistory == null || portfolioHistory.Count < 2)
            throw new ArgumentException("L'historique du portefeuille doit contenir au moins deux points de données.");

        // Trier l'historique par date
        var sortedHistory = portfolioHistory.OrderBy(kvp => kvp.Key).ToList();

        // Calcul des rendements journaliers
        List<double> dailyReturns = new List<double>();
        for (int i = 1; i < sortedHistory.Count; i++)
        {
            double previousValue = sortedHistory[i - 1].Value;
            double currentValue = sortedHistory[i].Value;
            double dailyReturn = (currentValue / previousValue) - 1;
            dailyReturns.Add(dailyReturn);
        }

        // Calcul de la moyenne et de l'écart-type des rendements journaliers
        double averageDailyReturn = dailyReturns.Average();
        double dailyStdDev = Math.Sqrt(dailyReturns.Sum(r => Math.Pow(r - averageDailyReturn, 2)) / (dailyReturns.Count - 1));

        // Nombre de jours total entre la première et la dernière date
        int totalDays = (sortedHistory.Last().Key - sortedHistory.First().Key).Days;

        // Calcul du rendement annualisé (en supposant 365.25 jours calendaire puisque totalDays est en jours calendaire)
        double annualizedReturn = Math.Pow(sortedHistory.Last().Value / sortedHistory.First().Value, 365.25 / totalDays) - 1;

        // Calcul de la volatilité annualisée (en supposant 252 jours de bourse par an)
        double annualizedVolatility = dailyStdDev * Math.Sqrt(252);

        // Volatilité totale (non annualisée), jours de bourse
        double totalVolatility = dailyStdDev * Math.Sqrt(dailyReturns.Count);

        // Ratio de Sharpe (taux sans risque supposé annualisé)
        double sharpeRatio = (annualizedReturn - riskFreeRate) / annualizedVolatility;

        // Calcul du rendement total
        double totalReturn = (sortedHistory.Last().Value / sortedHistory.First().Value) - 1;

        return new Results(annualizedReturn, annualizedVolatility, sharpeRatio, totalReturn, totalVolatility);
    }

    /// <summary>
    /// Retourne une représentation sous forme de chaîne de caractères 
    /// des principaux indicateurs de performance.
    /// </summary>
    /// <returns>
    /// Chaîne contenant le rendement annualisé, la volatilité annualisée, 
    /// le ratio de Sharpe, le rendement total et la volatilité totale.
    /// </returns>
    public override string ToString()
    {
        return $"Rendement annualisé : {AnnualizedReturn:P3}\n" +
               $"Volatilité annualisée : {AnnualizedVolatility:P3}\n" +
               $"Sharpe Ratio : {SharpeRatio:F2}\n" +
               $"Rendement total : {TotalReturn:P4}\n" +
               $"Volatilité totale : {TotalVolatility:P3}";
    }
}

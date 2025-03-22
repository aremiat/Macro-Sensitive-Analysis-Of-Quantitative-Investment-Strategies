using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Classe de base abstraite pour définir le comportement commun à toutes les stratégies.
/// </summary>
public abstract class StrategyBase
{
    /// <summary>
    /// Instance de la base de données utilisée pour récupérer les informations nécessaires aux calculs de la stratégie.
    /// </summary>
    /// Les classes dérivées peuvent utiliser pour accéder aux données du Database,
    /// Qui est fixé définitivement au moment de la construction de la stratégie et ne peut plus changer.
    protected readonly Database _database;

    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="StrategyBase"/> avec une instance de <see cref="Database"/>.
    /// </summary>
    /// <param name="database">Objet <see cref="Database"/> pour accéder aux données des actifs.</param>
    /// Autoriser uniquement les classes dérivées (héritant de StrategyBase) à appeler ce constructeur lorsqu’elles se construisent elles-mêmes.
    protected StrategyBase(Database database)
    {
        _database = database;
    }

    /// <summary>
    /// Méthode à implémenter par les classes dérivées pour calculer la pondération des actifs.
    /// </summary>
    /// <param name="startDate">Date de référence pour calculer les pondérations.</param>
    /// <returns>
    /// Un dictionnaire contenant en clé le ticker de l'actif et en valeur la pondération (positive ou négative).
    /// </returns>
    public abstract Dictionary<string, double> CalculateWeight(DateTime startDate, int numactifs);
}

/// <summary>
/// Stratégie Momentum qui sélectionne les actifs en fonction de leurs performances récentes.
/// Hérite de <see cref="StrategyBase"/>.
/// </summary>
public class MomentumStrategy : StrategyBase
{
    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="MomentumStrategy"/> avec la base de données spécifiée.
    /// </summary>
    /// <param name="database">Objet <see cref="Database"/> pour accéder aux données des actifs.</param>
    /// MomentumStrategy hérite de StrategyBase
    /// StrategyBase possède un constructeur prenant un paramètre Database database,
    public MomentumStrategy(Database database) : base(database) { }

    /// <summary>
    /// Calcule les pondérations des actifs en se basant sur leurs rendements sur les 12 derniers mois, 
    /// en excluant le dernier mois (30 jours).
    /// 
    /// Sélectionne ensuite les 10 meilleurs et 10 pires rendements pour créer une stratégie Long/Short.
    /// </summary>
    /// <param name="startDate">Date à partir de laquelle on souhaite établir la stratégie.</param>
    /// <returns>
    /// Un dictionnaire avec pour chaque ticker, une pondération positive (meilleurs rendements) ou négative (pires rendements).
    /// La somme des poids absolus est normalisée à 1.
    /// </returns>
    public override Dictionary<string, double> CalculateWeight(DateTime startDate, int numactifs)
    {
        // Déclaration des variables en dehors du bloc conditionnel
        DateTime firstDate, lastDate;

        // Définir la période de calcul (12 mois - 1 mois)
        DateTime startDateStrat = startDate.AddYears(-1);
        DateTime endDateStrat = startDate.AddDays(-30);

        firstDate = _database.GetClosestDate(startDateStrat);
        lastDate = _database.GetClosestDate(endDateStrat);

        DateTime earliestDbDate = _database.GetFirstDate();
        if (startDateStrat < earliestDbDate)
        {
            throw new Exception(
                $"Impossible d'appliquer la MomentumStrategy : " +
                $"pas assez d'historique (besoin d'1 an avant {startDate:yyyy-MM-dd}). " +
                $"La base commence le {earliestDbDate:yyyy-MM-dd}."
            );
        }

        // Récupération des données pour la période donnée
        var data = _database.GetData(firstDate, lastDate);

        // Si aucune date n'est trouvée, on récupère la date la plus proche dans la base de données
        if (!data.ContainsKey(startDateStrat) || !data.ContainsKey(endDateStrat))
        {
            firstDate = _database.GetClosestDate(firstDate);
            lastDate = _database.GetClosestDate(lastDate);
        }
        var firstPrices = data[firstDate];
        var lastPrices = data[lastDate];

        // Calcul des rendements pour chaque actif
        var percentages = new Dictionary<string, double>();
        foreach (var ticker in firstPrices.Keys)
        {
            if (lastPrices.ContainsKey(ticker) && firstPrices[ticker] > 1e-3)
            {
                double firstPrice = firstPrices[ticker];
                double lastPrice = lastPrices[ticker];
                percentages[ticker] = ((lastPrice - firstPrice) / firstPrice) * 100;
            }

        }

        // Tri des actifs par ordre décroissant de performance
        var sortedPercentages = percentages
            .OrderByDescending(kvp => kvp.Value)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var bestReturns = sortedPercentages.Take(numactifs).ToList();
        var worstReturns = sortedPercentages.Reverse().Take(numactifs).ToList();

        var weightedReturns = new Dictionary<string, double>();
        int n = bestReturns.Count;

        double totalWeight = bestReturns.Sum(br => Math.Abs(br.Value))
                            + worstReturns.Sum(br => Math.Abs(br.Value));

        // Attribution des poids initiaux
        for (int i = 0; i < n; i++)
        {
            double positiveWeight = bestReturns[i].Value / totalWeight;
            weightedReturns[bestReturns[i].Key] = positiveWeight;
            weightedReturns[worstReturns[i].Key] = -positiveWeight;
        }

        // Normalisation des poids (somme des poids absolus = 1)
        double totalAbsWeight = weightedReturns.Values.Sum(w => Math.Abs(w));
        double adjustmentFactor = 1.0 / totalAbsWeight;
        var adjustedWeightedReturns = weightedReturns
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value * adjustmentFactor);

        double finalAbsWeightSum = adjustedWeightedReturns.Values.Sum(w => Math.Abs(w));
        double epsilon = 1e-6; // seuil de tolérance
        // Vérification de la somme des poids
        if (Math.Abs(finalAbsWeightSum - 1) > epsilon)
        {
            throw new Exception($"La somme des poids absolus ({finalAbsWeightSum}) n'est pas égale à 1 !");
        }
        else
        {
            Console.WriteLine($"Somme des poids absolus après normalisation : {finalAbsWeightSum}");
        }

        return adjustedWeightedReturns;
    }
}

/// <summary>
/// Stratégie Value qui sélectionne les actifs en fonction d'un ratio de valorisation sur 5 ans.
/// Hérite de <see cref="StrategyBase"/>.
/// </summary>
public class ValueStrategy : StrategyBase
{
    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="ValueStrategy"/> avec la base de données spécifiée.
    /// </summary>
    /// <param name="database">Objet <see cref="Database"/> pour accéder aux données des actifs.</param>
    public ValueStrategy(Database database) : base(database) { }

    /// <summary>
    /// Calcule les pondérations des actifs en se basant sur leurs prix il y a 5 ans 
    /// et leurs prix actuels, afin d'établir un ratio "prix actuel / (prix actuel - prix d'il y a 5 ans)".
    /// 
    /// Sélectionne ensuite les 10 actifs les plus "chers" (coefficient élevé) et 10 les moins chers 
    /// pour créer une stratégie Long/Short.
    /// </summary>
    /// <param name="startDate">Date à partir de laquelle on souhaite établir la stratégie.</param>
    /// <returns>
    /// Un dictionnaire avec pour chaque ticker, une pondération positive (actifs chers) ou négative (actifs moins chers).
    /// La somme des poids absolus est normalisée à 1.
    /// </returns>
    public override Dictionary<string, double> CalculateWeight(DateTime startDate, int numactifs)
    {
        // Définir la période de calcul (5 ans en arrière)
        DateTime startDateStrat = startDate.AddYears(-5);
        DateTime endDateStrat = startDate.AddDays(-1);

        DateTime fiveYearsAgoDate = _database.GetClosestDate(startDateStrat);
        DateTime latestDate = _database.GetClosestDate(endDateStrat);

        DateTime earliestDbDate = _database.GetFirstDate();
        if (startDateStrat < earliestDbDate)
        {
            // On lève une exception explicite
            throw new Exception(
                $"Impossible d'appliquer la ValueStrategy : " +
                $"pas assez d'historique (besoin de 5 ans avant {startDate:yyyy-MM-dd}). " +
                $"La base commence le {earliestDbDate:yyyy-MM-dd}."
            );
        }

        var data = _database.GetData(fiveYearsAgoDate, latestDate);

        // Si aucune date n'est trouvée, on récupère la date la plus proche
        if (!data.ContainsKey(fiveYearsAgoDate) || !data.ContainsKey(latestDate))
        {
            fiveYearsAgoDate = _database.GetClosestDate(startDateStrat);
            latestDate = _database.GetClosestDate(endDateStrat);
        }

        // Récupération des prix à 5 ans et des prix actuels
        var firstPrices = data[fiveYearsAgoDate];
        var lastPrices = data[latestDate];

        // Calcul du coefficient pour chaque actif
        var coefficients = new Dictionary<string, double>();
        foreach (var ticker in lastPrices.Keys)
        {
            if (firstPrices.ContainsKey(ticker))
            {
                double currentPrice = lastPrices[ticker];
                double oldPrice = firstPrices[ticker];

                if (Math.Abs(oldPrice - currentPrice) > 1e-3)
                {
                    coefficients[ticker] = currentPrice / (currentPrice - oldPrice);
                }
                else
                {
                    coefficients[ticker] = 0;
                }
            }
        }

        // Tri des actifs par ordre décroissant du coefficient
        var rankedCoefficients = coefficients
            .OrderByDescending(kvp => kvp.Value)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // Sélectionner les 10 plus chers et 10 moins chers
        var expensiveAssets = rankedCoefficients.Take(numactifs).ToList();
        var cheapAssets = rankedCoefficients.Reverse().Take(numactifs).ToList();

        // Calcul du total pour normaliser
        double totalWeight = expensiveAssets.Sum(r => Math.Abs(r.Value))
                          + cheapAssets.Sum(r => Math.Abs(r.Value));

        var weightedReturns = new Dictionary<string, double>();
        int n = expensiveAssets.Count;

        // Attribution des poids initiaux
        for (int i = 0; i < n; i++)
        {
            double positiveWeight = expensiveAssets[i].Value / totalWeight;
            weightedReturns[expensiveAssets[i].Key] = positiveWeight;
            weightedReturns[cheapAssets[i].Key] = -positiveWeight;
        }

        // Normalisation des poids (somme des poids absolus = 1)
        double totalAbsWeight = weightedReturns.Values.Sum(w => Math.Abs(w));
        double adjustmentFactor = 1.0 / totalAbsWeight;
        var adjustedWeightedReturns = weightedReturns
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value * adjustmentFactor);

        double finalAbsWeightSum = adjustedWeightedReturns.Values.Sum(w => Math.Abs(w));
        double epsilon = 1e-6; // seuil de tolérance

        // Vérification de la somme des poids
        if (Math.Abs(finalAbsWeightSum - 1) > epsilon)
        {
            throw new Exception($"La somme des poids absolus ({finalAbsWeightSum}) n'est pas égale à 1 !");
        }
        else
        {
            Console.WriteLine($"Somme des poids absolus après normalisation : {finalAbsWeightSum}");
        }

        return adjustedWeightedReturns;
    }
}

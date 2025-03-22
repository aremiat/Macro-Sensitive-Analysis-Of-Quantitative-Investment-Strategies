// ---------------------------------------------------------------------------
// POUR INSTALLER LES PACKAGES NUNIT (dans un projet .NET SDK-style ou via
// la console NuGet dans Visual Studio si c'est un projet classique) :
//
// 1) Ouvrez la "Console du Gestionnaire de packages" (Package Manager Console)
//    dans Visual Studio, puis exécutez :
//
//    Install-Package NUnit
//    Install-Package NUnit3TestAdapter
//    Install-Package Microsoft.NET.Test.Sdk
//
// ---------------------------------------------------------------------------

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

// ---------------------------------------------------------------------------
// TESTS POUR LA CLASSE "Database"
// ---------------------------------------------------------------------------
namespace BacktestProject.Tests
{
    [TestFixture]
    public class DatabaseTests
    {
        private const string ValidCsvPath = @"C:\Users\admin\Desktop\cours dauphine\S1\C#\projet\Univers.csv";
        private const string InvalidCsvPath = @"C:\Chemin\Vers\FichierQuiNexistePas.csv";

        [Test]
        public void LoadData_ShouldThrowException_WhenFileNotFound()
        {
            // Vérifie qu'une exception est lancée si le fichier n'existe pas
            Assert.Throws<FileNotFoundException>(() =>
            {
                var db = new Database(InvalidCsvPath);
            });
        }

        [Test]
        public void LoadData_ShouldLoadDataCorrectly_WhenFileIsValid()
        {
            Database db = new Database(ValidCsvPath);
            var data = db.GetData(DateTime.MinValue, DateTime.MaxValue);

            // Vérifie que la base contient au moins une ligne
            Assert.That(data.Count, Is.GreaterThan(0),
                "La base de données devrait contenir au moins une ligne.");
        }

        [Test]
        public void GetData_ShouldReturnCorrectSubset()
        {
            Database db = new Database(ValidCsvPath);

            DateTime start = new DateTime(2000, 1, 1);
            DateTime end = new DateTime(2000, 12, 31);
            var subset = db.GetData(start, end);

            foreach (var date in subset.Keys)
            {
                Assert.That(date, Is.InRange(start, end),
                    $"Date {date} n’est pas dans l’intervalle.");
            }
        }

        [Test]
        public void CalculateLogReturns_ShouldReturnNonEmpty_WhenDataIsLoaded()
        {
            Database db = new Database(ValidCsvPath);
            var logReturns = db.CalculateLogReturns();

            Assert.That(logReturns.Count, Is.GreaterThan(0),
                "Le dictionnaire de log-returns ne doit pas être vide.");
        }

        [Test]
        public void GetClosestDate_ShouldReturnPreviousBusinessDay()
        {
            // Arrange
            Database db = new Database(ValidCsvPath);
            // Supposons que le 1er janvier 2023 est un dimanche,
            // et que le vendredi précédent (30 décembre 2022) est présent dans les données.
            DateTime targetDate = new DateTime(2023, 1, 1);
            DateTime expectedBusinessDay = new DateTime(2022, 12, 30);

            DateTime closest = db.GetClosestDate(targetDate);

            Assert.That(closest, Is.EqualTo(expectedBusinessDay),
                "Pour une date cible tombant un dimanche, la date retournée doit être le vendredi précédent.");
        }
    }

    // ---------------------------------------------------------------------------
    // TESTS POUR LES STRATEGIES (MomentumStrategy, ValueStrategy)
    // ---------------------------------------------------------------------------
    [TestFixture]
    public class StrategyTests
    {
        private Database _db;

        [SetUp]
        public void Setup()
        {
            _db = new Database(@"C:\Users\admin\Desktop\cours dauphine\S1\C#\projet\Univers.csv");
        }

        [Test]
        public void MomentumStrategy_ShouldSelectAssetsCorrectly()
        {
            MomentumStrategy momentum = new MomentumStrategy(_db);

            DateTime refDate = new DateTime(2020, 1, 1);
            var weights = momentum.CalculateWeight(refDate, 10);


            double sumAbs = 0;
            foreach (var w in weights.Values)
                sumAbs += Math.Abs(w);

            // Vérifie que la somme absolue des poids est ~ 1
            Assert.That(sumAbs, Is.EqualTo(1.0).Within(1e-3),
                "La somme des poids absolus doit être égale à 1.");

        }

        [Test]
        public void ValueStrategy_ShouldThrowException_WhenDataIsInsufficient()
        {
            ValueStrategy valueStrat = new ValueStrategy(_db);

            DateTime refDate = new DateTime(1986, 1, 1);

            var ex = Assert.Throws<Exception>(() => valueStrat.CalculateWeight(refDate, 10));

            Assert.That(ex.Message, Does.Contain("pas assez d'historique (besoin de 5 ans avant 1986-01-01)"));
        }

        [Test]
        public void ValueStrategy_ShouldSelectAssetsCorrectly()
        {
            ValueStrategy valueStrat = new ValueStrategy(_db);

            DateTime refDate = new DateTime(2020, 1, 1);
            var weights = valueStrat.CalculateWeight(refDate, 10);


            double sumAbs = 0;
            foreach (var w in weights.Values)
                sumAbs += Math.Abs(w);

            // Vérifie que la somme absolue des poids est ~ 1
            Assert.That(sumAbs, Is.EqualTo(1.0).Within(1e-3),
                "La somme des poids absolus doit être égale à 1.");

        }
    }

    // ---------------------------------------------------------------------------
    // TESTS POUR LA CLASSE "Backtest"
    // ---------------------------------------------------------------------------
    [TestFixture]
    public class BacktestTests
    {
        private Database _db;
        private List<(DateTime start, DateTime end, string label)> _periods;
        private List<StrategyBase> _strategies;

        [SetUp]
        public void Setup()
        {
            _db = new Database(@"C:\Users\admin\Desktop\cours dauphine\S1\C#\projet\Univers.csv");
            _periods = new List<(DateTime start, DateTime end, string label)>
            {
                (new DateTime(2020, 1, 1), new DateTime(2020, 12, 31), "Test Period 2020")
            };
            _strategies = new List<StrategyBase>
            {
                new MomentumStrategy(_db),
                new ValueStrategy(_db)
            };
        }

        [Test]
        public void Run_ShouldNotThrowException_WithValidData()
        {
            Backtest backtest = new Backtest(_db, _periods, _strategies);

            Assert.DoesNotThrow(() => backtest.Run());

        }

        [Test]
        public void Run_ShouldHandleEmptyData()
        {
            // Cas extrême : période hors-bornes
            var emptyPeriods = new List<(DateTime, DateTime, string)>
            {
                (new DateTime(1800,1,1), new DateTime(1800,1,2), "PeriodWithoutData")
            };

            Backtest backtest = new Backtest(_db, emptyPeriods, _strategies);
            Assert.DoesNotThrow(() => backtest.Run());

        }

        [Test]
        public void EnforceWeightConstraints_ShouldClipWeightsAndConverge()
        {
            // Arrange
            var initialWeights = new Dictionary<string, double>
            {
                // 10 actifs positifs
                { "Asset1", 0.50 },     // dépasse le maximum autorisé
                { "Asset2", 0.001 },    // en dessous du minimum autorisé
                { "Asset3", 0.15 },     // dans l'intervalle autorisé
                { "Asset4", 0.25 },     // dépasse le maximum autorisé
                { "Asset5", 0.006 },    // égal au minimum autorisé
                { "Asset6", 0.19 },     // dans l'intervalle autorisé
                { "Asset7", 0.12 },     // dans l'intervalle autorisé
                { "Asset8", 0.08 },     // dans l'intervalle autorisé
                { "Asset9", 0.04 },     // en dessous du minimum autorisé
                { "Asset10", 0.21 },    // dépasse le maximum autorisé

                // 10 actifs négatifs
                { "Asset11", -0.50 },   // dépasse le maximum autorisé (short trop important)
                { "Asset12", -0.001 },  // en dessous du minimum autorisé (short trop faible)
                { "Asset13", -0.15 },   // dépasse le maximum autorisé (short)
                { "Asset14", -0.25 },   // dépasse le maximum autorisé (short)
                { "Asset15", -0.006 },   // dans l'intervalle autorisé
                { "Asset16", -0.19 },   // dans l'intervalle autorisé
                { "Asset17", -0.12 },  // égal au minimum autorisé en valeur absolue
                { "Asset18", -0.08 },   // dans l'intervalle autorisé
                { "Asset19", -0.04 },   // dans l'intervalle autorisé
                { "Asset20", -0.21 }    // dans l'intervalle autorisé
            };

            double minAbs = 0.005; // minimum autorisé (0.5%)
            double maxAbs = 0.20;  // maximum autorisé (20%)

            double totalAbs = initialWeights.Sum(w => Math.Abs(w.Value));
            var normalizedWeights = initialWeights
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value / totalAbs);

            var backtest = new Backtest(null, new List<(DateTime, DateTime, string)>(), new List<StrategyBase>());

            // Utilisation de la réflexion pour accéder à la méthode privée EnforceWeightConstraints
            MethodInfo method = typeof(Backtest).GetMethod("EnforceWeightConstraints",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, "La méthode EnforceWeightConstraints n'a pas été trouvée via réflexion.");

            // Act
            var adjustedWeights = (Dictionary<string, double>)method.Invoke(backtest, new object[] { normalizedWeights, minAbs, maxAbs });


            // Vérifier que pour chaque actif, le poids (en valeur absolue) est dans l'intervalle [minAbs, maxAbs]
            foreach (var weight in adjustedWeights.Values)
            {
                double absWeight = Math.Abs(weight);
                Assert.That(absWeight, Is.GreaterThanOrEqualTo(minAbs),
                    $"Le poids absolu {absWeight} est inférieur au minimum autorisé de {minAbs}.");
                Assert.That(absWeight, Is.LessThanOrEqualTo(maxAbs),
                    $"Le poids absolu {absWeight} est supérieur au maximum autorisé de {maxAbs}.");
            }

            // Vérifier que la somme des poids en valeur absolue est normalisée à 1 (avec tolérance numérique)
            double sumAbs = adjustedWeights.Values.Sum(x => Math.Abs(x));
            Assert.That(sumAbs, Is.EqualTo(1.0).Within(1e-6),
                "La somme des poids absolus doit être normalisée à 1.");
        }

    }

    // ---------------------------------------------------------------------------
    // TESTS POUR LA CLASSE "Results"
    // ---------------------------------------------------------------------------
    [TestFixture]
    public class ResultsTests
    {
        [Test]
        public void Calculate_ShouldComputeCorrectValues()
        {
            var portfolioHistory = new Dictionary<DateTime, double>
            {
                { new DateTime(2020, 1, 1), 100.0 },
                { new DateTime(2020, 1, 2), 101.0 },
                { new DateTime(2020, 1, 3), 99.0 },
                { new DateTime(2020, 1, 4), 100.0 },
                { new DateTime(2020, 1, 5), 105.0 }
            };

            Results res = Results.Calculate(portfolioHistory, 0.0);

            double expectedTotalReturn = (105.0 / 100.0) - 1.0; // 5%
            Assert.That(res.TotalReturn, Is.EqualTo(expectedTotalReturn).Within(1e-6),
                "Le rendement total devrait être de 5% environ.");

            Assert.That(res.AnnualizedReturn, Is.GreaterThan(0),
                "Le rendement annualisé devrait être positif.");

            Assert.That(res.AnnualizedVolatility, Is.GreaterThanOrEqualTo(0),
                "La volatilité annualisée ne doit pas être négative.");
        }

        [Test]
        public void Calculate_ShouldThrowException_WhenHistoryHasLessThanTwoPoints()
        {
            var portfolioHistory = new Dictionary<DateTime, double>
            {
                { new DateTime(2020,1,1), 100.0 }
            };

            Assert.Throws<ArgumentException>(() =>
            {
                Results.Calculate(portfolioHistory);
            });
        }
    }
}

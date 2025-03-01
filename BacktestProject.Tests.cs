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

// ---------------------------------------------------------------------------
// EXEMPLE DE TESTS POUR LA CLASSE "Database"
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
        public void GetClosestDate_ShouldReturnOldest_WhenNoPreviousDateExists()
        {
            Database db = new Database(ValidCsvPath);
            DateTime targetDate = new DateTime(1900, 1, 1);
            DateTime closest = db.GetClosestDate(targetDate);

            DateTime expectedOldest = db.GetFirstDate();
            Assert.That(closest, Is.EqualTo(expectedOldest));
        }
    }

    // ---------------------------------------------------------------------------
    // EXEMPLE DE TESTS POUR LES STRATEGIES (MomentumStrategy, ValueStrategy)
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
            var weights = momentum.CalculateWeight(refDate);

            double sumAbs = 0;
            foreach (var w in weights.Values)
                sumAbs += Math.Abs(w);

            // Vérifie que la somme absolue des poids est ~ 1
            Assert.That(sumAbs, Is.EqualTo(1.0).Within(1e-3),
                "La somme des poids absolus doit être égale à 1.");

        }

        [Test]
        public void ValueStrategy_ShouldHandleMissingDataGracefully()
        {
            ValueStrategy valueStrat = new ValueStrategy(_db);

            DateTime refDate = new DateTime(1986, 1, 1);
            var weights = valueStrat.CalculateWeight(refDate);

            // Vérifie que le dictionnaire n'est pas null
            Assert.That(weights.Count, Is.GreaterThan(0),
                "Le poids des composantes ne doit pas être vide.");

            double sumAbs = 0;
            foreach (var w in weights.Values)
                sumAbs += Math.Abs(w);

            // Si la période est trop ancienne et qu'il n'y a pas 5 ans de data,
            // le dictionnaire peut être vide. On adapte le test :
            if (weights.Count > 0)
            {
                Assert.That(sumAbs, Is.EqualTo(1.0).Within(1e-3),
                    "La somme des poids absolus doit être égale à 1 si assez d’actifs.");
            }
        }
    }

    // ---------------------------------------------------------------------------
    // EXEMPLE DE TESTS POUR LA CLASSE "Backtest"
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

            // Vérifie que Run() ne lève pas d'exception
            Assert.DoesNotThrow(() => backtest.Run());



        }

        [Test]
        public void Run_ShouldHandleEmptyDataGracefully()
        {
            // Cas extrême : période hors-bornes
            var emptyPeriods = new List<(DateTime, DateTime, string)>
            {
                (new DateTime(1800,1,1), new DateTime(1800,1,2), "PeriodWithoutData")
            };

            Backtest backtest = new Backtest(_db, emptyPeriods, _strategies);
            Assert.DoesNotThrow(() => backtest.Run());

        }
    }

    // ---------------------------------------------------------------------------
    // EXEMPLE DE TESTS POUR LA CLASSE "Results"
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

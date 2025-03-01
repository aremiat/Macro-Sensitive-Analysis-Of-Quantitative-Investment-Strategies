using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BacktestUI
{
    public partial class BacktestForm : Form
    {
        // Chemin du fichier CSV source (à adapter)
        private readonly string _csvFilePath = @"C:\Users\admin\Desktop\cours dauphine\S1\C#\projet\Univers.csv";

        public BacktestForm()
        {
            InitializeComponent();
        }

        private void BacktestForm_Load(object sender, EventArgs e)
        {
            // Ajout de deux stratégies par défaut
            clbStrategies.Items.Add("MomentumStrategy", true);
            clbStrategies.Items.Add("ValueStrategy", true);

            // Configuration des DateTimePickers
            dtpStart.Value = new DateTime(2001, 4, 1);
            dtpEnd.Value = new DateTime(2023, 12, 1);
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            // 1) Récupération de la période choisie
            DateTime periodStart = dtpStart.Value.Date;
            DateTime periodEnd = dtpEnd.Value.Date;
            string periodLabel = $"Personnalisé: {periodStart:yyyy-MM-dd} au {periodEnd:yyyy-MM-dd}";

            // 2) Création d'une liste (une seule période)
            var periods = new List<(DateTime start, DateTime end, string label)>
            {
                (periodStart, periodEnd, periodLabel)
            };

            // 3) Instanciation de la base de données
            Database database = new Database(_csvFilePath);

            // 4) Construction de la liste des stratégies sélectionnées
            var strategies = new List<StrategyBase>();
            foreach (var item in clbStrategies.CheckedItems)
            {
                string strategyName = item.ToString();
                if (strategyName == "MomentumStrategy")
                    strategies.Add(new MomentumStrategy(database));
                else if (strategyName == "ValueStrategy")
                    strategies.Add(new ValueStrategy(database));
            }

            if (strategies.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner au moins une stratégie.",
                                "Avertissement",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            // 5) Exécution du backtest
            Backtest backtest = new Backtest(database, periods, strategies);
            backtest.Run();

            // 6) Affichage du CSV PerformanceMetrics.csv
            string resultsFile = Plotter.GetOutputPath("PerformanceMetrics.csv");
            if (File.Exists(resultsFile))
            {
                // Affiche le contenu brut dans le TextBox
                txtResults.Text = File.ReadAllText(resultsFile);

                // Affiche dans le DataGridView
                DisplayCsvInDataGrid(resultsFile);
            }
            else
            {
                txtResults.Text = "Aucun résultat à afficher.";
                dgvResults.Columns.Clear();
                dgvResults.Rows.Clear();
            }
        }

        /// <summary>
        /// Affiche un fichier CSV dans le DataGridView sous forme tabulaire.
        /// </summary>
        private void DisplayCsvInDataGrid(string csvPath)
        {
            if (!File.Exists(csvPath))
                return;

            var lines = File.ReadAllLines(csvPath);
            if (lines.Length < 2)
                return; // Au moins l'en-tête + 1 ligne

            // Première ligne = en-têtes
            var headers = lines[0].Split(',');

            // On vide d'éventuelles colonnes existantes
            dgvResults.Columns.Clear();

            // Crée les colonnes en fonction des en-têtes
            foreach (var header in headers)
            {
                dgvResults.Columns.Add(header, header);
            }

            // Ajoute chaque ligne de données
            for (int i = 1; i < lines.Length; i++)
            {
                var cells = lines[i].Split(',');
                dgvResults.Rows.Add(cells);
            }
        }
    }
}

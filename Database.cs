using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

/// <summary>
/// Représente une base de données pour charger et gérer des données de prix d'actifs à partir d'un fichier CSV.
/// </summary>
public class Database
{
    /// <summary>
    /// Chemin du fichier CSV.
    /// </summary>
    private readonly string _filePath;

    /// <summary>
    /// Dictionnaire principal pour stocker les données.
    /// La clé est la date (DateTime), et la valeur est un dictionnaire 
    /// (dont la clé est le ticker et la valeur est le prix).
    /// </summary>
    private readonly Dictionary<DateTime, Dictionary<string, double>> _data;

    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="Database"/>.
    /// Charge automatiquement les données depuis le fichier CSV spécifié.
    /// </summary>
    /// <param name="filePath">Chemin du fichier CSV à charger.</param>
    public Database(string filePath)
    {
        _filePath = filePath;
        _data = new Dictionary<DateTime, Dictionary<string, double>>();
        LoadData();
    }

    /// <summary>
    /// Charge les données depuis le fichier CSV spécifié dans le constructeur 
    /// et les stocke dans la collection <see cref="_data"/>.
    /// 
    /// Le fichier doit contenir une première colonne DateTime (format "yyyy-MM-dd HH:mm:ssK"), 
    /// puis une ou plusieurs colonnes de prix (une par ticker).
    /// </summary>
    /// <exception cref="Exception">Lève une exception si le fichier est vide 
    /// ou ne contient pas suffisamment de colonnes.</exception>
    private void LoadData()

    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException($"Fichier introuvable ou chemin invalide : {_filePath}");
        }

        using (var reader = new StreamReader(_filePath))
        {
            // Lecture de la ligne d'en-tête
            string headerLine = reader.ReadLine();
            if (headerLine == null)
                throw new Exception("Le fichier CSV est vide.");

            var headers = headerLine.Split(',');
            if (headers.Length < 2)
                throw new Exception("Le fichier CSV doit avoir au moins deux colonnes (une pour la date, au moins une pour un ticker).");

            try
            {
                // Lecture de chaque ligne de données
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue; // Ignore les lignes vides

                    var values = line.Split(',');
                    if (values.Length != headers.Length)
                        continue; // Ignore les lignes dont le nombre de valeurs ne correspond pas à l'en-tête

                    // Convertit la première colonne en DateTime
                    // Value de base est au format "1985-01-02 00:00:00-05:00"
                    if (!DateTime.TryParseExact(values[0], "yyyy-MM-dd HH:mm:ssK",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                    {
                        Console.WriteLine($"Format de date invalide pour la ligne: {line}");
                        continue;
                    }

                    // Normalise la date (heure mise à zéro)
                    var normalizedDate = date.Date;

                    // Dictionnaire pour stocker les valeurs (ticker -> prix)
                    var rowData = new Dictionary<string, double>();

                    // Parcourt les colonnes de prix
                    for (int i = 1; i < values.Length; i++)
                    {
                        if (double.TryParse(values[i], NumberStyles.Any,
                            CultureInfo.InvariantCulture, out double price))
                        {
                            rowData[headers[i]] = price;
                        }
                        else
                        {
                            Console.WriteLine($"Valeur non numérique trouvée pour la colonne {headers[i]} à la date {normalizedDate}");
                        }
                    }

                    // Ajoute ou met à jour la date normalisée dans le dictionnaire principal
                    _data[normalizedDate] = rowData;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des données : {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Récupère les données comprises entre <paramref name="startDate"/> et <paramref name="endDate"/> incluses.
    /// </summary>
    /// <param name="startDate">Date de début de l'intervalle.</param>
    /// <param name="endDate">Date de fin de l'intervalle.</param>
    /// <returns>
    /// Un dictionnaire dont la clé est la date, et la valeur un dictionnaire (ticker -> prix) 
    /// pour les dates comprises dans l'intervalle spécifié.
    /// </returns>
    public Dictionary<DateTime, Dictionary<string, double>> GetData(DateTime startDate, DateTime endDate)
    {
        var normalizedStartDate = startDate.Date;
        var normalizedEndDate = endDate.Date;

        var result = new Dictionary<DateTime, Dictionary<string, double>>();

        // Parcours des données pour filtrer par date
        // on regarde pour chaque [dictionnaire date, dictionnaire (Ticker,prix) ] si la date est comprise on récupére row
        foreach (var entry in _data)
        {
            if (entry.Key >= normalizedStartDate && entry.Key <= normalizedEndDate)
            {
                result[entry.Key] = entry.Value;
            }
        }

        return result;
    }

    /// <summary>
    /// Récupère la première date (la plus ancienne) présente dans la base de données.
    /// </summary>
    /// <returns>La date la plus ancienne disponible.</returns>
    /// <exception cref="Exception">Lève une exception si aucune donnée n'est disponible.</exception>
    public DateTime GetFirstDate()
    {
        if (_data.Count == 0)
            throw new Exception("Aucune donnée disponible dans la base de données.");

        return _data.Keys.Min();
    }

    /// <summary>
    /// Retourne la date la plus proche dans la base de données qui soit 
    /// strictement antérieure ou égale à <paramref name="targetDate"/>.
    /// Si aucune date n'est antérieure ou égale, on retourne la plus ancienne date disponible.
    /// </summary>
    /// <param name="targetDate">Date pour laquelle on cherche la correspondance la plus proche en revenant en arrière.</param>
    /// <returns>La date trouvée dans la base de données, selon la logique décrite.</returns>
    /// <exception cref="InvalidOperationException">Lève une exception si aucune donnée n'est disponible.</exception>
    public DateTime GetClosestDate(DateTime targetDate)
    {
        if (_data == null || !_data.Any())
            throw new InvalidOperationException("Aucune donnée disponible pour rechercher des dates.");

        // Filtrer les dates qui sont <= targetDate
        var possibleDates = _data.Keys.Where(d => d <= targetDate).ToList();

        // Si on en trouve, on prend la plus récente
        if (possibleDates.Any())
        {
            return possibleDates.Max(); // les dates dans le csv sont déja en jours ouvrés donc en prenant max (d <= targetDate) on est sur
                                        // de tomber sur un jours ouvrés
        }
        else
        {
            // Sinon, aucune date n'est <= targetDate,
            // on prend donc la plus ancienne date (Keys.Min()) ou la première date de la base
            return _data.Keys.Min();
        }
    }

    /// <summary>
    /// Calcule les rendements logarithmiques journaliers pour chaque ticker.
    /// </summary>
    /// <returns>
    /// Un dictionnaire dont la clé est la date, et la valeur un dictionnaire (ticker -> log-return)
    /// pour chaque date où un prix précédent est disponible.
    /// </returns>
    public Dictionary<DateTime, Dictionary<string, double>> CalculateLogReturns()
    {
        var logReturns = new Dictionary<DateTime, Dictionary<string, double>>();

        // Parcourt les dates dans l'ordre croissant
        foreach (var date in _data.Keys.OrderBy(d => d))
        {
            // Trouve la date précédente la plus récente
            var previousDate = _data.Keys
                .Where(d => d < date)
                .OrderByDescending(d => d)
                .FirstOrDefault();

            // S'il n'y a pas de date précédente, on ne peut pas calculer de rendement
            if (previousDate == default)
                continue;

            var currentPrices = _data[date];
            var previousPrices = _data[previousDate];

            var dailyLogReturns = new Dictionary<string, double>();

            foreach (var ticker in currentPrices.Keys)
            {
                if (previousPrices.ContainsKey(ticker))
                {
                    double currentPrice = currentPrices[ticker];
                    double previousPrice = previousPrices[ticker];

                    // S'assure que les prix sont valides pour le calcul
                    if (currentPrice > 0 && previousPrice > 0)
                    {
                        double logReturn = Math.Log(currentPrice / previousPrice);
                        dailyLogReturns[ticker] = logReturn;
                    }
                }
            }

            if (dailyLogReturns.Count > 0)
            {
                logReturns[date] = dailyLogReturns;
            }
        }

        return logReturns;
    }
}

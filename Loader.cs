//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net.Http;
//using System.Reflection;
//using System.Threading.Tasks;
//using OpenQA.Selenium;
//using OpenQA.Selenium.Chrome;
//using YahooFinanceApi;
//using Newtonsoft.Json;
//using OpenQA.Selenium.Support.UI;

//public class StockDataLoader
//{
//    private readonly IWebDriver _driver;
//    private const string secUrl = "https://www.sec.gov/files/company_tickers.json";

//    public StockDataLoader()
//    {
//        // Initialisation du WebDriver Chrome
//        var options = new ChromeOptions();
//        //options.AddArgument("--headless"); // Mode sans interface graphique (décommentez si besoin)
//        options.AddArgument("--disable-gpu");
//        options.AddArgument("--no-sandbox");
//        _driver = new ChromeDriver(options);
//    }

//    public async Task LoadStockDataAsync()
//    {
//        try
//        {
//            // Forcer TLS 1.2 pour HTTPS
//            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

//            // Tenter de définir un User-Agent personnalisé sur l'HttpClient interne de YahooFinanceApi via réflexion
//            SetCustomUserAgent();

//            Console.WriteLine("Chargement des données SEC via Selenium...");

//            _driver.Navigate().GoToUrl(secUrl);
//            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(3));
//            // Récupération de la page source contenant le JSON
//            string pageSource = _driver.PageSource;

//            // Extraction du JSON depuis la page source
//            string jsonData = ExtractJsonFromPage(pageSource);

//            // Désérialisation du JSON en dictionnaire
//            var companyData = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonData);

//            var closePricesDict = new Dictionary<string, List<decimal>>();

//            // Boucle sur chaque entreprise pour récupérer les données boursières
//            foreach (var company in companyData.Values)
//            {
//                // S'assurer que le ticker est en majuscules (Yahoo Finance est sensible à la casse)
//                string ticker = ((string)company["ticker"]).ToUpper();
//                string title = company["title"];

//                Console.WriteLine($"Récupération des données pour: {ticker} - {title}");

//                var fromDate = new DateTime(2022, 1, 1);
//                var toDate = new DateTime(2022, 12, 31);

//                try
//                {
//                    // Récupération des données historiques de Yahoo Finance
//                    var historicalData = await Yahoo.GetHistoricalAsync(ticker, fromDate, toDate, Period.Daily);

//                    if (historicalData.Count > 0)
//                    {
//                        var closePrices = historicalData.Select(price => price.Close).ToList();
//                        closePricesDict[ticker] = closePrices;
//                    }
//                    else
//                    {
//                        Console.WriteLine($"Aucune donnée retournée pour {ticker}.");
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Erreur lors de la récupération de {ticker}: {ex.Message}");
//                }

//                // Pause pour éviter d'être bloqué par Yahoo Finance
//                await Task.Delay(1000);
//            }

//            // Sauvegarde des données dans un fichier CSV
//            WriteDataToCsv(closePricesDict, @"C:\Users\admin\Desktop\cours dauphine\C#\Projet\Database\Univers_bis.csv");
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Erreur lors du chargement des données: {ex.Message}");
//        }
//        finally
//        {
//            _driver.Quit(); // Fermeture du navigateur
//        }
//    }

//    /// <summary>
//    /// Utilise la réflexion pour accéder à l'instance interne de HttpClient de YahooFinanceApi
//    /// et définir un User-Agent personnalisé.
//    /// </summary>
//    private void SetCustomUserAgent()
//    {
//        try
//        {
//            // Chercher le champ statique interne "httpClient" dans la classe Yahoo
//            var yahooType = typeof(Yahoo);
//            var httpClientField = yahooType.GetField("httpClient", BindingFlags.Static | BindingFlags.NonPublic);
//            if (httpClientField != null)
//            {
//                var httpClient = httpClientField.GetValue(null) as HttpClient;
//                if (httpClient != null)
//                {
//                    // Effacer d'éventuels User-Agent existants et en définir un nouveau
//                    httpClient.DefaultRequestHeaders.UserAgent.Clear();
//                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
//                        "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Safari/537.36");
//                    Console.WriteLine("User-Agent personnalisé défini via réflexion.");
//                }
//                else
//                {
//                    Console.WriteLine("Impossible d'obtenir l'instance HttpClient interne.");
//                }
//            }
//            else
//            {
//                Console.WriteLine("Champ HttpClient non trouvé dans YahooFinanceApi.");
//            }
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Erreur lors de la définition du User-Agent: {ex.Message}");
//        }
//    }

//    private string ExtractJsonFromPage(string pageSource)
//    {
//        // Recherche du JSON dans la page source en utilisant la première et dernière accolade
//        var startIdx = pageSource.IndexOf("{");
//        var endIdx = pageSource.LastIndexOf("}");

//        if (startIdx >= 0 && endIdx >= 0)
//        {
//            return pageSource.Substring(startIdx, endIdx - startIdx + 1);
//        }
//        else
//        {
//            throw new Exception("JSON non trouvé dans la page source.");
//        }
//    }

//    private void WriteDataToCsv(Dictionary<string, List<decimal>> data, string filePath)
//    {
//        using (var writer = new StreamWriter(filePath))
//        {
//            writer.WriteLine("Date," + string.Join(",", data.Keys));

//            int maxCount = data.Values.Max(list => list.Count);
//            for (int i = 0; i < maxCount; i++)
//            {
//                // Ici, nous utilisons une date décroissante à partir d'aujourd'hui (à adapter selon vos besoins)
//                var row = new List<string> { DateTime.Now.AddDays(-i).ToShortDateString() };
//                foreach (var closePrices in data.Values)
//                {
//                    row.Add(i < closePrices.Count ? closePrices[i].ToString() : "");
//                }
//                writer.WriteLine(string.Join(",", row));
//            }
//        }
//    }
//}

//class Program
//{
//    static async Task Main(string[] args)
//    {
//        StockDataLoader loader = new StockDataLoader();
//        await loader.LoadStockDataAsync();
//        Console.WriteLine("Données chargées avec succès !");
//        Console.ReadLine();
//    }
//}

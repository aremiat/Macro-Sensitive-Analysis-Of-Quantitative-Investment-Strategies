using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BacktestUI;
using System.Windows.Forms;

/// < summary >
/// Classe principale qui instancie la base de données, définit les périodes et stratégies,
/// puis lance le backtest via la classe Backtest.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        // Chemin complet du fichier CSV source pour les données
        string universFilePath = @"C:\Users\admin\Desktop\cours dauphine\S1\C#\projet\Univers.csv";

        // Instanciation de la base de données.
        Database database = new Database(universFilePath);

        // Définition des périodes à tester.
        var periods = new List<(DateTime start, DateTime end, string label)>
            {
                (new DateTime(2001, 4, 1), new DateTime(2001, 11, 1), "Recession US 1: Avril 2001 - Novembre 2001"),
                (new DateTime(2007, 12, 1), new DateTime(2009, 6, 1), "Recession US 2: Décembre 2007 - Juin 2009"),
                (new DateTime(2020, 2, 1), new DateTime(2020, 4, 1), "Recession US 3: Février 2020 - Avril 2020"),
                (new DateTime(2001, 11, 1), new DateTime(2007, 12, 1), "Expansion US 1: Novembre 2001 – Décembre 2007"),
                (new DateTime(2012, 2, 1), new DateTime(2020, 2, 1), "Expansion US 2: Juin 2009 – Février 2020"),
                (new DateTime(2020, 4, 1), new DateTime(2023, 12, 1), "Expansion US 3: Avril 2020 – Décembre 2023")
            };

        //var periods = new List<(DateTime start, DateTime end, string label)>
        //    {
        //        (new DateTime(1998, 4, 1),  new DateTime(2000, 3, 1),  "Inflation-Up 1: Avril 1998 – Mars 2000"),
        //        (new DateTime(2002, 5, 1),  new DateTime(2003, 3, 1),  "Inflation-Up 2: Mai 2002 – Mars 2003"),
        //        (new DateTime(2004, 2, 1),  new DateTime(2005, 9, 1),  "Inflation-Up 3: Février 2004 – Septembre 2005"),
        //        (new DateTime(2006, 11, 1), new DateTime(2008, 8, 1),  "Inflation-Up 4: Novembre 2006 – Août 2008"),
        //        (new DateTime(2009, 7, 1),  new DateTime(2009, 12, 1), "Inflation-Up 5: Juillet 2009 – Décembre 2009"),
        //        (new DateTime(2010, 11, 1), new DateTime(2011, 9, 1),  "Inflation-Up 6: Novembre 2010 – Septembre 2011"),
        //        (new DateTime(2015, 4, 1),  new DateTime(2018, 7, 1),  "Inflation-Up 7: Avril 2015 – Juillet 2018"),
        //        (new DateTime(2020, 4, 1),  new DateTime(2020, 6, 1),  "Inflation-Up 8: Avril 2020 – Juin 2020"),
        //        (new DateTime(1990, 11, 1), new DateTime(1998, 4, 1),  "Inflation-Down 1: Novembre 1990 – Avril 1998"),
        //        (new DateTime(2000, 3, 1),  new DateTime(2002, 5, 1),  "Inflation-Down 2: Mars 2000 – Mai 2002"),
        //        (new DateTime(2003, 3, 1),  new DateTime(2004, 2, 1),  "Inflation-Down 3: Mars 2003 – Février 2004"),
        //        (new DateTime(2005, 9, 1),  new DateTime(2006, 11, 1), "Inflation-Down 4: Septembre 2005 – Novembre 2006"),
        //        (new DateTime(2008, 8, 1),  new DateTime(2009, 7, 1),  "Inflation-Down 5: Août 2008 – Juillet 2009"),
        //        (new DateTime(2009, 12, 1), new DateTime(2010, 11, 1), "Inflation-Down 6: Décembre 2009 – Novembre 2010"),
        //        (new DateTime(2011, 9, 1),  new DateTime(2015, 4, 1),  "Inflation-Down 7: Septembre 2011 – Avril 2015"),
        //        (new DateTime(2018, 7, 1),  new DateTime(2020, 4, 1),  "Inflation-Down 8: Juillet 2018 – Avril 2020")
        //    };

        // Définition des stratégies à tester.
        var strategies = new List<StrategyBase>
            {
                new MomentumStrategy(database),
                new ValueStrategy(database)
            };

        // Instanciation et exécution du backtest.
        Backtest backtest = new Backtest(database, periods, strategies);
        backtest.Run();

        Console.WriteLine("\nAppuyez sur une touche pour terminer...");
        Console.ReadKey();
    }
}

//public static class Program
//{
//    [STAThread]
//    public static void Main()
//    {
//        Application.EnableVisualStyles();
//        Application.SetCompatibleTextRenderingDefault(false);

//        // Lancement du formulaire principal (BacktestForm)
//        Application.Run(new BacktestForm());
//    }
//}
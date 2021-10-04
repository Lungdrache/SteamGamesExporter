using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SteamGamesExporter.Classes
{
    public class App
    {
        public int appid { get; set; }
        public string name { get; set; }
    }

    public class Applist
    {
        public List<App> apps { get; set; }
        public void FilterList()
        {
            List<App> appsToRemove = new List<App>();
            for (int i = 0; i < apps.Count; i++)
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("  Filter Gamelist: (Searching for invalid Names)");
                Console.WriteLine("  Fortschritt: " + i + "|" + apps.Count);

                if (Regex.IsMatch(apps[i].name, "[^A-Za-z^0-9^-^_^/^\"]"))
                {
                    appsToRemove.Add(apps[i]);
                }
            }
            for (int i = 0; i < appsToRemove.Count; i++)
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("  Filter Gamelist: (Removing Invalid Names)");
                Console.WriteLine("  Fortschritt: " + i + "|" + apps.Count);
                apps.Remove(appsToRemove[i]);
            }
            appsToRemove.Clear();
        }
    }

    public class AllApps
    {
        public Applist applist { get; set; }
    }
}

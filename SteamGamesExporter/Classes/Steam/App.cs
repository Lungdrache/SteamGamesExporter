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
            foreach (App item in apps)
            {
                if (Regex.IsMatch(item.name,"[^A-Za-z^0-9^-^_^/^\"]"))
                {
                    appsToRemove.Add(item);
                }
            }
            foreach (App item in appsToRemove)
            {
                apps.Remove(item);
            }
            appsToRemove.Clear();
        }
    }

    public class AllApps
    {
        public Applist applist { get; set; }
    }
}

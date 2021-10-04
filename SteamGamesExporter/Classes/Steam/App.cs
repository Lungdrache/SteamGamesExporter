using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }

    public class AllApps
    {
        public Applist applist { get; set; }
    }
}

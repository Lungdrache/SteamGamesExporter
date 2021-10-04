using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamGamesExporter.Classes.Steam
{
    public class SteamData
    {
        public string about_the_game { get; set; }
        public string background { get; set; }
        public List<Category> categories { get; set; }
        public ContentDescriptors content_descriptors { get; set; }
        public string detailed_description { get; set; }
        public List<string> developers { get; set; }
        public Fullgame fullgame { get; set; }
        public List<Genre> genres { get; set; }
        public string header_image { get; set; }
        public bool is_free { get; set; }
        public List<object> linux_requirements { get; set; }
        public List<object> mac_requirements { get; set; }
        public string name { get; set; }
        public List<PackageGroup> package_groups { get; set; }
        public List<int> packages { get; set; }
        public PcRequirements pc_requirements { get; set; }
        public Platforms platforms { get; set; }
        public PriceOverview price_overview { get; set; }
        public List<string> publishers { get; set; }
        public ReleaseDate release_date { get; set; }
        public int required_age { get; set; }
        public List<Screenshot> screenshots { get; set; }
        public string short_description { get; set; }
        public int steam_appid { get; set; }
        public SupportInfo support_info { get; set; }
        public string supported_languages { get; set; }
        public string type { get; set; }
        public object website { get; set; }
        public List<Movy> movies { get; set; }
    }

    public class Game
    {
        public bool success { get; set; }
        public SteamData data { get; set; }
    }
    public class SteamRoot
    {
        public Game game { get; set; }
    }
}

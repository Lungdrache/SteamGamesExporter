using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamGamesExporter.Classes.Steam
{
    public class Sub
    {
        public int packageid { get; set; }
        public string percent_savings_text { get; set; }
        public int percent_savings { get; set; }
        public string option_text { get; set; }
        public string option_description { get; set; }
        public string can_get_free_license { get; set; }
        public bool is_free_license { get; set; }
        public int price_in_cents_with_discount { get; set; }
    }
}

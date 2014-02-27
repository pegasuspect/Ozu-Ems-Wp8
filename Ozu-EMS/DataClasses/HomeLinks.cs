using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozu_EMS
{
    public class HomeLinks : IJsonData
    {
        public const string homeDataKey = "HomeLinks";
        public HomeResult[] result { get; set; }

        public Info info { get; set; }        
    }

    public class HomeInfo
    {
        public string requestPath { get; set; }
        public int requestTime { get; set; }
        public int responseStatus { get; set; }
    }

    public class HomeResult
    {
        public string title { get; set; }
        public string slug { get; set; }
        public string description { get; set; }
        public string order { get; set; }
        public string link { get; set; }
    }

}

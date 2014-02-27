using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozu_EMS
{
    public class HomeVersion : IJsonData
    {
        public const string homeVersionKey = "HomeVersion";
        public Info info { get; set; }
        public HomeVersionResult result { get; set; }
    }

    public class HomeVersionInfo
    {
        public string requestPath { get; set; }
        public int requestTime { get; set; }
        public int responseStatus { get; set; }
    }

    public class HomeVersionResult
    {
        public string version { get; set; }
    }

}


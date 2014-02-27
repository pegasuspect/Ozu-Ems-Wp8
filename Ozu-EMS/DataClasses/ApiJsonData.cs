using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozu_EMS
{
    public class ApiJsonData
    {
        public HomeLinks HomeLinks { get; set; }
        public EventsData EventsData { get; set; }
        public HomeVersion HomeVersion { get; set; }
        public ClubsData ClubsData { get; set; }
    }
}

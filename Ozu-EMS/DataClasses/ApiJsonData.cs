using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozu_EMS
{
    public class ApiJsonData
    {
        private EventsData _calendar = null;
        public HomeLinks HomeLinks { get; set; }
        public EventsData EventsData { get; set; }
        public EventsData CalendarData { get {
            if (_calendar == null)
                _calendar = new EventsData();
            return _calendar;
        }
            set {
                _calendar = value;
            }
        }
        public HomeVersion HomeVersion { get; set; }
        public ClubsData ClubsData { get; set; }
        public Dictionary<string, bool> ClubIdIsCheked { get; set; }
    }
}

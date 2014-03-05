using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozu_EMS
{

    public class EventsData : IJsonData
    {
        public const string eventsDataKey = "EventsData";
        public Info info { get; set; }
        public ObservableCollection<EventsResult> result { get; set; }
    }

    public class EventsInfo
    {
        public string requestPath { get; set; }
        public int requestTime { get; set; }
        public int responseStatus { get; set; }
    }

    public class EventsResult
    {
        public string id { get; set; }
        public string event_id { get; set; }
        public string club_id { get; set; }
        public string event_date { get; set; }
        public string address { get; set; }
        public string duration { get; set; }
        public string cover_image { get; set; }
        public string created_at { get; set; }
        public string first_approver_longname { get; set; }
        public string first_approver_email { get; set; }
        public string second_approver_longname { get; set; }
        public string second_approver_email { get; set; }
        public string third_approver_longname { get; set; }
        public string third_approver_email { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string club_name { get; set; }
        public string club_logo { get; set; }
        public string club_cover_image { get; set; }
        public bool active { get; set; }
    }

}
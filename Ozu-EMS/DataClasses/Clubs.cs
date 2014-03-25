using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozu_EMS
{
    public class ClubsData : IJsonData
    {
        public const string clubsDataKey = "ClubsData";
        public const string IsCheckedKey = "IsCheckedKey";
        public Info info { get; set; }
        public List<ClubResult> result { get; set; }
    }

    public class ClubInfo
    {
        public string requestPath { get; set; }
        public int requestTime { get; set; }
        public int responseStatus { get; set; }
    }

    public class ClubResult : INotifyPropertyChanged
    {
        public bool IsChecked { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string id { get; set; }
        public string email { get; set; }
        public string professor_email { get; set; }
        public string manager_email { get; set; }
        public string logo { get; set; }
        public string cover_image { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
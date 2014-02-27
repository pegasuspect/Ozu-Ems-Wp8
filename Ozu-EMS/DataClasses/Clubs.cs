﻿using System;
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
        public Info info { get; set; }
        public ObservableCollection<ClubResult> result { get; set; }
    }

    public class ClubInfo
    {
        public string requestPath { get; set; }
        public int requestTime { get; set; }
        public int responseStatus { get; set; }
    }

    public class ClubResult
    {
        private bool _isChecked;
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                if (_isChecked != value)
                    _isChecked = value;
                NotifyPropertyChanged("IsChecked");
            }
        }
        public string id { get; set; }
        public string slug { get; set; }
        public string name_tr { get; set; }
        public string name_en { get; set; }
        public string description_tr { get; set; }
        public string description_en { get; set; }
        public string logo { get; set; }
        public string cover_image { get; set; }
        public string email { get; set; }
        public string professor_email { get; set; }
        public string manager_email { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

}

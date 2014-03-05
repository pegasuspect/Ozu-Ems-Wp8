using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using Newtonsoft.Json;
using Ozu_EMS.Resources;

namespace Ozu_EMS
{
    public partial class ClubSettings : PhoneApplicationPage
    {
        public ClubSettings()
        {
            InitializeComponent();
        }



        private void StackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ClubSelection.xaml", UriKind.RelativeOrAbsolute));
        }

        private void StackPanel_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/LanguageSelection.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}
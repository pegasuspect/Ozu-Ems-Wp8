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
using Coding4Fun.Toolkit.Controls;

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

        private void StackPanel_Tap_2(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AboutPromptItem[] people = new AboutPromptItem[4];
            //public void Show(string authorName, string twitterName = null, string emailAddress = null, string websiteUrl = null);
            people[0] = new AboutPromptItem() { AuthorName = "Osman Şekerlen", EmailAddress = "osmansekerlen@gmail.com", WebSiteUrl = "https://www.google.com.tr", Role="WP8" };
            people[1] = new AboutPromptItem() { AuthorName = "Doğukan Ergün", EmailAddress = "osmansekerlen@gmail.com", WebSiteUrl = "https://www.google.com.tr", Role = "Android" };
            people[2] = new AboutPromptItem() { AuthorName = "Taha Doğan Güneş", EmailAddress = "osmansekerlen@gmail.com", WebSiteUrl = "https://www.google.com.tr", Role = "iOS" };
            people[3] = new AboutPromptItem() { AuthorName = "Ömer Kala", EmailAddress = "osmansekerlen@gmail.com", WebSiteUrl = "https://www.google.com.tr", Role = "API" };
            AboutPrompt about = new AboutPrompt();
            about.Title = AppResources.AboutButtonText;
            about.VersionNumber = "v" + MainPage.version;
            about.Show(people);
        }
    }
}
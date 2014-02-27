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

namespace Ozu_EMS
{
    public partial class ClubSettings : PhoneApplicationPage
    {
        public ClubSettings()
        {
            InitializeComponent();

            DataContext = MainPage.data;

            Loaded += Settings_Loaded;
        }

        void Settings_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void HomeLinksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector lls = sender as LongListSelector;

            if (lls == null || lls.SelectedItem == null)
                return;

            EmsApi.SaveToPhone(JsonConvert.SerializeObject(MainPage.data.ClubsData), ClubsData.clubsDataKey);

            lls.SelectedItem = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            //TODO: Check for a valid page navigation and to aquire the data.

            Button button = sender as Button;

            NavigationService.Navigate(new Uri("/ClubDetails.xaml?id=" + button.DataContext, UriKind.RelativeOrAbsolute));

        }

        private void FillRectangle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SettinsCheckBoxList.ItemsSource = null;

            if (FillRectangle.Fill == (System.Windows.Media.Brush)Application.Current.Resources["CheckedBrush"])
            {
                foreach (ClubResult clubItem in MainPage.data.ClubsData.result)
                    clubItem.IsChecked = false;
                FillRectangle.Fill = (System.Windows.Media.Brush)Application.Current.Resources["UnCheckedBrush"];
            }
            else
            {
                foreach (ClubResult clubItem in MainPage.data.ClubsData.result)
                    clubItem.IsChecked = true;
                FillRectangle.Fill = (System.Windows.Media.Brush)Application.Current.Resources["CheckedBrush"];
            }

            SettinsCheckBoxList.ItemsSource = MainPage.data.ClubsData.result;

            EmsApi.SaveToPhone(JsonConvert.SerializeObject(MainPage.data.ClubsData), ClubsData.clubsDataKey);

        }
    }
}
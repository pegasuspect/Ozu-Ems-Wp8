using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Ozu_EMS.Resources;

namespace Ozu_EMS
{
    public partial class ClubSelection : PhoneApplicationPage
    {
        public ClubSelection()
        {
            InitializeComponent();

            DataContext = MainPage.data;
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
            TextBlock text = sender as TextBlock;

            if (FillRectangle.Fill == (System.Windows.Media.Brush)Application.Current.Resources["CheckedBrush"])
            {
                foreach (ClubResult clubItem in MainPage.data.ClubsData.result)
                    clubItem.IsChecked = false;
                selectAllText.Text = AppResources.SelectAll;
                FillRectangle.Fill = (System.Windows.Media.Brush)Application.Current.Resources["UnCheckedBrush"];
            }
            else
            {
                foreach (ClubResult clubItem in MainPage.data.ClubsData.result)
                    clubItem.IsChecked = true;
                selectAllText.Text = AppResources.SelectNone;
                FillRectangle.Fill = (System.Windows.Media.Brush)Application.Current.Resources["CheckedBrush"];
            }

            SettinsCheckBoxList.ItemsSource = MainPage.data.ClubsData.result;

            EmsApi.SaveToPhone(JsonConvert.SerializeObject(MainPage.data.ClubsData), ClubsData.clubsDataKey);

        }
    }
}
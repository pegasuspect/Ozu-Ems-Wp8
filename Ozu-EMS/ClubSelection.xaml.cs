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
using System.Collections.ObjectModel;

namespace Ozu_EMS
{
    public partial class ClubSelection : PhoneApplicationPage
    {
        public static bool isInitialized = false;
        public ClubSelection()
        {
            InitializeComponent();

            Loaded += ClubSelection_Loaded;
        }

        private async void ClubSelection_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isInitialized)
            {
                SelectionStackPanel.IsHitTestVisible = false;
                SettinsCheckBoxList.IsHitTestVisible = false;
                EmsApi.StartTrayLoadingAnimation();
                if (MainPage.data.ClubsData != null)
                    MainPage.data.ClubsData = await EmsApi.GetClubsData("", MainPage.EmsLanguage);
                EmsApi.SetProggressIndicatorVisibility(false);

                SelectionStackPanel.IsHitTestVisible = true;
                SettinsCheckBoxList.IsHitTestVisible = true;
                isInitialized = true;
            }

            DataContext = MainPage.data;
        }


        private void HomeLinksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector lls = sender as LongListSelector;

            if (lls == null || lls.SelectedItem == null)
                return;

            ClubResult res = lls.SelectedItem as ClubResult;

            MainPage.data.ClubIdIsCheked[res.id] = res.IsChecked;

            EmsApi.SaveToPhone(JsonConvert.SerializeObject(MainPage.data.ClubsData), ClubsData.clubsDataKey);
            EmsApi.SaveToPhone(JsonConvert.SerializeObject(MainPage.data.ClubIdIsCheked), ClubsData.IsCheckedKey);

            lls.SelectedItem = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Button button = sender as Button;

            NavigationService.Navigate(new Uri("/ClubDetails.xaml?id=" + button.DataContext, UriKind.RelativeOrAbsolute));

        }

        private void FillRectangle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (IsTickButtonChecked())
                ChangeCheckedStatus(false, "GrayButtonBrush");
            else ChangeCheckedStatus(true, "BackButtonBrush");

            EmsApi.SaveToPhone(JsonConvert.SerializeObject(MainPage.data.ClubsData), ClubsData.clubsDataKey);
            EmsApi.SaveToPhone(JsonConvert.SerializeObject(MainPage.data.ClubIdIsCheked), ClubsData.IsCheckedKey);
        }

        private bool IsTickButtonChecked()
        {
            return FillRectangle.Fill == (System.Windows.Media.Brush)Application.Current.Resources["BackButtonBrush"];
        }

        private void ChangeCheckedStatus(bool withValue, string BrushResourceKey)
        {
            foreach (ClubResult clubItem in MainPage.data.ClubsData.result)
            {   
                clubItem.IsChecked = withValue;
                MainPage.data.ClubIdIsCheked[clubItem.id] = clubItem.IsChecked;
            }

            DataContext = null;
            DataContext = MainPage.data;

            selectAllText.Text = withValue ? AppResources.SelectNone : AppResources.SelectAll;
            FillRectangle.Fill = (System.Windows.Media.Brush)Application.Current.Resources[BrushResourceKey];
        }
    }
}
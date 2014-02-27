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
using Microsoft.Phone.Tasks;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;

namespace Ozu_EMS
{
    public partial class Search : PhoneApplicationPage
    {
        private string SearchQuery = "";
        public static EventsData SearchData;
        public Search()
        {
            InitializeComponent();

            searchResultList.PositionChanged +=searchResultList_PositionChanged;

            Loaded += Search_Loaded;
        }

        private void Search_Loaded(object sender, RoutedEventArgs e)
        {
            //searchResultList.PositionChanged += searchResultList_PositionChanged;
        }

        private async void searchResultList_PositionChanged(object sender, EventArgs e)
        {
            ViewportControl viewport = sender as ViewportControl;

            if (viewport.Viewport.Bottom >= viewport.Bounds.Bottom && MainPage._isEventsLoaded
                && viewport.ManipulationState != System.Windows.Controls.Primitives.ManipulationState.Idle)
            {
                searchResultList.ListFooterTemplate = (DataTemplate)Application.Current.Resources["EventsFooterTemplate"];

                ObservableCollection<EventsResult> res = MainPage.data.EventsData.result;
                string baseUrl = EmsApi.getBaseUrl("events", "v1", "search", EmsApi.GetClubIds(), SearchQuery, "", SearchData.result.Count);
                EventsData oldEvents = await EmsApi.getRawResponseAs<EventsData>(baseUrl, "An error occured! Check your connection!");

                //Appending...
                foreach (EventsResult oldResult in oldEvents.result)
                    SearchData.result.Add(oldResult);

                if (oldEvents == null || oldEvents.result.Count < 10)
                    searchResultList.ListFooterTemplate = (DataTemplate)Application.Current.Resources["ReachedLastSearchEventFooterTemplate"];
                else searchResultList.ListHeaderTemplate = null;
            }
        }

        private void LongListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector lls = sender as LongListSelector;

            if (lls == null || lls.SelectedItem == null)
                return;

            EventsResult item = lls.SelectedItem as EventsResult;

            NavigationService.Navigate(new Uri("/EventDetails.xaml?id=" + item.id + "&isSearch=true", UriKind.RelativeOrAbsolute));

            lls.SelectedItem = null;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string rawClubData;
            string withKey = ClubsData.clubsDataKey;
            bool isClubSettingsInMemmory = IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(withKey, out rawClubData);

            string clubIds = EmsApi.GetClubIds();
            SearchQuery = textToBeSearched.Text.ToString();

            EventsData res = await EmsApi.getRawResponseAs<EventsData>(
                EmsApi.getBaseUrl("events", "v1", "search", 
                    clubIds, SearchQuery),
                    "No results! Check your settings and query then try again!"
            );

            if (res.result == null || res.result.Count == 0)
            {
                EmsApi.showToast("No results! Check your settings and query then try again!");
                searchResultList.ListFooterTemplate = null;
            }

            SearchData = res;
            searchResultList.ItemsSource = null;
            searchResultList.ItemsSource = SearchData.result;
        }
    }
}
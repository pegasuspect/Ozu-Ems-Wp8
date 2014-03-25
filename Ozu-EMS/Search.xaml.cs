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

            searchResultList.PositionChanged += searchResultList_PositionChanged;

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
                && viewport.ManipulationState == System.Windows.Controls.Primitives.ManipulationState.Animating)
            {
                searchResultList.ListFooterTemplate = (DataTemplate)Application.Current.Resources["EventsFooterTemplate"];

                ObservableCollection<EventsResult> res = MainPage.data.EventsData.result;
                string baseUrl = EmsApi.getBaseUrl("events", "v1", "search", EmsApi.GetClubIds(), SearchQuery, "", SearchData.result.Count);
                EventsData oldEvents = await EmsApi.getRawResponseAs<EventsData>(baseUrl);

                EmsApi.prettyDisplayDates(oldEvents.result);

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
            searchResultList.ListFooterTemplate = null;

            string clubIds = EmsApi.GetClubIds();
            SearchQuery = textToBeSearched.Text.ToString();

            AddInfoTemplate("EventsFooterTemplate");

            string url = EmsApi.getBaseUrl("events", "v1", "search", "", SearchQuery, "", 0, "", "", MainPage.EmsLanguage);
            EventsData res = await EmsApi.getRawResponseAs<EventsData>(url);

            if (res == null || res.result == null || res.result.Count == 0)
            {
                AddInfoTemplate("NoSearchEventFooterTemplate");
                return;
            }

            EmsApi.prettyDisplayDates(res.result);

            AddInfoTemplate("ListTemplate", true, res.result);

            //To reach it from details page.
            SearchData = res;
        }

        private void AddInfoTemplate(string templateKey, bool isHitVisible = false, ObservableCollection<EventsResult> itemSource = null)
        {
            if (itemSource == null)
                searchResultList.ItemsSource = new List<EventsResult>() { new EventsResult() };
            else searchResultList.ItemsSource = itemSource;
            searchResultList.ItemTemplate = (DataTemplate)Application.Current.Resources[templateKey];
            searchResultList.IsHitTestVisible = isHitVisible;
        }
    }
}
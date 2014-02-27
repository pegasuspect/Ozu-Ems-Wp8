using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Ozu_EMS.Resources;
using Microsoft.Phone.Tasks;
using Coding4Fun.Toolkit.Controls;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace Ozu_EMS
{
    public partial class MainPage : PhoneApplicationPage
    {
        public static ApiJsonData data;
        public static bool _isEventsLoaded = false;
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            Loaded += MainPage_Loaded;

            EventList.PositionChanged += EventList_PositionChanged;

            // Sample code to localize the ApplicationBar
            BuildLocalizedApplicationBar();
        }

        public async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                SystemTray.ProgressIndicator = new ProgressIndicator();
                SetProggressIndicator(true);
                SystemTray.ProgressIndicator.Text = AppResources.LoadingMessage;

                data = new ApiJsonData();
                data.ClubsData = await EmsApi.GetClubsData();
                data.EventsData = await EmsApi.GetEventsInfo();
                data.HomeLinks = await EmsApi.GetHomeLinks();

                //Set last button.
                HomeLinksList.ItemsSource = data.HomeLinks.result.Take<HomeResult>(9).ToList<HomeResult>();
                HomeResult[] last = { data.HomeLinks.result[data.HomeLinks.result.Length - 1] };
                LastLink.ItemsSource = last;

                //Reset the header and the footer of the events list
                EventList.ListHeaderTemplate = null;
                EventList.ListFooterTemplate = null;

                //Load everything to application.
                DataContext = data;

                SetProggressIndicator(false);

                _isEventsLoaded = true;
            }
            catch (Exception ex)
            {
                EmsApi.showToast(ex.Message);
            }


        }

        private async void EventList_PositionChanged(object sender, EventArgs e)
        {
            ViewportControl viewport = sender as ViewportControl;

            if (viewport.Viewport.Bottom >= viewport.Bounds.Bottom && MainPage._isEventsLoaded
                && viewport.ManipulationState != System.Windows.Controls.Primitives.ManipulationState.Idle)
            {
                
                //Loading...
                EventList.ListFooterTemplate = (DataTemplate)Application.Current.Resources["EventsFooterTemplate"];
                
                ObservableCollection<EventsResult> res = MainPage.data.EventsData.result;
                string baseUrl = EmsApi.getBaseUrl("events", "v1", "after", EmsApi.GetClubIds(), "", (res[res.Count - 1] as EventsResult).id);
                EventsData oldEvents = await EmsApi.getRawResponseAs<EventsData>(baseUrl, "An error occured! Check your connection!");

                //Appending...
                foreach (EventsResult oldResult in oldEvents.result)
                    MainPage.data.EventsData.result.Add(oldResult);

                //Displaying the footer acording to data obtained.
                if (oldEvents == null || oldEvents.result.Count < 10)
                    EventList.ListFooterTemplate = (DataTemplate)Application.Current.Resources["ReachedLastEventFooterTemplate"];
                else EventList.ListHeaderTemplate = null;
            }

            if (viewport.Viewport.Top <= viewport.Bounds.Top && MainPage._isEventsLoaded
                && viewport.ManipulationState != System.Windows.Controls.Primitives.ManipulationState.Idle)
            {
                //Loading...
                EventList.ListHeaderTemplate = (DataTemplate)Application.Current.Resources["EventsHeaderTemplate"];

                ObservableCollection<EventsResult> res = MainPage.data.EventsData.result;
                string baseUrl = EmsApi.getBaseUrl("events", "v1", "before", EmsApi.GetClubIds(), "", (res[0] as EventsResult).id);
                EventsData oldEvents = await EmsApi.getRawResponseAs<EventsData>(baseUrl, "An error occured! Check your connection!");

                //Appending to the top of the list...
                foreach (EventsResult oldResult in oldEvents.result)
                    MainPage.data.EventsData.result.Insert(0, oldResult);

                //Displaying the header acording to data obtained.
                if (oldEvents == null || oldEvents.result.Count < 10)
                    EventList.ListHeaderTemplate = (DataTemplate)Application.Current.Resources["ReachedFirstEventHeaderTemplate"];
                else EventList.ListHeaderTemplate = null;
            }
        }

        private void SetProggressIndicator(bool isVisible)
        {
            SystemTray.ProgressIndicator.IsIndeterminate = isVisible;
            SystemTray.ProgressIndicator.IsVisible = isVisible;
        }

        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create settings button.
            ApplicationBarIconButton settingsAppBarButton = new ApplicationBarIconButton(new Uri("/Assets/icons/settings.png", UriKind.Relative));
            settingsAppBarButton.Text = AppResources.SettingsButtonText;
            settingsAppBarButton.Click += settingsAppBarButton_Click;
            ApplicationBar.Buttons.Add(settingsAppBarButton);

            // Create search button.
            ApplicationBarIconButton searchAppBarButton = new ApplicationBarIconButton(new Uri("/Assets/icons/search.png", UriKind.Relative));
            searchAppBarButton.Text = AppResources.SearchButtonText;
            searchAppBarButton.Click += searchAppBarButton_Click;
            ApplicationBar.Buttons.Add(searchAppBarButton);

            ApplicationBar.Mode = ApplicationBarMode.Minimized;
            ApplicationBar.IsVisible = false;

            //// Create a new menu item with the localized string from AppResources.
            //ApplicationBarMenuItem appBarMenuItem = 
            //    new ApplicationBarMenuItem(AppResources.SettingsButtonText);
            //ApplicationBar.MenuItems.Add(appBarMenuItem);
        }

        private void searchAppBarButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Search.xaml", UriKind.RelativeOrAbsolute));
        }

        private void settingsAppBarButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.RelativeOrAbsolute));
        }

        private void HomeLinksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector lls = sender as LongListSelector;

            if (lls == null || lls.SelectedItem == null)
                return;

            HomeResult tile = lls.SelectedItem as HomeResult;

            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri(tile.link);
            webBrowserTask.Show();

            lls.SelectedItem = null;
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot item = sender as Pivot;

            if (item.SelectedIndex == 1)
            {
                try
                {
                    if (MainPage.data.EventsData.result.Count == 0)
                        EmsApi.showToast("No events! Check your settings for club filters!");
                }
                catch (Exception)
                {
                }

                ApplicationBar.IsVisible = true;
                return;
            }
            ApplicationBar.IsVisible = false;
        }

        private void LongListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector lls = sender as LongListSelector;

            if (lls == null || lls.SelectedItem == null)
                return;

            EventsResult item = lls.SelectedItem as EventsResult;

            NavigationService.Navigate(new Uri("/EventDetails.xaml?id=" + item.id, UriKind.RelativeOrAbsolute));

            lls.SelectedItem = null;
        }

    }
}
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
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using Newtonsoft.Json;

namespace Ozu_EMS
{
    public partial class MainPage : PhoneApplicationPage
    {
        public static ApiJsonData data;
        public static bool _isEventsLoaded = false;
        public static string version = "1.0";
        public static EmsApi.Languages EmsLanguage = EmsApi.Languages.defaultLanguage;
        private static ApplicationBarIconButton searchAppBarButton;
        private static ApplicationBarIconButton settingsAppBarButton;
        private bool isLoaded = false;        

        public MainPage()
        {
            InitializeComponent();

            initLanguage();

            Loaded += MainPage_Loaded;

            EventList.PositionChanged += EventList_PositionChanged;

            BuildLocalizedApplicationBar();
        }

        public async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _isEventsLoaded = false;
                EventList.IsEnabled = false;

                LoadingStarted();

                if (!isLoaded)
                {
                    await initApplication();
                    isLoaded = true;
                }
                else
                {
                    //Reset the header and the footer of the events list
                    EventList.ListHeaderTemplate = null;
                    EventList.ListFooterTemplate = null;

                    await updateEventsWithLanguage();

                    updateButtonTexts();
                }

                LoadingEnd();

                EventList.IsEnabled = true;
                _isEventsLoaded = true;

                //RaisePropertyChanged(() => data.EventsData.result);
                // data.EventsData.result
            }
            catch (Exception ex)
            {
                SetProggressIndicator(false);

                EventList.IsEnabled = true;
                _isEventsLoaded = true;

                EmsApi.showToast("Main Page: " + ex.Message);
            }


        }

        public static void LoadingEnd()
        {
            SetProggressIndicator(false);
        }

        public static void LoadingStarted()
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
            SetProggressIndicator(true);
            SystemTray.ProgressIndicator.Text = AppResources.LoadingMessage;
        }

        public static async Task updateClubssWithLanguage()
        {
            int length = data.ClubsData.result.Count;
            bool[] isCheckeds = new bool[length];

            for (int i = 0; i < length; i++)
			     isCheckeds[i] = data.ClubsData.result[i].IsChecked;
            
            data.ClubsData.result.Clear();

            ClubsData eventsResultsData = await EmsApi.GetClubsData("", EmsLanguage, true);

            foreach (ClubResult result in eventsResultsData.result)
                data.ClubsData.result.Add(result);

            for (int i = 0; i < length; i++)
                data.ClubsData.result[i].IsChecked = isCheckeds[i];
        }

        public static async Task updateEventsWithLanguage()
        {
            data.EventsData.result.Clear();

            EventsData eventsResultsData = await EmsApi.GetEventsInfo("", EmsLanguage);

            foreach (EventsResult result in eventsResultsData.result)
                data.EventsData.result.Add(result);
        }

        private async Task initApplication()
        {
            data = new ApiJsonData();
            data.ClubsData = await EmsApi.GetClubsData();
            data.EventsData = await EmsApi.GetEventsInfo("", EmsLanguage);
            data.HomeLinks = await EmsApi.GetHomeLinks();

            //Set last button.
            HomeLinksList.ItemsSource = data.HomeLinks.result.Take<HomeResult>(9).ToList<HomeResult>();
            HomeResult[] last = { data.HomeLinks.result[data.HomeLinks.result.Length - 1] };
            LastLink.ItemsSource = last;

            //Load everything to application.
            DataContext = data;

            if (data.EventsData != null)
                prettyDisplayDates();
        }

        private static void initLanguage()
        {
            string rawLanguageSettings;
            bool inPhoneMemmory = IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(LanguageSelection.languageKey, out rawLanguageSettings);
            if (inPhoneMemmory)
            {
                EmsLanguage = JsonConvert.DeserializeObject<EmsApi.Languages>(rawLanguageSettings);
                LanguageSelection.changeLanguageTo(EmsLanguage);
            }
        }

        private void prettyDisplayDates()
        {
            foreach (EventsResult item in EventList.ItemsSource as ObservableCollection<EventsResult>)
            {
                item.event_date = DateTime.Parse(item.event_date).ToLongDateString();
            }
        }

        private async void EventList_PositionChanged(object sender, EventArgs e)
        {
            ViewportControl viewport = sender as ViewportControl;
            if (MainPage._isEventsLoaded && viewport.ManipulationState != System.Windows.Controls.Primitives.ManipulationState.Idle)
            {
                ObservableCollection<EventsResult> res = MainPage.data.EventsData.result;

                if (viewport.Viewport.Bottom >= viewport.Bounds.Bottom)
                {
                    //Loading...
                    EventList.ListFooterTemplate = (DataTemplate)Application.Current.Resources["EventsFooterTemplate"];

                    string baseUrl = EmsApi.getBaseUrl("events", "v1", "after", EmsApi.GetClubIds(), "", (res[res.Count - 1] as EventsResult).id);
                    EventsData freshEventsList = await EmsApi.getRawResponseAs<EventsData>(baseUrl);

                    //Appending...
                    foreach (EventsResult item in freshEventsList.result)
                        MainPage.data.EventsData.result.Add(item);

                    prettyDisplayDates();

                    //Displaying the footer acording to data obtained.
                    if (freshEventsList == null || freshEventsList.result.Count < 10)
                        EventList.ListFooterTemplate = (DataTemplate)Application.Current.Resources["ReachedLastEventFooterTemplate"];
                    else EventList.ListHeaderTemplate = null;
                }

                if (viewport.Viewport.Top <= viewport.Bounds.Top)
                {
                    //Loading...
                    EventList.ListHeaderTemplate = (DataTemplate)Application.Current.Resources["EventsHeaderTemplate"];

                    string baseUrl = EmsApi.getBaseUrl("events", "v1", "before", EmsApi.GetClubIds(), "", (res[0] as EventsResult).id);
                    EventsData oldEvents = await EmsApi.getRawResponseAs<EventsData>(baseUrl);

                    //Appending to the top of the list...
                    foreach (EventsResult oldResult in oldEvents.result)
                        MainPage.data.EventsData.result.Insert(0, oldResult);

                    prettyDisplayDates();

                    //Displaying the header acording to data obtained.
                    if (oldEvents == null || oldEvents.result.Count < 10)
                        EventList.ListHeaderTemplate = (DataTemplate)Application.Current.Resources["ReachedFirstEventHeaderTemplate"];
                    else EventList.ListHeaderTemplate = null;
                }                
            }
        }

        private static void SetProggressIndicator(bool isVisible)
        {
            SystemTray.ProgressIndicator.IsIndeterminate = isVisible;
            SystemTray.ProgressIndicator.IsVisible = isVisible;
        }

        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            searchAppBarButton = new ApplicationBarIconButton(new Uri("/Assets/icons/search.png", UriKind.Relative));
            settingsAppBarButton = new ApplicationBarIconButton(new Uri("/Assets/icons/settings.png", UriKind.Relative));

            updateButtonTexts();

            // Add settings button.
            settingsAppBarButton.Click += settingsAppBarButton_Click;
            ApplicationBar.Buttons.Add(settingsAppBarButton);

            // Add search button.
            searchAppBarButton.Click += searchAppBarButton_Click;
            ApplicationBar.Buttons.Add(searchAppBarButton);

            ApplicationBar.Mode = ApplicationBarMode.Minimized;
            ApplicationBar.IsVisible = false;

            //// Create a new menu item with the localized string from AppResources.
            //ApplicationBarMenuItem appBarMenuItem = 
            //    new ApplicationBarMenuItem(AppResources.SettingsButtonText);
            //ApplicationBar.MenuItems.Add(appBarMenuItem);
        }


        public static void updateButtonTexts()
        {
            settingsAppBarButton.Text = AppResources.SettingsButtonText;
            searchAppBarButton.Text = AppResources.SearchButtonText;
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
                try {
                    if (MainPage.data.EventsData.result.Count == 0)
                        MainPage.data.EventsData.result.Add(new EventsResult() { name = "No Events!", event_date = "Check your settings for club filters!" });
                } catch { }

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
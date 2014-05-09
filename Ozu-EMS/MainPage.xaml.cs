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
using Microsoft.Phone.UserData;

namespace Ozu_EMS
{
    public partial class MainPage : PhoneApplicationPage
    {
        public static bool IsOnDebug = true;
        public static ApiJsonData data = new ApiJsonData();
        public static bool _isEventsLoaded = false;
        public static string version = "1.0";
        public static EmsApi.Languages EmsLanguage = EmsApi.Languages.defaultLanguage;

        private static ApplicationBarIconButton searchAppBarButton;
        private static ApplicationBarIconButton settingsAppBarButton;
        private bool isLoaded = false;

        public MainPage()
        {
            InitializeComponent();

            Loaded += MainPage_Loaded;

            EventList.PositionChanged += EventList_PositionChanged;

            BuildLocalizedApplicationBar();

            ThemeManager.ToLightTheme();
        }
        public async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!isLoaded)
                {
                    initLanguage();
                    LoadingStarted();
                    await initApplication();
                    isLoaded = true;
                }
                else
                {
                    ApplicationBar.IsVisible = false;
                    LoadingStarted();
                    await updateEventsWithLanguage();
                    await updateClubsWithLanguage();
                    updateButtonTexts();
                    ApplicationBar.IsVisible = MainPagePivotView.SelectedIndex != 0;
                }
                LoadingEnd();
            }
            catch (Exception ex)
            {
                EmsApi.Log(ex.Message);
                LoadingEnd();
            }
        }

        public void LoadingEnd()
        {
            EmsApi.SetProggressIndicatorVisibility(false);
            EventList.IsEnabled = true;
            _isEventsLoaded = true;
            ClubSelection.isInitialized = true;
        }
        public void LoadingStarted()
        {
            EmsApi.StartTrayLoadingAnimation();
            _isEventsLoaded = false;
            EventList.IsEnabled = false;

            //Reset the header and the footer of the events list
            EventList.ListHeaderTemplate = null;
            EventList.ListFooterTemplate = null;
        }

        private async Task updateEventsWithLanguage()
        {
            EventList.ItemsSource = null;
            data.EventsData = await EmsApi.GetEventsInfo("", EmsLanguage);
            EventList.ItemsSource = data.EventsData.result;
            EmsApi.prettyDisplayDates(data.EventsData.result);
        }
        private async Task updateClubsWithLanguage()
        {
            ClubsList.ItemsSource = null;
            ClubsList.IsHitTestVisible = false;
            EmsApi.StartTrayLoadingAnimation();
            if (data.ClubsData != null)
                data.ClubsData = await EmsApi.GetClubsData("", EmsLanguage);
            EmsApi.SetProggressIndicatorVisibility(false);
            ClubsList.ItemsSource = data.ClubsData.result;
            ClubsList.IsHitTestVisible = true;
        }

        private void initLanguage()
        {
            string rawLanguageSettings;
            bool inPhoneMemmory = IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(LanguageSelection.languageKey, out rawLanguageSettings);
            if (inPhoneMemmory)
            {
                EmsLanguage = JsonConvert.DeserializeObject<EmsApi.Languages>(rawLanguageSettings);
                LanguageSelection.changeLanguageTo(EmsLanguage);
            }
            else LanguageSelection.changeLanguageTo(EmsApi.Languages.tr);
        }
        private async Task initApplication()
        {
            data.ClubIdIsCheked = EmsApi.GetClubsIdIsChecked();
            data.CalendarData = EmsApi.GetCalendarData();
            resetCalendar();
            data.ClubsData = await EmsApi.GetClubsData("", EmsLanguage);
            data.EventsData = await EmsApi.GetEventsInfo("", EmsLanguage);
            data.HomeLinks = await EmsApi.GetHomeLinks();

            DataContext = data;

            EmsApi.prettyDisplayDates(data.EventsData.result);
        }

        private void resetCalendar()
        {
            foreach (EventsResult item in data.CalendarData.result)
                checkEvent(item);
        }
        private void checkEvent(EventsResult res)
        {
            Appointments appts = new Appointments();

            //Identify the method that runs after the asynchronous search completes.
            appts.SearchCompleted += appts_SearchCompleted;

            DateTime start = DateTime.Parse(res.event_date);
            DateTime end = DateTime.Parse(res.event_date).Add(TimeSpan.FromHours(double.Parse(res.duration)));
            int max = 20;

            //Start the asynchronous search.
            appts.SearchAsync(start, end, max, res);
        }
        void appts_SearchCompleted(object sender, AppointmentsSearchEventArgs e)
        {
            EventsResult res = e.State as EventsResult;
            double hours = double.Parse(res.duration);
            DateTime original = DateTime.Parse(res.event_date);
            original = original.AddSeconds(-original.Second);
            string desc = System.Text.RegularExpressions.Regex.Replace(res.description, "(?<!\r)\n", "\r\n");

            if (e.Results.Count() != 0)
            {
                foreach (Appointment appointment in e.Results)
                {
                    if (appointment.StartTime == original
                        && appointment.EndTime == original.Add(TimeSpan.FromHours(hours))
                        && appointment.Subject == res.name
                        && appointment.Location == res.address
                        && appointment.Details.Length == desc.Length)
                    {
                        res.isInCalendar = true;
                    }
                }
            }
            if (!res.isInCalendar)
            {
                data.CalendarData.result.Remove(res);
                EmsApi.SaveToPhone(JsonConvert.SerializeObject(data.CalendarData), EventsData.calendarDataKey);
            }

        }

        private async void EventList_PositionChanged(object sender, EventArgs e)
        {
            ViewportControl viewport = sender as ViewportControl;
            if (MainPage._isEventsLoaded && viewport.ManipulationState == System.Windows.Controls.Primitives.ManipulationState.Animating)
            {
                ObservableCollection<EventsResult> res = MainPage.data.EventsData.result;

                if (viewport.Viewport.Bottom >= viewport.Bounds.Bottom)
                {
                    //Loading...
                    EventList.ListFooterTemplate = (DataTemplate)Application.Current.Resources["EventsFooterTemplate"];

                    string baseUrl = EmsApi.getBaseUrl("events", "v1", "after", EmsApi.GetClubIds(), "", (res[res.Count - 1] as EventsResult).id, 0, "", "", EmsLanguage);
                    EventsData freshEventsList = await EmsApi.getRawResponseAs<EventsData>(baseUrl);

                    EmsApi.prettyDisplayDates(freshEventsList.result);

                    //Appending...
                    foreach (EventsResult item in freshEventsList.result)
                        MainPage.data.EventsData.result.Add(item);

                    //Displaying the footer acording to data obtained.
                    if (freshEventsList == null || freshEventsList.result.Count < 10)
                        EventList.ListFooterTemplate = (DataTemplate)Application.Current.Resources["ReachedLastEventFooterTemplate"];
                    else EventList.ListHeaderTemplate = null;
                }

                if (viewport.Viewport.Top <= viewport.Bounds.Top)
                {
                    //Loading...
                    EventList.ListHeaderTemplate = (DataTemplate)Application.Current.Resources["EventsHeaderTemplate"];

                    string baseUrl = EmsApi.getBaseUrl("events", "v1", "before", EmsApi.GetClubIds(), "", (res[0] as EventsResult).id, 0, "", "", EmsLanguage);
                    EventsData oldEvents = await EmsApi.getRawResponseAs<EventsData>(baseUrl);

                    EmsApi.prettyDisplayDates(oldEvents.result);

                    //Appending to the top of the list...
                    foreach (EventsResult oldResult in oldEvents.result)
                        MainPage.data.EventsData.result.Insert(0, oldResult);

                    //Displaying the header acording to data obtained.
                    if (oldEvents == null || oldEvents.result.Count < 10)
                        EventList.ListHeaderTemplate = (DataTemplate)Application.Current.Resources["ReachedFirstEventHeaderTemplate"];
                    else EventList.ListHeaderTemplate = null;
                }
            }
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

            NavigationService.Navigate(new Uri("/LinkerBrowser.xaml?url=" + tile.link, UriKind.RelativeOrAbsolute));

            lls.SelectedItem = null;
        }
        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot item = sender as Pivot;

            ApplicationBar.IsVisible = item.SelectedIndex != 0;
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
        private void ClubList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector lls = sender as LongListSelector;

            if (lls == null || lls.SelectedItem == null)
                return;

            ClubResult item = lls.SelectedItem as ClubResult;

            NavigationService.Navigate(new Uri("/ClubDetails.xaml?id=" + item.id, UriKind.RelativeOrAbsolute));

            lls.SelectedItem = null;
        }
        private void CalendarsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector lls = sender as LongListSelector;

            if (lls == null || lls.SelectedItem == null)
                return;

            EventsResult item = lls.SelectedItem as EventsResult;

            NavigationService.Navigate(new Uri("/EventDetails.xaml?id=" + item.id + "&isCalendar=true", UriKind.RelativeOrAbsolute));

            lls.SelectedItem = null;
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/LinkerBrowser.xaml?url=" + data.HomeLinks.result[9].link, UriKind.RelativeOrAbsolute));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.UserData;
using Ozu_EMS.Resources;
using System.Globalization;

namespace Ozu_EMS
{
    public partial class EventDetails : PhoneApplicationPage
    {
        private string _eventId;
        private bool _isSearch;
        private bool _isCalendar;
        public EventDetails()
        {
            InitializeComponent();

            Loaded += EventDetails_Loaded;
        }

        void EventDetails_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isSearch)
                DataContext = (EventsResult)Search.SearchData.result.Where<EventsResult>(item => item.id == _eventId).First<EventsResult>();
            else if (_isCalendar)
                DataContext = (EventsResult)MainPage.data.CalendarData.result.Where<EventsResult>(item => item.id == _eventId).First<EventsResult>(); 
            else DataContext = (EventsResult)MainPage.data.EventsData.result.Where<EventsResult>(item => item.id == _eventId).First<EventsResult>(); 

            EventsResult res = DataContext as EventsResult;
            reorginizeDates(res);
        }

        private void reorginizeDates(EventsResult res)
        {
            CultureInfo culture = new CultureInfo("en-US");
            double hours = double.Parse(res.duration, culture);

            Duration.Content = EmsApi.GetTimeSpan(DateTime.Now.AddHours(-hours), true);

            CreatedAt.Content = DateTime.Parse(res.created_at).ToLongDateString();

            Date.Content = DateTime.Parse(res.event_date).ToLongDateString();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _eventId = NavigationContext.QueryString["id"].ToString();
            if (!string.IsNullOrWhiteSpace(_eventId))
            {
                try
                {
                    string isSearch = NavigationContext.QueryString["isSearch"].ToString();
                    if (!string.IsNullOrWhiteSpace(isSearch))
                        _isSearch = bool.Parse(isSearch);
                }
                catch { }
                try
                {
                    string isCalendar = NavigationContext.QueryString["isCalendar"].ToString();
                    if (!string.IsNullOrWhiteSpace(isCalendar))
                        _isCalendar = bool.Parse(isCalendar);
                    CalendarAddButton.Visibility = System.Windows.Visibility.Collapsed;
                }
                catch { }
            }
        }

        private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double height = this.title.ActualHeight;
            Thickness margin = this.title.Margin;
            this.title.Margin = new Thickness(margin.Left, -height, margin.Right, margin.Bottom);
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText((sender as HyperlinkButton).Content.ToString());
            EmsApi.showToast(AppResources.CopyMessage);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EventsResult res = DataContext as EventsResult;

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
                        MessageBox.Show(AppResources.AlreadyInTheCalendar, AppResources.InfoTitle, MessageBoxButton.OK);
                        return;
                    }
                }

                if (MessageBox.Show(AppResources.CalendarPrompt, AppResources.CalenderPromptTitle, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    return;
            }

            SaveAppointmentTask saveAppointmentTask = new SaveAppointmentTask();

            saveAppointmentTask.StartTime = original;
            saveAppointmentTask.EndTime = original.Add(TimeSpan.FromHours(hours));
            saveAppointmentTask.Subject = res.name;
            saveAppointmentTask.Location = res.address;
            saveAppointmentTask.Details = res.description;
            saveAppointmentTask.IsAllDayEvent = false;
            saveAppointmentTask.Reminder = Reminder.EighteenHours;
            saveAppointmentTask.AppointmentStatus = Microsoft.Phone.UserData.AppointmentStatus.Busy;            

            saveAppointmentTask.Show();

            MainPage.data.CalendarData.result.Add(res);
            EmsApi.SaveToPhone(Newtonsoft.Json.JsonConvert.SerializeObject(MainPage.data.CalendarData), EventsData.calendarDataKey);
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            scrollViewForDesc.IsEnabled = (scrollViewForDesc.ExtentHeight < textBlock.ActualHeight ? true : false);
        }

    }
}
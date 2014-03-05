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

namespace Ozu_EMS
{
    public partial class EventDetails : PhoneApplicationPage
    {
        private string _eventId;
        private bool _isSearch;
        public EventDetails()
        {
            InitializeComponent();

            Loaded += EventDetails_Loaded;
        }

        void EventDetails_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isSearch)
                DataContext = (EventsResult)MainPage.data.EventsData.result.Where<EventsResult>(item => item.id == _eventId).First<EventsResult>();
            else DataContext = (EventsResult)Search.SearchData.result.Where<EventsResult>(item => item.id == _eventId).First<EventsResult>();

            EventsResult res = DataContext as EventsResult;
            double hours = double.Parse(res.duration);

            string hourText = AppResources.EventDetailsHour;
            string minutesText = AppResources.EventDetailsMinutes;
            string pluralSuffix = "s";

            if (AppResources.ResourceLanguage.StartsWith("tr"))
                pluralSuffix = "";
            
            if (hours == Math.Floor(hours))
                if (hours > 1)
                    Duration.Content = TimeSpan.FromHours(hours).ToString("%h' " + hourText + pluralSuffix +"'");
                else Duration.Content = TimeSpan.FromHours(hours).ToString("%h' " + hourText + pluralSuffix + "'");
            else Duration.Content = TimeSpan.FromHours(hours).ToString("%h' " + hourText + pluralSuffix + " '%m' " + minutesText + "'");

            CreatedAt.Content = DateTime.Parse(res.created_at).ToLongDateString();
                
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _eventId = NavigationContext.QueryString["id"];
            try
            {
                string isSearch = NavigationContext.QueryString["isSearch"];
                if (!string.IsNullOrWhiteSpace(isSearch))
                    _isSearch = bool.Parse(isSearch);
            }
            catch (Exception)
            {

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
            EmsApi.showToast("Copied to clipboard!");
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

            if (e.Results.Count() != 0)
            {
                foreach (Appointment appointment in e.Results)
                {
                    if(appointment.Details == res.description
                        && appointment.Subject == res.name
                        && appointment.Location == res.address)
                    {
                        EmsApi.showToast("You already added this event!");
                        return;
                    }
                }
            }

            SaveAppointmentTask saveAppointmentTask = new SaveAppointmentTask();

            saveAppointmentTask.StartTime = DateTime.Parse(res.event_date);
            saveAppointmentTask.EndTime = DateTime.Parse(res.event_date).Add(TimeSpan.FromHours(hours));
            saveAppointmentTask.Subject = res.name;
            saveAppointmentTask.Location = res.address;
            saveAppointmentTask.Details = res.description;
            saveAppointmentTask.IsAllDayEvent = false;
            saveAppointmentTask.Reminder = Reminder.EighteenHours;
            saveAppointmentTask.AppointmentStatus = Microsoft.Phone.UserData.AppointmentStatus.Busy;

            saveAppointmentTask.Show();
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            scrollViewForDesc.IsEnabled = (scrollViewForDesc.ExtentHeight < textBlock.ActualHeight ? true : false);
        }

    }
}
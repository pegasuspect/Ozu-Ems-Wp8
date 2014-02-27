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
            SaveAppointmentTask saveAppointmentTask = new SaveAppointmentTask();

            EventsResult res = DataContext as EventsResult;
            res.duration = res.duration.Replace('.', ':');

            saveAppointmentTask.StartTime = DateTime.Parse(res.event_date);
            saveAppointmentTask.EndTime = DateTime.Parse(res.event_date).Add(TimeSpan.Parse(res.duration));
            saveAppointmentTask.Subject = res.name_tr;
            saveAppointmentTask.Location = res.address;
            saveAppointmentTask.Details = res.description_tr;
            saveAppointmentTask.IsAllDayEvent = false;
            saveAppointmentTask.Reminder = Reminder.EighteenHours;
            saveAppointmentTask.AppointmentStatus = Microsoft.Phone.UserData.AppointmentStatus.Busy;

            saveAppointmentTask.Show();
        }

    }
}
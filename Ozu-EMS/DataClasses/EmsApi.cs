using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Ozu_EMS.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ozu_EMS
{
    public class EmsApi : PhoneApplicationPage
    {
        public static Dictionary<string, bool> GetClubsIdIsChecked()
        {
            string rawApiResponse;
            string withKey = ClubsData.IsCheckedKey;
            bool inPhoneMemmory = IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(withKey, out rawApiResponse);

            return inPhoneMemmory ? JsonConvert.DeserializeObject<Dictionary<string, bool>>(rawApiResponse) : new Dictionary<string,bool>();
        }

        public static EventsData GetCalendarData()
        {
            EventsData newData = new EventsData() { info = new Info(), result = new ObservableCollection<EventsResult>() };
            string rawApiResponse;
            string withKey = EventsData.calendarDataKey;
            bool inPhoneMemmory = IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(withKey, out rawApiResponse);

            return inPhoneMemmory ? JsonConvert.DeserializeObject<EventsData>(rawApiResponse) : newData;
        }
        public async static Task<HomeLinks> GetHomeLinks(string emsApiKey = "")
        {

            string rawApiResponse;
            string withKey = HomeLinks.homeDataKey;
            bool inPhoneMemmory = IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(withKey, out rawApiResponse);
            bool homeIsUpToDate = await IsHomeVersionUpToDate();

            if (homeIsUpToDate && inPhoneMemmory)
            {
                return JsonConvert.DeserializeObject<HomeLinks>(rawApiResponse);
            }
            else
            {
                Log("There is an update fo ya! =)");
                string baseUrl = getBaseUrl("linker","v1","list");

                HomeLinks homeLinks = await getRawResponseAs<HomeLinks>(baseUrl);

                SaveToPhone(JsonConvert.SerializeObject(homeLinks), withKey);

                return homeLinks;
            }
        }

        public async static Task<EventsData> GetEventsInfo(string emsApiKey = "", Languages lang = Languages.defaultLanguage)
        {
            string baseUrl;

            baseUrl = getBaseUrl("events", "v1", "after-date", GetClubIds(), "", "", 0, "", "", lang);

            return await getRawResponseAs<EventsData>(baseUrl);

            //if (IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(EmsApi.eventsDataKey, out rawApiResponse))
            //{
            //    return JsonConvert.DeserializeObject<EventsData>(rawApiResponse);
            //}
            //else
            //{
            //    string baseUrl = "http://api.ozucc.org/events/v1/after-date";

            //    EventsData data = await getRawResponseAs<EventsData>(baseUrl);

            //    SaveToPhone(rawApiResponse, data.info.responseStatus, EmsApi.eventsDataKey);

            //    return data;
            //}
        }

        public async static Task<ClubsData> GetClubsData(string emsApiKey = "", Languages lang = Languages.defaultLanguage)
        {

            string rawApiResponse;
            string withKey = ClubsData.IsCheckedKey;
            bool inPhoneMemmory = IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(withKey, out rawApiResponse);

            string baseUrl = getBaseUrl("clubs", "v1", "list", "", "", "", 0, "", "", lang);

            ClubsData clubsData = await getRawResponseAs<ClubsData>(baseUrl);

            if (inPhoneMemmory)
            {
                foreach (ClubResult club in clubsData.result)
                    club.IsChecked = MainPage.data.ClubIdIsCheked[club.id];
            }
            else
            {
                foreach (ClubResult club in clubsData.result)
                {
                    club.IsChecked = true;
                    MainPage.data.ClubIdIsCheked[club.id] = true;
                }
            }

            SaveToPhone(JsonConvert.SerializeObject(MainPage.data.ClubIdIsCheked), ClubsData.IsCheckedKey);

            return clubsData;
        }

        public static async Task<T> getRawResponseAs<T>(string baseUrl) where T : IJsonData, new()
        {
            T apiResponse = new T();

            string rawApiResponse = await getResponse(baseUrl);

            apiResponse = JsonConvert.DeserializeObject<T>(rawApiResponse);

            string errorMessage = "";

            if (string.IsNullOrWhiteSpace(rawApiResponse))
                errorMessage = AppResources.ConnectionFailedFeedback;
            else if (apiResponse.info.responseStatus == 400)
                foreach (Error error in apiResponse.info.log.error)
                    foreach (string validation in error.validation)
                        errorMessage += validation + "\n";

            if (!string.IsNullOrWhiteSpace(errorMessage))
                Log(errorMessage);

            return apiResponse;
        }

        private static async Task<string> getResponse(string baseUrl)
        {
            string rawApiResponse = "";

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(baseUrl);
            rawApiResponse = await response.Content.ReadAsStringAsync();

            return rawApiResponse;
        }

        public async static Task<bool> IsHomeVersionUpToDate(string emsApiKey = "")
        {
            string baseUrl = getBaseUrl("linker", "v1", "list-version");

            HomeVersion eventResults = await getRawResponseAs<HomeVersion>(baseUrl);

            string serverVersion = eventResults.result.version;
            string rawHomeVersion;

            if (!string.IsNullOrWhiteSpace(serverVersion))
                MainPage.version = serverVersion;

            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(HomeVersion.homeVersionKey, out rawHomeVersion))
            {
                return compareVersions(serverVersion, rawHomeVersion);
            }
            else
            {
                SaveToPhone(eventResults.result.version, HomeVersion.homeVersionKey);
                return false;
            }
        }

        public static string GetClubIds()
        {
            string temp;
            string withKey = ClubsData.IsCheckedKey;
            bool isClubSettingsInMemmory = IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(withKey, out temp);

            Dictionary<string, bool> results = JsonConvert.DeserializeObject<Dictionary<string, bool>>(temp);

            List<short> clubIds = new  List<short>();

            foreach (KeyValuePair<string, bool> item in results)
            {
                if (item.Value)
                {
                    clubIds.Add(short.Parse(item.Key));
                }
            }

            temp = string.Join<short>(",", clubIds);

            return temp;
        }

        public static void SaveToPhone(string rawApiResponse, string withKey)
        {
            IsolatedStorageSettings.ApplicationSettings[withKey] = rawApiResponse;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public static string getBaseUrl(
            string page, string version, string method,
            string clubIds = "", string q = "", string id = "",
            int start = 0, string date = "", string count = "", Languages lang = Languages.defaultLanguage)
        {
            string requestUrl = "http://api.ozucc.org";
            //string requestUrl = "http://176.41.7.135/api.ozu/public";
            int _count = 10, _id = 0;
            requestUrl += "/" + page + "/" + version + "/" + method + "?";

            if (!string.IsNullOrWhiteSpace(clubIds))
                requestUrl += "&club_ids=" + HttpUtility.UrlEncode(clubIds);
            if (!string.IsNullOrWhiteSpace(q))
                requestUrl += "&q=" + HttpUtility.UrlEncode(q);
            if (!string.IsNullOrWhiteSpace(date))
                requestUrl += "&date=" + date;
            if (int.TryParse(count, out _count))
                requestUrl += "&count=" + _count.ToString();
            if (int.TryParse(id, out _id))
                requestUrl += "&id=" + _id.ToString();
            if (start != 0)
                requestUrl += "&start=" + start;
            if (lang != Languages.defaultLanguage)
                requestUrl += "&lang=" + lang;

            return requestUrl;
        }

        private static bool compareVersions(string serverVersion, string rawApiResponse)
        {
            int[] _currentVersion = new int[5], _apiVersion = new int[5];

            string[] currentversion = rawApiResponse.Split('.');
            string[] apiVersion = serverVersion.Split('.');

            for (int i = 0; i < currentversion.Length; i++)
                _currentVersion[i] = int.Parse(currentversion[i]);
            for (int i = 0; i < apiVersion.Length; i++)
                _apiVersion[i] = int.Parse(apiVersion[i]);

            for (int i = 0; i < _apiVersion.Length; i++)
                if (_apiVersion[i] > _currentVersion[i])
                    return false;

            return true;
        }

        public static void showToast(string msg)
        {
            new ToastPrompt()
            {
                TextOrientation = System.Windows.Controls.Orientation.Vertical,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
                Message = msg + "\n\n ",
                FontWeight = FontWeights.Bold,
                Title = string.Empty,
                Margin = new Thickness(0, 250, 0, 0),
                TextWrapping = TextWrapping.Wrap
            }.Show();
        }

        public enum Languages
        {
            defaultLanguage,
            tr,
            en
        }

        public static void StartTrayLoadingAnimation()
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
            SetProggressIndicatorVisibility(true);
            SystemTray.ProgressIndicator.Text = AppResources.LoadingMessage;
        }
        public static void SetProggressIndicatorVisibility(bool isVisible)
        {
            if (SystemTray.ProgressIndicator != null)
            {
                SystemTray.ProgressIndicator.IsIndeterminate = isVisible;
                SystemTray.ProgressIndicator.IsVisible = isVisible;
            }
        }
        public static void Log(string msg)
        {
            if (MainPage.IsOnDebug)
                MessageBox.Show(msg);
        }

        public static void prettyDisplayDates(ObservableCollection<EventsResult> items)
        {
            foreach (EventsResult item in items)
            {
                DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local).AddSeconds(double.Parse(item.event_date));
                if (date != null && 2011 <= date.Year)
                    item.prettyDate = GetTimeSpan(date);
                else item.prettyDate = GetTimeSpan(DateTime.Now, true);

            }
        }
        public static string GetTimeSpan(DateTime postDate, bool isDuration = false)
        {
            string stringy = string.Empty;
            TimeSpan diff = DateTime.Now.Subtract(postDate);
            double days = diff.Days;
            double hours = diff.Hours + days * 24;
            double minutes = diff.Minutes + hours * 60;

            string
                year = AppResources.PrettyYear,
                day = AppResources.PrettyDay,
                week = AppResources.PrettyWeek,
                hour = AppResources.PrettyHour,
                minute = AppResources.PrettyMinute,
                pluralSuffix = AppResources.PrettyPluralSuffix,
                left = AppResources.PrettyLeft,
                ago = AppResources.PrettyAgo;

            if (IsInRange(-1, minutes, 1) && !isDuration)
                return "Just Now";

            double years = Math.Floor(diff.TotalDays / 365);
            if (IsInRange(double.NegativeInfinity, years, -1, false, false) || IsInRange(1, years, double.PositiveInfinity, false, false))
            {
                return string.Format("{0} " + year + "{1} {2}", years < 0 ? -1 * years : years, years >= 2 ? pluralSuffix : null, isDuration ? "" : years < 0 ? left : ago);
            }
            double weeks = Math.Floor(diff.TotalDays / 7);
            if (IsInRange(double.NegativeInfinity, weeks, -1) || IsInRange(1, weeks, double.PositiveInfinity))
            {
                double partOfWeek = days - weeks * 7;
                if (partOfWeek > 0)
                {
                    stringy = string.Format(", {0} " + day + "{1}", partOfWeek, partOfWeek > 1 ? pluralSuffix : null);
                }
                return string.Format("{0} " + week + "{1}{2} {3}", weeks < 0 ? -1 * weeks : weeks, weeks >= 2 ? pluralSuffix : null, stringy, isDuration ? "" : weeks < 0 ? left : ago);
            }
            if (IsInRange(double.NegativeInfinity, days, -1) || IsInRange(1, days, double.PositiveInfinity))
            {
                double partOfDay = hours - days * 24;
                if (partOfDay > 0)
                {
                    stringy = string.Format(", {0} " + hour + "{1}", partOfDay, partOfDay > 1 ? pluralSuffix : null);
                }
                return string.Format("{0} " + day + "{1}{2} {3}", days < 0 ? -1 * days : days, days >= 2 ? pluralSuffix : null, stringy, isDuration ? "" : days < 0 ? left : ago);
            }
            if (IsInRange(double.NegativeInfinity, hours, -1) || IsInRange(1, hours, double.PositiveInfinity))
            {
                double partOfHour = minutes - hours * 60;
                if (partOfHour > 0)
                {
                    stringy = string.Format(", {0} " + minute + "{1}", partOfHour, partOfHour > 1 ? pluralSuffix : null);
                }
                return string.Format("{0} " + hour + "{1}{2} {3}", hours < 0 ? -1 * hours : hours, hours >= 2 ? pluralSuffix : null, stringy, isDuration ? "" : hours < 0 ? left : ago);
            }

            // Only condition left is minutes > 1
            return string.Format("{0} " + minute + pluralSuffix + " {1}", minutes, isDuration ? "" : minutes < 0 ? left : ago);
        }
        public static bool IsInRange(double from, double number, double to, bool includingFrom = true, bool includingTo = true)
        {
            return (includingFrom ? from <= number : from < number) && (includingTo ? number <= to : number < to);
        }
    }
}

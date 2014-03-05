﻿using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using Ozu_EMS.Resources;
using System;
using System.Collections.Generic;
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
        public async static Task<HomeLinks> GetHomeLinks(string emsApiKey = "")
        {

            string rawApiResponse;
            string withKey = HomeLinks.homeDataKey;
            bool inPhoneMemmory = IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(withKey, out rawApiResponse);
            bool homeIsUpToDate = await IsHomeVersionUpToDate();

            if (inPhoneMemmory && homeIsUpToDate)
            {
                return JsonConvert.DeserializeObject<HomeLinks>(rawApiResponse);
            }
            else
            {
                MessageBox.Show("There is an update fo ya! =)");

                string baseUrl = "http://api.ozucc.org/linker/v1/list";

                HomeLinks homeLinks = await getRawResponseAs<HomeLinks>(baseUrl);

                SaveToPhone(JsonConvert.SerializeObject(homeLinks), withKey);

                return homeLinks;
            }
        }

        public async static Task<EventsData> GetEventsInfo(string emsApiKey = "")
        {
            string baseUrl;

            baseUrl = getBaseUrl("events", "v1", "after-date", GetClubIds());

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

        public async static Task<ClubsData> GetClubsData(string emsApiKey = "")
        {

            string rawApiResponse;
            string withKey = ClubsData.clubsDataKey;
            bool inPhoneMemmory = IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(withKey, out rawApiResponse);

            if (inPhoneMemmory)
            {
                return JsonConvert.DeserializeObject<ClubsData>(rawApiResponse);
            }
            else
            {
                string baseUrl = "http://api.ozucc.org/clubs/v1/list";

                ClubsData clubsData = await getRawResponseAs<ClubsData>(baseUrl);

                foreach (ClubResult club in clubsData.result)
                {
                    club.IsChecked = true;
                }

                SaveToPhone(JsonConvert.SerializeObject(clubsData), withKey);

                return clubsData;
            }
        }

        public static async Task<T> getRawResponseAs<T>(string baseUrl, string errorMessage = "") where T : IJsonData, new()
        {
            T apiResponse = new T();

            string rawApiResponse = await getResponse(baseUrl);

            apiResponse = JsonConvert.DeserializeObject<T>(rawApiResponse);

            if (string.IsNullOrWhiteSpace(rawApiResponse) || apiResponse.info.responseStatus != 200)
            {
                if (string.IsNullOrWhiteSpace(errorMessage))
                    showToast(AppResources.ConnectionFailedFeedback);
                else showToast(errorMessage);
            }

            return apiResponse;
        }

        private static async Task<string> getResponse(string baseUrl)
        {
            string rawApiResponse = "";

            try
            {
                HttpClient client = new HttpClient();

                rawApiResponse = await client.GetStringAsync(baseUrl);

                if (string.IsNullOrWhiteSpace(rawApiResponse))
                {
                    showToast(AppResources.ConnectionFailedFeedback);
                }

            }
            catch (Exception ex)
            {
                showToast(AppResources.RandomErrorFeedback + ex.Message);
            }

            return rawApiResponse;
        }

        public async static Task<bool> IsHomeVersionUpToDate(string emsApiKey = "")
        {
            string baseUrl = "http://api.ozucc.org/linker/v1/list-version";

            HomeVersion eventResults = await getRawResponseAs<HomeVersion>(baseUrl);

            string serverVersion = eventResults.result.version;
            string rawHomeVersion;

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
            string rawClubsData;
            string withKey = ClubsData.clubsDataKey;
            bool isClubSettingsInMemmory = IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(withKey, out rawClubsData);

            ClubResult[] results = JsonConvert.DeserializeObject<ClubsData>(rawClubsData).result
                .Where<ClubResult>(item => item.IsChecked == true).ToArray<ClubResult>();
            short[] clubIds = new short[results.Length];
            for (int i = 0; i < clubIds.Length; i++)
            {
                short.TryParse(results[i].id, out clubIds[i]);
            }

            rawClubsData = string.Join<short>(",", clubIds);

            return rawClubsData;
        }

        public static void SaveToPhone(string rawApiResponse, string withKey)
        {
            IsolatedStorageSettings.ApplicationSettings[withKey] = rawApiResponse;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public static string getBaseUrl(
            string page, string version, string method,
            string clubIds = "", string q = "", string id = "",
            int start = 0, string date = "", string count = "")
        {
            string requestUrl = "http://api.ozucc.org";
            int _count = 10, _id = 0;
            requestUrl += "/" + page + "/" + version + "/" + method + "?";

            if (!string.IsNullOrWhiteSpace(clubIds))
                requestUrl += "&club_ids=" + clubIds;
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
    }
}

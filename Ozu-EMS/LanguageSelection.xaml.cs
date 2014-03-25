using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading;
using System.IO.IsolatedStorage;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Ozu_EMS
{
    public partial class LanguageSelection : PhoneApplicationPage
    {
        public const string languageKey = "languageKey";
        private bool _isInitialized = false;

        public LanguageSelection()
        {
            InitializeComponent();

            Loaded += LanguageSelection_Loaded;

        }

        void LanguageSelection_Loaded(object sender, RoutedEventArgs e)
        {

            if (MainPage.EmsLanguage == EmsApi.Languages.tr)
                Turkish.IsChecked = true;
            else English.IsChecked = true;

            _isInitialized = true;
        }

        private static void updateContentWithLanguage()
        {
            MainPage.updateButtonTexts();
            ClubSelection.isInitialized = false;
        }

        public static void changeLanguageTo(EmsApi.Languages lang)
        {
            MainPage.EmsLanguage = lang;

            switch (lang)
            {
                case EmsApi.Languages.tr:
                    Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("tr");
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("tr");
                    break;
                case EmsApi.Languages.en:
                    Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
                    break;
                default:
                    break;
            }

            EmsApi.SaveToPhone(JsonConvert.SerializeObject(lang), languageKey);

            LocalizedStrings.LocalizedStringsResource.UpdateLanguage();

            updateContentWithLanguage();
        }

        private void English_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
                changeLanguageTo(EmsApi.Languages.en);
        }

        private void Turkish_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
                changeLanguageTo(EmsApi.Languages.tr);
        }
    }
}
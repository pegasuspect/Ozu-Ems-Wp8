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

namespace Ozu_EMS
{
    public partial class LanguageSelection : PhoneApplicationPage
    {
        public LanguageSelection()
        {
            InitializeComponent();

            Loaded += LanguageSelection_Loaded;

        }

        void LanguageSelection_Loaded(object sender, RoutedEventArgs e)
        {
            if (Thread.CurrentThread.CurrentCulture.Equals(new System.Globalization.CultureInfo("en-US")))
                ChangeLanguage.Content = "Change to Turkish";
            else ChangeLanguage.Content = "Change to English";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            if (Thread.CurrentThread.CurrentCulture.Equals(new System.Globalization.CultureInfo("en-US")))
            {
                b.Content = "Change to English";
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("tr");
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("tr");
            }
            else
            {
                b.Content = "Change to Turkish";
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
            }

            LocalizedStrings.LocalizedStringsResource.UpdateLanguage();
        }
    }
}
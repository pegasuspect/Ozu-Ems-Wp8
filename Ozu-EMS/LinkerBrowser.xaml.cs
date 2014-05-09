using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Ozu_EMS
{
    public partial class LinkerBrowser : PhoneApplicationPage
    {
        public LinkerBrowser()
        {
            InitializeComponent();

            Loaded += LinkerBrowser_Loaded;
        }

        void LinkerBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            string url;
            NavigationContext.QueryString.TryGetValue("url", out url);
            Browser.IsScriptEnabled = true;
            Browser.Navigate(new Uri(url, UriKind.Absolute));
        }

        private void FillRectangle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
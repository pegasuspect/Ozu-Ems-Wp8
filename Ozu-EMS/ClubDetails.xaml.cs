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
    public partial class ClubDetails : PhoneApplicationPage
    {
        private string _clubId;
        public ClubDetails()
        {
            InitializeComponent();

            Loaded += ClubDetails_Loaded;
        }

        void ClubDetails_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = (ClubResult)MainPage.data.ClubsData.result.Where<ClubResult>(item => item.id == _clubId).First<ClubResult>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _clubId = NavigationContext.QueryString["id"];
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("mailto:[" + (sender as HyperlinkButton).Content.ToString() + "]"));
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            scrollViewForDesc.IsEnabled = (scrollViewForDesc.ExtentHeight < textBlock.ActualHeight ? true : false);
        }

        private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double height = this.title.ActualHeight;
            Thickness margin = this.title.Margin;
            this.title.Margin = new Thickness(margin.Left, -height, margin.Right, margin.Bottom);
        }
    }
}
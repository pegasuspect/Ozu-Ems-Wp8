using Ozu_EMS.Resources;

namespace Ozu_EMS
{
    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    using GalaSoft.MvvmLight;
    using System.Windows;

    public class LocalizedStrings : ViewModelBase
    {
        private static AppResources localizedresources = new AppResources();

        public AppResources LocalizedResources
        {
            get { return localizedresources; }
        }

        public void UpdateLanguage()
        {
            localizedresources = new AppResources();
            RaisePropertyChanged(() => LocalizedResources);
        }

        public static LocalizedStrings LocalizedStringsResource
        {
            get
            {
                return Application.Current.Resources["LocalizedStrings"]
                    as LocalizedStrings;
            }
        }
    }
}
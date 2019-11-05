using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KinaUnaXamarin.Resources;
using Xamarin.Forms;
using KinaUnaXamarin.Services;
using Plugin.Multilingual;

namespace KinaUnaXamarin
{
    public partial class App : Application
    {
        static Database _database;

        public App()
        {
            InitializeComponent();

            Xamarin.Essentials.VersionTracking.Track();
            bool languageSet = false;
            string language;
            if (Device.RuntimePlatform == Device.UWP)
            {
                language = Task.Run(() => UserService.GetLanguage()).Result;
            }
            else
            {
                language = UserService.GetLanguage().GetAwaiter().GetResult();
                if (!string.IsNullOrEmpty(language))
                {
                    languageSet = true;
                }
            }
            
            if (languageSet)
            {
                CrossMultilingual.Current.CurrentCultureInfo = CrossMultilingual.Current.NeutralCultureInfoList.ToList().First(element => element.TwoLetterISOLanguageName == language);
                Translations.Culture = CrossMultilingual.Current.CurrentCultureInfo;
            }
            else
            {
                Translations.Culture = CrossMultilingual.Current.DeviceCultureInfo;
            }
            
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        public static Database Database
        {
            get
            {
                if (_database == null)
                {
                    _database = new Database(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "kinauna.db3"));
                }
                return _database;
            }
        }
    }
}

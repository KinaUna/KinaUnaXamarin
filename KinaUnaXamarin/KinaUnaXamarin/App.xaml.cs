using Xamarin.Forms;
using KinaUnaXamarin.Services;

namespace KinaUnaXamarin
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            Xamarin.Essentials.VersionTracking.Track();
                
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
    }
}

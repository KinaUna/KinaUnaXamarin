using Xamarin.Essentials;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views.Settings
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage
    {
        public AboutPage()
        {
            InitializeComponent();
            VersionLabel.Text = "Version: " + VersionTracking.CurrentVersion;
        }
    }
}
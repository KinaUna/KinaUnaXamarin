using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
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

        private async void HelpToolbarButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PushModalAsync(new HelpPage());
        }

        private async void AddItemToolbarButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PushModalAsync(new AddItemPage());
        }

        private async void ProgenyToolBarItem_OnClicked(object sender, EventArgs e)
        {
            string userEmail = await UserService.GetUserEmail();
            List<Progeny> progenyList = await ProgenyService.GetProgenyList(userEmail);
            ObservableCollection<Progeny> progenyCollection = new ObservableCollection<Progeny>();
            if (progenyList != null && progenyList.Any())
            {
                foreach (Progeny prog in progenyList)
                {
                    progenyCollection.Add(prog);
                }
            }

            SelectProgenyPage selProPage = new SelectProgenyPage(progenyCollection);
            await Shell.Current.Navigation.PushModalAsync(selProPage);
        }

        private async void WebsiteLinkLabel_Tapped(object sender, EventArgs e)
        {
            await Launcher.OpenAsync("https://www.kinauna.com");
        }

        private async void SupportLinkLabel_Tapped(object sender, EventArgs e)
        {
            await Launcher.OpenAsync("https://support.kinauna.com");
        }

        private async void SupportEmailLabel_Tapped(object sender, EventArgs e)
        {
            await Launcher.OpenAsync("mailto:support@kinauna.com");
        }

        private async void SourceCodeLinkLabel_Tapped(object sender, EventArgs e)
        {
            await Launcher.OpenAsync("https://github.com/KinaUna/KinaUnaXamarin");
        }
    }
}
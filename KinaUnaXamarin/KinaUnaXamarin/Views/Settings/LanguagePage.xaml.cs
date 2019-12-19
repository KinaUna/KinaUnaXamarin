using System;
using System.Linq;
using System.Reflection;
using System.Resources;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.Resources;
using KinaUnaXamarin.Services;
using Plugin.Multilingual;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views.Settings
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LanguagePage
    {
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));
        public LanguagePage()
        {
            InitializeComponent();
            picker.Items.Add("English");
            picker.Items.Add("Dansk");
            picker.Items.Add("Deutsch");

            string languageCode = CrossMultilingual.Current.CurrentCultureInfo.TwoLetterISOLanguageName;

            switch (languageCode)
            {
                case "da":
                    picker.SelectedIndex = 1;
                    break;
                case "de":
                    picker.SelectedIndex = 2;
                    break;
                default:
                    picker.SelectedIndex = 0;
                    break;
            }

        }

        private async void OnUpdateLangugeClicked(object sender, EventArgs e)
        {
            await SaveButtonFrame.ScaleTo(1.1, 100);
            await SaveButtonFrame.ScaleTo(1, 100);
            string languageCode;
            switch (picker.SelectedIndex)
            {
                case 0:
                    languageCode = "en";
                    break;
                case 1:
                    languageCode = "da";
                    break;
                case 2:
                    languageCode = "de";
                    break;
                default:
                    languageCode = "en";
                    break;
            }
            CrossMultilingual.Current.CurrentCultureInfo = CrossMultilingual.Current.NeutralCultureInfoList.ToList().First(element => element.TwoLetterISOLanguageName == languageCode);
            Translations.Culture = CrossMultilingual.Current.CurrentCultureInfo;
            await UserService.SetLanguage(CrossMultilingual.Current.CurrentCultureInfo.TwoLetterISOLanguageName).ConfigureAwait(false);

            MessagingCenter.Send(this, "LanguageChanged");
            // Todo: Reload Shell to update menus.
            // App.Current.MainPage = new AppShell(); // Causes null reference error when exiting app.
        }

        private void Picker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            string languageCode;
            switch (picker.SelectedIndex)
            {
                case 0:
                    languageCode = "en";
                    break;
                case 1:
                    languageCode = "da";
                    break;
                case 2:
                    languageCode = "de";
                    break;
                default:
                    languageCode = "en";
                    break;
            }
            var ci = CrossMultilingual.Current.NeutralCultureInfoList.ToList().First(element => element.TwoLetterISOLanguageName == languageCode);

            SaveLabel.Text = resmgr.Value.GetString("Save", ci);
            SelectLanguageLabel.Text = resmgr.Value.GetString("SelectLanguage", ci);
            Title = resmgr.Value.GetString("Language", ci);
            WarningLabel.Text = resmgr.Value.GetString("ChangeLanguageWarning", ci);
        }
    }
}
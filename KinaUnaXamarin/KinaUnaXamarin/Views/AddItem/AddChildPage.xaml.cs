using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels.AddItem;
using Plugin.Media;
using Plugin.Multilingual;
using TimeZoneConverter;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views.AddItem
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddChildPage : ContentPage
    {
        private readonly AddChildViewModel _addChildViewModel;
        private string _filePath;
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public AddChildPage()
        {
            InitializeComponent();
            _addChildViewModel = new AddChildViewModel();
            IReadOnlyCollection<TimeZoneInfo> timeZoneList = TimeZoneInfo.GetSystemTimeZones();
            foreach (TimeZoneInfo timeZoneInfo in timeZoneList)
            {
                _addChildViewModel.timeZoneList.Add(timeZoneInfo);
            }

            TimeZonePicker.ItemsSource = _addChildViewModel.timeZoneList;
            
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            string userTimeZone = await UserService.GetUserTimezone();
            TimeZoneInfo userTimeZoneInfo =
                _addChildViewModel.timeZoneList.SingleOrDefault(tz => tz.DisplayName == userTimeZone);
            int timeZoneIndex = _addChildViewModel.timeZoneList.IndexOf(userTimeZoneInfo);
            TimeZonePicker.SelectedIndex = timeZoneIndex;
        }

        private async void SelectImageButton_OnClicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                return;
            }

            var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                RotateImage = false,
                SaveMetaData = true
            });

            if (file == null)
            {
                return;
            }

            _filePath = file.Path;
            UploadImage.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();
                return stream;
            });

            
        }

        private async void SaveChildButton_OnClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(NameEntry.Text) || string.IsNullOrEmpty(DisplayNameEntry.Text))
            {
                return;
            }

            _addChildViewModel.IsBusy = true;
            
            Progeny progeny = new Progeny();
            if (!string.IsNullOrEmpty(_filePath))
            {
                progeny.PictureLink = await ProgenyService.UploadProgenyPicture(_filePath);
            }
            progeny.Name = NameEntry.Text;
            progeny.NickName = DisplayNameEntry.Text;
            progeny.BirthDay = new DateTime(BirthdayDatePicker.Date.Year, BirthdayDatePicker.Date.Month, BirthdayDatePicker.Date.Day, BirthdayTimePicker.Time.Hours, BirthdayTimePicker.Time.Minutes, 00);
            TimeZoneInfo timeZoneInfo = (TimeZoneInfo) TimeZonePicker.SelectedItem;
            string timeZoneName;
            if (TZConvert.TryIanaToWindows(timeZoneInfo.Id, out timeZoneName))
            {
                progeny.TimeZone = timeZoneName;
            }
            else
            {
                progeny.TimeZone = "Romance Standard Time";
            }
            
            progeny.Admins = await UserService.GetUserEmail();

            Progeny newProgeny = await ProgenyService.AddProgeny(progeny);
            
            MessageLabel.IsVisible = true;
            if (newProgeny.Id == 0)
            {
                var ci = CrossMultilingual.Current.CurrentCultureInfo;
                MessageLabel.Text = resmgr.Value.GetString("ErrorChildNotSaved", ci);
                MessageLabel.BackgroundColor = Color.Red;
                SaveChildButton.IsEnabled = true;
                CancelChildButton.IsEnabled = true;

            }
            else
            {
                var ci = CrossMultilingual.Current.CurrentCultureInfo;
                MessageLabel.Text = resmgr.Value.GetString("ChildSaved", ci) + newProgeny.Id;
                MessageLabel.BackgroundColor = Color.Green;
                SaveChildButton.IsVisible = false;
                CancelChildButton.Text = "Ok";
                CancelChildButton.BackgroundColor = Color.FromHex("#4caf50");
                CancelChildButton.IsEnabled = true;
            }

            _addChildViewModel.IsBusy = false;
        }

        private async void CancelChildButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
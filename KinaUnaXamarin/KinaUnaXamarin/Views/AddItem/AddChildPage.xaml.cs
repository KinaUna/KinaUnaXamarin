using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels.AddItem;
using Plugin.Media;
using Plugin.Multilingual;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views.AddItem
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddChildPage : ContentPage
    {
        private bool _online = true;
        private readonly AddChildViewModel _addChildViewModel;
        private string _filePath;
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public AddChildPage()
        {
            InitializeComponent();
            _addChildViewModel = new AddChildViewModel();
            BindingContext = _addChildViewModel;
            IReadOnlyCollection<TimeZoneInfo> timeZoneList = TimeZoneInfo.GetSystemTimeZones();
            foreach (TimeZoneInfo timeZoneInfo in timeZoneList)
            {
                _addChildViewModel.TimeZoneList.Add(timeZoneInfo);
            }

            TimeZonePicker.ItemsSource = _addChildViewModel.TimeZoneList;
            
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            var networkAccess = Connectivity.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess)
            {
                _addChildViewModel.Online = true;
                OfflineStackLayout.IsVisible = false;
            }
            else
            {
                _addChildViewModel.Online = false;
                OfflineStackLayout.IsVisible = true;
            }

            string userTimeZone = await UserService.GetUserTimezone();
            TimeZoneInfo userTimeZoneInfo =
                _addChildViewModel.TimeZoneList.SingleOrDefault(tz => tz.DisplayName == userTimeZone);
            if (userTimeZoneInfo == null)
            {
                userTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);
            }

            int timeZoneIndex = _addChildViewModel.TimeZoneList.IndexOf(userTimeZoneInfo);
            TimeZonePicker.SelectedIndex = timeZoneIndex;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var networkAccess = e.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess != _online)
            {
                _addChildViewModel.Online = false;
                OfflineStackLayout.IsVisible = true;
                SaveChildButton.IsEnabled = false;
            }
            else
            {
                _addChildViewModel.Online = true;
                OfflineStackLayout.IsVisible = false;
                SaveChildButton.IsEnabled = true;
            }
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
            string userEmail = await UserService.GetUserEmail();
            progeny.Admins = userEmail;

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

                // Update the progeny list.
                await ProgenyService.GetProgenyList(userEmail);
            }

            _addChildViewModel.IsBusy = false;
        }

        private async void CancelChildButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        private void DisplayNameEntry_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_addChildViewModel.Online)
            {
                if (DisplayNameEntry.Text.Length <= 1)
                {
                    SaveChildButton.IsEnabled = false;
                }
                else
                {
                    SaveChildButton.IsEnabled = true;
                }
            }
            else
            {
                SaveChildButton.IsEnabled = false;
            }
        }

        private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
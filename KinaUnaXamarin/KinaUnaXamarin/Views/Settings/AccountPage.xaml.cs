using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using Plugin.Media;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountPage : ContentPage
    {
        private readonly AccountViewModel _viewModel;
        private bool _online = true;
        private string _filePath;
        private bool _reload;
        public AccountPage()
        {
            InitializeComponent();
            _viewModel = new AccountViewModel();
            LoginStackLayout.BindingContext = _viewModel;
            _viewModel.LoggedIn = false;
            _viewModel.LoggedOut = true;
            _reload = true;
            IReadOnlyCollection<TimeZoneInfo> timeZoneList = TimeZoneInfo.GetSystemTimeZones();
            foreach (TimeZoneInfo timeZoneInfo in timeZoneList)
            {
                _viewModel.TimeZoneList.Add(timeZoneInfo);
            }

            TimeZonePicker.ItemsSource = _viewModel.TimeZoneList;

            MessagingCenter.Subscribe<AccountViewModel>(this, "Reload", async (sender) =>
            {
                await Reload();
            });
        }

        private async void RegisterButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//register");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            if (_reload)
            {
                await Reload();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        private async Task Reload()
        {
            _reload = false;
            var networkInfo = Connectivity.NetworkAccess;

            string userTimeZone = await UserService.GetUserTimezone();
            if (string.IsNullOrEmpty(userTimeZone))
            {
                userTimeZone = Constants.DefaultTimeZone;
            }

            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);
            }
            catch (Exception)
            {
                userTimeZone = TZConvert.WindowsToIana(userTimeZone);
            }

            TimeZoneInfo userTimeZoneInfo =
                _viewModel.TimeZoneList.SingleOrDefault(tz => tz.DisplayName == userTimeZone);

            if (userTimeZoneInfo == null)
            {
                userTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);
            }

            int timeZoneIndex = _viewModel.TimeZoneList.IndexOf(userTimeZoneInfo);
            TimeZonePicker.SelectedIndex = timeZoneIndex;

            if (networkInfo == NetworkAccess.Internet)
            {
                // Connection to internet is available
                _online = true;
                OfflineStackLayout.IsVisible = false;
                LogInButton.IsEnabled = true;
                LogOutButton.IsEnabled = true;
                _viewModel.EditMode = false;
                FullNameEntry.IsVisible = true;
                _viewModel.Username = await UserService.GetUsername();
                _viewModel.FullName = await UserService.GetFullname();
                _viewModel.Email = await UserService.GetUserEmail();
                _viewModel.Timezone = await UserService.GetUserTimezone();
                _viewModel.UserId = await UserService.GetUserId();
                UserInfo userInfo = await UserService.GetUserInfo(_viewModel.Email);
                
                if (!string.IsNullOrEmpty(userInfo.ProfilePicture))
                {
                    _viewModel.ProfilePicture = await UserService.GetUserPicture(userInfo.ProfilePicture);
                }
                else
                {
                    _viewModel.ProfilePicture = Constants.ProfilePicture;
                }

                ProfileImage.Source = _viewModel.ProfilePicture;
                
                _viewModel.FirstName = userInfo.FirstName;
                _viewModel.MiddleName = userInfo.MiddleName;
                _viewModel.LastName = userInfo.LastName;
                
                bool accessTokenCurrent = await UserService.IsAccessTokenCurrent();
                string accessToken = await UserService.GetAuthAccessToken();
                if (String.IsNullOrEmpty(accessToken) || !accessTokenCurrent)
                {
                    _viewModel.LoggedIn = false;
                }
                else
                {
                    _viewModel.LoggedIn = true;
                }

                List<Progeny> progenyList = await ProgenyService.GetProgenyList(_viewModel.Email);
                _viewModel.ProgenyCollection.Clear();
                foreach (Progeny prog in progenyList)
                {
                    _viewModel.ProgenyCollection.Add(prog);
                }

            }
            else
            {
                _online = false;
                OfflineStackLayout.IsVisible = true;
                LogInButton.IsEnabled = false;
                LogOutButton.IsEnabled = false;
            }
        }

        private async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var networkAccess = e.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess != _online)
            {
                await Reload();
            }
        }

        private async void EditButton_OnClicked(object sender, EventArgs e)
        {
            if (_viewModel.EditMode)
            {
                _viewModel.EditMode = false;
                FullNameEntry.IsVisible = true;
                _viewModel.IsBusy = true;
                // Save changes.
                UserInfo updatedUserInfo = await UserService.GetUserInfo(await UserService.GetUserEmail());
                updatedUserInfo.FirstName = FirstNameEntry.Text;
                updatedUserInfo.MiddleName = MiddleNameEntry.Text;
                updatedUserInfo.LastName = LastNameEntry.Text;
                // updatedUserInfo.UserEmail = UserEmailEntry.Text; Todo: Implement a change email process.
                TimeZoneInfo timeZoneInfo = (TimeZoneInfo)TimeZonePicker.SelectedItem;
                string timeZoneName;
                if (TZConvert.TryIanaToWindows(timeZoneInfo.Id, out timeZoneName))
                {
                    updatedUserInfo.Timezone = timeZoneName;
                }
                else
                {
                    updatedUserInfo.Timezone = "Romance Standard Time";
                }
                
                if (!string.IsNullOrEmpty(_filePath))
                {
                    updatedUserInfo.ProfilePicture = await UserService.UploadProfilePicture(_filePath);
                }

                UserInfo resultUserInfo = await UserService.UpdateUserInfo(updatedUserInfo);
                _viewModel.IsBusy = false;
                EditButton.Text = IconFont.AccountEdit;
                if (resultUserInfo != null)
                {
                    await Reload();
                }
            }
            else
            {
                EditButton.Text = IconFont.ContentSave;
                FullNameEntry.IsVisible = false;
                _viewModel.EditMode = true;
            }
        }

        private async void CancelButton_OnClicked(object sender, EventArgs e)
        {
            FullNameEntry.IsVisible = true;
            EditButton.Text = IconFont.AccountEdit;
            await Reload();
            
        }

        private async void ReloadToolbarButton_OnClicked(object sender, EventArgs e)
        {
            await Reload();
        }

        private async void ProgenyToolBarItem_OnClicked(object sender, EventArgs e)
        {
            ObservableCollection<Progeny> progenyCollection = new ObservableCollection<Progeny>();
            List<Progeny> progList = await ProgenyService.GetProgenyList(await UserService.GetUserEmail());
            foreach (Progeny progeny in progList)
            {
                progenyCollection.Add(progeny);
            }
            SelectProgenyPage selProPage = new SelectProgenyPage(progenyCollection);
            await Shell.Current.Navigation.PushModalAsync(selProPage);
        }

        private async void AddItemToolbarButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PushModalAsync(new AddItemPage());
        }

        private async void SelectPictureButton_OnClicked(object sender, EventArgs e)
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
            ProfileImage.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();
                return stream;
            });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PhotosPage : ContentPage
    {
        private readonly PhotosViewModel _photosViewModel;
        private bool _reload = true;
        private double _screenWidth;
        private double _screenHeight;

        public PhotosPage()
        {
            InitializeComponent();
            _photosViewModel = new PhotosViewModel();
            _photosViewModel.UserInfo = OfflineDefaultData.DefaultUserInfo;
            ContainerStackLayout.BindingContext = _photosViewModel;
            BindingContext = _photosViewModel;

            MessagingCenter.Subscribe<SelectProgenyPage>(this, "Reload", async (sender) =>
            {
                await SetUserAndProgeny();
                _photosViewModel.PageNumber = 1;
                await Reload();
            });

            MessagingCenter.Subscribe<AccountViewModel>(this, "Reload", async (sender) =>
            {
                await SetUserAndProgeny();
                _photosViewModel.PageNumber = 1;
                await Reload();
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_reload)
            {
                await SetUserAndProgeny();

            }

            PhotosListView.SelectedItem = null;
            
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            var networkAccess = Connectivity.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess)
            {
                _photosViewModel.Online = true;
            }
            else
            {
                _photosViewModel.Online = false;
            }

            if (_reload)
            {
                await Reload();
            }

            _reload = false;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        protected override async void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called
            bool screenChanged = false;
            if (Device.RuntimePlatform == Device.UWP)
            {
                if (_screenWidth != Application.Current.MainPage.Width ||
                    _screenHeight != Application.Current.MainPage.Height)
                {
                    _screenWidth = Application.Current.MainPage.Width;
                    _screenHeight = Application.Current.MainPage.Height;
                }

                screenChanged = true;
            }

            if (_screenWidth != width || _screenHeight != height)
            {
                _screenWidth = width;
                _screenHeight = height;
                screenChanged = true;
            }

            if(screenChanged)
            {
                int columns = (int)Math.Floor(width / 200);

                if (Device.RuntimePlatform == Device.UWP)
                {
                    columns = (int)Math.Floor(Application.Current.MainPage.Width / 200);
                }
                if (columns < 1)
                {
                    columns = 1;
                }
                if (Device.RuntimePlatform == Device.iOS)
                {
                    await Task.Yield();
                }

                if (PhotosListView.ItemsLayout is GridItemsLayout layout)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        layout.Span = columns;
                    });
                }
            }
        }

        private async Task SetUserAndProgeny()
        {
            _photosViewModel.UserInfo = OfflineDefaultData.DefaultUserInfo;

            string userEmail = await UserService.GetUserEmail();
            string userviewchild = await SecureStorage.GetAsync(Constants.UserViewChildKey);
            bool viewchildParsed = int.TryParse(userviewchild, out int viewChildId);

            if (viewchildParsed)
            {
                _photosViewModel.ViewChild = viewChildId;
                try
                {
                    _photosViewModel.Progeny = await App.Database.GetProgenyAsync(_photosViewModel.ViewChild);
                }
                catch (Exception)
                {
                    _photosViewModel.Progeny = await ProgenyService.GetProgeny(_photosViewModel.ViewChild);
                }

                _photosViewModel.UserInfo = await App.Database.GetUserInfoAsync(userEmail);
            }

            if (String.IsNullOrEmpty(_photosViewModel.UserInfo.Timezone))
            {
                _photosViewModel.UserInfo.Timezone = Constants.DefaultTimeZone;
            }
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(_photosViewModel.UserInfo.Timezone);
            }
            catch (Exception)
            {
                _photosViewModel.UserInfo.Timezone = TZConvert.WindowsToIana(_photosViewModel.UserInfo.Timezone);
            }
        }

        private async Task Reload()
        {
            _photosViewModel.IsBusy = true;
            await CheckAccount();
            await UpdatePhotos();
            var networkInfo = Connectivity.NetworkAccess;

            if (networkInfo == NetworkAccess.Internet)
            {
                // Connection to internet is available
                _photosViewModel.Online = true;
            }
            else
            {
                _photosViewModel.Online = false;
            }
            _photosViewModel.IsBusy = false;
        }

        private async Task CheckAccount()
        {
            string userEmail = await UserService.GetUserEmail();
            _photosViewModel.AccessToken = await UserService.GetAuthAccessToken();
            bool accessTokenCurrent = false;
            if (_photosViewModel.AccessToken != "")
            {
                accessTokenCurrent = await UserService.IsAccessTokenCurrent();

                if (!accessTokenCurrent)
                {
                    bool loginSuccess = await UserService.LoginIdsAsync();
                    if (loginSuccess)
                    {
                        _photosViewModel.AccessToken = await UserService.GetAuthAccessToken();
                        accessTokenCurrent = true;
                    }

                    await Reload();
                }
            }

            if (String.IsNullOrEmpty(_photosViewModel.AccessToken) || !accessTokenCurrent)
            {

                _photosViewModel.IsLoggedIn = false;
                _photosViewModel.LoggedOut = true;
                _photosViewModel.AccessToken = "";
                _photosViewModel.UserInfo = OfflineDefaultData.DefaultUserInfo;

            }
            else
            {
                _photosViewModel.IsLoggedIn = true;
                _photosViewModel.LoggedOut = false;
                _photosViewModel.UserInfo = await UserService.GetUserInfo(userEmail);
            }

            await SetUserAndProgeny();
            
            Progeny progeny = await ProgenyService.GetProgeny(_photosViewModel.ViewChild);
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(progeny.TimeZone);
            }
            catch (Exception)
            {
                progeny.TimeZone = TZConvert.WindowsToIana(progeny.TimeZone);
            }
            _photosViewModel.Progeny = progeny;

            List<Progeny> progenyList = await ProgenyService.GetProgenyList(userEmail);
            _photosViewModel.ProgenyCollection.Clear();
            _photosViewModel.CanUserAddItems = false;
            foreach (Progeny prog in progenyList)
            {
                _photosViewModel.ProgenyCollection.Add(prog);
                if (prog.Admins.ToUpper().Contains(_photosViewModel.UserInfo.UserEmail.ToUpper()))
                {
                    _photosViewModel.CanUserAddItems = true;
                }
            }

            _photosViewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_photosViewModel.ViewChild);
        }

        private async Task UpdatePhotos()
        {
            _photosViewModel.IsBusy = true;
            if (_photosViewModel.PageNumber < 1)
            {
                _photosViewModel.PageNumber = 1;
            }
            // _photosViewModel.TagsCollection.Clear();
            
            PicturePage photoPage = await ProgenyService.GetPicturePage(_photosViewModel.PageNumber, 8, _photosViewModel.ViewChild, _photosViewModel.UserAccessLevel, _photosViewModel.UserInfo.Timezone, 1, _photosViewModel.TagFilter);
            if (photoPage.PicturesList != null)
            {
                _photosViewModel.PhotoItems.ReplaceRange(photoPage.PicturesList);
                _photosViewModel.PageNumber = photoPage.PageNumber;
                _photosViewModel.PageCount = photoPage.TotalPages;
                PhotosListView.ScrollTo(0);

                if (!string.IsNullOrEmpty(photoPage.TagsList))
                {
                    List<string> tagsList = photoPage.TagsList.Split(',').ToList();
                    tagsList.Sort();
                    foreach (string tagString in tagsList)
                    {
                        if (!string.IsNullOrEmpty(tagString))
                        {
                            _photosViewModel.TagsCollection.Add(tagString);
                        }
                    }
                }
            }

            _photosViewModel.IsBusy = false;
        }
        
        private async void ProgenyToolBarItem_OnClicked(object sender, EventArgs e)
        {
            SelectProgenyPage selProPage = new SelectProgenyPage(_photosViewModel.ProgenyCollection);
            await Shell.Current.Navigation.PushModalAsync(selProPage);
        }

        private void OptionsToolBarItem_OnClicked(object sender, EventArgs e)
        {
            _photosViewModel.ShowOptions = !_photosViewModel.ShowOptions;
        }

        private async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var networkAccess = e.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess != _photosViewModel.Online)
            {
                _photosViewModel.Online = internetAccess;
                await Reload();
            }
        }

        private async void ReloadToolbarButton_OnClicked(object sender, EventArgs e)
        {
            await Reload();
        }

        private async void AddItemToolbarButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PushModalAsync(new AddItemPage());
        }

        private async void NewerButton_OnClicked(object sender, EventArgs e)
        {

            _photosViewModel.PageNumber--;
            if (_photosViewModel.PageNumber < 1)
            {
                _photosViewModel.PageNumber = _photosViewModel.PageCount;
            }
            await UpdatePhotos();
        }

        private async void OlderButton_OnClicked(object sender, EventArgs e)
        {
            _photosViewModel.PageNumber++;
            if (_photosViewModel.PageNumber > _photosViewModel.PageCount)
            {
                _photosViewModel.PageNumber = 1;
            }
            await UpdatePhotos();
        }

        private async void PhotosListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PhotosListView.SelectedItem is Picture selectedPicture)
            {
                PhotoDetailPage photoPage = new PhotoDetailPage(selectedPicture.PictureId);
                // Reset selection
                PhotosListView.SelectedItem = null;
                await Shell.Current.Navigation.PushModalAsync(photoPage);
            }

        }

        private async void SetTagsFilterButton_OnClicked(object sender, EventArgs e)
        {
            _photosViewModel.ShowOptions = false;
            _photosViewModel.PageNumber = 1;
            if (TagFilterPicker.SelectedIndex == -1)
            {
                _photosViewModel.TagFilter = "";
            }
            else
            {
                string selectedTag = TagFilterPicker.SelectedItem.ToString();
                _photosViewModel.TagFilter = selectedTag;
            }

            await Reload();
        }

        private async void ClearTagFilterButton_OnClicked(object sender, EventArgs e)
        {
            _photosViewModel.ShowOptions = false;
            _photosViewModel.PageNumber = 1;
            _photosViewModel.TagFilter = "";
            await Reload();
        }
    }
}
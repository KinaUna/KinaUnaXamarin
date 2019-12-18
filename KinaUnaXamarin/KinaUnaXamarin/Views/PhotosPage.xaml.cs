using System;
using System.Collections.Generic;
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
        private readonly PhotosViewModel _viewModel;
        private bool _reload = true;
        private double _screenWidth;
        private double _screenHeight;

        public PhotosPage()
        {
            InitializeComponent();
            _viewModel = new PhotosViewModel();
            _viewModel.UserInfo = OfflineDefaultData.DefaultUserInfo;
            ContainerStackLayout.BindingContext = _viewModel;
            BindingContext = _viewModel;
            TagFilterPicker.SelectedIndex = 0;
            
            MessagingCenter.Subscribe<SelectProgenyPage>(this, "Reload", async (sender) =>
            {
                await SetUserAndProgeny();
                _viewModel.PageNumber = 1;
                await Reload();
            });

            MessagingCenter.Subscribe<AccountViewModel>(this, "Reload", async (sender) =>
            {
                await SetUserAndProgeny();
                _viewModel.PageNumber = 1;
                await Reload();
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            PhotosListView.SelectedItem = null;
            if (_reload)
            {
                await SetUserAndProgeny();
                await Reload();
            }

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            var networkAccess = Connectivity.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess)
            {
                _viewModel.Online = true;
            }
            else
            {
                _viewModel.Online = false;
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
            _viewModel.UserInfo = OfflineDefaultData.DefaultUserInfo;

            string userEmail = await UserService.GetUserEmail();
            string userviewchild = await SecureStorage.GetAsync(Constants.UserViewChildKey);
            bool viewchildParsed = int.TryParse(userviewchild, out int viewChildId);

            if (viewchildParsed)
            {
                _viewModel.ViewChild = viewChildId;
                try
                {
                    _viewModel.Progeny = await App.Database.GetProgenyAsync(_viewModel.ViewChild);
                }
                catch (Exception)
                {
                    _viewModel.Progeny = await ProgenyService.GetProgeny(_viewModel.ViewChild);
                }

                _viewModel.UserInfo = await App.Database.GetUserInfoAsync(userEmail);
            }

            if (String.IsNullOrEmpty(_viewModel.UserInfo.Timezone))
            {
                _viewModel.UserInfo.Timezone = Constants.DefaultTimeZone;
            }
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(_viewModel.UserInfo.Timezone);
            }
            catch (Exception)
            {
                _viewModel.UserInfo.Timezone = TZConvert.WindowsToIana(_viewModel.UserInfo.Timezone);
            }
        }

        private async Task Reload()
        {
            _viewModel.IsBusy = true;
            await CheckAccount();
            await UpdatePhotos();
            var networkInfo = Connectivity.NetworkAccess;

            if (networkInfo == NetworkAccess.Internet)
            {
                // Connection to internet is available
                _viewModel.Online = true;
            }
            else
            {
                _viewModel.Online = false;
            }
            _viewModel.IsBusy = false;
        }

        private async Task CheckAccount()
        {
            string userEmail = await UserService.GetUserEmail();
            _viewModel.AccessToken = await UserService.GetAuthAccessToken();
            bool accessTokenCurrent = false;
            if (_viewModel.AccessToken != "")
            {
                accessTokenCurrent = await UserService.IsAccessTokenCurrent();

                if (!accessTokenCurrent)
                {
                    bool loginSuccess = await UserService.LoginIdsAsync();
                    if (loginSuccess)
                    {
                        _viewModel.AccessToken = await UserService.GetAuthAccessToken();
                        accessTokenCurrent = true;
                    }

                    await Reload();
                }
            }

            if (String.IsNullOrEmpty(_viewModel.AccessToken) || !accessTokenCurrent)
            {

                _viewModel.IsLoggedIn = false;
                _viewModel.AccessToken = "";
                _viewModel.UserInfo = OfflineDefaultData.DefaultUserInfo;

            }
            else
            {
                _viewModel.IsLoggedIn = true;
                _viewModel.UserInfo = await UserService.GetUserInfo(userEmail);
            }

            await SetUserAndProgeny();
            
            Progeny progeny = await ProgenyService.GetProgeny(_viewModel.ViewChild);
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(progeny.TimeZone);
            }
            catch (Exception)
            {
                progeny.TimeZone = TZConvert.WindowsToIana(progeny.TimeZone);
            }
            _viewModel.Progeny = progeny;

            List<Progeny> progenyList = await ProgenyService.GetProgenyList(userEmail);
            _viewModel.ProgenyCollection.Clear();
            _viewModel.CanUserAddItems = false;
            foreach (Progeny prog in progenyList)
            {
                _viewModel.ProgenyCollection.Add(prog);
                if (prog.Admins.ToUpper().Contains(_viewModel.UserInfo.UserEmail.ToUpper()))
                {
                    _viewModel.CanUserAddItems = true;
                }
            }

            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewModel.ViewChild);
        }

        private async Task UpdatePhotos()
        {
            _viewModel.IsBusy = true;
            if (_viewModel.PageNumber < 1)
            {
                _viewModel.PageNumber = 1;
            }
            // _viewModel.TagsCollection.Clear();
            _viewModel.ItemsPerPage = Preferences.Get(Constants.PhotosPerPage, 8);
            PicturePage photoPage = await ProgenyService.GetPicturePage(_viewModel.PageNumber, _viewModel.ItemsPerPage, _viewModel.ViewChild, _viewModel.UserAccessLevel, _viewModel.UserInfo.Timezone, 1, _viewModel.TagFilter);
            if (photoPage.PicturesList != null)
            {
                _viewModel.PhotoItems.ReplaceRange(photoPage.PicturesList);
                _viewModel.PageNumber = photoPage.PageNumber;
                _viewModel.PageCount = photoPage.TotalPages;
                PhotosListView.ScrollTo(0);

                if (!string.IsNullOrEmpty(photoPage.TagsList))
                {
                    List<string> tagsList = photoPage.TagsList.Split(',').ToList();
                    tagsList.Sort();
                    foreach (string tagString in tagsList)
                    {
                        if (!string.IsNullOrEmpty(tagString))
                        {
                            _viewModel.TagsCollection.Add(tagString);
                        }
                    }
                }
            }

            _viewModel.IsBusy = false;
        }
        
        private async void ProgenyToolBarItem_OnClicked(object sender, EventArgs e)
        {
            SelectProgenyPage selProPage = new SelectProgenyPage(_viewModel.ProgenyCollection);
            await Shell.Current.Navigation.PushModalAsync(selProPage);
        }

        private void OptionsToolBarItem_OnClicked(object sender, EventArgs e)
        {
            _viewModel.ShowOptions = !_viewModel.ShowOptions;
        }

        private async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var networkAccess = e.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess != _viewModel.Online)
            {
                _viewModel.Online = internetAccess;
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

            _viewModel.PageNumber--;
            if (_viewModel.PageNumber < 1)
            {
                _viewModel.PageNumber = _viewModel.PageCount;
            }
            await UpdatePhotos();
        }

        private async void OlderButton_OnClicked(object sender, EventArgs e)
        {
            _viewModel.PageNumber++;
            if (_viewModel.PageNumber > _viewModel.PageCount)
            {
                _viewModel.PageNumber = 1;
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
            _viewModel.ShowOptions = false;
            _viewModel.PageNumber = 1;
            if (TagFilterPicker.SelectedIndex < 1 )
            {
                _viewModel.TagFilter = "";
            }
            else
            {
                string selectedTag = TagFilterPicker.SelectedItem.ToString();
                _viewModel.TagFilter = selectedTag;
            }
            Preferences.Set(Constants.PhotosPerPage, _viewModel.ItemsPerPage);
            await Reload();
        }

        private async void ClearTagFilterButton_OnClicked(object sender, EventArgs e)
        {
            _viewModel.ShowOptions = false;
            _viewModel.PageNumber = 1;
            _viewModel.TagFilter = "";
            TagFilterPicker.SelectedIndex = 0;
            await Reload();
        }
    }
}
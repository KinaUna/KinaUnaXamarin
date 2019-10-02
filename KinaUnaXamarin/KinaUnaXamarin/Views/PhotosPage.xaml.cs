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
        private int _viewChild = Constants.DefaultChildId;
        private UserInfo _userInfo;
        private PhotosViewModel _photosViewModel;
        private string _accessToken;
        private bool _reload = true;
        private bool _online = true;
        public PhotosPage()
        {
            InitializeComponent();

            MessagingCenter.Subscribe<SelectProgenyPage>(this, "Reload", async (sender) =>
            {
                _photosViewModel.PageNumber = 1;
                await Reload();
            });

            MessagingCenter.Subscribe<AccountViewModel>(this, "Reload", async (sender) =>
            {
                _photosViewModel.PageNumber = 1;
                await Reload();
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            PhotosListView.SelectedItem = null;
            if (_reload)
            {
                _photosViewModel = new PhotosViewModel();
                _userInfo = OfflineDefaultData.DefaultUserInfo;
                ContainerStackLayout.BindingContext = _photosViewModel;
                BindingContext = _photosViewModel;
            }
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            var networkAccess = Connectivity.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess)
            {
                OfflineStackLayout.IsVisible = false;
            }
            else
            {
                OfflineStackLayout.IsVisible = true;
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

        private async Task Reload()
        {
            _photosViewModel.IsBusy = true;
            await CheckAccount();
            await UpdatePhotos();
            var networkInfo = Connectivity.NetworkAccess;

            if (networkInfo == NetworkAccess.Internet)
            {
                // Connection to internet is available
                _online = true;
                OfflineStackLayout.IsVisible = false;
            }
            else
            {
                _online = false;
                OfflineStackLayout.IsVisible = true;
            }
            _photosViewModel.IsBusy = false;
        }

        private async Task CheckAccount()
        {
            string userEmail = await UserService.GetUserEmail();
            _accessToken = await UserService.GetAuthAccessToken();
            bool accessTokenCurrent = false;
            if (_accessToken != "")
            {
                string accessTokenExpires = await UserService.GetAuthAccessTokenExpires();
                accessTokenCurrent = UserService.IsAccessTokenCurrent(accessTokenExpires);

                if (!accessTokenCurrent)
                {
                    bool loginSuccess = await UserService.LoginIdsAsync();
                    if (loginSuccess)
                    {
                        _accessToken = await UserService.GetAuthAccessToken();
                        accessTokenCurrent = true;
                    }

                    await Reload();
                }
            }

            if (String.IsNullOrEmpty(_accessToken) || !accessTokenCurrent)
            {

                _photosViewModel.IsLoggedIn = false;
                _photosViewModel.LoggedOut = true;
                _accessToken = "";
                _userInfo = OfflineDefaultData.DefaultUserInfo;

            }
            else
            {
                _photosViewModel.IsLoggedIn = true;
                _photosViewModel.LoggedOut = false;
                _userInfo = await UserService.GetUserInfo(userEmail);
            }

            string userviewchild = await SecureStorage.GetAsync(Constants.UserViewChildKey);
            bool viewchildParsed = int.TryParse(userviewchild, out _viewChild);
            if (!viewchildParsed)
            {
                _viewChild = _userInfo.ViewChild;
            }
            if (_viewChild == 0)
            {
                if (_userInfo.ViewChild != 0)
                {
                    _viewChild = _userInfo.ViewChild;
                }
                else
                {
                    _viewChild = Constants.DefaultChildId;
                }
            }

            if (String.IsNullOrEmpty(_userInfo.Timezone))
            {
                _userInfo.Timezone = Constants.DefaultTimeZone;
            }
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(_userInfo.Timezone);
            }
            catch (Exception)
            {
                _userInfo.Timezone = TZConvert.WindowsToIana(_userInfo.Timezone);
            }
            
            Progeny progeny = await ProgenyService.GetProgeny(_viewChild);
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
                if (prog.Admins.ToUpper().Contains(_userInfo.UserEmail.ToUpper()))
                {
                    _photosViewModel.CanUserAddItems = true;
                }
            }

            _photosViewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewChild);
        }

        private async Task UpdatePhotos()
        {
            _photosViewModel.IsBusy = true;
            if (_photosViewModel.PageNumber < 1)
            {
                _photosViewModel.PageNumber = 1;
            }
            _photosViewModel.TagsCollection.Clear();
            
            PicturePage photoPage = await ProgenyService.GetPicturePage(_photosViewModel.PageNumber, 8, _viewChild, _photosViewModel.UserAccessLevel, _userInfo.Timezone, 1, _photosViewModel.TagFilter);
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
                        _photosViewModel.TagsCollection.Add(tagString);
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
            if (internetAccess != _online)
            {
                _online = internetAccess;
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
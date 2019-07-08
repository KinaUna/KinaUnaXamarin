using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FFImageLoading.Forms;
using KinaUnaXamarin.Behaviors;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using PanCardView;
using PanCardView.EventArgs;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PhotoDetailPage : ContentPage
    {
        PhotoDetailViewModel _photoDetailViewModel = new PhotoDetailViewModel();
        private UserInfo _userInfo;
        private string _accessToken;
        private int _viewChild = Constants.DefaultChildId;
        private bool _online = true;

        public PhotoDetailPage(int pictureId)
        {
            InitializeComponent();
            _photoDetailViewModel = new PhotoDetailViewModel();
            _photoDetailViewModel.CurrentPictureId = pictureId;
            BindingContext = _photoDetailViewModel;
            ContentGrid.BindingContext = _photoDetailViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Reload();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;

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
        }

        private async Task Reload()
        {
            _photoDetailViewModel.IsBusy = true;
            await CheckAccount();

            PictureViewModel pictureViewModel = await ProgenyService.GetPictureViewModel(
                _photoDetailViewModel.CurrentPictureId, _photoDetailViewModel.UserAccessLevel, _userInfo.Timezone, 1);
            _photoDetailViewModel.PhotoItems.Add(pictureViewModel);
            
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
            
            _photoDetailViewModel.IsBusy = false;

        }

        private async Task LoadNewer()
        {
            _photoDetailViewModel.CanLoadMore = false;
            PictureViewModel pictureViewModel = _photoDetailViewModel.PhotoItems.FirstOrDefault();
            if (pictureViewModel != null)
            {
                PictureViewModel pictureViewModel2 = await ProgenyService.GetPictureViewModel(
                    pictureViewModel.PrevPicture, _photoDetailViewModel.UserAccessLevel, _userInfo.Timezone, 1);
                _photoDetailViewModel.PhotoItems.Insert(0, pictureViewModel2);
                if (_photoDetailViewModel.PhotoItems.Count > 10)
                {
                    _photoDetailViewModel.PhotoItems.RemoveAt(_photoDetailViewModel.PhotoItems.Count -1);
                }
            }

            _photoDetailViewModel.CanLoadMore = true;
        }

        private async Task LoadOlder()
        {
            _photoDetailViewModel.CanLoadMore = false;
            PictureViewModel pictureViewModel = _photoDetailViewModel.PhotoItems.LastOrDefault();
            if (pictureViewModel != null)
            {
                PictureViewModel pictureViewModel2 = await ProgenyService.GetPictureViewModel(
                    pictureViewModel.NextPicture, _photoDetailViewModel.UserAccessLevel, _userInfo.Timezone, 1);
                _photoDetailViewModel.PhotoItems.Add(pictureViewModel2);
                if (_photoDetailViewModel.PhotoItems.Count > 10)
                {
                    _photoDetailViewModel.PhotoItems.RemoveAt(0);
                }
            }

            _photoDetailViewModel.CanLoadMore = true;
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

                _photoDetailViewModel.IsLoggedIn = false;
                _photoDetailViewModel.LoggedOut = true;
                _accessToken = "";
                _userInfo = OfflineDefaultData.DefaultUserInfo;

            }
            else
            {
                _photoDetailViewModel.IsLoggedIn = true;
                _photoDetailViewModel.LoggedOut = false;
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
            _photoDetailViewModel.Progeny = progeny;

            _photoDetailViewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewChild);
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

        private async void CardsView_OnItemAppearing(CardsView view, ItemAppearingEventArgs args)
        {
            _photoDetailViewModel.IsZoomed = false;
            _photoDetailViewModel.IsBusy = true;
            // var ciView = view.CurrentView..SingleOrDefault(c => c.GetType() == typeof(CachedImage));
            CachedImage ciView = (CachedImage)view.CurrentView.FindByName("CardCachedImage");
            if (ciView != null)
            {
                CachedImage ci = ciView as CachedImage;
                if (ci.Behaviors[0] is MultiTouchBehavior mtb)
                {
                    mtb.OnAppearing();
                }
            }

            if (_photoDetailViewModel.CanLoadMore && _photoDetailViewModel.CurrentIndex < 1)
            {
                await LoadNewer();
               
            }

            if (_photoDetailViewModel.CanLoadMore && _photoDetailViewModel.CurrentIndex > _photoDetailViewModel.PhotoItems.Count - 2)
            {
                await LoadOlder();
                
            }

            _photoDetailViewModel.IsBusy = false;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called
            _photoDetailViewModel.ImageHeight = height;
            _photoDetailViewModel.ImageWidth = width;
        }
        
    }
}
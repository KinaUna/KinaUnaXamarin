using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public partial class VideoDetailPage : ContentPage
    {
        VideoDetailViewModel _viewModel = new VideoDetailViewModel();
        private UserInfo _userInfo;
        private string _accessToken;
        private int _viewChild = Constants.DefaultChildId;
        private bool _online = true;
        private bool _modalShowing;

        public VideoDetailPage(int videoId)
        {
            InitializeComponent();
            _viewModel = new VideoDetailViewModel();
            _viewModel.CurrentVideoId = videoId;
            BindingContext = _viewModel;
            ContentGrid.BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
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

            if (!_modalShowing)
            {
                await Reload();
            }
            else
            {
                _modalShowing = false;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
            _modalShowing = true;
        }

        private async Task Reload()
        {
            _viewModel.IsBusy = true;
            await CheckAccount();

            VideoViewModel videoViewModel = await ProgenyService.GetVideoViewModel(
                _viewModel.CurrentVideoId, _viewModel.UserAccessLevel, _userInfo.Timezone, 1);
            _viewModel.VideoItems.Add(videoViewModel);
            _viewModel.CurrentVideoViewModel = videoViewModel;

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

            _viewModel.IsBusy = false;

        }

        private async Task LoadNewer()
        {
            _viewModel.CanLoadMore = false;
            VideoViewModel videoViewModel = _viewModel.VideoItems.FirstOrDefault();
            if (videoViewModel != null)
            {
                VideoViewModel videoViewModel2 = await ProgenyService.GetVideoViewModel(
                    videoViewModel.PrevVideo, _viewModel.UserAccessLevel, _userInfo.Timezone, 1);
                _viewModel.VideoItems.Insert(0, videoViewModel2);
                if (_viewModel.VideoItems.Count > 10)
                {
                    _viewModel.VideoItems.RemoveAt(_viewModel.VideoItems.Count - 1);
                }
            }

            _viewModel.CanLoadMore = true;
        }

        private async Task LoadOlder()
        {
            _viewModel.CanLoadMore = false;
            VideoViewModel videoViewModel = _viewModel.VideoItems.LastOrDefault();
            if (videoViewModel != null)
            {
                VideoViewModel videoViewModel2 = await ProgenyService.GetVideoViewModel(
                    videoViewModel.NextVideo, _viewModel.UserAccessLevel, _userInfo.Timezone, 1);
                _viewModel.VideoItems.Add(videoViewModel2);
                if (_viewModel.VideoItems.Count > 10)
                {
                    _viewModel.VideoItems.RemoveAt(0);
                }
            }

            _viewModel.CanLoadMore = true;
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

                _viewModel.IsLoggedIn = false;
                _viewModel.LoggedOut = true;
                _accessToken = "";
                _userInfo = OfflineDefaultData.DefaultUserInfo;

            }
            else
            {
                _viewModel.IsLoggedIn = true;
                _viewModel.LoggedOut = false;
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
            _viewModel.Progeny = progeny;

            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewChild);
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

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called
            _viewModel.ImageHeight = height;
            _viewModel.ImageWidth = width;
        }

        private async void CardsView_OnItemAppearing(CardsView view, ItemAppearingEventArgs args)
        {
            _viewModel.IsZoomed = false;
            _viewModel.IsBusy = true;
            
            if (_viewModel.CanLoadMore && _viewModel.CurrentIndex < 1)
            {
                await LoadNewer();

            }

            if (_viewModel.CanLoadMore && _viewModel.CurrentIndex > _viewModel.VideoItems.Count - 2)
            {
                await LoadOlder();

            }

            _viewModel.CurrentVideoViewModel = _viewModel.VideoItems[_viewModel.CurrentIndex];
            PictureTime picTime = new PictureTime(new DateTime(2018, 02, 18, 20, 18, 00), new DateTime(2018, 02, 18, 20, 18, 00), TimeZoneInfo.FindSystemTimeZoneById(_viewModel.Progeny.TimeZone));
            if (_viewModel.CurrentVideoViewModel.VideoTime != null && _viewModel.Progeny.BirthDay.HasValue)
            {
                DateTime picTimeBirthday = new DateTime(_viewModel.Progeny.BirthDay.Value.Ticks, DateTimeKind.Unspecified);

                picTime = new PictureTime(picTimeBirthday, _viewModel.CurrentVideoViewModel.VideoTime, TimeZoneInfo.FindSystemTimeZoneById(_viewModel.Progeny.TimeZone));
                _viewModel.PicTimeValid = true;
                _viewModel.PicYears = picTime.CalcYears();
                _viewModel.PicMonths = picTime.CalcMonths();
                _viewModel.PicWeeks = picTime.CalcWeeks();
                _viewModel.PicDays = picTime.CalcDays();
                _viewModel.PicHours = picTime.CalcHours();
                _viewModel.PicMinutes = picTime.CalcMinutes();
            }
            _viewModel.IsBusy = false;
        }

        private double y;

        private void FrameOnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            // Source: https://github.com/rlingineni/Forms-BottomSheet/blob/master/XamJuly/MainPage.xaml.cs

            // Handle the pan
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    // Translate and ensure we don't y + e.TotalY pan beyond the wrapped user interface element bounds.
                    var translateY = Math.Max(Math.Min(0, y + e.TotalY), -Math.Abs((Height * .25) - Height));
                    BottomSheetFrame.TranslateTo(BottomSheetFrame.X, translateY, 20);
                    break;
                case GestureStatus.Completed:
                    // Store the translation applied during the pan
                    y = BottomSheetFrame.TranslationY;

                    //at the end of the event - snap to the closest location
                    var finalTranslation = Math.Max(Math.Min(0, -1000), -Math.Abs(getClosestLockState(e.TotalY + y)));

                    //depending on Swipe Up or Down - change the snapping animation
                    if (isSwipeUp(e))
                    {
                        BottomSheetFrame.TranslateTo(BottomSheetFrame.X, finalTranslation, 250, Easing.SpringIn);
                    }
                    else
                    {
                        BottomSheetFrame.TranslateTo(BottomSheetFrame.X, finalTranslation, 250, Easing.SpringOut);
                    }


                    y = BottomSheetFrame.TranslationY;

                    break;
            }
        }

        private bool isSwipeUp(PanUpdatedEventArgs e)
        {
            if (e.TotalY < 0)
            {
                return true;
            }
            return false;
        }

        private double getClosestLockState(double TranslationY)
        {
            //Play with these values to adjust the locking motions - this will change depending on the amount of content ona  apge
            var lockStates = new double[] { 0, .1, .2, .3, .4, .5, .6, .85 };

            //get the current proportion of the sheet in relation to the screen
            var distance = Math.Abs(TranslationY);
            var currentProportion = distance / Height;

            //calculate which lockstate it's the closest to
            var smallestDistance = 10000.0;
            var closestIndex = 0;
            for (var i = 0; i < lockStates.Length; i++)
            {
                var state = lockStates[i];
                var absoluteDistance = Math.Abs(state - currentProportion);
                if (absoluteDistance < smallestDistance)
                {
                    smallestDistance = absoluteDistance;
                    closestIndex = i;
                }
            }

            var selectedLockState = lockStates[closestIndex];
            var TranslateToLockState = GetProportionCoordinate(selectedLockState);

            return TranslateToLockState;
        }

        private double GetProportionCoordinate(double proportion)
        {
            return proportion * Height;
        }

        private async void CommentsClicked(object sender, EventArgs e)
        {
            CommentsPage commentsPage = new CommentsPage(_viewModel.CurrentVideoViewModel.CommentThreadNumber);
            await Shell.Current.Navigation.PushModalAsync(commentsPage);
        }

        private void FrameOnTap(object sender, EventArgs e)
        {
            y = BottomSheetFrame.TranslationY;
            if (Math.Abs(y) < Height / 10)
            {
                var finalTranslation = Math.Max(Math.Min(0, -1000), -Math.Abs(getClosestLockState(Height / 3)));
                BottomSheetFrame.TranslateTo(BottomSheetFrame.X, finalTranslation, 250, Easing.SpringIn);
            }
            else
            {
                BottomSheetFrame.TranslateTo(BottomSheetFrame.X, 0, 250, Easing.SpringOut);
            }


            y = BottomSheetFrame.TranslationY;
        }
    }
}
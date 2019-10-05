using System;
using System.Linq;
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
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PhotoDetailPage : ContentPage
    {
        private readonly PhotoDetailViewModel _photoDetailViewModel = new PhotoDetailViewModel();
        private UserInfo _userInfo;
        private string _accessToken;
        private int _viewChild = Constants.DefaultChildId;
        private bool _online = true;
        private bool _modalShowing;

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
            _photoDetailViewModel.IsBusy = true;
            await CheckAccount();

            PictureViewModel pictureViewModel = await ProgenyService.GetPictureViewModel(
                _photoDetailViewModel.CurrentPictureId, _photoDetailViewModel.UserAccessLevel, _userInfo.Timezone, 1);
            _photoDetailViewModel.PhotoItems.Add(pictureViewModel);
            _photoDetailViewModel.CurrentPictureViewModel = pictureViewModel;

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
        
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called
            _photoDetailViewModel.ImageHeight = height;
            _photoDetailViewModel.ImageWidth = width;
            
            if (height > width)
            {
                LocationMap.WidthRequest = width * 0.9;
                DataStackLayout.WidthRequest = width * 0.9;
                InfoStackLayout.Orientation = StackOrientation.Vertical;
                _detailsHeight = LocationMap.Height + DataStackLayout.Height;
            }
            else
            {
                LocationMap.WidthRequest = width * 0.45;
                DataStackLayout.WidthRequest = width * 0.45;
                InfoStackLayout.Orientation = StackOrientation.Horizontal;
                _detailsHeight = LocationMap.Height;
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

                if (ciView.Behaviors[0] is MultiTouchBehavior mtb)
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

            _photoDetailViewModel.CurrentPictureViewModel = _photoDetailViewModel.PhotoItems[_photoDetailViewModel.CurrentIndex];
            
            PictureTime picTime = new PictureTime(new DateTime(2018, 02, 18, 20, 18, 00), new DateTime(2018, 02, 18, 20, 18, 00), TimeZoneInfo.FindSystemTimeZoneById(_photoDetailViewModel.Progeny.TimeZone));
            if (_photoDetailViewModel.CurrentPictureViewModel.PictureTime != null && _photoDetailViewModel.Progeny.BirthDay.HasValue)
            {
                DateTime picTimeBirthday = new DateTime(_photoDetailViewModel.Progeny.BirthDay.Value.Ticks, DateTimeKind.Unspecified);

                picTime = new PictureTime(picTimeBirthday, _photoDetailViewModel.CurrentPictureViewModel.PictureTime, TimeZoneInfo.FindSystemTimeZoneById(_photoDetailViewModel.Progeny.TimeZone));
                _photoDetailViewModel.PicTimeValid = true;
                _photoDetailViewModel.PicYears = picTime.CalcYears();
                _photoDetailViewModel.PicMonths = picTime.CalcMonths();
                _photoDetailViewModel.PicWeeks = picTime.CalcWeeks();
                _photoDetailViewModel.PicDays = picTime.CalcDays();
                _photoDetailViewModel.PicHours = picTime.CalcHours();
                _photoDetailViewModel.PicMinutes = picTime.CalcMinutes();
            }

            LocationMap.Pins.Clear();
            if (!string.IsNullOrEmpty(_photoDetailViewModel.CurrentPictureViewModel.Latitude) &&
                !string.IsNullOrEmpty(_photoDetailViewModel.CurrentPictureViewModel.Longtitude))
            {
                LocationMap.IsVisible = true;
                double lat;
                double lon;
                bool latParsed = double.TryParse(_photoDetailViewModel.CurrentPictureViewModel.Latitude, out lat);
                bool lonParsed = double.TryParse(_photoDetailViewModel.CurrentPictureViewModel.Longtitude, out lon);
                if (latParsed && lonParsed)
                {
                    Position position = new Position(lat, lon);
                    Pin pin = new Pin();
                    pin.Position = position;
                    pin.Label = _photoDetailViewModel.CurrentPictureViewModel.Location;
                    pin.Type = PinType.SavedPin;
                    LocationMap.Pins.Add(pin);
                    LocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(2)));
                }
            }
            else
            {
                LocationMap.IsVisible = false;
            }
            _photoDetailViewModel.IsBusy = false;
        }

        private async void PhotoCarousel_OnItemDisappearing(CardsView view, ItemDisappearingEventArgs args)
        {
            CachedImage ciView = (CachedImage)view.CurrentView.FindByName("CardCachedImage");
            if (ciView != null)
            {

                if (ciView.Behaviors[0] is MultiTouchBehavior mtb)
                {
                    await mtb.OnDisAppearing();
                }
            }
        }

        private double y;
        private double _detailsHeight;

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
            var lockStates = new double[] { 0, .1, .2, .3, .4, .5, .6, .7, .8, .9 };

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
            var translateToLockState = GetProportionCoordinate(selectedLockState);

            return translateToLockState;
        }

        private double GetProportionCoordinate(double proportion)
        {
            return proportion * Height;
        }

        private async void CommentsClicked(object sender, EventArgs e)
        {
            if (_photoDetailViewModel.IsLoggedIn)
            {
                CommentsPage commentsPage = new CommentsPage(_photoDetailViewModel.CurrentPictureViewModel.CommentThreadNumber);
                await Shell.Current.Navigation.PushModalAsync(commentsPage);
            }
        }

        private void FrameOnTap(object sender, EventArgs e)
        {
            y = BottomSheetFrame.TranslationY;
            if (Math.Abs(y) < Height / 10)
            {
                var finalTranslation = Math.Max(Math.Min(0, -1000), -Math.Abs(getClosestLockState(_detailsHeight + 15)));
                BottomSheetFrame.TranslateTo(BottomSheetFrame.X, finalTranslation, 350, Easing.SpringIn);
            }
            else
            {
                BottomSheetFrame.TranslateTo(BottomSheetFrame.X, 0, 350, Easing.SpringOut);
            }


            y = BottomSheetFrame.TranslationY;
        }
    }
}
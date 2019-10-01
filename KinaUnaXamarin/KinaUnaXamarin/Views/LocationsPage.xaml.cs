using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KinaUnaXamarin.Extensions;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;
using Location = KinaUnaXamarin.Models.KinaUna.Location;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LocationsPage : ContentPage
    {
        private int _viewChild = Constants.DefaultChildId;
        private UserInfo _userInfo;
        private LocationsViewModel _viewModel;
        private string _accessToken;
        private bool _reload = true;
        private bool _online = true;
        private double _screenWidth;
        private double _screenHeight;

        public LocationsPage()
        {
            InitializeComponent();

            MessagingCenter.Subscribe<SelectProgenyPage>(this, "Reload", async (sender) =>
            {
                _viewModel.PageNumber = 1;
                await Reload();
            });

            MessagingCenter.Subscribe<AccountViewModel>(this, "Reload", async (sender) =>
            {
                _viewModel.PageNumber = 1;
                await Reload();
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_reload)
            {
                _viewModel = new LocationsViewModel();
                _userInfo = OfflineDefaultData.DefaultUserInfo;
                ContainerGrid.BindingContext = _viewModel;
                BindingContext = _viewModel;
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
            _viewModel.IsBusy = true;
            await CheckAccount();

            await UpdateLocations();
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

            List<Progeny> progenyList = await ProgenyService.GetProgenyList(userEmail);
            _viewModel.ProgenyCollection.Clear();
            _viewModel.CanUserAddItems = false;
            foreach (Progeny prog in progenyList)
            {
                _viewModel.ProgenyCollection.Add(prog);
                if (prog.Admins.ToUpper().Contains(_userInfo.UserEmail.ToUpper()))
                {
                    _viewModel.CanUserAddItems = true;
                }
            }

            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewChild);
        }

        private async Task UpdateLocations()
        {
            _viewModel.IsBusy = true;
            if (_viewModel.PageNumber < 1)
            {
                _viewModel.PageNumber = 1;
            }
            _viewModel.LocationItems.Clear();
            LocationsMap.Pins.Clear();
            List<Location> allLocations = await ProgenyService.GetLocationsList(_viewChild, _viewModel.UserAccessLevel);
            allLocations = allLocations.OrderByDescending(l => l.Date).ToList();
            if (allLocations != null && allLocations.Count > 0)
            {
                List<double> latitudes = new List<double>();
                List<double> longitudes = new List<double>();

                foreach (Location location in allLocations)
                {
                    if (location.Date.HasValue)
                    {
                        location.Date = TimeZoneInfo.ConvertTimeFromUtc(location.Date.Value,
                            TimeZoneInfo.FindSystemTimeZoneById(_userInfo.Timezone));
                    }

                    if (location.Position != null)
                    {
                        Pin pin = new Pin();
                        pin.Position = location.Position;
                        pin.Label = location.Name;
                        pin.Type = PinType.SavedPin;
                        LocationsMap.Pins.Add(pin);
                        latitudes.Add(pin.Position.Latitude);
                        longitudes.Add(pin.Position.Longitude);
                    }
                }

                if (latitudes.Any() && longitudes.Any())
                {
                    double lowestLat = latitudes.Min();
                    double highestLat = latitudes.Max();
                    double lowestLong = longitudes.Min();
                    double highestLong = longitudes.Max();
                    double finalLat = (lowestLat + highestLat) / 2;
                    double finalLong = (lowestLong + highestLong) / 2;
                    double distance = DistanceCalculation.GeoCodeCalc.CalcDistance(lowestLat, lowestLong, highestLat,
                        highestLong, DistanceCalculation.GeoCodeCalcMeasurement.Kilometers);
                    LocationsMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(finalLat, finalLong),
                        Distance.FromKilometers(distance)));

                    _viewModel.LocationItems.ReplaceRange(allLocations);
                }
                
            }

            _viewModel.IsBusy = false;
        }

        private void PinOnClicked(object sender, EventArgs e)
        {
            Pin clickedPin = sender as Pin;
            if (clickedPin != null)
            {
                Location clickedLocation = _viewModel.LocationItems.SingleOrDefault(l => l.Name == (sender as Pin).Label);
                LocationCollectionView.ScrollTo(clickedLocation, position:ScrollToPosition.Start);
            }
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

        private void LocationCollectionView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Todo: Navigate to LocationDetailsPage.
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called
            if (_screenWidth != width || _screenHeight != height)
            {
                _screenWidth = width;
                _screenHeight = height;

                if (width > height)
                {
                    LocationsMap.HeightRequest = _screenHeight - 15;
                    LocationsMap.WidthRequest = _screenWidth / 2.5;
                    ContainerStackLayout.Orientation = StackOrientation.Horizontal;
                }
                else
                {
                    LocationsMap.HeightRequest = _screenHeight / 2.5;
                    LocationsMap.WidthRequest = _screenWidth - 15;
                    ContainerStackLayout.Orientation = StackOrientation.Vertical;
                }
            }
        }

        private void LocationsMap_OnPinClicked(object sender, PinClickedEventArgs e)
        {
            
            if (e.Pin != null)
            {
                Location clickedLocation = _viewModel.LocationItems.SingleOrDefault(l => l.Name == (e.Pin).Label);
                if (clickedLocation != null)
                {
                    LocationCollectionView.ScrollTo(clickedLocation, position: ScrollToPosition.Start);
                }
            }
        }
    }
}
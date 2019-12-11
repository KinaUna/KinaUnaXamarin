using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class PhotoLocationsPage : ContentPage
    {
        private int _viewChild = Constants.DefaultChildId;
        private UserInfo _userInfo;
        private PhotoLocationsViewModel _viewModel;
        private string _accessToken;
        private bool _reload = true;
        private bool _online = true;
        private double _screenWidth;
        private double _screenHeight;

        public PhotoLocationsPage()
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
                _viewModel = new PhotoLocationsViewModel();
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

            await CheckAccount();

            await UpdateLocations();
            
            _viewModel.IsBusy = false;
        }

        private async Task CheckAccount()
        {
            string userEmail = await UserService.GetUserEmail();
            _accessToken = await UserService.GetAuthAccessToken();
            bool accessTokenCurrent = false;
            if (_accessToken != "")
            {
                accessTokenCurrent = await UserService.IsAccessTokenCurrent();

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

            LocationsMap.Pins.Clear();
            _viewModel.PictureItems.Clear();
            _viewModel.PictureItems.Clear();
            List<Picture> allPictures = await ProgenyService.GetPicturesList(_viewChild, _viewModel.UserAccessLevel, _userInfo.Timezone);
            List<double> latitudes = new List<double>();
            List<double> longitudes = new List<double>();
            // List<Picture> validPictures = new List<Picture>();
            if (allPictures != null && allPictures.Count > 0)
            {
                foreach (Picture picture in allPictures)
                {
                    if (!string.IsNullOrEmpty(picture.Latitude) && !string.IsNullOrEmpty(picture.Longtitude))
                    {
                        double lat;
                        bool validLat = double.TryParse(picture.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out lat);
                        double lon;
                        bool validLon = double.TryParse(picture.Longtitude, NumberStyles.Any, CultureInfo.InvariantCulture, out lon);

                        if (validLat && validLon)
                        {
                            Position position = new Position(lat, lon);
                            Pin pin = new Pin();
                            pin.Position = position;
                            pin.Label = picture.Location;
                            pin.Type = PinType.Place;
                            pin.Tag = picture;
                            bool nearAnotherPin = false;
                            foreach (Pin mapPin in LocationsMap.Pins)
                            {
                                if (DistanceCalculation.GeoCodeCalc.CalcDistance(pin.Position.Latitude,
                                        pin.Position.Longitude, mapPin.Position.Latitude, mapPin.Position.Longitude,
                                        DistanceCalculation.GeoCodeCalcMeasurement.Kilometers) < _viewModel.NearbyDistance + 0.005)
                                {
                                    nearAnotherPin = true;
                                    break;
                                }
                            }

                            if (!nearAnotherPin)
                            {
                                LocationsMap.Pins.Add(pin);
                                latitudes.Add(pin.Position.Latitude);
                                longitudes.Add(pin.Position.Longitude);
                                
                            }
                            //validPictures.Add(picture);
                            _viewModel.PictureItems.Add(picture);
                        }
                    }
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

                //_viewModel.PictureItems.ReplaceRange(validPictures);
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

        private async void LocationCollectionView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LocationCollectionView.SelectedItem is Picture selectedPicture)
            {
                PhotoDetailPage photoPage = new PhotoDetailPage(selectedPicture.PictureId);
                // Reset selection
                LocationCollectionView.SelectedItem = null;
                await Shell.Current.Navigation.PushModalAsync(photoPage);
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called

            if (Device.RuntimePlatform == Device.UWP)
            {
                if (_screenWidth != Application.Current.MainPage.Width || _screenHeight != Application.Current.MainPage.Height)
                {
                    _screenWidth = Application.Current.MainPage.Width;
                    _screenHeight = Application.Current.MainPage.Height;

                    if (_screenWidth > _screenHeight)
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
            else
            {
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
            
        }

        private void LocationsMap_OnPinClicked(object sender, PinClickedEventArgs e)
        {
            
            if (e.Pin != null)
            {
                LocationCollectionView.ScrollTo(0);
                _viewModel.NearbyPictures.Clear();
                Picture clickedPicture = e.Pin.Tag as Picture;
                if (clickedPicture != null)
                {
                    double clickedLat;
                    bool clickedLatValid = double.TryParse(clickedPicture.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out clickedLat);
                    double clickedLon;
                    bool clickedLonValid = double.TryParse(clickedPicture.Longtitude, NumberStyles.Any, CultureInfo.InvariantCulture, out clickedLon);

                    if (clickedLatValid && clickedLonValid)
                    {
                        foreach (Picture picture in _viewModel.PictureItems)
                        {
                            double lat;
                            bool validLat = double.TryParse(picture.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out lat);
                            double lon;
                            bool validLon = double.TryParse(picture.Longtitude, NumberStyles.Any, CultureInfo.InvariantCulture, out lon);
                            if (validLat && validLon)
                            {
                                Position position = new Position(lat, lon);
                                var distance = DistanceCalculation.GeoCodeCalc.CalcDistance(clickedLat, clickedLon, lat,
                                    lon, DistanceCalculation.GeoCodeCalcMeasurement.Kilometers);
                                if (distance < _viewModel.NearbyDistance)
                                {
                                    _viewModel.NearbyPictures.Add(picture);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using Plugin.Multilingual;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LocationDetailPage : ContentPage
    {
        private readonly LocationDetailViewModel _viewModel = new LocationDetailViewModel();
        private UserInfo _userInfo;
        private string _accessToken;
        private int _viewChild = Constants.DefaultChildId;
        private bool _online = true;

        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public LocationDetailPage(Models.KinaUna.Location locationItem)
        {
            _viewModel = new LocationDetailViewModel();
            InitializeComponent();
            _viewModel.CurrentLocationId = locationItem.LocationId;
            _viewModel.AccessLevel = locationItem.AccessLevel;
            _viewModel.City = locationItem.City;
            _viewModel.Country = locationItem.Country;
            _viewModel.County = locationItem.County;
            _viewModel.District = locationItem.District;
            _viewModel.HouseNumber = locationItem.HouseNumber;
            _viewModel.Latitude = locationItem.Latitude.ToString(CultureInfo.InvariantCulture);
            _viewModel.Longitude = locationItem.Longitude.ToString(CultureInfo.InvariantCulture);
            _viewModel.Name = locationItem.Name;
            _viewModel.Notes = locationItem.Notes;
            _viewModel.PostalCode = locationItem.PostalCode;
            _viewModel.State = locationItem.State;
            _viewModel.Street = locationItem.StreetName;
            _viewModel.Tags = locationItem.Tags;
            
            BindingContext = _viewModel;
            
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

            await CheckAccount();
            await Reload();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
            
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
            if (_viewModel.UserAccessLevel == 0)
            {
                _viewModel.CanUserEditItems = true;
            }
        }

        private async Task Reload()
        {
            _viewModel.IsBusy = true;
            await CheckAccount();
            _viewModel.CurrentLocation =
                await ProgenyService.GetLocation(_viewModel.CurrentLocationId, _accessToken, _userInfo.Timezone);

            _viewModel.AccessLevel = _viewModel.CurrentLocation.AccessLevel;
            _viewModel.CurrentLocation.Progeny = await ProgenyService.GetProgeny(_viewModel.CurrentLocation.ProgenyId);

            if (_viewModel.CurrentLocation.Date.HasValue)
            {
                _viewModel.DateYear = _viewModel.CurrentLocation.Date.Value.Year;
                _viewModel.DateMonth = _viewModel.CurrentLocation.Date.Value.Month;
                _viewModel.DateDay = _viewModel.CurrentLocation.Date.Value.Day;
                _viewModel.DateHours = _viewModel.CurrentLocation.Date.Value.Hour;
                _viewModel.DateMinutes = _viewModel.CurrentLocation.Date.Value.Minute;

                LocationDatePicker.Date = new DateTime(_viewModel.DateYear, _viewModel.DateMonth, _viewModel.DateDay);
                LocationTimePicker.Time = new TimeSpan(_viewModel.CurrentLocation.Date.Value.Hour, _viewModel.CurrentLocation.Date.Value.Minute, 0);
            }

            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewModel.CurrentLocation.ProgenyId);
            if (_viewModel.UserAccessLevel == 0)
            {
                _viewModel.CanUserEditItems = true;
            }
            else
            {
                _viewModel.CanUserEditItems = false;
            }

            _viewModel.CurrentLocationId = _viewModel.CurrentLocation.LocationId;
            _viewModel.AccessLevel = _viewModel.CurrentLocation.AccessLevel;
            _viewModel.City = _viewModel.CurrentLocation.City;
            _viewModel.Country = _viewModel.CurrentLocation.Country;
            _viewModel.County = _viewModel.CurrentLocation.County;
            _viewModel.District = _viewModel.CurrentLocation.District;
            _viewModel.HouseNumber = _viewModel.CurrentLocation.HouseNumber;
            _viewModel.Latitude = _viewModel.CurrentLocation.Latitude.ToString(CultureInfo.InvariantCulture);
            _viewModel.Longitude = _viewModel.CurrentLocation.Longitude.ToString(CultureInfo.InvariantCulture);
            _viewModel.Name = _viewModel.CurrentLocation.Name;
            _viewModel.Notes = _viewModel.CurrentLocation.Notes;
            _viewModel.PostalCode = _viewModel.CurrentLocation.PostalCode;
            _viewModel.State = _viewModel.CurrentLocation.State;
            _viewModel.Street = _viewModel.CurrentLocation.StreetName;
            _viewModel.Tags = _viewModel.CurrentLocation.Tags;
            
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
        
        private async void EditButton_OnClicked(object sender, EventArgs e)
        {
            if (_viewModel.EditMode)
            {
                _viewModel.EditMode = false;
                _viewModel.IsBusy = true;

                DateTime locDate = new DateTime(_viewModel.DateYear, _viewModel.DateMonth, _viewModel.DateDay, _viewModel.DateHours, _viewModel.DateMinutes, 0);
                _viewModel.CurrentLocation.Date = locDate;
                _viewModel.CurrentLocation.Name = _viewModel.Name;
                _viewModel.CurrentLocation.StreetName = _viewModel.Street;
                _viewModel.CurrentLocation.HouseNumber = _viewModel.HouseNumber;
                _viewModel.CurrentLocation.District = _viewModel.District;
                _viewModel.CurrentLocation.City = _viewModel.City;
                _viewModel.CurrentLocation.PostalCode = _viewModel.PostalCode;
                _viewModel.CurrentLocation.County = _viewModel.County;
                _viewModel.CurrentLocation.State = _viewModel.State;
                _viewModel.CurrentLocation.Country = _viewModel.Country;
                double latitude = 0.0;
                bool latitudeParsed = double.TryParse(_viewModel.Latitude.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out latitude);
                if (latitudeParsed)
                {
                    _viewModel.CurrentLocation.Latitude = latitude;
                }
                double longitude = 0.0;
                bool longitudeParsed = double.TryParse(_viewModel.Longitude.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out longitude);
                if (longitudeParsed)
                {
                    _viewModel.CurrentLocation.Longitude = longitude;
                }

                _viewModel.CurrentLocation.Notes = _viewModel.Notes;
                _viewModel.CurrentLocation.Tags = _viewModel.Tags;
                _viewModel.CurrentLocation.AccessLevel = _viewModel.AccessLevel;

                // Save changes.
                Models.KinaUna.Location resultLocation = await ProgenyService.UpdateLocation(_viewModel.CurrentLocation);
                _viewModel.IsBusy = false;
                EditButton.Text = IconFont.CalendarEdit;
                if (resultLocation != null)  // Todo: Error message if update fails.
                {
                    MessageLabel.Text = "Location Item Updated"; // Todo: Translate
                    MessageLabel.BackgroundColor = Color.DarkGreen;
                    MessageLabel.IsVisible = true;
                    await Reload();
                }
            }
            else
            {
                EditButton.Text = IconFont.ContentSave;

                _viewModel.EditMode = true;
            }
        }

        private async void CancelButton_OnClicked(object sender, EventArgs e)
        {
            EditButton.Text = IconFont.AccountEdit;
            _viewModel.EditMode = false;
            await Reload();
        }

        private void LocationDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            _viewModel.DateYear = LocationDatePicker.Date.Year;
            _viewModel.DateMonth = LocationDatePicker.Date.Month;
            _viewModel.DateDay = LocationDatePicker.Date.Day;
            _viewModel.DateHours = LocationDatePicker.Date.Hour;
            _viewModel.DateMinutes = LocationDatePicker.Date.Minute;
        }

        private void LocationTimePicker_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _viewModel.DateHours = LocationTimePicker.Time.Hours;
            _viewModel.DateMinutes = LocationTimePicker.Time.Minutes;
        }
        
        private async void DeleteButton_OnClickedButton_OnClicked(object sender, EventArgs e)
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string confirmTitle = resmgr.Value.GetString("DeleteLocation", ci);
            string confirmMessage = resmgr.Value.GetString("DeleteLocationMessage", ci) + " ? ";
            string yes = resmgr.Value.GetString("Yes", ci);
            string no = resmgr.Value.GetString("No", ci); ;
            bool confirmDelete = await DisplayAlert(confirmTitle, confirmMessage, yes, no);
            if (confirmDelete)
            {
                _viewModel.IsBusy = true;
                _viewModel.EditMode = false;
                Models.KinaUna.Location deletedLocation = await ProgenyService.DeleteLocation(_viewModel.CurrentLocation);
                if (deletedLocation.LocationId == 0)
                {
                    _viewModel.EditMode = false;
                    // Todo: Show success message

                }
                else
                {
                    _viewModel.EditMode = true;
                    // Todo: Show failed message
                }
                _viewModel.IsBusy = false;
            }
        }
    }
}
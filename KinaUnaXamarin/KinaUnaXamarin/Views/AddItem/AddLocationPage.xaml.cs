using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels.AddItem;
using Plugin.Multilingual;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Location = KinaUnaXamarin.Models.KinaUna.Location;

namespace KinaUnaXamarin.Views.AddItem
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddLocationPage : ContentPage
    {
        private readonly AddLocationViewModel _viewModel;
        private bool _online = true;
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public AddLocationPage()
        {
            InitializeComponent();
            _viewModel = new AddLocationViewModel();
            _viewModel.Date = DateTime.Now;
            _viewModel.Time = DateTime.Now.TimeOfDay;
            BindingContext = _viewModel;
            ProgenyCollectionView.ItemsSource = _viewModel.ProgenyCollection;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            var networkAccess = Connectivity.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess)
            {
                _viewModel.Online = true;
                OfflineStackLayout.IsVisible = false;
            }
            else
            {
                _viewModel.Online = false;
                OfflineStackLayout.IsVisible = true;
            }

            await ProgenyService.GetProgenyList(await UserService.GetUserEmail());
            List<Progeny> progenyList = await ProgenyService.GetProgenyAdminList();
            if (progenyList.Any())
            {
                foreach (Progeny progeny in progenyList)
                {
                    _viewModel.ProgenyCollection.Add(progeny);
                }

                string userviewchild = await SecureStorage.GetAsync(Constants.UserViewChildKey);
                bool viewchildParsed = int.TryParse(userviewchild, out int viewChild);
                Progeny viewProgeny = _viewModel.ProgenyCollection.SingleOrDefault(p => p.Id == viewChild);
                if (viewProgeny != null)
                {
                    ProgenyCollectionView.SelectedItem =
                        _viewModel.ProgenyCollection.SingleOrDefault(p => p.Id == viewChild);
                    ProgenyCollectionView.ScrollTo(ProgenyCollectionView.SelectedItem);
                }
                else
                {
                    ProgenyCollectionView.SelectedItem = _viewModel.ProgenyCollection[0];
                }
            }
            _viewModel.AccessLevel = 0;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var networkAccess = e.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess != _online)
            {
                _viewModel.Online = false;
                OfflineStackLayout.IsVisible = true;
                SaveLocationButton.IsEnabled = false;
            }
            else
            {
                _viewModel.Online = true;
                OfflineStackLayout.IsVisible = false;
                SaveLocationButton.IsEnabled = true;
            }
        }

        private async void SaveLocationButton_OnClicked(object sender, EventArgs e)
        {
            Progeny progeny = ProgenyCollectionView.SelectedItem as Progeny;
            if (progeny == null)
            {
                return;
            }

            SaveLocationButton.IsEnabled = false;
            CancelLocationButton.IsEnabled = false;
            _viewModel.IsBusy = true;


            Location location = new Location();
            
            location.ProgenyId = progeny.Id;
            location.AccessLevel = _viewModel?.AccessLevel ?? 0;
            string userEmail = await UserService.GetUserEmail();
            UserInfo userinfo = await UserService.GetUserInfo(userEmail);
            location.Author = userinfo.UserId;
            location.Name = NameEntry?.Text ?? "";
            location.StreetName = StreetEntry?.Text ?? "";
            location.HouseNumber = HouseNumberEntry?.Text ?? "";
            location.District = DistrictEntry?.Text ?? "";
            location.City = CityEntry?.Text ?? "";
            location.PostalCode = PostalCodeEntry?.Text ?? "";
            location.County = CountyEntry?.Text ?? "";
            location.State = StateEntry?.Text ?? "";
            location.Country = CountryEntry?.Text ?? "";
            location.DateAdded = DateTime.UtcNow;
            double lat;
            double lon;
            bool latParsed = double.TryParse(LatitudeEntry.Text, out lat);
            bool lonParsed = double.TryParse(LongitudeEntry.Text, out lon);
            if (latParsed)
            {
                location.Latitude = lat;
            }

            if (lonParsed)
            {
                location.Longitude = lon;
            }
            location.Notes = NotesEditor?.Text ?? "";
            location.Tags = TagsEntry?.Text ?? "";

            DateTime locationTime = new DateTime(LocationDatePicker.Date.Year, LocationDatePicker.Date.Month, LocationDatePicker.Date.Day, LocationTimePicker.Time.Hours, LocationTimePicker.Time.Minutes, 0);
            location.Date = locationTime;
            Location newLocation = await ProgenyService.SaveLocation(location);
            
            ErrorLabel.IsVisible = true;
            if (newLocation.LocationId == 0)
            {
                var ci = CrossMultilingual.Current.CurrentCultureInfo;
                ErrorLabel.Text = resmgr.Value.GetString("ErrorLocationNotSaved", ci);
                ErrorLabel.BackgroundColor = Color.Red;
                SaveLocationButton.IsEnabled = true;
                SaveLocationButton.IsEnabled = true;

            }
            else
            {
                var ci = CrossMultilingual.Current.CurrentCultureInfo;
                ErrorLabel.Text = resmgr.Value.GetString("LocationSaved", ci) + newLocation.LocationId;
                ErrorLabel.BackgroundColor = Color.Green;
                SaveLocationButton.IsVisible = false;
                CancelLocationButton.Text = "Ok";
                CancelLocationButton.BackgroundColor = Color.FromHex("#4caf50");
                CancelLocationButton.IsEnabled = true;
            }

            _viewModel.IsBusy = false;
        }

        private async void CancelLocationButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        
        private void NameEntry_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (NameEntry.Text.Length >= 1 && _viewModel.Online)
            {
                SaveLocationButton.IsEnabled = true;
            }
            else
            {
                SaveLocationButton.IsEnabled = false;
            }
        }

        private async void GetMyLocationButton_OnClicked(object sender, EventArgs e)
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                var myLocation = await Geolocation.GetLocationAsync(request);

                if (myLocation != null)
                {
                    LatitudeEntry.Text = myLocation.Longitude.ToString();
                    LongitudeEntry.Text = myLocation.Longitude.ToString();
                }
            }
            catch (FeatureNotSupportedException)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException)
            {
                // Handle permission exception
            }
            catch (Exception)
            {
                // Unable to get location
            }
        }
    }
}
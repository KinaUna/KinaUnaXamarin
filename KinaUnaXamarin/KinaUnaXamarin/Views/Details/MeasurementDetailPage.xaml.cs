using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels.Details;
using Plugin.Multilingual;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views.Details
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MeasurementDetailPage
    {
        private readonly MeasurementDetailViewModel _viewModel;
        private UserInfo _userInfo;
        private string _accessToken;
        private int _viewChild = Constants.DefaultChildId;
        private bool _online = true;

        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public MeasurementDetailPage(Measurement measurementItem)
        {
            _viewModel = new MeasurementDetailViewModel();
            InitializeComponent();
            _viewModel.CurrentMeasurementId = measurementItem.MeasurementId;
            _viewModel.AccessLevel = measurementItem.AccessLevel;
            _viewModel.Height = measurementItem.Height.ToString(CultureInfo.CurrentCulture);
            _viewModel.Weight = measurementItem.Weight.ToString(CultureInfo.CurrentCulture);
            _viewModel.Circumference = measurementItem.Circumference.ToString(CultureInfo.CurrentCulture);
            _viewModel.HairColor = measurementItem.HairColor;
            _viewModel.EyeColor = measurementItem.EyeColor;
            _viewModel.Date = measurementItem.Date;
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
            _viewModel.CurrentMeasurement =
                await ProgenyService.GetMeasurement(_viewModel.CurrentMeasurementId, _accessToken);

            _viewModel.Date = _viewModel.CurrentMeasurement.Date;
            _viewModel.AccessLevel = _viewModel.CurrentMeasurement.AccessLevel;
            _viewModel.CurrentMeasurement.Progeny = _viewModel.Progeny = await ProgenyService.GetProgeny(_viewModel.CurrentMeasurement.ProgenyId);

            _viewModel.DateYear = _viewModel.CurrentMeasurement.Date.Year;
            _viewModel.DateMonth = _viewModel.CurrentMeasurement.Date.Month;
            _viewModel.DateDay = _viewModel.CurrentMeasurement.Date.Day;
            
            MeasurementDatePicker.Date = new DateTime(_viewModel.DateYear, _viewModel.DateMonth, _viewModel.DateDay);
            
            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewModel.CurrentMeasurement.ProgenyId);
            if (_viewModel.UserAccessLevel == 0)
            {
                _viewModel.CanUserEditItems = true;
            }
            else
            {
                _viewModel.CanUserEditItems = false;
            }

            _viewModel.CurrentMeasurementId = _viewModel.CurrentMeasurement.MeasurementId;
            _viewModel.AccessLevel = _viewModel.CurrentMeasurement.AccessLevel;
            _viewModel.Height = _viewModel.CurrentMeasurement.Height.ToString(CultureInfo.CurrentCulture);
            _viewModel.Weight = _viewModel.CurrentMeasurement.Weight.ToString(CultureInfo.CurrentCulture);
            _viewModel.Circumference = _viewModel.CurrentMeasurement.Circumference.ToString(CultureInfo.CurrentCulture);
            _viewModel.HairColor = _viewModel.CurrentMeasurement.HairColor;
            _viewModel.EyeColor = _viewModel.CurrentMeasurement.EyeColor;
            
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

                DateTime mesDate = new DateTime(_viewModel.DateYear, _viewModel.DateMonth, _viewModel.DateDay);
                _viewModel.CurrentMeasurement.Date = mesDate;
                _viewModel.CurrentMeasurement.HairColor = _viewModel.HairColor;
                _viewModel.CurrentMeasurement.EyeColor = _viewModel.EyeColor;

                bool heightParsed = double.TryParse(_viewModel.Height, NumberStyles.Any, CultureInfo.CurrentCulture, out double height);
                if (heightParsed)
                {
                    _viewModel.CurrentMeasurement.Height = height;
                }

                bool weightParsed = double.TryParse(_viewModel.Weight, NumberStyles.Any, CultureInfo.CurrentCulture, out double weight);
                if (weightParsed)
                {
                    _viewModel.CurrentMeasurement.Weight = weight;
                }

                bool circumferenceParsed = double.TryParse(_viewModel.Circumference, NumberStyles.Any, CultureInfo.CurrentCulture, out double circumference);
                if (circumferenceParsed)
                {
                    _viewModel.CurrentMeasurement.Circumference = circumference;
                }
                
                _viewModel.CurrentMeasurement.AccessLevel = _viewModel.AccessLevel;

                // Save changes.
                Measurement resultMeasurement = await ProgenyService.UpdateMeasurement(_viewModel.CurrentMeasurement);
                _viewModel.IsBusy = false;
                EditButton.Text = IconFont.CalendarEdit;
                if (resultMeasurement != null)  // Todo: Error message if update fails.
                {
                    MessageLabel.Text = "Measurement Updated"; // Todo: Translate
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

        private void MeasurementDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            _viewModel.DateYear = MeasurementDatePicker.Date.Year;
            _viewModel.DateMonth = MeasurementDatePicker.Date.Month;
            _viewModel.DateDay = MeasurementDatePicker.Date.Day;
            
        }
        
        private async void DeleteButton_OnClickedButton_OnClicked(object sender, EventArgs e)
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string confirmTitle = resmgr.Value.GetString("DeleteMeasurement", ci);
            string confirmMessage = resmgr.Value.GetString("DeleteMeasurementMessage", ci) + " ? ";
            string yes = resmgr.Value.GetString("Yes", ci);
            string no = resmgr.Value.GetString("No", ci);
            bool confirmDelete = await DisplayAlert(confirmTitle, confirmMessage, yes, no);
            if (confirmDelete)
            {
                _viewModel.IsBusy = true;
                _viewModel.EditMode = false;
                Measurement deletedMeasurement = await ProgenyService.DeleteMeasurement(_viewModel.CurrentMeasurement);
                if (deletedMeasurement.MeasurementId == 0)
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

        private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
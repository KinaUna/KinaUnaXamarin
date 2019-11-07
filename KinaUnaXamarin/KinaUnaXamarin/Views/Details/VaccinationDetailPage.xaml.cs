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
    public partial class VaccinationDetailPage : ContentPage
    {
        private readonly VaccinationDetailViewModel _viewModel = new VaccinationDetailViewModel();
        private UserInfo _userInfo;
        private string _accessToken;
        private int _viewChild = Constants.DefaultChildId;
        private bool _online = true;

        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public VaccinationDetailPage(Vaccination vaccinationItem)
        {
            
            InitializeComponent();
            _viewModel.CurrentVaccinationId = vaccinationItem.VaccinationId;
            _viewModel.AccessLevel = vaccinationItem.AccessLevel;
            _viewModel.Name = vaccinationItem.VaccinationName;
            _viewModel.Notes = vaccinationItem.Notes;
            _viewModel.Description = vaccinationItem.VaccinationDescription;
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
            _viewModel.CurrentVaccination =
                await ProgenyService.GetVaccination(_viewModel.CurrentVaccinationId, _accessToken, _userInfo.Timezone);

            _viewModel.AccessLevel = _viewModel.CurrentVaccination.AccessLevel;
            _viewModel.CurrentVaccination.Progeny = _viewModel.Progeny = await ProgenyService.GetProgeny(_viewModel.CurrentVaccination.ProgenyId);

            _viewModel.DateYear = _viewModel.CurrentVaccination.VaccinationDate.Year;
            _viewModel.DateMonth = _viewModel.CurrentVaccination.VaccinationDate.Month;
            _viewModel.DateDay = _viewModel.CurrentVaccination.VaccinationDate.Day;

            VaccinationDatePicker.Date = new DateTime(_viewModel.DateYear, _viewModel.DateMonth, _viewModel.DateDay);

            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewModel.CurrentVaccination.ProgenyId);
            if (_viewModel.UserAccessLevel == 0)
            {
                _viewModel.CanUserEditItems = true;
            }
            else
            {
                _viewModel.CanUserEditItems = false;
            }

            _viewModel.CurrentVaccinationId = _viewModel.CurrentVaccination.VaccinationId;
            _viewModel.AccessLevel = _viewModel.CurrentVaccination.AccessLevel;
            _viewModel.Name = _viewModel.CurrentVaccination.VaccinationName;
            _viewModel.Description = _viewModel.CurrentVaccination.VaccinationDescription;
            _viewModel.Notes = _viewModel.CurrentVaccination.Notes;
            
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

                DateTime vacDate = new DateTime(_viewModel.DateYear, _viewModel.DateMonth, _viewModel.DateDay);
                _viewModel.CurrentVaccination.VaccinationDate = vacDate;
                _viewModel.CurrentVaccination.VaccinationName = _viewModel.Name;
                _viewModel.CurrentVaccination.VaccinationDescription = _viewModel.Description;
                _viewModel.CurrentVaccination.Notes = _viewModel.Notes;
                _viewModel.CurrentVaccination.AccessLevel = _viewModel.AccessLevel;

                // Save changes.
                Vaccination resultVaccination = await ProgenyService.UpdateVaccination(_viewModel.CurrentVaccination);
                _viewModel.IsBusy = false;
                EditButton.Text = IconFont.CalendarEdit;
                if (resultVaccination != null)  // Todo: Error message if update fails.
                {
                    MessageLabel.Text = "Vaccination Updated"; // Todo: Translate
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
            EditButton.Text = IconFont.CalendarEdit;
            _viewModel.EditMode = false;
            await Reload();
        }

        private void VaccinationDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            _viewModel.DateYear = VaccinationDatePicker.Date.Year;
            _viewModel.DateMonth = VaccinationDatePicker.Date.Month;
            _viewModel.DateDay = VaccinationDatePicker.Date.Day;
        }

        private async void DeleteButton_OnClickedButton_OnClicked(object sender, EventArgs e)
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string confirmTitle = resmgr.Value.GetString("DeleteVaccination", ci);
            string confirmMessage = resmgr.Value.GetString("DeleteVaccinationMessage", ci) + " ? ";
            string yes = resmgr.Value.GetString("Yes", ci);
            string no = resmgr.Value.GetString("No", ci); ;
            bool confirmDelete = await DisplayAlert(confirmTitle, confirmMessage, yes, no);
            if (confirmDelete)
            {
                _viewModel.IsBusy = true;
                _viewModel.EditMode = false;
                Vaccination deletedVaccination = await ProgenyService.DeleteVaccination(_viewModel.CurrentVaccination);
                if (deletedVaccination.VaccinationId == 0)
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
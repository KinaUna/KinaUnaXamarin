using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    public partial class SleepDetailPage
    {
        private readonly SleepDetailViewModel _viewModel;
        private UserInfo _userInfo;
        private string _accessToken;
        private int _viewChild = Constants.DefaultChildId;
        private bool _online = true;

        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public SleepDetailPage(Sleep sleep)
        {
            _viewModel = new SleepDetailViewModel();
            InitializeComponent();
            _viewModel.CurrentSleepId = sleep.SleepId;
            _viewModel.CurrentSleep = sleep;
            _viewModel.AccessLevel = sleep.AccessLevel;
            _viewModel.Duration = sleep.SleepDuration;
            _viewModel.Rating = sleep.SleepRating;
            
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

            List<Sleep> sleepList = await ProgenyService.GetSleepDetails(_viewModel.CurrentSleepId, _viewModel.UserAccessLevel, _userInfo.Timezone, 1);
            if (sleepList.Any())
            {
                _viewModel.CurrentSleep = sleepList[0];
                _viewModel.AccessLevel = _viewModel.CurrentSleep.AccessLevel;
                _viewModel.Rating = _viewModel.CurrentSleep.SleepRating;
                _viewModel.CurrentSleep.Progeny = _viewModel.Progeny = await ProgenyService.GetProgeny(_viewModel.CurrentSleep.ProgenyId);
                _viewModel.Duration = _viewModel.CurrentSleep.SleepDuration;

                sleepList = sleepList.OrderByDescending(s => s.SleepStart).ToList();
                _viewModel.SleepItems.Add(sleepList[0]);
                _viewModel.SleepItems.Add(sleepList[1]);
                _viewModel.SleepItems.Add(sleepList[2]);
                
                _viewModel.StartYear = _viewModel.CurrentSleep.SleepStart.Year;
                _viewModel.StartMonth = _viewModel.CurrentSleep.SleepStart.Month;
                _viewModel.StartDay = _viewModel.CurrentSleep.SleepStart.Day;
                _viewModel.StartHours = _viewModel.CurrentSleep.SleepStart.Hour;
                _viewModel.StartMinutes = _viewModel.CurrentSleep.SleepStart.Minute;

                _viewModel.EndYear = _viewModel.CurrentSleep.SleepEnd.Year;
                _viewModel.EndMonth = _viewModel.CurrentSleep.SleepEnd.Month;
                _viewModel.EndDay = _viewModel.CurrentSleep.SleepEnd.Day;
                _viewModel.EndHours = _viewModel.CurrentSleep.SleepEnd.Hour;
                _viewModel.EndMinutes = _viewModel.CurrentSleep.SleepEnd.Minute;
                SleepStartDatePicker.Date = new DateTime(_viewModel.CurrentSleep.SleepStart.Year, _viewModel.CurrentSleep.SleepStart.Month, _viewModel.CurrentSleep.SleepStart.Day);
                SleepStartTimePicker.Time = new TimeSpan(_viewModel.CurrentSleep.SleepStart.Hour, _viewModel.CurrentSleep.SleepStart.Minute, 0);
                SleepEndDatePicker.Date = new DateTime(_viewModel.CurrentSleep.SleepEnd.Year, _viewModel.CurrentSleep.SleepEnd.Month, _viewModel.CurrentSleep.SleepEnd.Day);
                SleepEndTimePicker.Time = new TimeSpan(_viewModel.CurrentSleep.SleepEnd.Hour, _viewModel.CurrentSleep.SleepEnd.Minute, 0);
            }

            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewModel.CurrentSleep.ProgenyId);
            if (_viewModel.UserAccessLevel == 0)
            {
                _viewModel.CanUserEditItems = true;
            }
            else
            {
                _viewModel.CanUserEditItems = false;
            }

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
            CalculateDuration();
            _viewModel.IsBusy = false;

        }
        
        private async void EditButton_OnClicked(object sender, EventArgs e)
        {
            if (_viewModel.EditMode)
            {
                _viewModel.EditMode = false;
                _viewModel.IsBusy = true;
                _viewModel.IsSaving = true;
                CheckDates();
                DateTime start = new DateTime(_viewModel.StartYear, _viewModel.StartMonth, _viewModel.StartDay, _viewModel.StartHours, _viewModel.StartMinutes, 0);
                DateTime end = new DateTime(_viewModel.EndYear, _viewModel.EndMonth, _viewModel.EndDay, _viewModel.EndHours, _viewModel.EndMinutes, 0);
                _viewModel.CurrentSleep.SleepStart = start;
                _viewModel.CurrentSleep.SleepEnd = end;
                _viewModel.CurrentSleep.AccessLevel = _viewModel.AccessLevel;
                _viewModel.CurrentSleep.SleepRating = _viewModel.Rating;
                _viewModel.CurrentSleep.SleepNotes = NotesEditor.Text;
                // Todo: Add timezone selection: Use progeny timezone or user timezone, if they are not the same?

                // Save changes.
                Sleep resultSleep = await ProgenyService.UpdateSleep(_viewModel.CurrentSleep);
                _viewModel.IsBusy = false;
                _viewModel.IsSaving = false;
                EditButton.Text = IconFont.AccountEdit;
                if (resultSleep != null)  // Todo: Error message if update fails.
                {
                    MessageLabel.Text = "Sleep Updated"; // Todo: Translate
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

        private void SleepStartDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            CheckDates();
            CalculateDuration();
        }

        private void SleepStartTimePicker_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CheckDates();
            CalculateDuration();
        }

        private void SleepEndDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            CheckDates();
            CalculateDuration();
        }

        private void SleepEndTimePicker_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CheckDates();
            CalculateDuration();

        }

        private void CheckDates()
        {
            if (_viewModel.EditMode)
            {
                _viewModel.StartYear = SleepStartDatePicker.Date.Year;
                _viewModel.StartMonth = SleepStartDatePicker.Date.Month;
                _viewModel.StartDay = SleepStartDatePicker.Date.Day;
                _viewModel.StartHours = SleepStartDatePicker.Date.Hour;
                _viewModel.StartMinutes = SleepStartDatePicker.Date.Minute;
                _viewModel.StartHours = SleepStartTimePicker.Time.Hours;
                _viewModel.StartMinutes = SleepStartTimePicker.Time.Minutes;

                _viewModel.EndYear = SleepEndDatePicker.Date.Year;
                _viewModel.EndMonth = SleepEndDatePicker.Date.Month;
                _viewModel.EndDay = SleepEndDatePicker.Date.Day;
                _viewModel.EndHours = SleepEndDatePicker.Date.Hour;
                _viewModel.EndMinutes = SleepEndDatePicker.Date.Minute;
                _viewModel.EndHours = SleepEndTimePicker.Time.Hours;
                _viewModel.EndMinutes = SleepEndTimePicker.Time.Minutes;

                DateTime start = new DateTime(_viewModel.StartYear, _viewModel.StartMonth, _viewModel.StartDay, _viewModel.StartHours, _viewModel.StartMinutes, 0);
                DateTime end = new DateTime(_viewModel.EndYear, _viewModel.EndMonth, _viewModel.EndDay, _viewModel.EndHours, _viewModel.EndMinutes, 0);

                if (start > end && _viewModel.EditMode)
                {
                    EditButton.IsEnabled = false;
                    MessageLabel.Text = "Error: Start is after End.";
                    MessageLabel.BackgroundColor = Color.Red;
                    MessageLabel.IsVisible = true;
                }
                else
                {
                    EditButton.IsEnabled = true;
                    MessageLabel.Text = "";
                    MessageLabel.IsVisible = false;
                }
            }
            
        }

        private void CalculateDuration()
        {
            DateTime start = new DateTime(_viewModel.StartYear, _viewModel.StartMonth, _viewModel.StartDay, _viewModel.StartHours, _viewModel.StartMinutes, 0);
            DateTime end = new DateTime(_viewModel.EndYear, _viewModel.EndMonth, _viewModel.EndDay, _viewModel.EndHours, _viewModel.EndMinutes, 0);
            _viewModel.CurrentSleep.SleepStart = start;
            _viewModel.CurrentSleep.SleepEnd = end;
            if (_userInfo != null)
            {
                try
                {
                    TimeZoneInfo.FindSystemTimeZoneById(_userInfo.Timezone);
                }
                catch (Exception)
                {
                    _userInfo.Timezone = TZConvert.WindowsToIana(_userInfo.Timezone);
                }

                DateTimeOffset sOffset = new DateTimeOffset(_viewModel.CurrentSleep.SleepStart,
                    TimeZoneInfo.FindSystemTimeZoneById(_userInfo.Timezone).GetUtcOffset(_viewModel.CurrentSleep.SleepStart));
                DateTimeOffset eOffset = new DateTimeOffset(_viewModel.CurrentSleep.SleepEnd,
                    TimeZoneInfo.FindSystemTimeZoneById(_userInfo.Timezone).GetUtcOffset(_viewModel.CurrentSleep.SleepEnd));
                _viewModel.CurrentSleep.SleepDuration = eOffset - sOffset;
                _viewModel.Duration = eOffset - sOffset;
            }
            
        }

        private async void DeleteButton_OnClickedButton_OnClicked(object sender, EventArgs e)
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string confirmTitle = resmgr.Value.GetString("DeleteSleep", ci);
            string confirmMessage = resmgr.Value.GetString("DeleteSleepMessage", ci) + " ? ";
            string yes = resmgr.Value.GetString("Yes", ci);
            string no = resmgr.Value.GetString("No", ci);
            bool confirmDelete = await DisplayAlert(confirmTitle, confirmMessage, yes, no);
            if (confirmDelete)
            {
                _viewModel.IsBusy = true;
                _viewModel.EditMode = false;
                Sleep deletedSleep = await ProgenyService.DeleteSleep(_viewModel.CurrentSleep);
                if (deletedSleep.SleepId == 0)
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

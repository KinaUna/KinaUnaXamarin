﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SleepDetailPage : ContentPage
    {
        private readonly SleepDetailViewModel _viewModel = new SleepDetailViewModel();
        private UserInfo _userInfo;
        private string _accessToken;
        private int _viewChild = Constants.DefaultChildId;
        private bool _online = true;
        private bool _modalShowing;

        public SleepDetailPage(int sleepId)
        {
            _viewModel = new SleepDetailViewModel();
            InitializeComponent();
            _viewModel.CurrentSleepId = sleepId;
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

            List<Sleep> sleepList = await ProgenyService.GetSleepDetails(_viewModel.CurrentSleepId, _viewModel.UserAccessLevel, _userInfo.Timezone, 1);
            if (sleepList.Any())
            {
                _viewModel.CurrentSleep = sleepList[0];
                _viewModel.AccessLevel = _viewModel.CurrentSleep.AccessLevel;
                _viewModel.Rating = _viewModel.CurrentSleep.SleepRating;
                _viewModel.CurrentSleep.Progeny = await ProgenyService.GetProgeny(_viewModel.CurrentSleep.ProgenyId);
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
                SleepStartTimePicker.Time = new TimeSpan(_viewModel.CurrentSleep.SleepStart.Hour, _viewModel.CurrentSleep.SleepStart.Minute, 0);
                SleepEndTimePicker.Time = new TimeSpan(_viewModel.CurrentSleep.SleepEnd.Hour, _viewModel.CurrentSleep.SleepEnd.Minute, 0);
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

            _viewModel.IsBusy = false;

        }
        
        private async void EditButton_OnClicked(object sender, EventArgs e)
        {
            if (_viewModel.EditMode)
            {
                _viewModel.EditMode = false;
                _viewModel.IsBusy = true;

                DateTime start = new DateTime(_viewModel.StartYear, _viewModel.StartMonth, _viewModel.StartDay, _viewModel.StartHours, _viewModel.StartMinutes, 0);
                DateTime end = new DateTime(_viewModel.EndYear, _viewModel.EndMonth, _viewModel.EndDay, _viewModel.EndHours, _viewModel.EndMinutes, 0);
                _viewModel.CurrentSleep.SleepStart = start;
                _viewModel.CurrentSleep.SleepEnd = end;
                _viewModel.CurrentSleep.AccessLevel = _viewModel.AccessLevel;
                _viewModel.CurrentSleep.SleepRating = _viewModel.Rating;
                _viewModel.CurrentSleep.SleepNotes = NotesEditor.Text;
                
                // Save changes.
                Sleep resultSleep = await ProgenyService.UpdateSleep(_viewModel.CurrentSleep);
                _viewModel.IsBusy = false;
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
            _viewModel.StartYear = SleepStartDatePicker.Date.Year;
            _viewModel.StartMonth = SleepStartDatePicker.Date.Month;
            _viewModel.StartDay = SleepStartDatePicker.Date.Day;
            _viewModel.StartHours = SleepStartDatePicker.Date.Hour;
            _viewModel.StartMinutes = SleepStartDatePicker.Date.Minute;
            CheckDates();
            CalculateDuration();
        }

        private void SleepStartTimePicker_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _viewModel.StartHours = SleepStartTimePicker.Time.Hours;
            _viewModel.StartMinutes = SleepStartTimePicker.Time.Minutes;
            CheckDates();
            CalculateDuration();
        }

        private void SleepEndDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            _viewModel.EndYear = SleepEndDatePicker.Date.Year;
            _viewModel.EndMonth = SleepEndDatePicker.Date.Month;
            _viewModel.EndDay = SleepEndDatePicker.Date.Day;
            _viewModel.EndHours = SleepEndDatePicker.Date.Hour;
            _viewModel.EndMinutes = SleepEndDatePicker.Date.Minute;
            CheckDates();
            CalculateDuration();
        }

        private void SleepEndTimePicker_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _viewModel.EndHours = SleepEndTimePicker.Time.Hours;
            _viewModel.EndMinutes = SleepEndTimePicker.Time.Minutes;
            CheckDates();
            CalculateDuration();

        }

        private void CheckDates()
        {
            DateTime start = new DateTime(_viewModel.StartYear, _viewModel.StartMonth, _viewModel.StartDay, _viewModel.StartHours, _viewModel.StartMinutes, 0);
            DateTime end = new DateTime(_viewModel.EndYear, _viewModel.EndMonth, _viewModel.EndDay, _viewModel.EndHours, _viewModel.EndMinutes, 0);

            if (start > end)
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
            }
            
        }
    }
}
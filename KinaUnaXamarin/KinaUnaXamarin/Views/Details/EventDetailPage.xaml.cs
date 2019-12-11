using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using dotMorten.Xamarin.Forms;
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
    public partial class EventDetailPage : ContentPage
    {
        private readonly EventDetailViewModel _viewModel = new EventDetailViewModel();
        private UserInfo _userInfo;
        private string _accessToken;
        private int _viewChild = Constants.DefaultChildId;
        private bool _online = true;

        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public EventDetailPage(CalendarItem calendarItem)
        {
            _viewModel = new EventDetailViewModel();
            InitializeComponent();
            _viewModel.CurrentEventId = calendarItem.EventId;
            _viewModel.AllDay = calendarItem.AllDay;
            _viewModel.AccessLevel = calendarItem.AccessLevel;
            _viewModel.CurrentEvent = calendarItem;
            _viewModel.EventTitle = calendarItem.Title;
            _viewModel.Location = calendarItem.Location;
            _viewModel.Notes = calendarItem.Notes;

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
            _viewModel.CurrentEvent =
                await ProgenyService.GetCalendarItem(_viewModel.CurrentEventId, _accessToken, _userInfo.Timezone);

            _viewModel.AccessLevel = _viewModel.CurrentEvent.AccessLevel;
            _viewModel.CurrentEvent.Progeny = _viewModel.Progeny = await ProgenyService.GetProgeny(_viewModel.CurrentEvent.ProgenyId);

            if (_viewModel.CurrentEvent.StartTime.HasValue)
            {
                _viewModel.StartYear = _viewModel.CurrentEvent.StartTime.Value.Year;
                _viewModel.StartMonth = _viewModel.CurrentEvent.StartTime.Value.Month;
                _viewModel.StartDay = _viewModel.CurrentEvent.StartTime.Value.Day;
                _viewModel.StartHours = _viewModel.CurrentEvent.StartTime.Value.Hour;
                _viewModel.StartMinutes = _viewModel.CurrentEvent.StartTime.Value.Minute;

                if (_viewModel.CurrentEvent.EndTime.HasValue)
                {
                    _viewModel.EndYear = _viewModel.CurrentEvent.EndTime.Value.Year;
                    _viewModel.EndMonth = _viewModel.CurrentEvent.EndTime.Value.Month;
                    _viewModel.EndDay = _viewModel.CurrentEvent.EndTime.Value.Day;
                    _viewModel.EndHours = _viewModel.CurrentEvent.EndTime.Value.Hour;
                    _viewModel.EndMinutes = _viewModel.CurrentEvent.EndTime.Value.Minute;
                    EventStartDatePicker.Date = new DateTime(_viewModel.StartYear, _viewModel.StartMonth, _viewModel.StartDay);
                    EventEndDatePicker.Date = new DateTime(_viewModel.EndYear, _viewModel.EndMonth, _viewModel.EndDay);
                    EventStartTimePicker.Time = new TimeSpan(_viewModel.CurrentEvent.StartTime.Value.Hour, _viewModel.CurrentEvent.StartTime.Value.Minute, 0);
                    EventEndTimePicker.Time = new TimeSpan(_viewModel.CurrentEvent.EndTime.Value.Hour, _viewModel.CurrentEvent.EndTime.Value.Minute, 0);
                }
            }

            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewModel.CurrentEvent.ProgenyId);
            if (_viewModel.UserAccessLevel == 0)
            {
                _viewModel.CanUserEditItems = true;
            }
            else
            {
                _viewModel.CanUserEditItems = false;
            }

            _viewModel.AllDay = _viewModel.CurrentEvent.AllDay;
            _viewModel.EventTitle = _viewModel.CurrentEvent.Title;
            _viewModel.Context = _viewModel.CurrentEvent.Context;
            _viewModel.Location = _viewModel.CurrentEvent.Location;
            _viewModel.Notes = _viewModel.CurrentEvent.Notes;

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
                _viewModel.CurrentEvent.StartTime = start;
                _viewModel.CurrentEvent.EndTime = end;
                _viewModel.CurrentEvent.Title = _viewModel.EventTitle;
                _viewModel.CurrentEvent.Notes = NotesEditor.Text;
                _viewModel.CurrentEvent.Location = LocationEntry.Text;
                _viewModel.CurrentEvent.Context = ContextEntry.Text;
                _viewModel.CurrentEvent.AllDay = _viewModel.AllDay;
                _viewModel.CurrentEvent.AccessLevel = _viewModel.AccessLevel;

                // Save changes.
                CalendarItem resultEvent = await ProgenyService.UpdateCalendarItem(_viewModel.CurrentEvent);
                _viewModel.IsBusy = false;
                EditButton.Text = IconFont.AccountEdit;
                if (resultEvent != null)  // Todo: Error message if update fails.
                {
                    MessageLabel.Text = "Calendar Item Updated"; // Todo: Translate
                    MessageLabel.BackgroundColor = Color.DarkGreen;
                    MessageLabel.IsVisible = true;
                    await Reload();
                }
            }
            else
            {
                EditButton.Text = IconFont.ContentSave;

                _viewModel.EditMode = true;
                _viewModel.LocationAutoSuggestList = await ProgenyService.GetLocationAutoSuggestList(_viewModel.CurrentEvent.ProgenyId, 0);
                _viewModel.ContextAutoSuggestList = await ProgenyService.GetContextAutoSuggestList(_viewModel.CurrentEvent.ProgenyId, 0);
            }
        }

        private async void CancelButton_OnClicked(object sender, EventArgs e)
        {
            EditButton.Text = IconFont.AccountEdit;
            _viewModel.EditMode = false;
            await Reload();
        }

        private void EventStartDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            _viewModel.StartYear = EventStartDatePicker.Date.Year;
            _viewModel.StartMonth = EventStartDatePicker.Date.Month;
            _viewModel.StartDay = EventStartDatePicker.Date.Day;
            _viewModel.StartHours = EventStartDatePicker.Date.Hour;
            _viewModel.StartMinutes = EventStartDatePicker.Date.Minute;
            CheckDates();
        }

        private void EventStartTimePicker_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _viewModel.StartHours = EventStartTimePicker.Time.Hours;
            _viewModel.StartMinutes = EventStartTimePicker.Time.Minutes;
            CheckDates();
        }

        private void EventEndDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            _viewModel.EndYear = EventEndDatePicker.Date.Year;
            _viewModel.EndMonth = EventEndDatePicker.Date.Month;
            _viewModel.EndDay = EventEndDatePicker.Date.Day;
            _viewModel.EndHours = EventEndDatePicker.Date.Hour;
            _viewModel.EndMinutes = EventEndDatePicker.Date.Minute;
            CheckDates();
        }

        private void EventEndTimePicker_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _viewModel.EndHours = EventEndTimePicker.Time.Hours;
            _viewModel.EndMinutes = EventEndTimePicker.Time.Minutes;
            CheckDates();
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

        private async void DeleteButton_OnClickedButton_OnClicked(object sender, EventArgs e)
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string confirmTitle = resmgr.Value.GetString("DeleteEvent", ci);
            string confirmMessage = resmgr.Value.GetString("DeleteEventMessage", ci) + " ? ";
            string yes = resmgr.Value.GetString("Yes", ci);
            string no = resmgr.Value.GetString("No", ci); ;
            bool confirmDelete = await DisplayAlert(confirmTitle, confirmMessage, yes, no);
            if (confirmDelete)
            {
                _viewModel.IsBusy = true;
                _viewModel.EditMode = false;
                CalendarItem deletedEvent = await ProgenyService.DeleteCalendarItem(_viewModel.CurrentEvent);
                if (deletedEvent.EventId == 0)
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

        private void LocationEntry_OnTextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
                if (autoSuggestBox != null && autoSuggestBox.Text.Length > 0)
                {
                    List<string> filteredLocations = new List<string>();
                    foreach (string locationString in _viewModel.LocationAutoSuggestList)
                    {
                        if (locationString.ToUpper().Contains(autoSuggestBox.Text.Trim().ToUpper()))
                        {
                            filteredLocations.Add(locationString);
                        }
                    }
                    //Set the ItemsSource to be your filtered dataset
                    autoSuggestBox.ItemsSource = filteredLocations;
                }
                else
                {
                    if (autoSuggestBox != null)
                    {
                        autoSuggestBox.ItemsSource = null;
                    }
                }
            }
        }

        private void LocationEntry_OnQuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            if (e.ChosenSuggestion != null)
            {
                AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
                if (autoSuggestBox != null)
                {
                    // User selected an item from the suggestion list, take an action on it here.
                    autoSuggestBox.Text = e.ChosenSuggestion.ToString();
                    autoSuggestBox.ItemsSource = null;
                }
            }
            else
            {
                // User hit Enter from the search box. Use e.QueryText to determine what to do.
            }
        }

        private void LocationEntry_OnSuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
            // Set sender.Text. You can use e.SelectedItem to build your text string.
            if (autoSuggestBox != null)
            {
                autoSuggestBox.Text = e.SelectedItem.ToString();
            }
        }
        
        private void ContextEntry_OnTextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
                if (autoSuggestBox != null && autoSuggestBox.Text.Length > 0)
                {
                    List<string> filteredContexts = new List<string>();
                    foreach (string contextString in _viewModel.ContextAutoSuggestList)
                    {
                        if (contextString.ToUpper().Contains(autoSuggestBox.Text.Trim().ToUpper()))
                        {
                            filteredContexts.Add(contextString);
                        }
                    }
                    //Set the ItemsSource to be your filtered dataset
                    autoSuggestBox.ItemsSource = filteredContexts;
                }
                else
                {
                    if (autoSuggestBox != null)
                    {
                        autoSuggestBox.ItemsSource = null;
                    }
                }
            }
        }

        private void ContextEntry_OnQuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            if (e.ChosenSuggestion != null)
            {
                AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
                if (autoSuggestBox != null)
                {
                    // User selected an item from the suggestion list, take an action on it here.
                    autoSuggestBox.Text = e.ChosenSuggestion.ToString();
                    autoSuggestBox.ItemsSource = null;
                }
            }
            else
            {
                // User hit Enter from the search box. Use e.QueryText to determine what to do.
            }
        }

        private void ContextEntry_OnSuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
            // Set sender.Text. You can use e.SelectedItem to build your text string.
            if (autoSuggestBox != null)
            {
                autoSuggestBox.Text = e.SelectedItem.ToString();
            }
        }

        private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
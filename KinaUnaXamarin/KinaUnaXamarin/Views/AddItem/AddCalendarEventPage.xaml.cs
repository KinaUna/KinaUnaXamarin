using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace KinaUnaXamarin.Views.AddItem
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddCalendarEventPage : ContentPage
    {
        private readonly AddCalendarEventViewModel _viewModel;
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public AddCalendarEventPage()
        {
            _viewModel = new AddCalendarEventViewModel();
            _viewModel.CalendarItem = new CalendarItem();
            _viewModel.CalendarItem.StartTime = DateTime.Now;
            _viewModel.CalendarItem.EndTime = DateTime.Now + TimeSpan.FromMinutes(10);
            _viewModel.CalendarItem.AccessLevel = 0;
            _viewModel.CalendarItem.AllDay = false;

            _viewModel.StartYear = _viewModel.CalendarItem.StartTime.Value.Year;
            _viewModel.StartMonth = _viewModel.CalendarItem.StartTime.Value.Month;
            _viewModel.StartDay = _viewModel.CalendarItem.StartTime.Value.Day;
            _viewModel.StartHours = _viewModel.CalendarItem.StartTime.Value.Hour;
            _viewModel.StartMinutes = _viewModel.CalendarItem.StartTime.Value.Minute;

            _viewModel.EndYear = _viewModel.CalendarItem.EndTime.Value.Year;
            _viewModel.EndMonth = _viewModel.CalendarItem.EndTime.Value.Month;
            _viewModel.EndDay = _viewModel.CalendarItem.EndTime.Value.Day;
            _viewModel.EndHours = _viewModel.CalendarItem.EndTime.Value.Hour;
            _viewModel.EndMinutes = _viewModel.CalendarItem.EndTime.Value.Minute;
            InitializeComponent();
            BindingContext = _viewModel;
            ProgenyCollectionView.ItemsSource = _viewModel.ProgenyCollection;

            CalendarEventStartTimePicker.Time = new TimeSpan(_viewModel.CalendarItem.StartTime.Value.Hour, _viewModel.CalendarItem.StartTime.Value.Minute, 0);
            CalendarEventEndTimePicker.Time = new TimeSpan(_viewModel.CalendarItem.EndTime.Value.Hour, _viewModel.CalendarItem.EndTime.Value.Minute, 0);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

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
            
            
            

        }

        private void CalendarEventStartDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            _viewModel.StartYear = CalendarEventStartDatePicker.Date.Year;
            _viewModel.StartMonth = CalendarEventStartDatePicker.Date.Month;
            _viewModel.StartDay = CalendarEventStartDatePicker.Date.Day;
            _viewModel.StartHours = CalendarEventStartDatePicker.Date.Hour;
            _viewModel.StartMinutes = CalendarEventStartDatePicker.Date.Minute;
            CheckDates();
        }

        private void CalendarEventStartTimePicker_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _viewModel.StartHours = CalendarEventStartTimePicker.Time.Hours;
            _viewModel.StartMinutes = CalendarEventStartTimePicker.Time.Minutes;
            CheckDates();
        }

        private void CalendarEventEndDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            _viewModel.EndYear = CalendarEventEndDatePicker.Date.Year;
            _viewModel.EndMonth = CalendarEventEndDatePicker.Date.Month;
            _viewModel.EndDay = CalendarEventEndDatePicker.Date.Day;
            _viewModel.EndHours = CalendarEventEndDatePicker.Date.Hour;
            _viewModel.EndMinutes = CalendarEventEndDatePicker.Date.Minute;
            CheckDates();
        }

        private void CalendarEventEndTimePicker_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _viewModel.EndHours = CalendarEventEndTimePicker.Time.Hours;
            _viewModel.EndMinutes = CalendarEventEndTimePicker.Time.Minutes;
            CheckDates();

        }

        private void CheckDates()
        {
            DateTime start = new DateTime(_viewModel.StartYear, _viewModel.StartMonth, _viewModel.StartDay, _viewModel.StartHours, _viewModel.StartMinutes, 0);
            DateTime end = new DateTime(_viewModel.EndYear, _viewModel.EndMonth, _viewModel.EndDay, _viewModel.EndHours, _viewModel.EndMinutes, 0);

            if (start > end)
            {
                SaveEventButton.IsEnabled = false;
                ErrorLabel.Text = "Error: Start is after End.";
                ErrorLabel.BackgroundColor = Color.Red;
                ErrorLabel.IsVisible = true;
            }
            else
            {
                SaveEventButton.IsEnabled = true;
                ErrorLabel.Text = "";
                ErrorLabel.IsVisible = false;
            }
        }

        private async void CancelEventButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        private async void SaveEventButton_OnClicked(object sender, EventArgs e)
        {
            _viewModel.IsBusy = true;
            Progeny progeny = ProgenyCollectionView.SelectedItem as Progeny;
            if (progeny != null)
            {
                DateTime start = new DateTime(_viewModel.StartYear, _viewModel.StartMonth, _viewModel.StartDay, _viewModel.StartHours, _viewModel.StartMinutes, 0);
                DateTime end = new DateTime(_viewModel.EndYear, _viewModel.EndMonth, _viewModel.EndDay, _viewModel.EndHours, _viewModel.EndMinutes, 0);

                CalendarItem saveEvent = new CalendarItem();
                saveEvent.ProgenyId = progeny.Id;
                saveEvent.AccessLevel = _viewModel.AccessLevel;
                saveEvent.StartTime = start;
                saveEvent.EndTime = end;
                saveEvent.Notes = _viewModel.Notes;
                saveEvent.Progeny = progeny;
                string userEmail = await UserService.GetUserEmail();
                UserInfo userinfo = await UserService.GetUserInfo(userEmail);
                saveEvent.Author = userinfo.UserId;
                saveEvent.Title = TitleEntry.Text;
                saveEvent.Location = LocationEntry.Text;
                saveEvent.Context = ContextEntry.Text;
                saveEvent.AllDay = AllDayCheckBox.IsChecked;
                
                if (ProgenyService.Online())
                {
                    // Todo: Translate messages.
                    saveEvent = await ProgenyService.SaveCalendarEvent(saveEvent);
                    if (saveEvent.EventId == 0)
                    {
                        var ci = CrossMultilingual.Current.CurrentCultureInfo;
                        ErrorLabel.Text = resmgr.Value.GetString("ErrorEventNotSaved", ci);
                        ErrorLabel.BackgroundColor = Color.Red;

                    }
                    else
                    {
                        var ci = CrossMultilingual.Current.CurrentCultureInfo;
                        ErrorLabel.Text = resmgr.Value.GetString("EventSaved", ci) + saveEvent.EventId;
                        ErrorLabel.BackgroundColor = Color.Green;
                        SaveEventButton.IsVisible = false;
                        CancelEventButton.Text = "Ok";
                        CancelEventButton.BackgroundColor = Color.FromHex("#4caf50");
                    }
                }
                else
                {
                    // Todo: Translate message.
                    ErrorLabel.Text = $"Error: No internet connection. Calendar Event for {progeny.NickName} was not saved. Try again later.";
                    ErrorLabel.BackgroundColor = Color.Red;
                }
                
                ErrorLabel.IsVisible = true;
            }

            _viewModel.IsBusy = false;
        }
    }
}
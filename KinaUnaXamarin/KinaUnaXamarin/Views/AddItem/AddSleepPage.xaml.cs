using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels.AddItem;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views.AddItem
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddSleepPage : ContentPage
    {
        private readonly AddSleepViewModel _addSleepViewModel;

        public AddSleepPage()
        {
            _addSleepViewModel = new AddSleepViewModel();
            _addSleepViewModel.SleepItem = new Sleep();
            _addSleepViewModel.SleepItem.SleepStart = DateTime.Now;
            _addSleepViewModel.SleepItem.SleepEnd = DateTime.Now + TimeSpan.FromMinutes(10);
            _addSleepViewModel.SleepItem.AccessLevel = 0;
            _addSleepViewModel.SleepItem.SleepRating = 3;

            _addSleepViewModel.StartYear = _addSleepViewModel.SleepItem.SleepStart.Year;
            _addSleepViewModel.StartMonth = _addSleepViewModel.SleepItem.SleepStart.Month;
            _addSleepViewModel.StartDay = _addSleepViewModel.SleepItem.SleepStart.Day;
            _addSleepViewModel.StartHours = _addSleepViewModel.SleepItem.SleepStart.Hour;
            _addSleepViewModel.StartMinutes = _addSleepViewModel.SleepItem.SleepStart.Minute;

            _addSleepViewModel.EndYear = _addSleepViewModel.SleepItem.SleepEnd.Year;
            _addSleepViewModel.EndMonth = _addSleepViewModel.SleepItem.SleepEnd.Month;
            _addSleepViewModel.EndDay = _addSleepViewModel.SleepItem.SleepEnd.Day;
            _addSleepViewModel.EndHours = _addSleepViewModel.SleepItem.SleepEnd.Hour;
            _addSleepViewModel.EndMinutes = _addSleepViewModel.SleepItem.SleepEnd.Minute;
            InitializeComponent();
            BindingContext = _addSleepViewModel;
            ProgenyCollectionView.ItemsSource = _addSleepViewModel.ProgenyCollection;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            List<Progeny> progenyList = await ProgenyService.GetProgenyAdminList();
            if (progenyList.Any())
            {
                foreach (Progeny progeny in progenyList)
                {
                    _addSleepViewModel.ProgenyCollection.Add(progeny);
                }
            }
            string userviewchild = await SecureStorage.GetAsync(Constants.UserViewChildKey);
            bool viewchildParsed = int.TryParse(userviewchild, out int viewChild);
            ProgenyCollectionView.SelectedItem =
                _addSleepViewModel.ProgenyCollection.SingleOrDefault(p => p.Id == viewChild);
            ProgenyCollectionView.ScrollTo(ProgenyCollectionView.SelectedItem);
            _addSleepViewModel.SleepItem.ProgenyId = viewChild;
            _addSleepViewModel.SleepItem.SleepStart = DateTime.Now;
            _addSleepViewModel.SleepItem.SleepEnd = DateTime.Now + TimeSpan.FromMinutes(10);
            _addSleepViewModel.SleepItem.AccessLevel = 0;
            _addSleepViewModel.SleepItem.SleepRating = 3;

            _addSleepViewModel.StartYear = _addSleepViewModel.SleepItem.SleepStart.Year;
            _addSleepViewModel.StartMonth = _addSleepViewModel.SleepItem.SleepStart.Month;
            _addSleepViewModel.StartDay = _addSleepViewModel.SleepItem.SleepStart.Day;
            _addSleepViewModel.StartHours = _addSleepViewModel.SleepItem.SleepStart.Hour;
            _addSleepViewModel.StartMinutes = _addSleepViewModel.SleepItem.SleepStart.Minute;

            _addSleepViewModel.EndYear = _addSleepViewModel.SleepItem.SleepEnd.Year;
            _addSleepViewModel.EndMonth = _addSleepViewModel.SleepItem.SleepEnd.Month;
            _addSleepViewModel.EndDay = _addSleepViewModel.SleepItem.SleepEnd.Day;
            _addSleepViewModel.EndHours = _addSleepViewModel.SleepItem.SleepEnd.Hour;
            _addSleepViewModel.EndMinutes = _addSleepViewModel.SleepItem.SleepEnd.Minute;
            SleepStartTimePicker.Time = new TimeSpan(_addSleepViewModel.SleepItem.SleepStart.Hour, _addSleepViewModel.SleepItem.SleepStart.Minute, 0);
            SleepEndTimePicker.Time = new TimeSpan(_addSleepViewModel.SleepItem.SleepEnd.Hour, _addSleepViewModel.SleepItem.SleepEnd.Minute, 0);
            //CheckDates();

        }

        private void SleepStartDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            _addSleepViewModel.StartYear = SleepStartDatePicker.Date.Year;
            _addSleepViewModel.StartMonth = SleepStartDatePicker.Date.Month;
            _addSleepViewModel.StartDay = SleepStartDatePicker.Date.Day;
            _addSleepViewModel.StartHours = SleepStartDatePicker.Date.Hour;
            _addSleepViewModel.StartMinutes = SleepStartDatePicker.Date.Minute;
            CheckDates();
        }

        private void SleepStartTimePicker_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _addSleepViewModel.StartHours = SleepStartTimePicker.Time.Hours;
            _addSleepViewModel.StartMinutes = SleepStartTimePicker.Time.Minutes;
            CheckDates();
        }

        private void SleepEndDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            _addSleepViewModel.EndYear = SleepEndDatePicker.Date.Year;
            _addSleepViewModel.EndMonth = SleepEndDatePicker.Date.Month;
            _addSleepViewModel.EndDay = SleepEndDatePicker.Date.Day;
            _addSleepViewModel.EndHours = SleepEndDatePicker.Date.Hour;
            _addSleepViewModel.EndMinutes = SleepEndDatePicker.Date.Minute;
            CheckDates();
        }

        private void SleepEndTimePicker_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _addSleepViewModel.EndHours = SleepEndTimePicker.Time.Hours;
            _addSleepViewModel.EndMinutes = SleepEndTimePicker.Time.Minutes;
            CheckDates();

        }

        private void CheckDates()
        {
            DateTime start = new DateTime(_addSleepViewModel.StartYear, _addSleepViewModel.StartMonth, _addSleepViewModel.StartDay, _addSleepViewModel.StartHours, _addSleepViewModel.StartMinutes, 0);
            DateTime end = new DateTime(_addSleepViewModel.EndYear, _addSleepViewModel.EndMonth, _addSleepViewModel.EndDay, _addSleepViewModel.EndHours, _addSleepViewModel.EndMinutes, 0);

            if (start > end)
            {
                SaveSleepButton.IsEnabled = false;
                ErrorLabel.Text = "Error: Start is after End.";
                ErrorLabel.BackgroundColor = Color.Red;
                ErrorLabel.IsVisible = true;
            }
            else
            {
                SaveSleepButton.IsEnabled = true;
                ErrorLabel.Text = "";
                ErrorLabel.IsVisible = false;
            }
        }

        private async void CancelSleepButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        private async void SaveSleepButton_OnClicked(object sender, EventArgs e)
        {
            _addSleepViewModel.IsBusy = true;
            Progeny progeny = ProgenyCollectionView.SelectedItem as Progeny;
            if (progeny != null)
            {
                DateTime start = new DateTime(_addSleepViewModel.StartYear, _addSleepViewModel.StartMonth, _addSleepViewModel.StartDay, _addSleepViewModel.StartHours, _addSleepViewModel.StartMinutes, 0);
                DateTime end = new DateTime(_addSleepViewModel.EndYear, _addSleepViewModel.EndMonth, _addSleepViewModel.EndDay, _addSleepViewModel.EndHours, _addSleepViewModel.EndMinutes, 0);

                Sleep saveSleep = new Sleep();
                saveSleep.ProgenyId = progeny.Id;
                saveSleep.AccessLevel = _addSleepViewModel.AccessLevel;
                saveSleep.SleepRating = _addSleepViewModel.Rating;
                saveSleep.SleepStart = start;
                saveSleep.SleepEnd = end;
                saveSleep.SleepNotes = _addSleepViewModel.Notes;
                saveSleep.Progeny = progeny;
                if (ProgenyService.Online())
                {
                    saveSleep = await ProgenyService.SaveSleep(saveSleep);
                    if (saveSleep.SleepId == 0)
                    {
                        ErrorLabel.Text = "Error: Sleep was not saved. Try again later.";
                        ErrorLabel.BackgroundColor = Color.Red;

                    }
                    else
                    {
                        ErrorLabel.Text = $"Sleep for {progeny.NickName} saved successfully. Sleep Id: {saveSleep.SleepId} ";
                        ErrorLabel.BackgroundColor = Color.Green;
                        SaveSleepButton.IsVisible = false;
                        CancelSleepButton.Text = "Ok";
                        CancelSleepButton.BackgroundColor = Color.FromHex("#4caf50");
                    }
                }
                else
                {
                    ErrorLabel.Text = $"Error: No internet connection. Sleep for {progeny.NickName} was not saved. Try again later.";
                    ErrorLabel.BackgroundColor = Color.Red;
                }
                
                ErrorLabel.IsVisible = true;
            }

            _addSleepViewModel.IsBusy = false;
        }
    }
}
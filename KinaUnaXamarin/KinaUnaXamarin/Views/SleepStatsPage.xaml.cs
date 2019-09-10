using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using Microcharts;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SkiaSharp;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Entry = Microcharts.Entry;

namespace KinaUnaXamarin.Views
{
    // Uses OxyPlot, see: https://oxyplot.readthedocs.io/en/master/getting-started/hello-xamarin-forms.html

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SleepStatsPage : ContentPage
    {
        private int _viewChild = Constants.DefaultChildId;
        private UserInfo _userInfo;
        private SleepStatsViewModel _viewModel;
        private string _accessToken;
        private bool _reload = true;
        private bool _online = true;
        
        public SleepStatsPage()
        {
            InitializeComponent();
            MessagingCenter.Subscribe<SelectProgenyPage>(this, "Reload", async (sender) =>
            {
                await Reload();
            });

            MessagingCenter.Subscribe<AccountViewModel>(this, "Reload", async (sender) =>
            {
                await Reload();
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_reload)
            {
                _viewModel = new SleepStatsViewModel();
                _userInfo = OfflineDefaultData.DefaultUserInfo;
                SleepStatsGrid.BindingContext = _viewModel;
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
            await CheckAccount();
            if (_reload)
            {
                if (_viewModel.Progeny.BirthDay.HasValue)
                {
                    StartDatePicker.Date = DateTime.Now - TimeSpan.FromDays(14);
                }

                EndDatePicker.Date = DateTime.Now;
                // ChartTypePicker.SelectedIndex = 1;
            }

            await UpdateSleep();
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

        private async Task UpdateSleep()
        {
            _viewModel.IsBusy = true;
            _viewModel.TodayDate = DateTime.Now;
            
            _viewModel.SleepStats =
                await ProgenyService.GetSleepStats(_viewChild, _viewModel.UserAccessLevel);

            _viewModel.SleepTotal = _viewModel.SleepStats.SleepTotal;
            _viewModel.TotalAverage = _viewModel.SleepStats.TotalAverage;
            _viewModel.SleepLastMonth = _viewModel.SleepStats.SleepLastMonth;
            _viewModel.LastMonthAverage = _viewModel.SleepStats.LastMonthAverage;
            _viewModel.SleepLastYear = _viewModel.SleepStats.SleepLastYear;
            _viewModel.LastYearAverage = _viewModel.SleepStats.LastYearAverage;

            List<Sleep> sleepList =
                await ProgenyService.GetSleepChartData(_viewChild, _viewModel.UserAccessLevel);
            _viewModel.SleepItems.ReplaceRange(sleepList);
            //List<Microcharts.Entry> chartsEntryList = new List<Entry>();
            LineSeries sleepSeries = new LineSeries()
            {
                Color = OxyColors.DarkGreen,
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.Green,
                MarkerStrokeThickness = 1.5
            };
            
            double maxSleep = 0;
            double minSleep = 24;
            foreach (Sleep slp in _viewModel.SleepItems)
            {
                if (slp.SleepStart >= StartDatePicker.Date && slp.SleepStart <= EndDatePicker.Date)
                {
                    slp.SleepStart = slp.SleepStart.Date;
                    slp.SleepDurDouble = slp.SleepDuration.TotalMinutes / 60.0;
                    double startDateDouble = DateTimeAxis.ToDouble(slp.SleepStart.Date);
                    sleepSeries.Points.Add(new DataPoint(startDateDouble, slp.SleepDurDouble));
                    if (slp.SleepDurDouble > maxSleep)
                    {
                        maxSleep = slp.SleepDurDouble;
                    }

                    if (slp.SleepDurDouble < minSleep)
                    {
                        minSleep = slp.SleepDurDouble;
                    }
                }
                
            }

            _viewModel.MinValue = Math.Floor(minSleep);
            _viewModel.MaxValue = Math.Ceiling(maxSleep);

            LinearAxis durationAxis = new LinearAxis();
            durationAxis.Key = "DurationAxis";
            durationAxis.Minimum = 0; //_viewModel.MinValue -1;
            durationAxis.Maximum = _viewModel.MaxValue; // + 1;
            durationAxis.Position = AxisPosition.Left;
            durationAxis.MajorStep = 5;
            durationAxis.MinorStep = 1;
            durationAxis.MajorGridlineStyle = LineStyle.Solid;
            durationAxis.MinorGridlineStyle = LineStyle.Solid;
            durationAxis.MajorGridlineColor = OxyColors.LightGreen;
            durationAxis.MinorGridlineColor = OxyColors.LightBlue;
            durationAxis.AxislineColor = OxyColor.FromRgb(0, 0, 0);
           
            DateTimeAxis dateAxis = new DateTimeAxis();
            dateAxis.Key = "DateAxis";
            dateAxis.Minimum = DateTimeAxis.ToDouble(StartDatePicker.Date);
            dateAxis.Maximum = DateTimeAxis.ToDouble(EndDatePicker.Date);
            dateAxis.Position = AxisPosition.Bottom;
            dateAxis.IntervalType = DateTimeIntervalType.Days;
            dateAxis.AxislineColor = OxyColor.FromRgb(0, 0,0);
            dateAxis.StringFormat = "dd-MMM-yyyy";

            _viewModel.SleepPlotModel = new PlotModel();
            _viewModel.SleepPlotModel.Background = OxyColors.White;
            _viewModel.SleepPlotModel.Axes.Add(durationAxis);
            _viewModel.SleepPlotModel.Axes.Add(dateAxis);
            _viewModel.SleepPlotModel.Series.Add(sleepSeries);
            
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


        private async void SubmitOptionsButton_OnClicked(object sender, EventArgs e)
        {
            _viewModel.ShowOptions = false;
            await Reload();
        }

    }
}
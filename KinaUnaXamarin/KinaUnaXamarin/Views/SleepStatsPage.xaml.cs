using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    // Uses OxyPlot, see: https://oxyplot.readthedocs.io/en/master/getting-started/hello-xamarin-forms.html

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SleepStatsPage : ContentPage
    {
        private SleepStatsViewModel _viewModel;
        private bool _reload = true;
        
        public SleepStatsPage()
        {
            InitializeComponent();
            _viewModel = new SleepStatsViewModel();
            SleepStatsGrid.BindingContext = _viewModel;
            BindingContext = _viewModel;

            MessagingCenter.Subscribe<SelectProgenyPage>(this, "Reload", async (sender) =>
            {
                _reload = true;
                await SetUserAndProgeny();
                await Reload();
            });

            MessagingCenter.Subscribe<AccountViewModel>(this, "Reload", async (sender) =>
            {
                _reload = true;
                await SetUserAndProgeny();
                await Reload();
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_reload)
            {
                await SetUserAndProgeny();
                await Reload();
            }

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            var networkAccess = Connectivity.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess)
            {
                _viewModel.Online = true;
            }
            else
            {
                _viewModel.Online = false;
            }

            _reload = false;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        private async Task SetUserAndProgeny()
        {
            _viewModel.UserInfo = OfflineDefaultData.DefaultUserInfo;

            string userEmail = await UserService.GetUserEmail();
            string userviewchild = await SecureStorage.GetAsync(Constants.UserViewChildKey);
            bool viewchildParsed = int.TryParse(userviewchild, out int viewChildId);

            if (viewchildParsed)
            {
                _viewModel.ViewChild = viewChildId;
                try
                {
                    _viewModel.Progeny = await App.Database.GetProgenyAsync(_viewModel.ViewChild);
                }
                catch (Exception)
                {
                    _viewModel.Progeny = await ProgenyService.GetProgeny(_viewModel.ViewChild);
                }

                _viewModel.UserInfo = await App.Database.GetUserInfoAsync(userEmail);
            }

            if (String.IsNullOrEmpty(_viewModel.UserInfo.Timezone))
            {
                _viewModel.UserInfo.Timezone = Constants.DefaultTimeZone;
            }
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(_viewModel.UserInfo.Timezone);
            }
            catch (Exception)
            {
                _viewModel.UserInfo.Timezone = TZConvert.WindowsToIana(_viewModel.UserInfo.Timezone);
            }
        }

        private async Task Reload()
        {
            _viewModel.IsBusy = true;
            await CheckAccount();
            if (_reload)
            {
                if (_viewModel.Progeny.BirthDay.HasValue)
                {
                    Device.BeginInvokeOnMainThread(() => { StartDatePicker.Date = DateTime.Now - TimeSpan.FromDays(30); });
                    
                }
                Device.BeginInvokeOnMainThread(() =>
                {
                    EndDatePicker.Date = DateTime.Now;
                    ChartTypePicker.SelectedItem = _viewModel.ChartTypeList[0];
                });
                
            }

            await UpdateSleep();

            var networkInfo = Connectivity.NetworkAccess;

            if (networkInfo == NetworkAccess.Internet)
            {
                // Connection to internet is available
                _viewModel.Online = true;
            }
            else
            {
                _viewModel.Online = false;
            }

            _viewModel.IsBusy = false;
        }

        private async Task CheckAccount()
        {
            string userEmail = await UserService.GetUserEmail();
            _viewModel.AccessToken = await UserService.GetAuthAccessToken();
            bool accessTokenCurrent = false;
            if (_viewModel.AccessToken != "")
            {
                accessTokenCurrent = await UserService.IsAccessTokenCurrent();

                if (!accessTokenCurrent)
                {
                    bool loginSuccess = await UserService.LoginIdsAsync();
                    if (loginSuccess)
                    {
                        _viewModel.AccessToken = await UserService.GetAuthAccessToken();
                        accessTokenCurrent = true;
                    }

                    await Reload();
                }
            }

            if (String.IsNullOrEmpty(_viewModel.AccessToken) || !accessTokenCurrent)
            {

                _viewModel.IsLoggedIn = false;
                _viewModel.AccessToken = "";
                _viewModel.UserInfo = OfflineDefaultData.DefaultUserInfo;

            }
            else
            {
                _viewModel.IsLoggedIn = true;
                _viewModel.UserInfo = await UserService.GetUserInfo(userEmail);
            }

            await SetUserAndProgeny();

            Progeny progeny = await ProgenyService.GetProgeny(_viewModel.ViewChild);
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
                if (prog.Admins.ToUpper().Contains(_viewModel.UserInfo.UserEmail.ToUpper()))
                {
                    _viewModel.CanUserAddItems = true;
                }
            }

            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewModel.ViewChild);
        }

        private async Task UpdateSleep()
        {
            _viewModel.IsBusy = true;
            _viewModel.TodayDate = DateTime.Now;
            
            _viewModel.SleepStats =
                await ProgenyService.GetSleepStats(_viewModel.ViewChild, _viewModel.UserAccessLevel);

            if (_viewModel.SleepStats != null)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    _viewModel.SleepTotal = _viewModel.SleepStats.SleepTotal;
                    _viewModel.TotalAverage = _viewModel.SleepStats.TotalAverage;
                    _viewModel.SleepLastMonth = _viewModel.SleepStats.SleepLastMonth;
                    _viewModel.LastMonthAverage = _viewModel.SleepStats.LastMonthAverage;
                    _viewModel.SleepLastYear = _viewModel.SleepStats.SleepLastYear;
                    _viewModel.LastYearAverage = _viewModel.SleepStats.LastYearAverage;
                });
            }
            

            List<Sleep> sleepList =
                await ProgenyService.GetSleepChartData(_viewModel.ViewChild, _viewModel.UserAccessLevel);
            _viewModel.SleepItems.ReplaceRange(sleepList);
            if (sleepList != null && sleepList.Count > 0)
            {
                LineSeries sleepLineSeries = new LineSeries()
                {
                    Color = OxyColors.DarkGreen,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.White,
                    MarkerFill = OxyColors.Green,
                    MarkerStrokeThickness = 1.5,
                    Title = this.Title
                    
                };

                StairStepSeries sleepStairStepSeries = new StairStepSeries()
                {
                    Color = OxyColors.DarkGreen,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.White,
                    MarkerFill = OxyColors.Green,
                    MarkerStrokeThickness = 1.5,
                    Title = this.Title
                };

                StemSeries sleepStemSeries = new StemSeries()
                {
                    Color = OxyColors.DarkGreen,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.White,
                    MarkerFill = OxyColors.Green,
                    MarkerStrokeThickness = 1.5,
                    Title = this.Title
                };

                double maxSleep = 0;
                double minSleep = 24;
                DateTime firstSleepItem = _viewModel.EndDate;
                DateTime lastSleepItem = _viewModel.StartDate;

                foreach (Sleep slp in _viewModel.SleepItems)
                {
                    if (slp.SleepStart >= StartDatePicker.Date && slp.SleepStart <= EndDatePicker.Date)
                    {
                        slp.SleepStart = slp.SleepStart.Date;
                        slp.SleepDurDouble = slp.SleepDuration.TotalMinutes / 60.0;
                        double startDateDouble = DateTimeAxis.ToDouble(slp.SleepStart.Date);
                        sleepLineSeries.Points.Add(new DataPoint(startDateDouble, slp.SleepDurDouble));
                        sleepStairStepSeries.Points.Add(new DataPoint(startDateDouble, slp.SleepDurDouble));
                        sleepStemSeries.Points.Add(new DataPoint(startDateDouble, slp.SleepDurDouble));

                        if (slp.SleepDurDouble > maxSleep)
                        {
                            maxSleep = slp.SleepDurDouble;
                        }

                        if (slp.SleepDurDouble < minSleep)
                        {
                            minSleep = slp.SleepDurDouble;
                        }
                    }
                    if (slp.SleepStart < firstSleepItem)
                    {
                        firstSleepItem = slp.SleepStart;
                    }

                    if (slp.SleepStart > lastSleepItem)
                    {
                        lastSleepItem = slp.SleepStart;
                    }
                }

                if (sleepLineSeries.Points.Any())
                {
                    _viewModel.MinValue = Math.Floor(minSleep);
                    _viewModel.MaxValue = Math.Ceiling(maxSleep);
                    _viewModel.FirstDate = firstSleepItem;
                    _viewModel.LastDate = lastSleepItem;

                    LinearAxis durationAxis = new LinearAxis();
                    durationAxis.Key = "DurationAxis";
                    durationAxis.Minimum = 0; //_viewModel.MinValue -1;
                    durationAxis.Maximum = _viewModel.MaxValue; // + 1;
                    durationAxis.AbsoluteMinimum = 0; //_viewModel.MinValue -1;
                    durationAxis.AbsoluteMaximum = _viewModel.MaxValue; // + 1;
                    durationAxis.Position = AxisPosition.Left;
                    durationAxis.MajorStep = 1;
                    durationAxis.MinorStep = 0.5;
                    durationAxis.MajorGridlineStyle = LineStyle.Solid;
                    durationAxis.MinorGridlineStyle = LineStyle.Solid;
                    durationAxis.MajorGridlineColor = OxyColor.FromRgb(200, 190, 170);
                    durationAxis.MinorGridlineColor = OxyColor.FromRgb(250, 225, 205);
                    durationAxis.AxislineColor = OxyColor.FromRgb(0, 0, 0);

                    DateTimeAxis dateAxis = new DateTimeAxis();
                    dateAxis.Key = "DateAxis";
                    dateAxis.Minimum = DateTimeAxis.ToDouble(StartDatePicker.Date);
                    dateAxis.Maximum = DateTimeAxis.ToDouble(EndDatePicker.Date);
                    dateAxis.AbsoluteMinimum = DateTimeAxis.ToDouble(StartDatePicker.Date);
                    dateAxis.AbsoluteMaximum = DateTimeAxis.ToDouble(EndDatePicker.Date);
                    dateAxis.Position = AxisPosition.Bottom;
                    dateAxis.AxislineColor = OxyColor.FromRgb(0, 0, 0);
                    dateAxis.StringFormat = "dd-MMM-yyyy";
                    dateAxis.MajorGridlineStyle = LineStyle.Solid;
                    dateAxis.MajorGridlineColor = OxyColor.FromRgb(230, 190, 190);
                    dateAxis.IntervalType = DateTimeIntervalType.Auto;
                    dateAxis.FirstDayOfWeek = DayOfWeek.Monday;
                    dateAxis.MinorIntervalType = DateTimeIntervalType.Auto;

                    _viewModel.SleepPlotModel = new PlotModel();
                    _viewModel.SleepPlotModel.Background = OxyColors.White;
                    _viewModel.SleepPlotModel.Axes.Add(dateAxis);
                    _viewModel.SleepPlotModel.Axes.Add(durationAxis);

                    _viewModel.SleepPlotModel.LegendPosition = LegendPosition.BottomCenter;
                    _viewModel.SleepPlotModel.LegendBackground = OxyColors.LightYellow;

                    Func<double, double> averageFunc = (x) => _viewModel.SleepStats.TotalAverage.TotalMinutes / 60.0;
                    _viewModel.SleepPlotModel.Series.Add(new FunctionSeries(averageFunc, dateAxis.Minimum, dateAxis.Maximum, (int)(dateAxis.Maximum - dateAxis.Minimum), AverageSleepTitle.Text));

                    Func<double, double> averageYearFunc = (x) => _viewModel.SleepStats.LastYearAverage.TotalMinutes / 60.0;
                    _viewModel.SleepPlotModel.Series.Add(new FunctionSeries(averageYearFunc, dateAxis.Minimum, dateAxis.Maximum, (int)(dateAxis.Maximum - dateAxis.Minimum), AverageSleepYearTitle.Text));

                    Func<double, double> averageMonthFunc = (x) => _viewModel.SleepStats.LastMonthAverage.TotalMinutes / 60.0;
                    _viewModel.SleepPlotModel.Series.Add(new FunctionSeries(averageMonthFunc, dateAxis.Minimum, dateAxis.Maximum, (int)(dateAxis.Maximum - dateAxis.Minimum), AverageSleepMonthTitle.Text));

                    if (ChartTypePicker.SelectedIndex == 0)
                    {
                        _viewModel.SleepPlotModel.Series.Add(sleepLineSeries);
                    }
                    if (ChartTypePicker.SelectedIndex == 1)
                    {
                        _viewModel.SleepPlotModel.Series.Add(sleepStairStepSeries);
                    }
                    if (ChartTypePicker.SelectedIndex == 2)
                    {
                        _viewModel.SleepPlotModel.Series.Add(sleepStemSeries);
                    }
                    _viewModel.SleepPlotModel.InvalidatePlot(true);
                }
            }

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
            if (internetAccess != _viewModel.Online)
            {
                _viewModel.Online = internetAccess;
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
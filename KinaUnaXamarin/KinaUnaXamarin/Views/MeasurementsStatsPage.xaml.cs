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
    public partial class MeasurementsStatsPage : ContentPage
    {
        private readonly MeasurementsStatsViewModel _viewModel;
        private bool _reload = true;
        
        public MeasurementsStatsPage()
        {
            InitializeComponent();

            _viewModel = new MeasurementsStatsViewModel();
            MeasurementsStatsGrid.BindingContext = _viewModel;
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
                    Device.BeginInvokeOnMainThread(() => { _viewModel.StartDate = _viewModel.FirstDate = _viewModel.Progeny.BirthDay.Value.Date; });
                    
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    _viewModel.EndDate = _viewModel.LastDate = EndDatePicker.Date = DateTime.Now.Date;
                    ChartTypePicker.SelectedItem = _viewModel.ChartTypeList[0];
                });
            }

            await UpdateMeasurements();
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

        private async Task UpdateMeasurements()
        {
            _viewModel.IsBusy = true;
            _viewModel.TodayDate = DateTime.Now;
            
           // _viewModel.MeasurementsList = await ProgenyService.GetMeasurementsList(_viewChild, _viewModel.UserAccessLevel, _userInfo.Timezone);
           
            List<Measurement> measurementsList =  await ProgenyService.GetMeasurementsList(_viewModel.ViewChild, _viewModel.UserAccessLevel);
            
            if (measurementsList != null && measurementsList.Count > 0)
            {
                measurementsList = measurementsList.OrderBy(m => m.Date).ToList();
                LineSeries heightLineSeries = new LineSeries()
                {
                    Color = OxyColors.DarkGreen,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.White,
                    MarkerFill = OxyColors.Green,
                    MarkerStrokeThickness = 1.5,
                    Title = HeightLabel.Text
                    
                };

                StairStepSeries heightStairStepSeries = new StairStepSeries()
                {
                    Color = OxyColors.DarkGreen,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.White,
                    MarkerFill = OxyColors.Green,
                    MarkerStrokeThickness = 1.5,
                    Title = HeightLabel.Text
                };

                StemSeries heightStemSeries = new StemSeries()
                {
                    Color = OxyColors.DarkGreen,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.White,
                    MarkerFill = OxyColors.Green,
                    MarkerStrokeThickness = 1.5,
                    Title = HeightLabel.Text
                };

                LineSeries weightLineSeries = new LineSeries()
                {
                    Color = OxyColors.DarkGreen,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.White,
                    MarkerFill = OxyColors.Green,
                    MarkerStrokeThickness = 1.5,
                    Title = WeightLabel.Text

                };

                StairStepSeries weightStairStepSeries = new StairStepSeries()
                {
                    Color = OxyColors.DarkGreen,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.White,
                    MarkerFill = OxyColors.Green,
                    MarkerStrokeThickness = 1.5,
                    Title = WeightLabel.Text
                };

                StemSeries weightStemSeries = new StemSeries()
                {
                    Color = OxyColors.DarkGreen,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.White,
                    MarkerFill = OxyColors.Green,
                    MarkerStrokeThickness = 1.5,
                    Title = WeightLabel.Text
                };

                double maxHeight = 0;
                double maxWeight = 0;
                double minHeight = 1000;
                double minWeight = 1000;
                DateTime firstMeasurementItem = _viewModel.EndDate;
                DateTime lastMeasurementItem = _viewModel.StartDate;

                foreach (Measurement mes in measurementsList)
                {
                    if (mes.Date >= StartDatePicker.Date && mes.Date <= EndDatePicker.Date)
                    {
                        double dateDouble = DateTimeAxis.ToDouble(mes.Date.Date);
                        if (mes.Height > 0)
                        {
                            heightLineSeries.Points.Add(new DataPoint(dateDouble, mes.Height));
                            heightStairStepSeries.Points.Add(new DataPoint(dateDouble, mes.Height));
                            heightStemSeries.Points.Add(new DataPoint(dateDouble, mes.Height));
                            if (mes.Height > maxHeight)
                            {
                                maxHeight = mes.Height;
                            }

                            if (mes.Height < minHeight)
                            {
                                minHeight = mes.Height;
                            }
                        }

                        if (mes.Weight > 0)
                        {
                            weightLineSeries.Points.Add(new DataPoint(dateDouble, mes.Weight));
                            weightStairStepSeries.Points.Add(new DataPoint(dateDouble, mes.Weight));
                            weightStemSeries.Points.Add(new DataPoint(dateDouble, mes.Weight));
                            if (mes.Weight > maxWeight)
                            {
                                maxWeight = mes.Weight;
                            }

                            if (mes.Weight < minWeight)
                            {
                                minWeight = mes.Weight;
                            }
                        }
                    }
                    if (mes.Date < firstMeasurementItem)
                    {
                        firstMeasurementItem = mes.Date;
                    }

                    if (mes.Date > lastMeasurementItem)
                    {
                        lastMeasurementItem = mes.Date;
                    }
                }
                
                _viewModel.HeightMinValue = Math.Floor(minHeight);
                _viewModel.HeightMaxValue = Math.Ceiling(maxHeight);
                _viewModel.WeightMinValue = Math.Floor(minWeight);
                _viewModel.WeightMaxValue = Math.Ceiling(maxWeight);
                _viewModel.FirstDate = firstMeasurementItem;
                _viewModel.LastDate = lastMeasurementItem;

                LinearAxis heightAxis = new LinearAxis();
                heightAxis.Key = "HeightAxis";
                heightAxis.Minimum = _viewModel.HeightMinValue * 0.9;
                heightAxis.Maximum = _viewModel.HeightMaxValue * 1.1;
                heightAxis.Position = AxisPosition.Left;
                heightAxis.MajorStep = 10;
                heightAxis.MinorStep = 5;
                heightAxis.MajorGridlineStyle = LineStyle.Solid;
                heightAxis.MinorGridlineStyle = LineStyle.Solid;
                heightAxis.MajorGridlineColor = OxyColor.FromRgb(200, 190, 170);
                heightAxis.MinorGridlineColor = OxyColor.FromRgb(250, 225, 205);
                heightAxis.AxislineColor = OxyColor.FromRgb(0, 0, 0);

                LinearAxis weightAxis = new LinearAxis();
                weightAxis.Key = "WeightAxis";
                weightAxis.Minimum = _viewModel.WeightMinValue * 0.9;
                weightAxis.Maximum = _viewModel.WeightMaxValue * 1.1;
                weightAxis.Position = AxisPosition.Left;
                weightAxis.MajorStep = 2;
                weightAxis.MinorStep = 1;
                weightAxis.MajorGridlineStyle = LineStyle.Solid;
                weightAxis.MinorGridlineStyle = LineStyle.Solid;
                weightAxis.MajorGridlineColor = OxyColor.FromRgb(200, 190, 170);
                weightAxis.MinorGridlineColor = OxyColor.FromRgb(250, 225, 205);
                weightAxis.AxislineColor = OxyColor.FromRgb(0, 0, 0);

                DateTimeAxis dateAxis = new DateTimeAxis();
                dateAxis.Key = "DateAxisHeight";
                dateAxis.Minimum = DateTimeAxis.ToDouble(StartDatePicker.Date);
                dateAxis.Maximum = DateTimeAxis.ToDouble(EndDatePicker.Date);
                dateAxis.Position = AxisPosition.Bottom;
                dateAxis.AxislineColor = OxyColor.FromRgb(0, 0, 0);
                dateAxis.StringFormat = "dd-MMM-yyyy";
                dateAxis.MajorGridlineStyle = LineStyle.Solid;
                dateAxis.MajorGridlineColor = OxyColor.FromRgb(230, 190, 190);
                dateAxis.IntervalType = DateTimeIntervalType.Auto;
                dateAxis.FirstDayOfWeek = DayOfWeek.Monday;
                dateAxis.MinorIntervalType = DateTimeIntervalType.Auto;

                DateTimeAxis dateAxisWeight = new DateTimeAxis();
                dateAxisWeight.Key = "DateAxisWeight";
                dateAxisWeight.Minimum = DateTimeAxis.ToDouble(StartDatePicker.Date);
                dateAxisWeight.Maximum = DateTimeAxis.ToDouble(EndDatePicker.Date);
                dateAxisWeight.Position = AxisPosition.Bottom;
                dateAxisWeight.AxislineColor = OxyColor.FromRgb(0, 0, 0);
                dateAxisWeight.StringFormat = "dd-MMM-yyyy";
                dateAxisWeight.MajorGridlineStyle = LineStyle.Solid;
                dateAxisWeight.MajorGridlineColor = OxyColor.FromRgb(230, 190, 190);
                dateAxisWeight.IntervalType = DateTimeIntervalType.Auto;
                dateAxisWeight.FirstDayOfWeek = DayOfWeek.Monday;
                dateAxisWeight.MinorIntervalType = DateTimeIntervalType.Auto;

                _viewModel.PlotModelHeight = new PlotModel();
                _viewModel.PlotModelHeight.Background = OxyColors.White;
                _viewModel.PlotModelHeight.Axes.Add(heightAxis);
                _viewModel.PlotModelHeight.Axes.Add(dateAxis);
                _viewModel.PlotModelHeight.LegendPosition = LegendPosition.BottomCenter;
                _viewModel.PlotModelHeight.LegendBackground = OxyColors.LightYellow;

                _viewModel.PlotModelWeight = new PlotModel();
                _viewModel.PlotModelWeight.Background = OxyColors.White;
                _viewModel.PlotModelWeight.Axes.Add(weightAxis);
                _viewModel.PlotModelWeight.Axes.Add(dateAxisWeight);
                _viewModel.PlotModelWeight.LegendPosition = LegendPosition.BottomCenter;
                _viewModel.PlotModelWeight.LegendBackground = OxyColors.LightYellow;


                if (ChartTypePicker.SelectedIndex == 0)
                {
                    _viewModel.PlotModelHeight.Series.Add(heightLineSeries);
                    _viewModel.PlotModelWeight.Series.Add(weightLineSeries);
                }
                if (ChartTypePicker.SelectedIndex == 1)
                {
                    _viewModel.PlotModelHeight.Series.Add(heightStairStepSeries);
                    _viewModel.PlotModelWeight.Series.Add(weightStairStepSeries);
                }
                if (ChartTypePicker.SelectedIndex == 2)
                {
                    _viewModel.PlotModelHeight.Series.Add(heightStemSeries);
                    _viewModel.PlotModelWeight.Series.Add(weightStemSeries);
                }

                _viewModel.PlotModelHeight.InvalidatePlot(true);
                _viewModel.PlotModelWeight.InvalidatePlot(true);
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
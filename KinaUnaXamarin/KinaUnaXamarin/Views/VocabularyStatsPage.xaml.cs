﻿using System;
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
    public partial class VocabularyStatsPage
    {
        private VocabularyStatsViewModel _viewModel;
        private bool _reload = true;
        
        public VocabularyStatsPage()
        {
            InitializeComponent();
            _viewModel = new VocabularyStatsViewModel();
            VocabularyStatsGrid.BindingContext = _viewModel;
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

            await UpdateVocabulary();

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

        private async Task UpdateVocabulary()
        {
            _viewModel.IsBusy = true;
            _viewModel.TodayDate = DateTime.Now;
            

            List<VocabularyItem> vocabularyList =
                await ProgenyService.GetVocabularyList(_viewModel.ViewChild, _viewModel.UserAccessLevel, _viewModel.UserInfo.Timezone);
            
            _viewModel.VocabularyItems.ReplaceRange(vocabularyList);
            if (vocabularyList != null && vocabularyList.Count > 0)
            {
                vocabularyList = vocabularyList.OrderBy(v => v.Date).ToList();
                List<WordDateCount> dateTimesList = new List<WordDateCount>();
                int wordCount = 0;
                foreach (VocabularyItem vocabularyItem in vocabularyList)
                {
                    wordCount++;
                    if (vocabularyItem.Date != null)
                    {
                        if (dateTimesList.SingleOrDefault(d => d.WordDate.Date == vocabularyItem.Date.Value.Date) == null)
                        {
                            WordDateCount newDate = new WordDateCount();
                            newDate.WordDate = vocabularyItem.Date.Value.Date;
                            newDate.WordCount = wordCount;
                            dateTimesList.Add(newDate);
                        }
                        else
                        {
                            WordDateCount wrdDateCount = dateTimesList.SingleOrDefault(d => d.WordDate.Date == vocabularyItem.Date.Value.Date);
                            if (wrdDateCount != null)
                            {
                                wrdDateCount.WordCount = wordCount;
                            }
                        }
                    }
                }

                LineSeries vocabularyLineSeries = new LineSeries()
                {
                    Color = OxyColors.DarkGreen,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.White,
                    MarkerFill = OxyColors.Green,
                    MarkerStrokeThickness = 1.5,
                    Title = this.Title
                    
                };

                StairStepSeries vocabularyStairStepSeries = new StairStepSeries()
                {
                    Color = OxyColors.DarkGreen,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.White,
                    MarkerFill = OxyColors.Green,
                    MarkerStrokeThickness = 1.5,
                    Title = this.Title
                };

                StemSeries vocabularyStemSeries = new StemSeries()
                {
                    Color = OxyColors.DarkGreen,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.White,
                    MarkerFill = OxyColors.Green,
                    MarkerStrokeThickness = 1.5,
                    Title = this.Title
                };

                double maxCount = 0;
                double minCount = 100000;
                DateTime firstWordDate = _viewModel.EndDate;
                DateTime lastWordDate = _viewModel.StartDate;

                foreach (WordDateCount wordDateCount in dateTimesList)
                {
                    if (wordDateCount.WordDate >= StartDatePicker.Date && wordDateCount.WordDate <= EndDatePicker.Date)
                    {
                        double wordDateDouble = DateTimeAxis.ToDouble(wordDateCount.WordDate.Date);
                        vocabularyLineSeries.Points.Add(new DataPoint(wordDateDouble, wordDateCount.WordCount));
                        vocabularyStairStepSeries.Points.Add(new DataPoint(wordDateDouble, wordDateCount.WordCount));
                        vocabularyStemSeries.Points.Add(new DataPoint(wordDateDouble, wordDateCount.WordCount));

                        if (wordDateCount.WordCount > maxCount)
                        {
                            maxCount = wordDateCount.WordCount;
                        }

                        if (wordDateCount.WordCount < minCount)
                        {
                            minCount = wordDateCount.WordCount;
                        }
                    }
                    if (wordDateCount.WordDate < firstWordDate)
                    {
                        firstWordDate = wordDateCount.WordDate;
                    }

                    if (wordDateCount.WordDate > lastWordDate)
                    {
                        lastWordDate = wordDateCount.WordDate;
                    }
                }
                
                _viewModel.MinValue = Math.Floor(minCount);
                _viewModel.MaxValue = Math.Ceiling(maxCount);
                _viewModel.FirstDate = firstWordDate;
                _viewModel.LastDate = lastWordDate;

                LinearAxis wordCountAxis = new LinearAxis();
                wordCountAxis.Key = "WordCountAxis";
                wordCountAxis.Minimum = 0; //_viewModel.MinValue -1;
                wordCountAxis.Maximum = _viewModel.MaxValue; // + 1;
                wordCountAxis.Position = AxisPosition.Left;
                wordCountAxis.MajorStep = 10;
                wordCountAxis.MinorStep = 5;
                wordCountAxis.MajorGridlineStyle = LineStyle.Solid;
                wordCountAxis.MinorGridlineStyle = LineStyle.Solid;
                wordCountAxis.MajorGridlineColor = OxyColor.FromRgb(200, 190, 170);
                wordCountAxis.MinorGridlineColor = OxyColor.FromRgb(250, 225, 205);
                wordCountAxis.AxislineColor = OxyColor.FromRgb(0, 0, 0);

                DateTimeAxis dateAxis = new DateTimeAxis();
                dateAxis.Key = "DateAxis";
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

                _viewModel.VocabularyPlotModel = new PlotModel();
                _viewModel.VocabularyPlotModel.Background = OxyColors.White;
                _viewModel.VocabularyPlotModel.Axes.Add(wordCountAxis);
                _viewModel.VocabularyPlotModel.Axes.Add(dateAxis);
                _viewModel.VocabularyPlotModel.LegendPosition = LegendPosition.TopLeft;
                _viewModel.VocabularyPlotModel.LegendBackground = OxyColors.LightYellow;
                
                if (ChartTypePicker.SelectedIndex == 0)
                {
                    _viewModel.VocabularyPlotModel.Series.Add(vocabularyLineSeries);
                }
                if (ChartTypePicker.SelectedIndex == 1)
                {
                    _viewModel.VocabularyPlotModel.Series.Add(vocabularyStairStepSeries);
                }
                if (ChartTypePicker.SelectedIndex == 2)
                {
                    _viewModel.VocabularyPlotModel.Series.Add(vocabularyStemSeries);
                }

                _viewModel.VocabularyPlotModel.InvalidatePlot(true);
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
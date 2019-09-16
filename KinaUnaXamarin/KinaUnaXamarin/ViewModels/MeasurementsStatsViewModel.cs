using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using MvvmHelpers;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Plugin.Multilingual;
using Xamarin.Forms;

namespace KinaUnaXamarin.ViewModels
{
    class MeasurementsStatsViewModel:BaseViewModel
    {
        private bool _isLoggedIn;
        private Progeny _progeny;
        private int _userAccessLevel;
        private bool _loggedOut;
        private bool _showOptions;
        private bool _canUserAddItems;
        private DateTime _startDate;
        private DateTime _endDate;
        private DateTime _todayDate;
        private DateTime _firstDate;
        private DateTime _lastDate;
        private List<string> _chartTypeList;
        private double _heightMaxValue;
        private double _heightMinValue;
        private double _weightMaxValue;
        private double _weightMinValue;
        private OxyPlot.PlotModel _plotModelHeight;
        private OxyPlot.PlotModel _plotModelWeight;

        public ObservableCollection<Progeny> ProgenyCollection { get; set; }

        public MeasurementsStatsViewModel()
        {
            LoginCommand = new Command(Login);
            ProgenyCollection = new ObservableCollection<Progeny>();
            _lastDate = _todayDate = DateTime.Now;
            _firstDate = _startDate = DateTime.Now - TimeSpan.FromDays(30);
            _endDate = DateTime.Now;
            _heightMaxValue = 1;
            _heightMinValue = 0;
            _weightMaxValue = 1;
            _weightMinValue = 0;
            
            _chartTypeList = new List<string>();
            var ci = CrossMultilingual.Current.CurrentCultureInfo.TwoLetterISOLanguageName;
            if (ci == "da")
            {
                _chartTypeList.Add("Linjediagram");
                _chartTypeList.Add("Trappetrinsdiagram");
                _chartTypeList.Add("Stammediagram");
            }
            else
            {
                if (ci == "de")
                {
                    _chartTypeList.Add("Liniendiagramm");
                    _chartTypeList.Add("Treppenstufen-Diagramm");
                    _chartTypeList.Add("Stammdiagramm");
                }
                else
                {
                    _chartTypeList.Add("Line Chart");
                    _chartTypeList.Add("Stair Steps Chart");
                    _chartTypeList.Add("Stem Chart");
                }
            }
        }

        public List<string> ChartTypeList
        {

            get => _chartTypeList;
            set => SetProperty(ref _chartTypeList, value);
        }
        

        public OxyPlot.PlotModel PlotModelHeight
        {
            get => _plotModelHeight;
            set => SetProperty(ref _plotModelHeight, value);
        }

        public OxyPlot.PlotModel PlotModelWeight
        {
            get => _plotModelWeight;
            set => SetProperty(ref _plotModelWeight, value);
        }

        public DateTime TodayDate
        {
            get => _todayDate;
            set => SetProperty(ref _todayDate, value);
        }

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public DateTime FirstDate
        {
            get => _firstDate;
            set => SetProperty(ref _firstDate, value);
        }

        public DateTime LastDate
        {
            get => _lastDate;
            set => SetProperty(ref _lastDate, value);
        }

        
        public double HeightMaxValue
        {
            get => _heightMaxValue;
            set => SetProperty(ref _heightMaxValue, value);
        }

        public double HeightMinValue
        {
            get => _heightMinValue;
            set => SetProperty(ref _heightMinValue, value);
        }

        public double WeightMaxValue
        {
            get => _weightMaxValue;
            set => SetProperty(ref _weightMaxValue, value);
        }

        public double WeightMinValue
        {
            get => _weightMinValue;
            set => SetProperty(ref _weightMinValue, value);
        }

        public bool LoggedOut
        {
            get => _loggedOut;
            set => SetProperty(ref _loggedOut, value);
        }

        public ICommand LoginCommand
        {
            get;
            private set;
        }

        public async void Login()
        {
            IsLoggedIn = await UserService.LoginIdsAsync();
            if (IsLoggedIn)
            {
                LoggedOut = !IsLoggedIn;
            }
        }

        public bool CanUserAddItems
        {
            get => _canUserAddItems;
            set => SetProperty(ref _canUserAddItems, value);
        }

        public bool ShowOptions
        {
            get => _showOptions;
            set => SetProperty(ref _showOptions, value);
        }

        public Progeny Progeny
        {
            get => _progeny;
            set => SetProperty(ref _progeny, value);
        }

        public int UserAccessLevel
        {
            get => _userAccessLevel;
            set => SetProperty(ref _userAccessLevel, value);
        }

        
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }
    }
}

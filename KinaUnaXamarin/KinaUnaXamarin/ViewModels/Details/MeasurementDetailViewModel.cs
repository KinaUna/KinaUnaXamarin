using System;
using System.Collections.Generic;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;
using Plugin.Multilingual;

namespace KinaUnaXamarin.ViewModels.Details
{
    class MeasurementDetailViewModel : BaseViewModel
    {
        private int _currentMeasurementId;
        private int _currentIndex;
        private int _userAccessLevel;
        private bool _loggedOut;
        private Progeny _progeny;
        private bool _isLoggedIn;
        private Measurement _currentMeasurement;
        private bool _editMode;
        private bool _canUserEditItems;
        private List<string> _accessLevelList;
        private int _accessLevel;
        private int _dateYear;
        private int _dateMonth;
        private int _dateDay;
        private string _height;
        private string _weight;
        private string _circumference;
        private string _hairColor;
        private string _eyeColor;
        private DateTime _date;
        private bool _isSaving;

        public MeasurementDetailViewModel()
        {
            _progeny = new Progeny();
            _currentMeasurement = new Measurement();

            _currentMeasurement.Date = DateTime.Now;
            _currentMeasurement.AccessLevel = 0;
            
            _dateYear = _currentMeasurement.Date.Year;
            _dateMonth = _currentMeasurement.Date.Month;
            _dateDay = _currentMeasurement.Date.Day;
            
            MeasurementItems = new ObservableRangeCollection<Measurement>();

            _accessLevelList = new List<string>();
            var ci = CrossMultilingual.Current.CurrentCultureInfo.TwoLetterISOLanguageName;
            if (ci == "da")
            {
                _accessLevelList.Add("Administratorer");
                _accessLevelList.Add("Familie");
                _accessLevelList.Add("Omsorgspersoner/Speciel adgang");
                _accessLevelList.Add("Venner");
                _accessLevelList.Add("Registrerede brugere");
                _accessLevelList.Add("Offentlig/alle");
            }
            else
            {
                if (ci == "de")
                {
                    _accessLevelList.Add("Administratoren");
                    _accessLevelList.Add("Familie");
                    _accessLevelList.Add("Betreuer/Spezial");
                    _accessLevelList.Add("Freunde");
                    _accessLevelList.Add("Registrierte Benutzer");
                    _accessLevelList.Add("Allen zugänglich");
                }
                else
                {
                    _accessLevelList.Add("Hidden/Private");
                    _accessLevelList.Add("Family");
                    _accessLevelList.Add("Caretakers/Special Access");
                    _accessLevelList.Add("Friends");
                    _accessLevelList.Add("Registered Users");
                    _accessLevelList.Add("Public/Anyone");
                }
            }
        }

        public ObservableRangeCollection<Measurement> MeasurementItems { get; set; }

        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        public List<string> AccessLevelList
        {
            get => _accessLevelList;
            set => SetProperty(ref _accessLevelList, value);
        }

        public bool EditMode
        {
            get => _editMode;
            set => SetProperty(ref _editMode, value);
        }

        public bool CanUserEditItems
        {
            get => _canUserEditItems;
            set => SetProperty(ref _canUserEditItems, value);
        }

        public Measurement CurrentMeasurement
        {
            get => _currentMeasurement;
            set => SetProperty(ref _currentMeasurement, value);
        }

        public int CurrentMeasurementId
        {
            get => _currentMeasurementId;
            set => SetProperty(ref _currentMeasurementId, value);
        }

        public int CurrentIndex
        {
            get => _currentIndex;
            set => SetProperty(ref _currentIndex, value);
        }

        public bool LoggedOut
        {
            get => _loggedOut;
            set => SetProperty(ref _loggedOut, value);
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
        
        public int DateYear
        {
            get => _dateYear;
            set => SetProperty(ref _dateYear, value);
        }

        public int DateMonth
        {
            get => _dateMonth;
            set => SetProperty(ref _dateMonth, value);
        }

        public int DateDay
        {
            get => _dateDay;
            set => SetProperty(ref _dateDay, value);
        }
        
        public int AccessLevel
        {
            get => _accessLevel;
            set => SetProperty(ref _accessLevel, value);
        }
        
        public string Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public string Weight
        {
            get => _weight;
            set => SetProperty(ref _weight, value);
        }

        public string Circumference
        {
            get => _circumference;
            set => SetProperty(ref _circumference, value);
        }

        public string HairColor
        {
            get => _hairColor;
            set => SetProperty(ref _hairColor, value);
        }

        public string EyeColor
        {
            get => _eyeColor;
            set => SetProperty(ref _eyeColor, value);
        }

        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }
    }
}

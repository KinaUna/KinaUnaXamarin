using System;
using System.Collections.Generic;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;
using Plugin.Multilingual;

namespace KinaUnaXamarin.ViewModels.Details
{
    class SleepDetailViewModel : BaseViewModel
    {
        private int _currentSleepId;
        private int _currentIndex;
        private int _userAccessLevel;
        private bool _loggedOut;
        private Progeny _progeny;
        private bool _isLoggedIn;
        private Sleep _currentSleep;
        private bool _editMode;
        private bool _canUserEditItems;
        private List<string> _accessLevelList;
        private int _startHours;
        private int _endHours;
        private int _startMinutes;
        private int _endMinutes;
        private int _startYear;
        private int _endYear;
        private int _startMonth;
        private int _endMonth;
        private int _startDay;
        private int _endDay;
        private int _rating = 3;
        private int _accessLevel;
        private TimeSpan _duration;
        private bool _isSaving;

        public SleepDetailViewModel()
        {
            _progeny = new Progeny();
            _currentSleep = new Sleep();

            _currentSleep.SleepStart = DateTime.Now;
            _currentSleep.SleepEnd = DateTime.Now + TimeSpan.FromMinutes(10);
            _currentSleep.AccessLevel = 0;
            _currentSleep.SleepRating = 3;

            _startYear = _currentSleep.SleepStart.Year;
            _startMonth = _currentSleep.SleepStart.Month;
            _startDay = _currentSleep.SleepStart.Day;
            _startHours = _currentSleep.SleepStart.Hour;
            _startMinutes = _currentSleep.SleepStart.Minute;

            _endYear = _currentSleep.SleepEnd.Year;
            _endMonth = _currentSleep.SleepEnd.Month;
            _endDay = _currentSleep.SleepEnd.Day;
            _endHours = _currentSleep.SleepEnd.Hour;
            _endMinutes = _currentSleep.SleepEnd.Minute;

            SleepItems = new ObservableRangeCollection<Sleep>();
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

        public ObservableRangeCollection<Sleep> SleepItems { get; set; }

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

        public Sleep CurrentSleep
        {
            get => _currentSleep;
            set => SetProperty(ref _currentSleep, value);
        }

        public int CurrentSleepId
        {
            get => _currentSleepId;
            set => SetProperty(ref _currentSleepId, value);
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

        public int StartHours
        {
            get => _startHours;
            set => SetProperty(ref _startHours, value);
        }

        public int EndHours
        {
            get => _endHours;
            set => SetProperty(ref _endHours, value);
        }

        public int StartMinutes
        {
            get => _startMinutes;
            set => SetProperty(ref _startMinutes, value);
        }

        public int EndMinutes
        {
            get => _endMinutes;
            set => SetProperty(ref _endMinutes, value);
        }

        public int StartYear
        {
            get => _startYear;
            set => SetProperty(ref _startYear, value);
        }

        public int EndYear
        {
            get => _endYear;
            set => SetProperty(ref _endYear, value);
        }

        public int StartMonth
        {
            get => _startMonth;
            set => SetProperty(ref _startMonth, value);
        }

        public int EndMonth
        {
            get => _endMonth;
            set => SetProperty(ref _endMonth, value);
        }

        public int StartDay
        {
            get => _startDay;
            set => SetProperty(ref _startDay, value);
        }

        public int EndDay
        {
            get => _endDay;
            set => SetProperty(ref _endDay, value);
        }

        public int Rating
        {
            get => _rating;
            set => SetProperty(ref _rating, value);
        }

        public int AccessLevel
        {
            get => _accessLevel;
            set => SetProperty(ref _accessLevel, value);
        }

        public TimeSpan Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }
    }
}

using System;
using System.Collections.Generic;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;
using Plugin.Multilingual;

namespace KinaUnaXamarin.ViewModels.Details
{
    class EventDetailViewModel : BaseViewModel
    {
        private int _currentEventId;
        private int _currentIndex;
        private int _userAccessLevel;
        private bool _loggedOut;
        private Progeny _progeny;
        private bool _isLoggedIn;
        private CalendarItem _currentEvent;
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
        private bool _allDay;
        private int _accessLevel;
        private string _eventTitle;
        private string _notes;
        private string _context;
        private string _location;
        private List<string> _locationAutoSuggestList;
        private List<string> _contextAutoSuggestList;
        private bool _isSaving;

        public EventDetailViewModel()
        {
            _progeny = new Progeny();
            _currentEvent = new CalendarItem();

            _currentEvent.StartTime = DateTime.Now;
            _currentEvent.EndTime = DateTime.Now + TimeSpan.FromMinutes(10);
            _currentEvent.AccessLevel = 0;
            

            _startYear = _currentEvent.StartTime.Value.Year;
            _startMonth = _currentEvent.StartTime.Value.Month;
            _startDay = _currentEvent.StartTime.Value.Day;
            _startHours = _currentEvent.StartTime.Value.Hour;
            _startMinutes = _currentEvent.StartTime.Value.Minute;

            _endYear = _currentEvent.EndTime.Value.Year;
            _endMonth = _currentEvent.EndTime.Value.Month;
            _endDay = _currentEvent.EndTime.Value.Day;
            _endHours = _currentEvent.EndTime.Value.Hour;
            _endMinutes = _currentEvent.EndTime.Value.Minute;

            EventItems = new ObservableRangeCollection<CalendarItem>();
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

        public ObservableRangeCollection<CalendarItem> EventItems { get; set; }

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

        public CalendarItem CurrentEvent
        {
            get => _currentEvent;
            set => SetProperty(ref _currentEvent, value);
        }

        public int CurrentEventId
        {
            get => _currentEventId;
            set => SetProperty(ref _currentEventId, value);
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

        
        public int AccessLevel
        {
            get => _accessLevel;
            set => SetProperty(ref _accessLevel, value);
        }

        public bool AllDay
        {
            get => _allDay;
            set => SetProperty(ref _allDay, value);
        }

        public string EventTitle
        {
            get => _eventTitle;
            set => SetProperty(ref _eventTitle, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        public string Context
        {
            get => _context;
            set => SetProperty(ref _context, value);
        }

        public List<string> LocationAutoSuggestList
        {
            get => _locationAutoSuggestList;
            set => SetProperty(ref _locationAutoSuggestList, value);
        }

        public List<string> ContextAutoSuggestList
        {
            get => _contextAutoSuggestList;
            set => SetProperty(ref _contextAutoSuggestList, value);
        }
    }
}

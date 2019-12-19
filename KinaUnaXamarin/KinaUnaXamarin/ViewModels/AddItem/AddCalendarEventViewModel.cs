using System.Collections.Generic;
using System.Collections.ObjectModel;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;
using Plugin.Multilingual;

namespace KinaUnaXamarin.ViewModels.AddItem
{
    class AddCalendarEventViewModel: BaseViewModel
    {
        private CalendarItem _calendarItem;
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
        private int _accessLevel;
        private string _notes;
        private bool _isSaving;
        private List<string> _accessLevelList;
        private List<string> _locationAutoSuggestList;
        private List<string> _contextAutoSuggestList;

        public AddCalendarEventViewModel()
        {
            ProgenyCollection = new ObservableCollection<Progeny>();
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

        public ObservableCollection<Progeny> ProgenyCollection { get; set; }
        
        public List<string> AccessLevelList {

            get => _accessLevelList;
            set => SetProperty(ref _accessLevelList, value);
        }
        public CalendarItem CalendarItem
        {
            get => _calendarItem;
            set => SetProperty(ref _calendarItem, value);
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

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
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

        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }
    }
}

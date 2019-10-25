using System;
using System.Collections.Generic;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;
using Plugin.Multilingual;

namespace KinaUnaXamarin.ViewModels
{
    class NoteDetailViewModel : BaseViewModel
    {
        private int _currentNoteId;
        private int _currentIndex;
        private int _userAccessLevel;
        private bool _loggedOut;
        private Progeny _progeny;
        private bool _isLoggedIn;
        private Note _currentNote;
        private bool _editMode;
        private bool _canUserEditItems;
        private List<string> _accessLevelList;
        private int _accessLevel;
        private int _dateYear;
        private int _dateMonth;
        private int _dateDay;
        private int _dateHours;
        private int _dateMinutes;
        private string _noteTitle;
        private string _content;
        private string _category;
        private DateTime _date;
        private TimeSpan _time;
        private List<string> _categoryAutoSuggestList;

        public NoteDetailViewModel()
        {
            _progeny = new Progeny();
            _currentNote = new Note();

            _currentNote.CreatedDate = DateTime.Now;
            _currentNote.AccessLevel = 0;
            
            _dateYear = _currentNote.CreatedDate.Year;
            _dateMonth = _currentNote.CreatedDate.Month;
            _dateDay = _currentNote.CreatedDate.Day;
            _dateHours = _currentNote.CreatedDate.Hour;
            _dateMinutes = _currentNote.CreatedDate.Minute;
            
            NoteItems = new ObservableRangeCollection<Note>();

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

        public ObservableRangeCollection<Note> NoteItems { get; set; }

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

        public Note CurrentNote
        {
            get => _currentNote;
            set => SetProperty(ref _currentNote, value);
        }

        public int CurrentNoteId
        {
            get => _currentNoteId;
            set => SetProperty(ref _currentNoteId, value);
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

        public string NoteTitle
        {
            get => _noteTitle;
            set => SetProperty(ref _noteTitle, value);
        }

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
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

        public int DateHours
        {
            get => _dateHours;
            set => SetProperty(ref _dateHours, value);
        }

        public int DateMinutes
        {
            get => _dateMinutes;
            set => SetProperty(ref _dateMinutes, value);
        }

        public int AccessLevel
        {
            get => _accessLevel;
            set => SetProperty(ref _accessLevel, value);
        }

        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        public TimeSpan Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

        public List<string> CategoryAutoSuggestList
        {
            get => _categoryAutoSuggestList;
            set => SetProperty(ref _categoryAutoSuggestList, value);
        }
    }
}

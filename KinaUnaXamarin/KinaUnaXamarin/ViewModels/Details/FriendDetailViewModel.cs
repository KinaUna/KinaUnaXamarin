using System;
using System.Collections.Generic;
using System.Linq;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;
using Plugin.Multilingual;

namespace KinaUnaXamarin.ViewModels
{
    class FriendDetailViewModel : BaseViewModel
    {
        private int _currentFriendId;
        private int _currentIndex;
        private int _userAccessLevel;
        private bool _loggedOut;
        private Progeny _progeny;
        private bool _isLoggedIn;
        private Friend _currentFriend;
        private bool _editMode;
        private bool _canUserEditItems;
        private List<string> _accessLevelList;
        private List<string> _friendTypeList;
        private int _friendType;
        private string _friendTypeString;
        private int _accessLevel;
        private int _dateYear;
        private int _dateMonth;
        private int _dateDay;
        private DateTime? _friendSince;
        private string _notes;
        private string _tags;
        private string _name;
        private string _description;
        private List<string> _tagsAutoSuggestList;
        private List<string> _contextAutoSuggestList;
        private string _context;

        public FriendDetailViewModel()
        {
            _progeny = new Progeny();
            _currentFriend = new Friend();

            _currentFriend.FriendSince = DateTime.Now;
            _currentFriend.AccessLevel = 0;
            
            _dateYear = _currentFriend.FriendSince.Value.Year;
            _dateMonth = _currentFriend.FriendSince.Value.Month;
            _dateDay = _currentFriend.FriendSince.Value.Day;
            
            FriendItems = new ObservableRangeCollection<Friend>();

            _friendTypeList = new List<string>();
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

                _friendTypeList.Add("Personlige venner");
                _friendTypeList.Add("Legetøj/Dyr");
                _friendTypeList.Add("Forældre");
                _friendTypeList.Add("Familie");
                _friendTypeList.Add("Omsorgspersoner");
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

                    _friendTypeList.Add("Persönliche Freunde");
                    _friendTypeList.Add("Spielzeuge/Tiere");
                    _friendTypeList.Add("Eltern");
                    _friendTypeList.Add("Familie");
                    _friendTypeList.Add("Betreuer");
                }
                else
                {
                    _accessLevelList.Add("Hidden/Private");
                    _accessLevelList.Add("Family");
                    _accessLevelList.Add("Caretakers/Special Access");
                    _accessLevelList.Add("Friends");
                    _accessLevelList.Add("Registered Users");
                    _accessLevelList.Add("Public/Anyone");

                    _friendTypeList.Add("Personal Friends");
                    _friendTypeList.Add("Toy/Animal");
                    _friendTypeList.Add("Parents");
                    _friendTypeList.Add("Family");
                    _friendTypeList.Add("Caretakers");
                }
            }
        }

        public ObservableRangeCollection<Friend> FriendItems { get; set; }

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

        public Friend CurrentFriend
        {
            get => _currentFriend;
            set => SetProperty(ref _currentFriend, value);
        }

        public int CurrentFriendId
        {
            get => _currentFriendId;
            set => SetProperty(ref _currentFriendId, value);
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

        public List<string> FriendTypeList
        {

            get => _friendTypeList;
            set => SetProperty(ref _friendTypeList, value);
        }

        public int FriendType
        {
            get => _friendType;
            set => SetProperty(ref _friendType, value);
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
        
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public string Tags
        {
            get => _tags;
            set => SetProperty(ref _tags, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string Context
        {
            get => _context;
            set => SetProperty(ref _context, value);
        }

        public string FriendTypeString
        {
            get {
                if (_friendTypeList.Any() && _friendTypeList.Count > _friendType)
                {
                    return _friendTypeList[_friendType];
                }
                else
                {
                    return "";
                }
            }

            set => SetProperty(ref _friendTypeString, value);
        }

        public DateTime? FriendSince
        {
            get => _friendSince;
            set => SetProperty(ref _friendSince, value);
        }

        public List<string> ContextAutoSuggestList
        {
            get => _contextAutoSuggestList;
            set => SetProperty(ref _contextAutoSuggestList, value);
        }

        public List<string> TagsAutoSuggestList
        {
            get => _tagsAutoSuggestList;
            set => SetProperty(ref _tagsAutoSuggestList, value);
        }
    }
}

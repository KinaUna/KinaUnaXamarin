using System;
using System.Collections.Generic;
using System.Linq;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;
using Plugin.Multilingual;

namespace KinaUnaXamarin.ViewModels
{
    class ContactDetailViewModel : BaseViewModel
    {
        private int _currentContactId;
        private int _currentIndex;
        private int _userAccessLevel;
        private bool _loggedOut;
        private Progeny _progeny;
        private bool _isLoggedIn;
        private Contact _currentContact;
        private bool _editMode;
        private bool _canUserEditItems;
        private List<string> _accessLevelList;
        private int _accessLevel;
        private string _firstName;
        private string _lastName;
        private string _middleName;
        private string _displayName;
        private string _addressLine1;
        private string _addressLine2;
        private string _city;
        private string _state;
        private string _postalCode;
        private string _country;
        private string _email1;
        private string _email2;
        private string _phoneNumber;
        private string _mobileNumber;
        private string _website;
        private string _context;
        private string _notes;
        private string _tags;
        private List<string> _tagsAutoSuggestList;
        private List<string> _contextAutoSuggestList;
        private bool _active;
        private int _addressIdNumber;
        private int _dateYear;
        private int _dateMonth;
        private int _dateDay;
        private DateTime? _date;

        public ContactDetailViewModel()
        {
            _progeny = new Progeny();
            _currentContact = new Contact();

            _currentContact.AccessLevel = 0;
            
            ContactItems = new ObservableRangeCollection<Contact>();
            _currentContact.DateAdded = DateTime.Now;
            _dateYear = _currentContact.DateAdded.Value.Year;
            _dateMonth = _currentContact.DateAdded.Value.Month;
            _dateDay = _currentContact.DateAdded.Value.Day;

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

        public ObservableRangeCollection<Contact> ContactItems { get; set; }

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

        public Contact CurrentContact
        {
            get => _currentContact;
            set => SetProperty(ref _currentContact, value);
        }

        public int CurrentContactId
        {
            get => _currentContactId;
            set => SetProperty(ref _currentContactId, value);
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

        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        public string MiddleName
        {
            get => _middleName;
            set => SetProperty(ref _middleName, value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        public string AddressLine1
        {
            get => _addressLine1;
            set => SetProperty(ref _addressLine1, value);
        }

        public string AddressLine2
        {
            get => _addressLine2;
            set => SetProperty(ref _addressLine2, value);
        }

        public string City
        {
            get => _city;
            set => SetProperty(ref _city, value);
        }

        public string State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        public string PostalCode
        {
            get => _postalCode;
            set => SetProperty(ref _postalCode, value);
        }

        public string Country
        {
            get => _country;
            set => SetProperty(ref _country, value);
        }

        public string Email1
        {
            get => _email1;
            set => SetProperty(ref _email1, value);
        }

        public string Email2
        {
            get => _email2;
            set => SetProperty(ref _email2, value);
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        public string MobileNumber
        {
            get => _mobileNumber;
            set => SetProperty(ref _mobileNumber, value);
        }

        public string Website
        {
            get => _website;
            set => SetProperty(ref _website, value);
        }

        public string Context
        {
            get => _context;
            set => SetProperty(ref _context, value);
        }

        public bool Active
        {
            get => _active;
            set => SetProperty(ref _active, value);
        }

        public int AddressIdNumber
        {
            get => _addressIdNumber;
            set => SetProperty(ref _addressIdNumber, value);
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

        public DateTime? Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }
    }
}

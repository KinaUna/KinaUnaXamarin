using System;
using System.Collections.Generic;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;
using Plugin.Multilingual;

namespace KinaUnaXamarin.ViewModels.Details
{
    class VaccinationDetailViewModel : BaseViewModel
    {
        private int _currentVaccinationId;
        private int _currentIndex;
        private int _userAccessLevel;
        private bool _loggedOut;
        private Progeny _progeny;
        private bool _isLoggedIn;
        private Vaccination _currentVaccination;
        private bool _editMode;
        private bool _canUserEditItems;
        private List<string> _accessLevelList;
        private int _accessLevel;
        private int _dateYear;
        private int _dateMonth;
        private int _dateDay;
        private string _notes;
        private string _description;
        private string _name;
        
        public VaccinationDetailViewModel()
        {
            _progeny = new Progeny();
            _currentVaccination = new Vaccination();

            _currentVaccination.VaccinationDate = DateTime.Now;
            _currentVaccination.AccessLevel = 0;
            
            _dateYear = _currentVaccination.VaccinationDate.Year;
            _dateMonth = _currentVaccination.VaccinationDate.Month;
            _dateDay = _currentVaccination.VaccinationDate.Day;
            
            VaccinationItems = new ObservableRangeCollection<Vaccination>();

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

        public ObservableRangeCollection<Vaccination> VaccinationItems { get; set; }

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

        public Vaccination CurrentVaccination
        {
            get => _currentVaccination;
            set => SetProperty(ref _currentVaccination, value);
        }

        public int CurrentVaccinationId
        {
            get => _currentVaccinationId;
            set => SetProperty(ref _currentVaccinationId, value);
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
        
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}

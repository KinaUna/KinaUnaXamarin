using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using MvvmHelpers;
using Plugin.Multilingual;
using Xamarin.Forms;

namespace KinaUnaXamarin.ViewModels
{
    class ContactsViewModel:BaseViewModel
    {
        private bool _isLoggedIn;
        private Progeny _progeny;
        private int _userAccessLevel;
        private bool _loggedOut;
        private bool _showOptions;
        private bool _canUserAddItems;
        private List<string> _sortByList;
        private int _columns;

        public ObservableCollection<Progeny> ProgenyCollection { get; set; }
        public ObservableRangeCollection<Contact> ContactItems { get; set; }

        public ContactsViewModel()
        {
            LoginCommand = new Command(Login);
            ProgenyCollection = new ObservableCollection<Progeny>();
            ContactItems = new ObservableRangeCollection<Contact>();
            _sortByList = new List<string>();
            _columns = 2;
            var ci = CrossMultilingual.Current.CurrentCultureInfo.TwoLetterISOLanguageName;
            if (ci == "da")
            {
                _sortByList.Add("Visningsnavn");
                _sortByList.Add("Fornavn");
                _sortByList.Add("Efternavn");
            }
            else
            {
                if (ci == "de")
                {
                    _sortByList.Add("Anzeigename");
                    _sortByList.Add("Vorname");
                    _sortByList.Add("Nachname");
                }
                else
                {
                    _sortByList.Add("Display Name");
                    _sortByList.Add("First Name");
                    _sortByList.Add("Last Name");
                }
            }
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

        public int Columns
        {
            get => _columns;
            set => SetProperty(ref _columns, value);
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }
        

        public List<string> SortByList
        {
            get => _sortByList;
            set => SetProperty(ref _sortByList, value);
        }
    }
}

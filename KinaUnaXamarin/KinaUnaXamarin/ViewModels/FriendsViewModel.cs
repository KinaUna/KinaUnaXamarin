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
    class FriendsViewModel:BaseViewModel
    {
        private bool _isLoggedIn;
        private Progeny _progeny;
        private int _userAccessLevel;
        private bool _loggedOut;
        private bool _showOptions;
        private bool _canUserAddItems;
        private List<string> _friendTypeList;
        private List<string> _sortByList;
        private int _columns;

        public ObservableCollection<Progeny> ProgenyCollection { get; set; }
        public ObservableRangeCollection<Friend> FriendItems { get; set; }

        public FriendsViewModel()
        {
            LoginCommand = new Command(Login);
            ProgenyCollection = new ObservableCollection<Progeny>();
            FriendItems = new ObservableRangeCollection<Friend>();
            _friendTypeList = new List<string>();
            _sortByList = new List<string>();
            _columns = 2;
            var ci = CrossMultilingual.Current.CurrentCultureInfo.TwoLetterISOLanguageName;
            if (ci == "da")
            {
                _friendTypeList.Add("Personlige venner");
                _friendTypeList.Add("Legetøj/Dyr");
                _friendTypeList.Add("Forældre");
                _friendTypeList.Add("Familie");
                _friendTypeList.Add("Omsorgspersoner");
                _sortByList.Add("Venner siden");
                _sortByList.Add("Navn");
            }
            else
            {
                if (ci == "de")
                {
                    _friendTypeList.Add("Persönliche Freunde");
                    _friendTypeList.Add("Spielzeuge/Tiere");
                    _friendTypeList.Add("Eltern");
                    _friendTypeList.Add("Familie");
                    _friendTypeList.Add("Betreuer");
                    _sortByList.Add("Freunde zeit");
                    _sortByList.Add("Name");
                }
                else
                {
                    _friendTypeList.Add("Personal Friends");
                    _friendTypeList.Add("Toy/Animal");
                    _friendTypeList.Add("Parents");
                    _friendTypeList.Add("Family");
                    _friendTypeList.Add("Caretakers");
                    _sortByList.Add("Friends Since");
                    _sortByList.Add("Name");
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

        public List<string> FriendTypeList
        {
            get => _friendTypeList;
            set => SetProperty(ref _friendTypeList, value);
        }

        public List<string> SortByList
        {
            get => _sortByList;
            set => SetProperty(ref _sortByList, value);
        }
    }
}

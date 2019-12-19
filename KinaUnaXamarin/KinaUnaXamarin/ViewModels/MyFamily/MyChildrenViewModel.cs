using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using MvvmHelpers;
using Xamarin.Forms;

namespace KinaUnaXamarin.ViewModels.MyFamily
{
    class MyChildrenViewModel:BaseViewModel
    {
        private Progeny _progeny;
        private bool _editMode;
        private DateTime _progenyBirthDay;
        private TimeZoneInfo _selectedTimeZone;
        private string _profilePicture;
        private bool _anyChildren;
        private bool _isLoggedIn;
        private bool _loggedOut;
        private int _userAccessLevel;
        private bool _showOptions;
        private bool _canUserAddItems;

        public MyChildrenViewModel()
        {
            LoginCommand = new Command(Login);
            ProgenyCollection = new ObservableCollection<Progeny>();
            ProgenyAdminCollection = new ObservableCollection<Progeny>();
            TimeZoneList = new ObservableCollection<TimeZoneInfo>();
            _progeny = OfflineDefaultData.DefaultProgeny;
            _progenyBirthDay = new DateTime(2018, 02, 18, 18, 02, 00);
        }

        public ObservableCollection<Progeny> ProgenyCollection { get; set; }
        public ObservableCollection<Progeny> ProgenyAdminCollection { get; set; }
        public ObservableCollection<TimeZoneInfo> TimeZoneList { get; set; }

        public Progeny Progeny
        {
            get => _progeny;
            set
            {
                SetProperty(ref _progeny, value);
                if (value.BirthDay.HasValue)
                {
                    ProgenyBirthDay = value.BirthDay.Value;
                }
                else
                {
                    ProgenyBirthDay = new DateTime(2018, 02, 18, 18, 02, 00);
                }

                if (string.IsNullOrEmpty(value.PictureLink))
                {
                    ProfilePicture = Constants.ProfilePicture;
                }
                else
                {
                    ProfilePicture = _progeny.PictureLink;
                }
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

        public int UserAccessLevel
        {
            get => _userAccessLevel;
            set => SetProperty(ref _userAccessLevel, value);
        }

        public bool EditMode
        {
            get => _editMode;
            set => SetProperty(ref _editMode, value);
        }

        public string ProfilePicture
        {
            get => _profilePicture;
            set => SetProperty(ref _profilePicture, value);
        }

        public DateTime ProgenyBirthDay
        {
            get => _progenyBirthDay;
            set => SetProperty(ref _progenyBirthDay, value);
        }

        public TimeZoneInfo SelectedTimeZone
        {
            get => _selectedTimeZone;
            set => SetProperty(ref _selectedTimeZone, value);
        }

        public bool AnyChildren
        {
            get => _anyChildren;
            set => SetProperty(ref _anyChildren, value);
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

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }
    }
}

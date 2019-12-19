using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using MvvmHelpers;
using Xamarin.Forms;

namespace KinaUnaXamarin.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        private string _message;
        private string _username;
        private string _firstName;
        private string _middleName;
        private string _lastName;
        private string _fullName;
        private string _email;
        private string _userId;
        private string _timezone;
        private bool _loggedIn;
        private bool _loggedOut;
        private string _profilePicture;
        private bool _editMode;

        public AccountViewModel()
        {
            TimeZoneList = new ObservableCollection<TimeZoneInfo>();
            ProgenyCollection = new ObservableCollection<Progeny>();
            ProgenyCollection.Add(OfflineDefaultData.DefaultProgeny);
            LoginCommand = new Command(Login);
            LogoutCommand = new Command(Logout);
        }

        public ObservableCollection<TimeZoneInfo> TimeZoneList { get; set; }

        public ObservableCollection<Progeny> ProgenyCollection { get; set; }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
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

        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string ProfilePicture
        {
            get => _profilePicture;
            set => SetProperty(ref _profilePicture, value);
        }

        public string UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        public string Timezone
        {
            get => _timezone;
            set => SetProperty(ref _timezone, value);
        }

        public bool EditMode
        {
            get => _editMode;
            set => SetProperty(ref _editMode, value);
        }

        public bool LoggedIn
        {
            get => _loggedIn;
            set => SetProperty(ref _loggedIn, value);
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

        public ICommand LogoutCommand
        {
            get;
            private set;
        }

        private async void Login()
        {
            LoggedIn = await UserService.LoginIdsAsync();
            if (LoggedIn)
            {
                Message = "";
                Username = await UserService.GetUsername();
                FullName = await UserService.GetFullname();
                Email = await UserService.GetUserEmail();
                Timezone = await UserService.GetUserTimezone();
                UserId = await UserService.GetUserId();
            }

            MessagingCenter.Send(this, "Reload");
        }

        private async void Logout()
        {
            LoggedIn = !(await UserService.LogoutIdsAsync());
            if (!LoggedIn)
            {
                UserId = "";
                Username = "";
                FullName = "";
                Email = "";
                Timezone = "";
                Message = "";
            }

            MessagingCenter.Send(this, "Reload");
        }
    }
}

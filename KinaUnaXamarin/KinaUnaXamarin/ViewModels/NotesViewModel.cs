using System.Collections.ObjectModel;
using System.Windows.Input;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using MvvmHelpers;
using Xamarin.Forms;

namespace KinaUnaXamarin.ViewModels
{
    class NotesViewModel:BaseViewModel
    {
        private bool _isLoggedIn = true;
        private Progeny _progeny;
        private bool _showOptions;
        private bool _canUserAddItems;
        private int _pageNumber;
        private int _pageCount;
        private bool _online = true;
        private int _itemsPerPage = 5;

        public ObservableCollection<Progeny> ProgenyCollection { get; set; }
        public ObservableRangeCollection<Note> NoteItems { get; set; }

        public NotesViewModel()
        {
            LoginCommand = new Command(Login);
            ViewChild = Constants.DefaultChildId;
            UserInfo = OfflineDefaultData.DefaultUserInfo;
            ProgenyCollection = new ObservableCollection<Progeny>();
            NoteItems = new ObservableRangeCollection<Note>();
            
        }

        public int ViewChild { get; set; }

        public UserInfo UserInfo { get; set; }

        public string AccessToken { get; set; }

        public bool Online
        {
            get => _online;
            set => SetProperty(ref _online, value);
        }

        public int ItemsPerPage
        {
            get => _itemsPerPage;
            set => SetProperty(ref _itemsPerPage, value);
        }

        public int PageNumber
        {
            get => _pageNumber;
            set => SetProperty(ref _pageNumber, value);
        }

        public int PageCount
        {
            get => _pageCount;
            set => SetProperty(ref _pageCount, value);
        }
        

        public ICommand LoginCommand
        {
            get;
            private set;
        }

        public async void Login()
        {
            IsLoggedIn = await UserService.LoginIdsAsync();
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

        public int UserAccessLevel { get; set; }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using MvvmHelpers;
using Xamarin.Forms;

namespace KinaUnaXamarin.ViewModels
{
    class VocabularyViewModel: BaseViewModel
    {
        private bool _isLoggedIn;
        private int _viewChild = Constants.DefaultChildId;
        private UserInfo _userInfo;
        private string _accessToken;
        private Progeny _progeny;
        private int _userAccessLevel;
        private bool _loggedOut;
        private bool _showOptions;
        private bool _canUserAddItems;
        private int _pageNumber;
        private int _pageCount;
        private bool _online = true;
        private bool _isRefreshing;

        public ObservableCollection<Progeny> ProgenyCollection { get; set; }

        public VocabularyViewModel()
        {
            LoginCommand = new Command(Login);
            ProgenyCollection = new ObservableCollection<Progeny>();
            VocabularyItems = new ObservableRangeCollection<VocabularyItem>();
        }

        public ICommand RefreshCommand => new Command(async () => await RefreshAsync());

        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            if (PageNumber < 1)
            {
                PageNumber = 1;
            }

            VocabularyListPage vocabularyListPage = await ProgenyService.GetVocabularyListPage(PageNumber, 20, ViewChild, UserAccessLevel, 1);
            if (vocabularyListPage.VocabularyList != null)
            {
                vocabularyListPage.VocabularyList =
                    vocabularyListPage.VocabularyList.OrderByDescending(v => v.Date).ToList();
                VocabularyItems.ReplaceRange(vocabularyListPage.VocabularyList);
                PageNumber = vocabularyListPage.PageNumber;
                PageCount = vocabularyListPage.TotalPages;
                // VocabularyListView.ScrollTo(0);
            }
            IsRefreshing = false;
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public bool Online
        {
            get => _online;
            set => SetProperty(ref _online, value);
        }

        public int ViewChild
        {
            get => _viewChild;
            set => SetProperty(ref _viewChild, value);
        }

        public UserInfo UserInfo
        {
            get => _userInfo;
            set => SetProperty(ref _userInfo, value);
        }

        public string AccessToken
        {
            get => _accessToken;
            set => SetProperty(ref _accessToken, value);
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

        public ObservableRangeCollection<VocabularyItem> VocabularyItems { get; set; }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }
    }
}

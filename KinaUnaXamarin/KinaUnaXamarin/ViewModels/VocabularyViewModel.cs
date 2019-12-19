using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using MvvmHelpers;
using Xamarin.Forms;

namespace KinaUnaXamarin.ViewModels
{
    class VocabularyViewModel: BaseViewModel
    {
        private bool _isLoggedIn = true;
        private Progeny _progeny;
        private bool _showOptions;
        private bool _canUserAddItems;
        private int _pageNumber;
        private int _pageCount;
        private bool _online = true;
        private bool _isRefreshing;
        private int _itemsPerPage = 20;

        public ObservableCollection<Progeny> ProgenyCollection { get; set; }

        public VocabularyViewModel()
        {
            LoginCommand = new Command(Login);
            ViewChild = Constants.DefaultChildId;
            UserInfo = OfflineDefaultData.DefaultUserInfo;
            ProgenyCollection = new ObservableCollection<Progeny>();
            VocabularyItems = new ObservableRangeCollection<VocabularyItem>();
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
            }
            IsRefreshing = false;
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
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

        public ObservableRangeCollection<VocabularyItem> VocabularyItems { get; set; }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }
    }
}

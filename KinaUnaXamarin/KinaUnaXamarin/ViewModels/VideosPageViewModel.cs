using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Resources;
using System.Windows.Input;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using MvvmHelpers;
using Plugin.Multilingual;
using Xamarin.Forms;

namespace KinaUnaXamarin.ViewModels
{
    class VideosPageViewModel : BaseViewModel
    {
        private bool _isLoggedIn = true;
        private Progeny _progeny;
        private int _userAccessLevel;
        private bool _showOptions;
        private bool _canUserAddItems;
        private int _pageNumber;
        private int _pageCount;
        private int _itemsPerPage = 8;
        private string _tagFilter = "";
        private bool _online = true;
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public ObservableCollection<string> TagsCollection { get; set; }

        public ObservableCollection<Progeny> ProgenyCollection { get; set; }

        public VideosPageViewModel()
        {
            LoginCommand = new Command(Login);
            ViewChild = Constants.DefaultChildId;
            ProgenyCollection = new ObservableCollection<Progeny>();
            VideoItems = new ObservableRangeCollection<Video>();
            TagsCollection = new ObservableCollection<string>();
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string allTags = resmgr.Value.GetString("AllTags", ci);
            TagsCollection.Add(allTags);
        }

        public int ViewChild { get; set; }

        public UserInfo UserInfo { get; set; }

        public string AccessToken { get; set; }

        public bool Online
        {
            get => _online;
            set => SetProperty(ref _online, value);
        }

        public string TagFilter
        {
            get => _tagFilter;
            set => SetProperty(ref _tagFilter, value);
        }

        public int PageNumber
        {
            get => _pageNumber;
            set => SetProperty(ref _pageNumber, value);
        }

        public int ItemsPerPage
        {
            get => _itemsPerPage;
            set => SetProperty(ref _itemsPerPage, value);
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

        public int UserAccessLevel
        {
            get => _userAccessLevel;
            set => SetProperty(ref _userAccessLevel, value);
        }

        public ObservableRangeCollection<Video> VideoItems { get; set; }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }
    }
}

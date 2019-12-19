using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels.MyFamily;
using Plugin.Multilingual;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views.MyFamily
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserAccessPage
    {
        private readonly UserAccessViewModel _viewModel;
        private bool _online = true;
        private bool _reload;
        private int _selectedProgenyId;
        private int _viewChild = Constants.DefaultChildId;
        private UserInfo _userInfo;
        private string _accessToken;
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));
        
        public UserAccessPage()
        {
            InitializeComponent();
            _viewModel = new UserAccessViewModel();
            _viewModel.AnyChildren = true;
            _reload = true;
            BindingContext = _viewModel;
            ProgenyCollectionView.ItemsSource = _viewModel.ProgenyCollection;
            UserAccessCollectionView.ItemsSource = _viewModel.UserAccessCollection;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            if (_reload)
            {
                _reload = false;
                await CheckAccount();
                await Reload();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        private async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var networkAccess = e.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess != _online)
            {
                await Reload();
            }
        }

        private async Task CheckAccount()
        {
            string userEmail = await UserService.GetUserEmail();
            _accessToken = await UserService.GetAuthAccessToken();
            bool accessTokenCurrent = false;
            if (_accessToken != "")
            {
                accessTokenCurrent = await UserService.IsAccessTokenCurrent();

                if (!accessTokenCurrent)
                {
                    bool loginSuccess = await UserService.LoginIdsAsync();
                    if (loginSuccess)
                    {
                        _accessToken = await UserService.GetAuthAccessToken();
                        accessTokenCurrent = true;
                    }

                    await Reload();
                }
            }

            if (String.IsNullOrEmpty(_accessToken) || !accessTokenCurrent)
            {

                _viewModel.IsLoggedIn = false;
                _viewModel.LoggedOut = true;
                _accessToken = "";
                _userInfo = OfflineDefaultData.DefaultUserInfo;

            }
            else
            {
                _viewModel.IsLoggedIn = true;
                _viewModel.LoggedOut = false;
                _userInfo = await UserService.GetUserInfo(userEmail);
            }

            string userviewchild = await SecureStorage.GetAsync(Constants.UserViewChildKey);
            bool viewchildParsed = int.TryParse(userviewchild, out _viewChild);
            if (!viewchildParsed)
            {
                _viewChild = _userInfo.ViewChild;
            }
            if (_viewChild == 0)
            {
                if (_userInfo.ViewChild != 0)
                {
                    _viewChild = _userInfo.ViewChild;
                }
                else
                {
                    _viewChild = Constants.DefaultChildId;
                }
            }

            if (String.IsNullOrEmpty(_userInfo.Timezone))
            {
                _userInfo.Timezone = Constants.DefaultTimeZone;
            }
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(_userInfo.Timezone);
            }
            catch (Exception)
            {
                _userInfo.Timezone = TZConvert.WindowsToIana(_userInfo.Timezone);
            }

            Progeny progeny = await ProgenyService.GetProgeny(_viewChild);
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(progeny.TimeZone);
            }
            catch (Exception)
            {
                progeny.TimeZone = TZConvert.WindowsToIana(progeny.TimeZone);
            }
            _viewModel.Progeny = progeny;

            List<Progeny> progenyList = await ProgenyService.GetProgenyList(userEmail);
            _viewModel.ProgenyCollection.Clear();
            if (progenyList.Any())
            {
                foreach (Progeny prog in progenyList)
                {
                    _viewModel.ProgenyCollection.Add(prog);
                }
            }
        }

        private async Task Reload()
        {
            _viewModel.IsBusy = true;
            _viewModel.EditMode = false;
            _viewModel.ProgenyCollection.Clear();
            UserAccessCollectionView.SelectedItem = null;
            _viewModel.SelectedAccess = null;
            await ProgenyService.GetProgenyList(await UserService.GetUserEmail());
            List<Progeny> progenyList = await ProgenyService.GetProgenyAdminList();
            if (progenyList.Any())
            {
                _viewModel.AnyChildren = true;
                foreach (Progeny progeny in progenyList)
                {
                    _viewModel.ProgenyCollection.Add(progeny);
                }

                if (_selectedProgenyId == 0)
                {
                    string userviewchild = await SecureStorage.GetAsync(Constants.UserViewChildKey);
                    bool viewchildParsed = int.TryParse(userviewchild, out int viewChild);
                    Progeny viewProgeny = new Progeny();
                    if (viewchildParsed)
                    {
                        viewProgeny = _viewModel.ProgenyCollection.SingleOrDefault(p => p.Id == viewChild);
                    }
                    
                    if (viewProgeny != null)
                    {
                        ProgenyCollectionView.SelectedItem =
                            _viewModel.ProgenyCollection.SingleOrDefault(p => p.Id == viewChild);
                        ProgenyCollectionView.ScrollTo(ProgenyCollectionView.SelectedItem);
                    }
                    else
                    {
                        ProgenyCollectionView.SelectedItem = _viewModel.ProgenyCollection[0];
                    }
                }
                else
                {
                    ProgenyCollectionView.SelectedItem =
                        _viewModel.ProgenyCollection.SingleOrDefault(p => p.Id == _selectedProgenyId);
                }

                //_viewModel.Progeny = (Progeny)ProgenyCollectionView.SelectedItem;
                
                //_selectedProgenyId = _viewModel.Progeny.Id;
                
                //List<UserAccess> userAccessList = await ProgenyService.GetProgenyAccessList(_selectedProgenyId);
                //_viewModel.UserAccessCollection.Clear();
                //foreach (UserAccess ua in userAccessList)
                //{
                //    ua.AccessLevelString = _viewModel.AccessLevelList[ua.AccessLevel];
                //    if (string.IsNullOrEmpty(ua.User.UserName))
                //    {
                //        ua.User.UserName = ua.User.Email;
                //    }
                //    _viewModel.UserAccessCollection.Add(ua);
                //}
            }
            else
            {
                _viewModel.AnyChildren = false;
            }

            _viewModel.IsBusy = false;
        }

        private async void ProgenyCollectionView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserAccessCollectionView.SelectedItem = null;
            _viewModel.SelectedAccess = null;
            _viewModel.EditMode = false;
            _viewModel.IsBusy = true;
            _viewModel.Progeny = (Progeny)ProgenyCollectionView.SelectedItem;
            if (_viewModel.Progeny != null)
            {
                ProgenyCollectionView.ScrollTo(ProgenyCollectionView.SelectedItem);
                _selectedProgenyId = _viewModel.Progeny.Id;

                List<UserAccess> userAccessList = await ProgenyService.GetProgenyAccessList(_selectedProgenyId);
                _viewModel.UserAccessCollection.Clear();
                foreach (UserAccess ua in userAccessList)
                {
                    ua.AccessLevelString = _viewModel.AccessLevelList[ua.AccessLevel];
                    if (string.IsNullOrEmpty(ua.User.UserName))
                    {
                        ua.User.UserName = ua.User.Email;
                    }
                    _viewModel.UserAccessCollection.Add(ua);
                }
                UserAccessCollectionView.ScrollTo(0);
            }
            
            _viewModel.IsBusy = false;
        }

        private void UserAccessCollectionView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.SelectedAccess = (UserAccess) UserAccessCollectionView.SelectedItem;
            if (_viewModel.SelectedAccess != null)
            {
                _viewModel.EditMode = true;
            }
            else
            {
                _viewModel.EditMode = false;
            }
        }

        private async void SaveAccessButton_OnClicked(object sender, EventArgs e)
        {
            _viewModel.IsBusy = true;
            _viewModel.EditMode = false;
            _viewModel.SelectedAccess.AccessLevel = AccessLevelPicker.SelectedIndex;
            UserAccess updatedUserAccess = await ProgenyService.UpdateUserAccess(_viewModel.SelectedAccess);
            if (updatedUserAccess.AccessId != 0)
            {
                _viewModel.EditMode = false;
                _viewModel.SelectedAccess = null;
                UserAccessCollectionView.SelectedItem = null;
                // Todo: Show success message	
                await Reload();
            }
            else
            {
                _viewModel.EditMode = true;
                // Todo: Show failed message	
            }

            _viewModel.IsBusy = false;
        }

        private void CancelAccessButton_OnClicked(object sender, EventArgs e)
        {
            _viewModel.EditMode = false;
            _viewModel.SelectedAccess = null;
            UserAccessCollectionView.SelectedItem = null;
        }

        private async void ReloadToolbarButton_OnClicked(object sender, EventArgs e)
        {
            await Reload();
        }

        private async void ProgenyToolBarItem_OnClicked(object sender, EventArgs e)
        {
            ObservableCollection<Progeny> progenyCollection = new ObservableCollection<Progeny>();
            List<Progeny> progList = await ProgenyService.GetProgenyList(await UserService.GetUserEmail());
            foreach (Progeny progeny in progList)
            {
                progenyCollection.Add(progeny);
            }
            SelectProgenyPage selProPage = new SelectProgenyPage(progenyCollection);
            await Shell.Current.Navigation.PushModalAsync(selProPage);
        }

        private async void AddItemToolbarButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PushModalAsync(new AddItemPage());
        }

        private async void DeleteAccessButton_OnClicked(object sender, EventArgs e)
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string confirmTitle = resmgr.Value.GetString("DeleteAccess", ci);
            string confirmMessage = resmgr.Value.GetString("DeleteAccessMessage", ci) + " " + _viewModel.SelectedAccess.User.UserName + " ? ";
            string yes = resmgr.Value.GetString("Yes", ci);
            string no = resmgr.Value.GetString("No", ci);
            bool confirmDelete = await DisplayAlert(confirmTitle, confirmMessage, yes, no);
            if (confirmDelete)
            {
                _viewModel.IsBusy = true;
                _viewModel.EditMode = false;
                UserAccess updatedUserAccess = await ProgenyService.DeleteUserAccess(_viewModel.SelectedAccess);
                if (updatedUserAccess.AccessId != 0)
                {
                    _viewModel.EditMode = false;
                    _viewModel.SelectedAccess = null;
                    UserAccessCollectionView.SelectedItem = null;
                    // Todo: Show success message
                    await Reload();
                }
                else
                {
                    _viewModel.EditMode = true;
                    // Todo: Show failed message
                }
                _viewModel.IsBusy = false;
            }
        }
    }
}
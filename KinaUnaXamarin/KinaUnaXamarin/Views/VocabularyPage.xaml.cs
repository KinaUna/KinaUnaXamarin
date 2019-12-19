using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VocabularyPage : ContentPage
    {
        private VocabularyViewModel _viewModel;
        private bool _reload = true;
        
        public VocabularyPage()
        {
            InitializeComponent();
            
            if (Device.RuntimePlatform == Device.UWP || Device.RuntimePlatform == Device.iOS)
            {
                VocabularyListView.Header = null;
            }

            _viewModel = new VocabularyViewModel();
            ContainerStackLayout.BindingContext = _viewModel;
            BindingContext = _viewModel;

            MessagingCenter.Subscribe<SelectProgenyPage>(this, "Reload", async (sender) =>
            {
                _reload = true;
                await SetUserAndProgeny();
                _viewModel.PageNumber = 1;
                await Reload();
            });

            MessagingCenter.Subscribe<AccountViewModel>(this, "Reload", async (sender) =>
            {
                _reload = true;
                await SetUserAndProgeny();
                _viewModel.PageNumber = 1;
                await Reload();
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            VocabularyListView.SelectedItem = null;

            if (_reload)
            {
                await SetUserAndProgeny();
                await Reload();
            }

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            var networkAccess = Connectivity.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess)
            {
                _viewModel.Online = true;
            }
            else
            {
                _viewModel.Online = false;
            }

            _reload = false;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        private async Task Reload()
        {
            _viewModel.IsBusy = true;
            await CheckAccount();
            await UpdateVocabulary();

            var networkInfo = Connectivity.NetworkAccess;

            if (networkInfo == NetworkAccess.Internet)
            {
                // Connection to internet is available
                _viewModel.Online = true;
            }
            else
            {
                _viewModel.Online = false;
            }
            _viewModel.IsBusy = false;
        }

        private async Task SetUserAndProgeny()
        {
            _viewModel.UserInfo = OfflineDefaultData.DefaultUserInfo;

            string userEmail = await UserService.GetUserEmail();
            string userviewchild = await SecureStorage.GetAsync(Constants.UserViewChildKey);
            bool viewchildParsed = int.TryParse(userviewchild, out int viewChildId);

            if (viewchildParsed)
            {
                _viewModel.ViewChild = viewChildId;
                try
                {
                    _viewModel.Progeny = await App.Database.GetProgenyAsync(_viewModel.ViewChild);
                }
                catch (Exception)
                {
                    _viewModel.Progeny = await ProgenyService.GetProgeny(_viewModel.ViewChild);
                }

                _viewModel.UserInfo = await App.Database.GetUserInfoAsync(userEmail);
            }

            if (String.IsNullOrEmpty(_viewModel.UserInfo.Timezone))
            {
                _viewModel.UserInfo.Timezone = Constants.DefaultTimeZone;
            }
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(_viewModel.UserInfo.Timezone);
            }
            catch (Exception)
            {
                _viewModel.UserInfo.Timezone = TZConvert.WindowsToIana(_viewModel.UserInfo.Timezone);
            }
        }

        private async Task CheckAccount()
        {
            string userEmail = await UserService.GetUserEmail();
            _viewModel.AccessToken = await UserService.GetAuthAccessToken();
            bool accessTokenCurrent = false;
            if (_viewModel.AccessToken != "")
            {
                accessTokenCurrent = await UserService.IsAccessTokenCurrent();

                if (!accessTokenCurrent)
                {
                    bool loginSuccess = await UserService.LoginIdsAsync();
                    if (loginSuccess)
                    {
                        _viewModel.AccessToken = await UserService.GetAuthAccessToken();
                        accessTokenCurrent = true;
                    }

                    await Reload();
                }
            }

            if (String.IsNullOrEmpty(_viewModel.AccessToken) || !accessTokenCurrent)
            {

                _viewModel.IsLoggedIn = false;
                _viewModel.AccessToken = "";
                _viewModel.UserInfo = OfflineDefaultData.DefaultUserInfo;

            }
            else
            {
                _viewModel.IsLoggedIn = true;
                _viewModel.UserInfo = await UserService.GetUserInfo(userEmail);
            }

            await SetUserAndProgeny();

            Progeny progeny = await ProgenyService.GetProgeny(_viewModel.ViewChild);
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
            _viewModel.CanUserAddItems = false;
            foreach (Progeny prog in progenyList)
            {
                _viewModel.ProgenyCollection.Add(prog);
                if (prog.Admins.ToUpper().Contains(_viewModel.UserInfo.UserEmail.ToUpper()))
                {
                    _viewModel.CanUserAddItems = true;
                }
            }

            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewModel.ViewChild);
        }

        private async Task UpdateVocabulary()
        {
            _viewModel.IsBusy = true;
            if (_viewModel.PageNumber < 1)
            {
                _viewModel.PageNumber = 1;
            }

            _viewModel.ItemsPerPage = Preferences.Get(Constants.VocabularyPerPage, 20);
            VocabularyListPage vocabularyListPage = await ProgenyService.GetVocabularyListPage(_viewModel.PageNumber, _viewModel.ItemsPerPage, _viewModel.ViewChild, _viewModel.UserAccessLevel, 1);
            if (vocabularyListPage.VocabularyList != null)
            {
                vocabularyListPage.VocabularyList =
                    vocabularyListPage.VocabularyList.OrderByDescending(v => v.Date).ToList();
                _viewModel.VocabularyItems.ReplaceRange(vocabularyListPage.VocabularyList);
                _viewModel.PageNumber = vocabularyListPage.PageNumber;
                _viewModel.PageCount = vocabularyListPage.TotalPages;
                VocabularyListView.ScrollTo(0);
            }
            _viewModel.IsBusy = false;
        }

        private async void ProgenyToolBarItem_OnClicked(object sender, EventArgs e)
        {
            SelectProgenyPage selProPage = new SelectProgenyPage(_viewModel.ProgenyCollection);
            await Shell.Current.Navigation.PushModalAsync(selProPage);
        }

        private void OptionsToolBarItem_OnClicked(object sender, EventArgs e)
        {
            _viewModel.ShowOptions = !_viewModel.ShowOptions;
        }

        private async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var networkAccess = e.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess != _viewModel.Online)
            {
                _viewModel.Online = internetAccess;
                await Reload();
            }
        }

        private async void ReloadToolbarButton_OnClicked(object sender, EventArgs e)
        {
            await Reload();
        }

        private async void AddItemToolbarButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PushModalAsync(new AddItemPage());
        }

        private async void NewerButton_OnClicked(object sender, EventArgs e)
        {

            _viewModel.PageNumber--;
            if (_viewModel.PageNumber < 1)
            {
                _viewModel.PageNumber = _viewModel.PageCount;
            }
            await UpdateVocabulary();
        }

        private async void OlderButton_OnClicked(object sender, EventArgs e)
        {
            _viewModel.PageNumber++;
            if (_viewModel.PageNumber > _viewModel.PageCount)
            {
                _viewModel.PageNumber = 1;
            }
            await UpdateVocabulary();
        }

        private async void VocabularyListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VocabularyListView.SelectedItem is VocabularyItem selectedVocabularyItem)
            {
                VocabularyDetailPage vocabularyDetailPage = new VocabularyDetailPage(selectedVocabularyItem);
                // Reset selection
                VocabularyListView.SelectedItem = null;
                await Shell.Current.Navigation.PushModalAsync(vocabularyDetailPage);
            }

        }

        private async void SetOptionsButton_OnClicked(object sender, EventArgs e)
        {
            _viewModel.ShowOptions = false;
            _viewModel.PageNumber = 1;
            Preferences.Set(Constants.VocabularyPerPage, _viewModel.ItemsPerPage);
            await Reload();
        }
    }
}
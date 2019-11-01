using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public partial class FriendsPage : ContentPage
    {
        private int _viewChild = Constants.DefaultChildId;
        private UserInfo _userInfo;
        private FriendsViewModel _viewModel;
        private string _accessToken;
        private bool _reload = true;
        private bool _online = true;

        public FriendsPage()
        {
            InitializeComponent();
            
            MessagingCenter.Subscribe<SelectProgenyPage>(this, "Reload", async (sender) =>
            {
                await Reload();
            });

            MessagingCenter.Subscribe<AccountViewModel>(this, "Reload", async (sender) =>
            {
                await Reload();
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_reload)
            {
                _viewModel = new FriendsViewModel();
                _userInfo = OfflineDefaultData.DefaultUserInfo;
                ContainerGrid.BindingContext = _viewModel;
                BindingContext = _viewModel;
                SortByPicker.SelectedIndex = 0;
            }
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            var networkAccess = Connectivity.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess)
            {
                OfflineStackLayout.IsVisible = false;
            }
            else
            {
                OfflineStackLayout.IsVisible = true;
            }

            if (_reload)
            {
                await Reload();

            }

            _reload = false;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called
            int columns = (int)Math.Floor(width / 200);
            if (columns < 1)
            {
                columns = 1;
            }

            _viewModel.Columns = columns;
            FriendsCollectionView.ItemsLayout = new GridItemsLayout(columns, ItemsLayoutOrientation.Vertical);
        }

        private async Task Reload()
        {
            _viewModel.IsBusy = true;
            await CheckAccount();
            
            await UpdateFriends();
            var networkInfo = Connectivity.NetworkAccess;

            if (networkInfo == NetworkAccess.Internet)
            {
                // Connection to internet is available
                _online = true;
                OfflineStackLayout.IsVisible = false;
            }
            else
            {
                _online = false;
                OfflineStackLayout.IsVisible = true;
            }
            _viewModel.IsBusy = false;
        }

        private async Task CheckAccount()
        {
            string userEmail = await UserService.GetUserEmail();
            _accessToken = await UserService.GetAuthAccessToken();
            bool accessTokenCurrent = false;
            if (_accessToken != "")
            {
                string accessTokenExpires = await UserService.GetAuthAccessTokenExpires();
                accessTokenCurrent = UserService.IsAccessTokenCurrent(accessTokenExpires);

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
            _viewModel.CanUserAddItems = false;
            foreach (Progeny prog in progenyList)
            {
                _viewModel.ProgenyCollection.Add(prog);
                if (prog.Admins.ToUpper().Contains(_userInfo.UserEmail.ToUpper()))
                {
                    _viewModel.CanUserAddItems = true;
                }
            }

            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewChild);
        }

        private async Task UpdateFriends()
        {
            _viewModel.IsBusy = true;
            List<Friend> friendsList = await ProgenyService.GetFriendsList(_viewModel.Progeny.Id, _viewModel.UserAccessLevel);
            List<Friend> filteredFriends = new List<Friend>();

            if (PersonalCheckBox.IsChecked && ToyCheckBox.IsChecked && ParentCheckBox.IsChecked &&
                FamilyCheckBox.IsChecked && CaretakerCheckBox.IsChecked)
            {
                filteredFriends = friendsList;
            }
            else
            {
                foreach (Friend frn in friendsList)
                {
                    if (frn.Type == 0 && PersonalCheckBox.IsChecked)
                    {
                        filteredFriends.Add(frn);
                    }

                    if (frn.Type == 1 && ToyCheckBox.IsChecked)
                    {
                        filteredFriends.Add(frn);
                    }

                    if (frn.Type == 2 && ParentCheckBox.IsChecked)
                    {
                        filteredFriends.Add(frn);
                    }

                    if (frn.Type == 3 && FamilyCheckBox.IsChecked)
                    {
                        filteredFriends.Add(frn);
                    }

                    if (frn.Type == 4 && CaretakerCheckBox.IsChecked)
                    {
                        filteredFriends.Add(frn);
                    }
                }
            }

            if (SortByPicker.SelectedIndex == 1)
            {
                friendsList = filteredFriends.OrderBy(f => f.Name).ToList();
            }
            else
            {
                friendsList = filteredFriends.OrderBy(f => f.FriendSince).ToList();
            }
            _viewModel.FriendItems.ReplaceRange(friendsList);
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
            if (internetAccess != _online)
            {
                _online = internetAccess;
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


        private async void SubmitOptionsButton_OnClicked(object sender, EventArgs e)
        {
            _viewModel.ShowOptions = false;
            await Reload();
        }

        private async void FriendsCollectionView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FriendsCollectionView.SelectedItem is Friend friendItem)
            {
                FriendDetailPage friendDetailPage = new FriendDetailPage(friendItem);
                FriendsCollectionView.SelectedItem = null;
                await Shell.Current.Navigation.PushModalAsync(friendDetailPage);
            }
        }
    }
}
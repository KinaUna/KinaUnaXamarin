using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using KinaUnaXamarin.Views.Details;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideosPage
    {
        private VideosPageViewModel _viewModel;
        private bool _reload = true;
        private double _screenWidth;
        private double _screenHeight;

        public VideosPage()
        {
            InitializeComponent();
            _viewModel = new VideosPageViewModel();
            _viewModel.UserInfo = OfflineDefaultData.DefaultUserInfo;
            ContainerStackLayout.BindingContext = _viewModel;
            BindingContext = _viewModel;
            TagFilterPicker.SelectedIndex = 0;

            MessagingCenter.Subscribe<SelectProgenyPage>(this, "Reload", async (sender) =>
            {
                await SetUserAndProgeny();
                _viewModel.PageNumber = 1;
                await Reload();
            });

            MessagingCenter.Subscribe<AccountViewModel>(this, "Reload", async (sender) =>
            {
                await SetUserAndProgeny();
                _viewModel.PageNumber = 1;
                await Reload();
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            VideosListView.SelectedItem = null;

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

        private async Task Reload()
        {
            _viewModel.IsBusy = true;
            await CheckAccount();
            await UpdateVideos();
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

        protected override async void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called
            bool screenChanged = false;
            if (Device.RuntimePlatform == Device.UWP)
            {
                if (Math.Abs(_screenWidth - Application.Current.MainPage.Width) > 0.0001 ||
                    Math.Abs(_screenHeight - Application.Current.MainPage.Height) > 0.0001)
                {
                    _screenWidth = Application.Current.MainPage.Width;
                    _screenHeight = Application.Current.MainPage.Height;
                }

                screenChanged = true;
            }

            if (Math.Abs(_screenWidth - width) > 0.0001 || Math.Abs(_screenHeight - height) > 0.0001)
            {
                _screenWidth = width;
                _screenHeight = height;
                screenChanged = true;
            }

            if (screenChanged)
            {
                int columns = (int)Math.Floor(width / 200);

                if (Device.RuntimePlatform == Device.UWP)
                {
                    columns = (int)Math.Floor(Application.Current.MainPage.Width / 200);
                }
                if (columns < 1)
                {
                    columns = 1;
                }
                if (Device.RuntimePlatform == Device.iOS)
                {
                    await Task.Yield();
                }

                if (VideosListView.ItemsLayout is GridItemsLayout layout)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        layout.Span = columns;
                    });
                }
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

        private async Task UpdateVideos()
        {
            _viewModel.IsBusy = true;
            if (_viewModel.PageNumber < 1)
            {
                _viewModel.PageNumber = 1;
            }

            // _viewModel.TagsCollection.Clear();
            _viewModel.ItemsPerPage = Preferences.Get(Constants.VideosPerPage, 8);
            VideoPage videosPage = await ProgenyService.GetVideoPage(_viewModel.PageNumber, _viewModel.ItemsPerPage, _viewModel.ViewChild, _viewModel.UserAccessLevel, _viewModel.UserInfo.Timezone, 1, _viewModel.TagFilter);
            if (videosPage.VideosList != null)
            {
                _viewModel.VideoItems.ReplaceRange(videosPage.VideosList);
                _viewModel.PageNumber = videosPage.PageNumber;
                _viewModel.PageCount = videosPage.TotalPages;
                VideosListView.ScrollTo(0);

                if (!string.IsNullOrEmpty(videosPage.TagsList))
                {
                    List<string> tagsList = videosPage.TagsList.Split(',').ToList();
                    tagsList.Sort();
                    foreach (string tagString in tagsList)
                    {
                        _viewModel.TagsCollection.Add(tagString);
                    }
                }
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
            await UpdateVideos();
        }

        private async void OlderButton_OnClicked(object sender, EventArgs e)
        {
            _viewModel.PageNumber++;
            if (_viewModel.PageNumber > _viewModel.PageCount)
            {
                _viewModel.PageNumber = 1;
            }
            await UpdateVideos();
        }

        private async void VideosListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VideosListView.SelectedItem is Video selectedVideo)
            {
                VideoDetailPage videoPage = new VideoDetailPage(selectedVideo.VideoId);
                // Reset selection
                VideosListView.SelectedItem = null;
                await Shell.Current.Navigation.PushModalAsync(videoPage);
            }

        }

        private async void SetTagsFilterButton_OnClicked(object sender, EventArgs e)
        {
            _viewModel.ShowOptions = false;
            _viewModel.PageNumber = 1;
            if (TagFilterPicker.SelectedIndex < 1)
            {
                _viewModel.TagFilter = "";
            }
            else
            {
                string selectedTag = TagFilterPicker.SelectedItem.ToString();
                _viewModel.TagFilter = selectedTag;
            }
            Preferences.Set(Constants.VideosPerPage, _viewModel.ItemsPerPage);
            await Reload();
        }

        private async void ClearTagFilterButton_OnClicked(object sender, EventArgs e)
        {
            _viewModel.ShowOptions = false;
            _viewModel.PageNumber = 1;
            _viewModel.TagFilter = "";
            TagFilterPicker.SelectedIndex = 0;
            await Reload();
        }
    }
}
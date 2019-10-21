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
using Location = KinaUnaXamarin.Models.KinaUna.Location;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class YearAgoPage : ContentPage
    {
        private int _viewChild = Constants.DefaultChildId;
        private UserInfo _userInfo;
        private TimelineFeedViewModel _timelineModel;
        private string _accessToken;
        private bool _reload = true;
        private bool _online = true;
        public YearAgoPage()
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
                _timelineModel = new TimelineFeedViewModel();
                _userInfo = OfflineDefaultData.DefaultUserInfo;
                ContainerStackLayout.BindingContext = _timelineModel;
                BindingContext = _timelineModel;
                _timelineModel.SelectedYear = DateTime.UtcNow.Year;
                _timelineModel.SelectedMonth = DateTime.UtcNow.Month;
                _timelineModel.SelectedDay = DateTime.UtcNow.Day;
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

        private async Task Reload()
        {
            _timelineModel.IsBusy = true;
            await CheckAccount();
            await UpdateTimeLine();
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
            _timelineModel.IsBusy = false;
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

                _timelineModel.IsLoggedIn = false;
                _timelineModel.LoggedOut = true;
                _accessToken = "";
                _userInfo = OfflineDefaultData.DefaultUserInfo;

            }
            else
            {
                _timelineModel.IsLoggedIn = true;
                _timelineModel.LoggedOut = false;
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
            _timelineModel.Progeny = progeny;

            List<Progeny> progenyList = await ProgenyService.GetProgenyList(userEmail);
            _timelineModel.ProgenyCollection.Clear();
            _timelineModel.CanUserAddItems = false;
            foreach (Progeny prog in progenyList)
            {
                _timelineModel.ProgenyCollection.Add(prog);
                if (prog.Admins.ToUpper().Contains(_userInfo.UserEmail.ToUpper()))
                {
                    _timelineModel.CanUserAddItems = true;
                }
            }

            _timelineModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewChild);
        }

        private async Task UpdateTimeLine()
        {
            DateTime timeLineStart = new DateTime(_timelineModel.SelectedYear, _timelineModel.SelectedMonth, _timelineModel.SelectedDay);
            List<TimeLineItem> timeLineList = await ProgenyService.GetTimeLineYearAgo(_timelineModel.Progeny.Id,
                _timelineModel.UserAccessLevel, _userInfo.Timezone).ConfigureAwait(false);
            _timelineModel.TimeLineItems.Clear();
            if (timeLineList.Any())
            {
                foreach (TimeLineItem ti in timeLineList)
                {
                    ti.VisibleBefore = false;
                    _timelineModel.TimeLineItems.Add(ti);
                }
                
                // RemainingItemsThreshold not implemented yet. When it is available try out CollectionView instead of ListView.
                // https://github.com/xamarin/Xamarin.Forms/issues/5623

            }

            // TimeLineListView.ScrollTo(timeLineList.FirstOrDefault(), ScrollToPosition.Start, false);
        }
        

        
        private async void ProgenyToolBarItem_OnClicked(object sender, EventArgs e)
        {
            SelectProgenyPage selProPage = new SelectProgenyPage(_timelineModel.ProgenyCollection);
            await Shell.Current.Navigation.PushModalAsync(selProPage);
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

        private async void TimeLineListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is TimeLineItem timeLineItem)
            {
                if (timeLineItem.ItemObject is Picture picture)
                {
                    PhotoDetailPage photoPage = new PhotoDetailPage(picture.PictureId);
                    // Reset selection
                    TimeLineListView.SelectedItem = null;
                    await Shell.Current.Navigation.PushModalAsync(photoPage);
                }

                if (timeLineItem.ItemObject is Video video)
                {
                    VideoDetailPage videoPage = new VideoDetailPage(video.VideoId);
                    // Reset selection
                    TimeLineListView.SelectedItem = null;
                    await Shell.Current.Navigation.PushModalAsync(videoPage);
                }

                if (timeLineItem.ItemObject is Sleep sleep)
                {
                    SleepDetailPage sleepDetailPage = new SleepDetailPage(sleep);
                    TimeLineListView.SelectedItem = null;
                    await Shell.Current.Navigation.PushModalAsync(sleepDetailPage);
                }

                if (timeLineItem.ItemObject is CalendarItem calendarItem)
                {
                    EventDetailPage eventDetailPage = new EventDetailPage(calendarItem);
                    TimeLineListView.SelectedItem = null;
                    await Shell.Current.Navigation.PushModalAsync(eventDetailPage);
                }

                if (timeLineItem.ItemObject is Location locationItem)
                {
                    LocationDetailPage locationDetailPage = new LocationDetailPage(locationItem);
                    TimeLineListView.SelectedItem = null;
                    await Shell.Current.Navigation.PushModalAsync(locationDetailPage);
                }

                if (timeLineItem.ItemObject is Vaccination vaccinationItem)
                {
                    VaccinationDetailPage vaccinationDetailPage = new VaccinationDetailPage(vaccinationItem);
                    TimeLineListView.SelectedItem = null;
                    await Shell.Current.Navigation.PushModalAsync(vaccinationDetailPage);
                }
            }
        }
    }
}
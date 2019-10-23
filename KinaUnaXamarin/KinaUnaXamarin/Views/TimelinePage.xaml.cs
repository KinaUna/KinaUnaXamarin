using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using MvvmHelpers;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Location = KinaUnaXamarin.Models.KinaUna.Location;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TimelinePage : ContentPage
    {
        private int _viewChild = Constants.DefaultChildId;
        private UserInfo _userInfo;
        private TimelineFeedViewModel _timelineModel;
        private string _accessToken;
        private bool _reload = true;
        private bool _online = true;
        private string lastItemDateString;
        private int _dateHeaderCount = 0;

        public TimelinePage()
        {
            InitializeComponent();

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            TimeLineListView.ItemAppearing += ItemAppearingEvent;
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
            //Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            //TimeLineListView.ItemAppearing += ItemAppearingEvent;
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
            base.OnAppearing();
            
        }

        protected override void OnDisappearing()
        {
            //Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
            //TimeLineListView.ItemAppearing -= ItemAppearingEvent;
            base.OnDisappearing();
            
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
            List<TimeLineItem> timeLineList = await ProgenyService.GetTimeLine(_timelineModel.Progeny.Id,
                _timelineModel.UserAccessLevel, 6, 0, _userInfo.Timezone, timeLineStart, "").ConfigureAwait(false);
            _timelineModel.TimeLineItems.Clear();
            _dateHeaderCount = 0;
            if (timeLineList.Any())
            {
                foreach (TimeLineItem ti in timeLineList)
                {
                    ti.VisibleBefore = false;
                    if (ti.ItemType == 9999)
                    {
                        _dateHeaderCount++;
                    }
                    _timelineModel.TimeLineItems.Add(ti);
                }

                lastItemDateString = timeLineList.Last().ProgenyTime.ToLongDateString();
                // _timelineModel.TimeLineItems.ReplaceRange(timeLineList);

                // RemainingItemsThreshold not implemented yet. When it is available try out CollectionView instead of ListView.
                // https://github.com/xamarin/Xamarin.Forms/issues/5623
                
            }

            _timelineModel.MaxDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            TimelineStartDatePicker.MaximumDate = _timelineModel.MaxDate;
            // TimeLineListView.ScrollTo(timeLineList.FirstOrDefault(), ScrollToPosition.Start, false);
        }

        private async void ItemAppearingEvent(object sender, ItemVisibilityEventArgs e)
        {
            TimeLineItem tItem = e.Item as TimeLineItem;
            if (tItem == null)
            {
                return;
            }
            
            int progenyId = tItem.ProgenyId;
            if (!_timelineModel.CanLoadMore || _timelineModel.TimeLineItems.Count == 0 || tItem.VisibleBefore || progenyId != _timelineModel.Progeny.Id)
                return;

            //hit bottom!
            if (_timelineModel.TimeLineItems.Count > 5)
            {
                if (tItem.ItemId == (_timelineModel.TimeLineItems[_timelineModel.TimeLineItems.Count - 5]).ItemId && !tItem.VisibleBefore)
                {
                    await LoadItems(_timelineModel.TimeLineItems.Count - _dateHeaderCount, _timelineModel.Progeny.Id, _accessToken, _userInfo.Timezone);
                }
                tItem.VisibleBefore = true;
            }
        }

        private async Task LoadItems(int startItem, int progenyId, string accessToken, string userTimeZone)
        {
            _timelineModel.CanLoadMore = false;
            _timelineModel.IsBusy = true;
            DateTime timeLineStart = new DateTime(_timelineModel.SelectedYear, _timelineModel.SelectedMonth,
                _timelineModel.SelectedDay);
            List<TimeLineItem> timeLineList = await ProgenyService.GetTimeLine(progenyId,
                _timelineModel.UserAccessLevel, 10, startItem, userTimeZone, timeLineStart, lastItemDateString).ConfigureAwait(false);
            if (timeLineList.Any())
            {
                foreach (TimeLineItem ti in timeLineList)
                {
                    ti.VisibleBefore = false;
                    if (ti.ItemType == 9999)
                    {
                        _dateHeaderCount++;
                    }
                    _timelineModel.TimeLineItems.Add(ti);
                }
                lastItemDateString = timeLineList.Last().ProgenyTime.ToLongDateString();
                // _timelineModel.TimeLineItems.AddRange(timeLineList);
            }
            
            // Run GetTimeLine a second time to add more items.
            timeLineList = await ProgenyService.GetTimeLine(progenyId,
                _timelineModel.UserAccessLevel, 15, startItem + 10, userTimeZone, timeLineStart, lastItemDateString).ConfigureAwait(false);
            if (timeLineList.Any())
            {
                foreach (TimeLineItem ti in timeLineList)
                {
                    ti.VisibleBefore = false;
                    if (ti.ItemType == 9999)
                    {
                        _dateHeaderCount++;
                    }
                    _timelineModel.TimeLineItems.Add(ti);
                }
                lastItemDateString = timeLineList.Last().ProgenyTime.ToLongDateString();
                // _timelineModel.TimeLineItems.AddRange(timeLineList);
            }
            
            _timelineModel.CanLoadMore = true;
            _timelineModel.IsBusy = false;
        }

        private async void ProgenyToolBarItem_OnClicked(object sender, EventArgs e)
        {
            SelectProgenyPage selProPage = new SelectProgenyPage(_timelineModel.ProgenyCollection);
            await Shell.Current.Navigation.PushModalAsync(selProPage);
        }
        
        private void OptionsToolBarItem_OnClicked(object sender, EventArgs e)
        {
            _timelineModel.ShowOptions = !_timelineModel.ShowOptions;
        }

        private async void TimelineStartDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            _timelineModel.SelectedYear = TimelineStartDatePicker.Date.Year;
            _timelineModel.SelectedMonth = TimelineStartDatePicker.Date.Month;
            _timelineModel.SelectedDay = TimelineStartDatePicker.Date.Day;
            _timelineModel.ShowOptions = false;
            
            await Reload();
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
            _timelineModel.SelectedYear = DateTime.Now.Year;
            _timelineModel.SelectedMonth = DateTime.Now.Month;
            _timelineModel.SelectedDay = DateTime.Now.Day;
            TimelineStartDatePicker.Date = DateTime.Now;
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

                if (timeLineItem.ItemObject is VocabularyItem vocabularyItem)
                {
                    VocabularyDetailPage vocabularyDetailPage = new VocabularyDetailPage(vocabularyItem);
                    TimeLineListView.SelectedItem = null;
                    await Shell.Current.Navigation.PushModalAsync(vocabularyDetailPage);
                }

                if (timeLineItem.ItemObject is Skill skillItem)
                {
                    SkillDetailPage skillDetailPage = new SkillDetailPage(skillItem);
                    TimeLineListView.SelectedItem = null;
                    await Shell.Current.Navigation.PushModalAsync(skillDetailPage);
                }

                if (timeLineItem.ItemObject is Measurement measurementItem)
                {
                    MeasurementDetailPage measurementDetailPage = new MeasurementDetailPage(measurementItem);
                    TimeLineListView.SelectedItem = null;
                    await Shell.Current.Navigation.PushModalAsync(measurementDetailPage);
                }

                if (timeLineItem.ItemObject is Friend friendItem)
                {
                    FriendDetailPage friendDetailPage = new FriendDetailPage(friendItem);
                    TimeLineListView.SelectedItem = null;
                    await Shell.Current.Navigation.PushModalAsync(friendDetailPage);
                }
            }
        }
    }
}
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
        private readonly TimelineFeedViewModel _timelineModel;
        private string _accessToken;
        private bool _reload = true;
        private bool _online = true;
        private string _lastItemDateString;
        private int _dateHeaderCount = 0;
        private List<TimeLineItem> _timeLineList;

        public TimelinePage()
        {
            InitializeComponent();
            _timeLineList = new List<TimeLineItem>();
            _timelineModel = new TimelineFeedViewModel();
            _userInfo = OfflineDefaultData.DefaultUserInfo;
            BindingContext = _timelineModel;
            ContainerStackLayout.BindingContext = _timelineModel;
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
            base.OnAppearing();
            if (_reload)
            {
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
            _timeLineList = new List<TimeLineItem>();
            // Device.BeginInvokeOnMainThread(() => { _timelineModel.TimeLineItems.Clear(); });
            while (_timelineModel.TimeLineItems.Count > 0)
            {
                _timelineModel.TimeLineItems.RemoveAt(0);
            }

            _dateHeaderCount = 0;
            _lastItemDateString = "";


            _timelineModel.MaxDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            if (Device.RuntimePlatform == Device.UWP || Device.RuntimePlatform == Device.iOS)
            {
                Device.BeginInvokeOnMainThread(() => { TimelineStartDatePicker.MaximumDate = _timelineModel.MaxDate; });
            }
            else
            {
                TimelineStartDatePicker.MaximumDate = _timelineModel.MaxDate;
            }

            await LoadItems(0, _timelineModel.Progeny.Id, _accessToken, _userInfo.Timezone);
            if (Device.RuntimePlatform == Device.iOS)
            {
                Device.BeginInvokeOnMainThread( async() => { await ShowNextItem(); });
            }
            else
            {
                await ShowNextItem();
            }
        }

        private async void ItemAppearingEvent(object sender, ItemVisibilityEventArgs e)
        {
            TimeLineItem tItem = e.Item as TimeLineItem;
            if (tItem == null)
            {
                return;
            }
            
            int progenyId = tItem.ProgenyId;
            if (progenyId != _timelineModel.Progeny.Id)
                return;
            tItem.VisibleBefore = true;
            int itemsNotVisibleBeforeCount = _timelineModel.TimeLineItems.Where(t => t.VisibleBefore == false).Count();
            if (_timelineModel.CanLoadMore && itemsNotVisibleBeforeCount < 20)
            {
                await LoadItems(_timeLineList.Count - _dateHeaderCount, _timelineModel.Progeny.Id, _accessToken, _userInfo.Timezone);
            }

            if (_timelineModel.CanShowMore)
            {
                await ShowNextItem();
            }
        }

        private async Task ShowNextItem()
        {
            if (_timelineModel.CanShowMore && _timeLineList.Count - _timelineModel.TimeLineItems.Count < 25)
            {
                _timelineModel.CanShowMore = false;
                int start = _timelineModel.TimeLineItems.Count;
                if (start < 1)
                {
                    start = 1;
                }

                List<TimeLineItem> itemsToAdd = new List<TimeLineItem>();
                
                if (_timeLineList.Count > start)
                {
                    int itemsToAddCount = 5;
                    while (itemsToAddCount > 0)
                    {
                        TimeLineItem newItem = _timeLineList.FirstOrDefault(t => t.AddedToListView == false);
                        if (newItem != null)
                        {
                            if (!_timelineModel.TimeLineItems.Contains(newItem))
                            {
                                newItem.AddedToListView = true;
                                itemsToAdd.Add(newItem);
                            }
                        }

                        itemsToAddCount--;
                    }

                    if (itemsToAdd.Any())
                    {
                        if (Device.RuntimePlatform == Device.iOS)
                        {
                            
                            Device.BeginInvokeOnMainThread(() => { _timelineModel.TimeLineItems.AddRange(itemsToAdd); });
                            await Task.Delay(500);
                        }
                        else
                        {
                            _timelineModel.TimeLineItems.AddRange(itemsToAdd);
                        }
                    }
                }

                
                _timelineModel.CanShowMore = true;
            }
        }

        private async Task LoadItems(int startItem, int progenyId, string accessToken, string userTimeZone)
        {
            _timelineModel.CanLoadMore = false;
            _timelineModel.IsBusy = true;
            DateTime timeLineStart = new DateTime(_timelineModel.SelectedYear, _timelineModel.SelectedMonth,
                _timelineModel.SelectedDay);
            List<TimeLineItem> timeLineList = await ProgenyService.GetTimeLine(progenyId,
                _timelineModel.UserAccessLevel, 5, startItem, userTimeZone, timeLineStart, _lastItemDateString).ConfigureAwait(false);
            if (timeLineList.Any())
            {
                foreach (TimeLineItem ti in timeLineList)
                {
                    ti.VisibleBefore = false;
                    if (ti.ItemType == 9999)
                    {
                        _dateHeaderCount++;
                    }
                    _timeLineList.Add(ti);

                }
                _lastItemDateString = timeLineList.Last().ProgenyTime.ToLongDateString();
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

                if (timeLineItem.ItemObject is Contact contactItem)
                {
                    ContactDetailPage contactDetailPage = new ContactDetailPage(contactItem);
                    TimeLineListView.SelectedItem = null;
                    await Shell.Current.Navigation.PushModalAsync(contactDetailPage);
                }

                if (timeLineItem.ItemObject is Note noteItem)
                {
                    NoteDetailPage noteDetailPage = new NoteDetailPage(noteItem);
                    TimeLineListView.SelectedItem = null;
                    await Shell.Current.Navigation.PushModalAsync(noteDetailPage);
                }
            }
        }
    }
}
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
using Location = KinaUnaXamarin.Models.KinaUna.Location;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TimelinePage : ContentPage
    {
        private readonly TimelineFeedViewModel _viewModel;
        private bool _reload = true;
        private string _lastItemDateString;
        private int _dateHeaderCount = 0;
        private List<TimeLineItem> _timeLineList;

        public TimelinePage()
        {
            InitializeComponent();
            _timeLineList = new List<TimeLineItem>();
            _viewModel = new TimelineFeedViewModel();
            _viewModel.UserInfo = OfflineDefaultData.DefaultUserInfo;
            BindingContext = _viewModel;
            ContainerStackLayout.BindingContext = _viewModel;
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            TimeLineListView.ItemAppearing += ItemAppearingEvent;
            MessagingCenter.Subscribe<SelectProgenyPage>(this, "Reload", async (sender) =>
            {
                await SetUserAndProgeny();
                await Reload();
            });

            MessagingCenter.Subscribe<AccountViewModel>(this, "Reload", async (sender) =>
            {
                await SetUserAndProgeny();
                await Reload();
            });
        }
        
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_reload)
            {
                await SetUserAndProgeny();
                _viewModel.SelectedYear = DateTime.UtcNow.Year;
                _viewModel.SelectedMonth = DateTime.UtcNow.Month;
                _viewModel.SelectedDay = DateTime.UtcNow.Day;
                await Reload();
            }

            TimeLineListView.SelectedItem = null;

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
            //Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
            //TimeLineListView.ItemAppearing -= ItemAppearingEvent;
            base.OnDisappearing();
            
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
            await UpdateTimeLine();

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
    
        private async Task UpdateTimeLine()
        {
            _timeLineList = new List<TimeLineItem>();
            _viewModel.TimeLineItems.Clear();
            _dateHeaderCount = 0;
            _lastItemDateString = "";
            
            _viewModel.MaxDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            if (Device.RuntimePlatform == Device.UWP || Device.RuntimePlatform == Device.iOS)
            {
                Device.BeginInvokeOnMainThread(() => { TimelineStartDatePicker.MaximumDate = _viewModel.MaxDate; });
            }
            else
            {
                TimelineStartDatePicker.MaximumDate = _viewModel.MaxDate;
            }

            await LoadItems(0, _viewModel.Progeny.Id, _viewModel.AccessToken, _viewModel.UserInfo.Timezone);
            await ShowNextItem();
        }

        private async void ItemAppearingEvent(object sender, ItemVisibilityEventArgs e)
        {
            TimeLineItem tItem = e.Item as TimeLineItem;
            if (tItem == null)
            {
                return;
            }
            
            int progenyId = tItem.ProgenyId;
            if (progenyId != _viewModel.Progeny.Id)
                return;
            tItem.VisibleBefore = true;
            int itemsNotVisibleBeforeCount = _viewModel.TimeLineItems.Count(t => t.VisibleBefore == false);
            if (_viewModel.CanLoadMore && itemsNotVisibleBeforeCount < 30)
            {
                await LoadItems(_timeLineList.Count - _dateHeaderCount, _viewModel.Progeny.Id, _viewModel.AccessToken, _viewModel.UserInfo.Timezone);
            }

            if (_viewModel.CanShowMore)
            {
                await ShowNextItem();
            }
        }

        private async Task ShowNextItem()
        {
            if (_viewModel.CanShowMore && _timeLineList.Count - _viewModel.TimeLineItems.Count < 30)
            {
                _viewModel.CanShowMore = false;
                int start = _viewModel.TimeLineItems.Count;
                if (start < 1)
                {
                    start = 1;
                }

                List<TimeLineItem> itemsToAdd = new List<TimeLineItem>();
                
                if (_timeLineList.Count > start)
                {
                    int itemsToAddCount = 10;
                    while (itemsToAddCount > 0)
                    {
                        TimeLineItem newItem = _timeLineList.FirstOrDefault(t => t.AddedToListView == false);
                        if (newItem != null)
                        {
                            if (!_viewModel.TimeLineItems.Contains(newItem))
                            {
                                newItem.AddedToListView = true;
                                itemsToAdd.Add(newItem);
                            }
                        }

                        itemsToAddCount--;
                    }

                    if (itemsToAdd.Any())
                    {
                        _viewModel.TimeLineItems.AddRange(itemsToAdd);
                    }
                }
                
                _viewModel.CanShowMore = true;
            }
        }

        private async Task LoadItems(int startItem, int progenyId, string accessToken, string userTimeZone)
        {
            _viewModel.CanLoadMore = false;
            _viewModel.IsBusy = true;
            DateTime timeLineStart = new DateTime(_viewModel.SelectedYear, _viewModel.SelectedMonth,
                _viewModel.SelectedDay);
            List<TimeLineItem> timeLineList = await ProgenyService.GetTimeLine(progenyId,
                _viewModel.UserAccessLevel, 5, startItem, userTimeZone, timeLineStart, _lastItemDateString).ConfigureAwait(false);
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
            
            _viewModel.CanLoadMore = true;
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

        private async void TimelineStartDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            _viewModel.SelectedYear = TimelineStartDatePicker.Date.Year;
            _viewModel.SelectedMonth = TimelineStartDatePicker.Date.Month;
            _viewModel.SelectedDay = TimelineStartDatePicker.Date.Day;
            _viewModel.ShowOptions = false;
            
            await Reload();
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
            _viewModel.SelectedYear = DateTime.Now.Year;
            _viewModel.SelectedMonth = DateTime.Now.Month;
            _viewModel.SelectedDay = DateTime.Now.Day;
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
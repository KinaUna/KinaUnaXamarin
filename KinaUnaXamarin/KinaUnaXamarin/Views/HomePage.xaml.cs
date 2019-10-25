using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using MvvmHelpers;
using Newtonsoft.Json;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Location = KinaUnaXamarin.Models.KinaUna.Location;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        private int _viewChild;
        private HomeFeedViewModel _feedModel;
        private UserInfo _userInfo;
        private string _accessToken;
        private double _screenWidth;
        private double _screenHeight;
        private bool _reload = true;
        private bool _online = true;
        
        public HomePage()
        {
            InitializeComponent();

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

            MessagingCenter.Subscribe<HomeFeedViewModel>(this, "Reload", async (sender) =>
            {
                await Reload();
            });

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
                _feedModel = new HomeFeedViewModel();

                _userInfo = OfflineDefaultData.DefaultUserInfo;

                string userEmail = await UserService.GetUserEmail();
                string userviewchild = await SecureStorage.GetAsync(Constants.UserViewChildKey);
                bool viewchildParsed = int.TryParse(userviewchild, out _viewChild);
                if (viewchildParsed)
                {
                    try
                    {
                        _feedModel.Progeny = JsonConvert.DeserializeObject<Progeny>(await SecureStorage.GetAsync("ProgenyObject" + userviewchild));
                    }
                    catch (Exception)
                    {
                        _feedModel.Progeny = await ProgenyService.GetProgeny(_viewChild);
                    }
                    
                    _userInfo = JsonConvert.DeserializeObject<UserInfo>(await SecureStorage.GetAsync("UserInfo" + userEmail));
                }
                BindingContext = _feedModel;
            }

            var networkAccess = Connectivity.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess)
            {
                _online = true;
                OfflineStackLayout.IsVisible = false;
            }
            else
            {
                _online = false;
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
            // Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }
        
        private async Task Reload()
        {
            _feedModel.IsBusy = true;
            UpcomingEventsStatckLayout.IsVisible = false;
            EventFrame0.IsVisible = false;
            EventFrame1.IsVisible = false;
            EventFrame2.IsVisible = false;
            EventFrame3.IsVisible = false;
            EventFrame4.IsVisible = false;
            AgeInfoStackLayout.IsVisible = false;
            RandomPictureStackLayout.IsVisible = false;
            LatestPostsStackLayout.IsVisible = false;

            await CheckAccount();
            await UpdateProgenyData();
            await UpdateEvents();
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
            _feedModel.IsBusy = false;
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

                if (!accessTokenCurrent && _online)
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

                _feedModel.IsLoggedIn = false;
                _feedModel.LoggedOut = true;
                _accessToken = "";
                _userInfo = OfflineDefaultData.DefaultUserInfo;

            }
            else
            {
                _feedModel.IsLoggedIn = true;
                _feedModel.LoggedOut = false;
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
            _feedModel.Progeny = progeny;

            List<Progeny> progenyList = await ProgenyService.GetProgenyList(userEmail);
            _feedModel.ProgenyCollection.Clear();
            _feedModel.CanUserAddItems = false;
            foreach (Progeny prog in progenyList)
            {
                _feedModel.ProgenyCollection.Add(prog);
                if (prog.Admins.ToUpper().Contains(_userInfo.UserEmail.ToUpper()))
                {
                    _feedModel.CanUserAddItems = true;
                }
            }

            _feedModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewChild);
            
        }
        

        private async Task UpdateProgenyData()
        {
            AgeInfoStackLayout.IsVisible = false;
            RandomPictureStackLayout.IsVisible = false;
            _feedModel.CurrentTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            BirthTime progBirthTime;
            if (!String.IsNullOrEmpty(_feedModel.Progeny.NickName) && _feedModel.Progeny.BirthDay.HasValue && _feedModel.UserAccessLevel < 5)
            {
                progBirthTime = new BirthTime(_feedModel.Progeny.BirthDay.Value,
                    TimeZoneInfo.FindSystemTimeZoneById(_feedModel.Progeny.TimeZone));
            }
            else
            {
                progBirthTime = new BirthTime(new DateTime(2018, 02, 18, 18, 02, 00), TimeZoneInfo.FindSystemTimeZoneById(_feedModel.Progeny.TimeZone));
            }

            _feedModel.CurrentTime = progBirthTime.CurrentTime;
            _feedModel.Years = progBirthTime.CalcYears();
            _feedModel.Months = progBirthTime.CalcMonths();
            _feedModel.Weeks = progBirthTime.CalcWeeks();
            _feedModel.Days = progBirthTime.CalcDays();
            _feedModel.Hours = progBirthTime.CalcHours();
            _feedModel.Minutes = progBirthTime.CalcMinutes();
            _feedModel.NextBirthday = progBirthTime.CalcNextBirthday();
            _feedModel.MinutesMileStone = progBirthTime.CalcMileStoneMinutes();
            _feedModel.HoursMileStone = progBirthTime.CalcMileStoneHours();
            _feedModel.DaysMileStone = progBirthTime.CalcMileStoneDays();
            _feedModel.WeeksMileStone = progBirthTime.CalcMileStoneWeeks();

            Picture tempPicture = OfflineDefaultData.DefaultPicture;

            Picture displayPicture = tempPicture;

            if (_feedModel.UserAccessLevel < 5 && _online)
            {
                displayPicture = await ProgenyService.GetRandomPicture(_feedModel.Progeny.Id, _feedModel.UserAccessLevel, _userInfo.Timezone);
            }
            _feedModel.ImageLink600 = displayPicture.PictureLink600;
            _feedModel.ImageLink = displayPicture.PictureLink1200;
            _feedModel.ImageId = displayPicture.PictureId;
            PictureTime picTime = new PictureTime(new DateTime(2018, 02, 18, 20, 18, 00), new DateTime(2018, 02, 18, 20, 18, 00), TimeZoneInfo.FindSystemTimeZoneById(_feedModel.Progeny.TimeZone));
            if (displayPicture.PictureTime != null && _feedModel.Progeny.BirthDay.HasValue)
            {
                DateTime picTimeBirthday = new DateTime(_feedModel.Progeny.BirthDay.Value.Ticks, DateTimeKind.Unspecified);

                picTime = new PictureTime(picTimeBirthday, displayPicture.PictureTime, TimeZoneInfo.FindSystemTimeZoneById(_feedModel.Progeny.TimeZone));
                _feedModel.PicTimeValid = true;
            }

            _feedModel.Tags = displayPicture.Tags;
            _feedModel.Location = displayPicture.Location;
            _feedModel.PicTime = picTime.PictureDateTime;
            _feedModel.PicYears = picTime.CalcYears();
            _feedModel.PicMonths = picTime.CalcMonths();
            _feedModel.PicWeeks = picTime.CalcWeeks();
            _feedModel.PicDays = picTime.CalcDays();
            _feedModel.PicHours = picTime.CalcHours();
            _feedModel.PicMinutes = picTime.CalcMinutes();
            AgeInfoStackLayout.IsVisible = true;
            RandomPictureStackLayout.IsVisible = true;
        }

        private async Task UpdateEvents()
        {
            UpcomingEventsStatckLayout.IsVisible = false;
            EventFrame0.IsVisible = false;
            EventFrame1.IsVisible = false;
            EventFrame2.IsVisible = false;
            EventFrame3.IsVisible = false;
            EventFrame4.IsVisible = false;
            _feedModel.EventsList = new List<CalendarItem>();
            List<CalendarItem> eventsList = await ProgenyService.GetUpcommingEventsList(_feedModel.Progeny.Id, _feedModel.UserAccessLevel);
            int eventListCurrent = 0;
            
            if (eventsList.Any())
            {
                
                UpcomingEventsStatckLayout.IsVisible = true;
                foreach (CalendarItem ev in eventsList)
                {
                    ev.StartTime = TimeZoneInfo.ConvertTimeFromUtc(ev.StartTime.Value, TimeZoneInfo.FindSystemTimeZoneById(_userInfo.Timezone));
                    ev.EndTime = TimeZoneInfo.ConvertTimeFromUtc(ev.EndTime.Value, TimeZoneInfo.FindSystemTimeZoneById(_userInfo.Timezone));
                    ev.StartString = ev.StartTime.Value.ToString("dd-MMM-yyyy HH:mm") + " - " + ev.EndTime.Value.ToString("dd-MMM-yyyy HH:mm");

                    if (eventListCurrent == 0)
                    {
                        EventFrame0.IsVisible = true;
                        EventStart0.Text = ev.StartString;
                        EventTitle0.Text = ev.Title;
                    }

                    if (eventListCurrent == 1)
                    {
                        EventFrame1.IsVisible = true;
                        EventStart1.Text = ev.StartString;
                        EventTitle1.Text = ev.Title;
                    }

                    if (eventListCurrent == 2)
                    {
                        EventFrame2.IsVisible = true;
                        EventStart2.Text = ev.StartString;
                        EventTitle2.Text = ev.Title;
                    }

                    if (eventListCurrent == 3)
                    {
                        EventFrame3.IsVisible = true;
                        EventStart3.Text = ev.StartString;
                        EventTitle3.Text = ev.Title;
                    }

                    if (eventListCurrent == 4)
                    {
                        EventFrame4.IsVisible = true;
                        EventStart4.Text = ev.StartString;
                        EventTitle4.Text = ev.Title;
                    }
                    
                    eventListCurrent++;
                }
            }
        }

        private async Task UpdateTimeLine()
        {
            LatestPostsStackLayout.IsVisible = false;
            _feedModel.LatestPosts = new List<TimeLineItem>();
            _feedModel.TimeLineItems.Clear();
            _feedModel.LatestPosts = await ProgenyService.GetLatestPosts(_feedModel.Progeny.Id, _feedModel.UserAccessLevel, _userInfo.Timezone);
            

            if (_feedModel.LatestPosts.Any())
            {
                LatestPostsStackLayout.IsVisible = true;
                _feedModel.TimeLineItems.AddRange(_feedModel.LatestPosts);
            }
        }

        
        private async void ReloadToolbarButton_OnClicked(object sender, EventArgs e)
        {
            await Reload();
        }

        private async void ProgenyToolBarItem_OnClicked(object sender, EventArgs e)
        {
            SelectProgenyPage selProPage = new SelectProgenyPage(_feedModel.ProgenyCollection);
            await Shell.Current.Navigation.PushModalAsync(selProPage);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called
            if (_screenWidth != width || _screenHeight != height)
            {
                _screenWidth = width;
                _screenHeight = height;
                
                if (Device.Idiom == TargetIdiom.Desktop)
                {
                    if (width > 1100)
                    {
                        ContainerStackLayout.Orientation = StackOrientation.Horizontal;
                    }
                    else
                    {
                        ContainerStackLayout.Orientation = StackOrientation.Vertical;
                    }
                }
                else
                {
                    if (width > height && (width - ProgenyDetailsStackLayout.WidthRequest) > 200) //if (width > height && width > 800)
                    {
                        ProgenyDetailsStackLayout.WidthRequest = (int)(width * 6 / 11);
                        if (_feedModel != null)
                        {
                            _feedModel.ImageLinkWidth = (int)ProgenyDetailsStackLayout.WidthRequest - 20;
                        }
                        ProgenyDetailsStackLayout.Margin = new Thickness(5, 0, 2, 5);
                        UpcomingEventsStatckLayout.Margin = new Thickness(3, 0, 5, 5);
                        LatestPostsStackLayout.Margin = new Thickness(3, 0, 5, 5);
                        ContainerStackLayout.Orientation = StackOrientation.Horizontal;
                    }
                    else
                    {
                        ProgenyDetailsStackLayout.WidthRequest = width;
                        if (_feedModel != null)
                        {
                            _feedModel.ImageLinkWidth = (int)ProgenyDetailsStackLayout.WidthRequest - 20;
                        }
                        
                        ProgenyDetailsStackLayout.Margin = new Thickness(5, 0, 5, 5);
                        UpcomingEventsStatckLayout.Margin = new Thickness(5, 0, 5, 5);
                        LatestPostsStackLayout.Margin = new Thickness(5, 0, 5, 5);
                        ContainerStackLayout.Orientation = StackOrientation.Vertical;
                    }
                }

            }
        }

        private async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var networkAccess = e.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if ( internetAccess != _online)
            {
                _online = internetAccess;
                await Reload();
            }
        }

        private async void AddItemToolbarButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PushModalAsync(new AddItemPage());
        }

        private async void RandomPhotoClickGestureRecognizer_OnClicked(object sender, EventArgs e)
        {
            PhotoDetailPage photoPage = new PhotoDetailPage(_feedModel.ImageId);
            await Shell.Current.Navigation.PushModalAsync(photoPage);
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
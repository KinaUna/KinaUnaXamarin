using System;
using System.Collections.Generic;
using System.Globalization;
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
using Application = Xamarin.Forms.Application;
using Location = KinaUnaXamarin.Models.KinaUna.Location;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        private readonly HomeFeedViewModel _feedModel;
        private double _screenWidth;
        private double _screenHeight;
        private bool _reload = true;
        
        public HomePage()
        {
            InitializeComponent();
            _feedModel = new HomeFeedViewModel();
            BindingContext = _feedModel;
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

            MessagingCenter.Subscribe<HomeFeedViewModel>(this, "Reload", async (sender) =>
            {
                await SetUserAndProgeny();
                await Reload();
            });

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

            }

            var networkAccess = Connectivity.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess)
            {
                _feedModel.Online = true;
            }
            else
            {
                _feedModel.Online = false;
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

        private async Task SetUserAndProgeny()
        {
            _feedModel.UserInfo = OfflineDefaultData.DefaultUserInfo;

            string userEmail = await UserService.GetUserEmail();
            string userviewchild = await SecureStorage.GetAsync(Constants.UserViewChildKey);
            bool viewchildParsed = int.TryParse(userviewchild, out int viewChildId);

            if (viewchildParsed)
            {
                _feedModel.ViewChild = viewChildId;
                try
                {
                    _feedModel.Progeny = await App.Database.GetProgenyAsync(_feedModel.ViewChild);
                }
                catch (Exception)
                {
                    _feedModel.Progeny = await ProgenyService.GetProgeny(_feedModel.ViewChild);
                }

                _feedModel.UserInfo = await App.Database.GetUserInfoAsync(userEmail);
            }
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
                _feedModel.Online = true;
            }
            else
            {
                _feedModel.Online = false;
            }
            _feedModel.IsBusy = false;
        }

        private async Task CheckAccount()
        {
            string userEmail = await UserService.GetUserEmail();
            _feedModel.AccessToken = await UserService.GetAuthAccessToken();
            bool accessTokenCurrent = false;
            if (_feedModel.AccessToken != "")
            {
                accessTokenCurrent = await UserService.IsAccessTokenCurrent();

                if (!accessTokenCurrent && _feedModel.Online)
                {
                    bool loginSuccess = await UserService.LoginIdsAsync();
                    if (loginSuccess)
                    {
                        _feedModel.AccessToken = await UserService.GetAuthAccessToken();
                        accessTokenCurrent = true;
                    }

                    await Reload();
                }
            }

            if (String.IsNullOrEmpty(_feedModel.AccessToken) || !accessTokenCurrent)
            {

                _feedModel.IsLoggedIn = false;
                _feedModel.LoggedOut = true;
                _feedModel.AccessToken = "";
                _feedModel.UserInfo = OfflineDefaultData.DefaultUserInfo;

            }
            else
            {
                _feedModel.IsLoggedIn = true;
                _feedModel.LoggedOut = false;
                _feedModel.UserInfo = await UserService.GetUserInfo(userEmail);
            }

            string userviewchild = await SecureStorage.GetAsync(Constants.UserViewChildKey);
            bool viewchildParsed = int.TryParse(userviewchild, out int viewChildId);
            if (!viewchildParsed)
            {
                _feedModel.ViewChild = _feedModel.UserInfo.ViewChild;
            }
            if (_feedModel.ViewChild == 0)
            {
                if (_feedModel.UserInfo.ViewChild != 0)
                {
                    _feedModel.ViewChild = _feedModel.UserInfo.ViewChild;
                }
                else
                {
                    _feedModel.ViewChild = Constants.DefaultChildId;
                }
            }


            if (String.IsNullOrEmpty(_feedModel.UserInfo.Timezone))
            {
                _feedModel.UserInfo.Timezone = Constants.DefaultTimeZone;
            }
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(_feedModel.UserInfo.Timezone);
            }
            catch (Exception)
            {
                _feedModel.UserInfo.Timezone = TZConvert.WindowsToIana(_feedModel.UserInfo.Timezone);
            }
            
            Progeny progeny = await ProgenyService.GetProgeny(_feedModel.ViewChild);
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
            if (progenyList != null && progenyList.Any())
            {
                foreach (Progeny prog in progenyList)
                {
                    _feedModel.ProgenyCollection.Add(prog);
                    if (prog.Admins.ToUpper().Contains(_feedModel.UserInfo.UserEmail.ToUpper()))
                    {
                        _feedModel.CanUserAddItems = true;
                    }
                }
            }

            _feedModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_feedModel.ViewChild);
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

            if (_feedModel.UserAccessLevel < 5 && _feedModel.Online)
            {
                displayPicture = await ProgenyService.GetRandomPicture(_feedModel.Progeny.Id, _feedModel.UserAccessLevel, _feedModel.UserInfo.Timezone);
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
            List<CalendarItem> eventsList = await ProgenyService.GetUpcomingEventsList(_feedModel.Progeny.Id, _feedModel.UserAccessLevel);
            int eventListCurrent = 0;
            
            if (eventsList.Any())
            {
                
                UpcomingEventsStatckLayout.IsVisible = true;
                foreach (CalendarItem ev in eventsList)
                {
                    ev.StartTime = TimeZoneInfo.ConvertTimeFromUtc(ev.StartTime.Value, TimeZoneInfo.FindSystemTimeZoneById(_feedModel.UserInfo.Timezone));
                    ev.EndTime = TimeZoneInfo.ConvertTimeFromUtc(ev.EndTime.Value, TimeZoneInfo.FindSystemTimeZoneById(_feedModel.UserInfo.Timezone));
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
            _feedModel.TimeLineItems.Clear();
            _feedModel.LatestPosts = await ProgenyService.GetLatestPosts(_feedModel.Progeny.Id, _feedModel.UserAccessLevel, _feedModel.UserInfo.Timezone);
            
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


            if (Device.RuntimePlatform == Device.UWP)
            {
                if (_screenWidth != Application.Current.MainPage.Width || _screenHeight != Application.Current.MainPage.Height)
                {
                    _screenWidth = Application.Current.MainPage.Width;
                    _screenHeight = Application.Current.MainPage.Height;

                    if (_screenWidth > 800) //if (width > height && width > 800)
                    {
                        ProgenyDetailsStackLayout.WidthRequest = _screenWidth * 0.4;
                        ProgenyTimeInfoStackLayout.WidthRequest = _screenWidth * 0.4;

                        if (_feedModel != null)
                        {
                            _feedModel.ImageLinkWidth = (int)(_screenWidth * 0.4 - 20);
                        }
                        ProgenyDetailsStackLayout.Margin = new Thickness(5, 0, 2, 5);
                        UpcomingEventsStatckLayout.Margin = new Thickness(3, 0, 5, 5);
                        LatestPostsStackLayout.Margin = new Thickness(3, 0, 5, 5);
                        ContainerStackLayout.Margin = new Thickness(0, 0, 0, 0);
                        ContainerStackLayout.Orientation = StackOrientation.Horizontal;
                    }
                    else
                    {
                        ProgenyDetailsStackLayout.WidthRequest = _screenWidth * 0.9;
                        ProgenyTimeInfoStackLayout.WidthRequest = _screenWidth * 0.9;
                        if (_feedModel != null)
                        {
                            _feedModel.ImageLinkWidth = (int)(_screenWidth - 20);
                        }

                        ProgenyDetailsStackLayout.Margin = new Thickness(5, 0, 5, 5);
                        UpcomingEventsStatckLayout.Margin = new Thickness(5, 5, 5, 5);
                        LatestPostsStackLayout.Margin = new Thickness(5, 0, 5, 5);
                        ContainerStackLayout.Margin = new Thickness(0, 0, 0, 0);
                        ContainerStackLayout.Orientation = StackOrientation.Vertical;
                    }
                }
            }
            else
            {
                if (_screenWidth != width || _screenHeight != height)
                {
                    _screenWidth = width;
                    _screenHeight = height;

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
            if ( internetAccess != _feedModel.Online)
            {
                _feedModel.Online = internetAccess;
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
                    // await Shell.Current.GoToAsync($"photodetailpage?pictureId={picture.PictureId}");
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
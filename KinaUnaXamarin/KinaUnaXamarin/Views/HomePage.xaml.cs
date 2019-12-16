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
        private readonly HomeFeedViewModel _viewModel;
        private double _screenWidth;
        private double _screenHeight;
        private bool _reload = true;
        
        public HomePage()
        {
            InitializeComponent();
            _viewModel = new HomeFeedViewModel();
            BindingContext = _viewModel;
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
            
            var networkAccess = Connectivity.NetworkAccess;
            _viewModel.Online = networkAccess == NetworkAccess.Internet;
            if (_reload)
            {
                await SetUserAndProgeny();
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
            _viewModel.UserInfo = OfflineDefaultData.DefaultUserInfo;
            _viewModel.UserEmail = await UserService.GetUserEmail();
            _viewModel.AccessToken = await UserService.GetAuthAccessToken();
            _viewModel.UserInfo = await UserService.GetUserInfo(_viewModel.UserEmail);
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

                _viewModel.UserInfo = await App.Database.GetUserInfoAsync(_viewModel.UserEmail);
            }
            else
            {
                _viewModel.ViewChild = _viewModel.UserInfo.ViewChild;
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

            List<Progeny> progenyList = await ProgenyService.GetProgenyList(_viewModel.UserEmail);
            _viewModel.ProgenyCollection.Clear();
            _viewModel.CanUserAddItems = false;
            if (progenyList != null && progenyList.Any())
            {
                foreach (Progeny prog in progenyList)
                {
                    _viewModel.ProgenyCollection.Add(prog);
                    if (prog.Admins.ToUpper().Contains(_viewModel.UserInfo.UserEmail.ToUpper()))
                    {
                        _viewModel.CanUserAddItems = true;
                    }
                }
            }

            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewModel.ViewChild);
        }
        private async Task Reload()
        {
            _viewModel.IsBusy = true;
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
            bool accessTokenCurrent = false;
            if (_viewModel.AccessToken != "")
            {
                accessTokenCurrent = await UserService.IsAccessTokenCurrent();

                if (!accessTokenCurrent && _viewModel.Online)
                {
                    bool loginSuccess = await UserService.LoginIdsAsync();
                    if (loginSuccess)
                    {
                        _viewModel.AccessToken = await UserService.GetAuthAccessToken();
                        accessTokenCurrent = true;
                    }

                    await SetUserAndProgeny();
                    await Reload();
                }
            }

            if (String.IsNullOrEmpty(_viewModel.AccessToken) || !accessTokenCurrent)
            {

                _viewModel.IsLoggedIn = false;
                _viewModel.LoggedOut = true;
                _viewModel.AccessToken = "";
                _viewModel.UserInfo = OfflineDefaultData.DefaultUserInfo;

            }
            else
            {
                _viewModel.IsLoggedIn = true;
                _viewModel.LoggedOut = false;
            }
        }
        

        private async Task UpdateProgenyData()
        {
            AgeInfoStackLayout.IsVisible = false;
            RandomPictureStackLayout.IsVisible = false;
            _viewModel.CurrentTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            BirthTime progBirthTime;
            if (!String.IsNullOrEmpty(_viewModel.Progeny.NickName) && _viewModel.Progeny.BirthDay.HasValue && _viewModel.UserAccessLevel < 5)
            {
                progBirthTime = new BirthTime(_viewModel.Progeny.BirthDay.Value,
                    TimeZoneInfo.FindSystemTimeZoneById(_viewModel.Progeny.TimeZone));
            }
            else
            {
                progBirthTime = new BirthTime(new DateTime(2018, 02, 18, 18, 02, 00), TimeZoneInfo.FindSystemTimeZoneById(_viewModel.Progeny.TimeZone));
            }

            _viewModel.CurrentTime = progBirthTime.CurrentTime;
            _viewModel.Years = progBirthTime.CalcYears();
            _viewModel.Months = progBirthTime.CalcMonths();
            _viewModel.Weeks = progBirthTime.CalcWeeks();
            _viewModel.Days = progBirthTime.CalcDays();
            _viewModel.Hours = progBirthTime.CalcHours();
            _viewModel.Minutes = progBirthTime.CalcMinutes();
            _viewModel.NextBirthday = progBirthTime.CalcNextBirthday();
            _viewModel.MinutesMileStone = progBirthTime.CalcMileStoneMinutes();
            _viewModel.HoursMileStone = progBirthTime.CalcMileStoneHours();
            _viewModel.DaysMileStone = progBirthTime.CalcMileStoneDays();
            _viewModel.WeeksMileStone = progBirthTime.CalcMileStoneWeeks();

            Picture tempPicture = OfflineDefaultData.DefaultPicture;

            Picture displayPicture = tempPicture;

            if (_viewModel.UserAccessLevel < 5 && _viewModel.Online)
            {
                displayPicture = await ProgenyService.GetRandomPicture(_viewModel.Progeny.Id, _viewModel.UserAccessLevel, _viewModel.UserInfo.Timezone);
            }
            _viewModel.ImageLink600 = displayPicture.PictureLink600;
            _viewModel.ImageLink = displayPicture.PictureLink1200;
            _viewModel.ImageId = displayPicture.PictureId;
            PictureTime picTime = new PictureTime(new DateTime(2018, 02, 18, 20, 18, 00), new DateTime(2018, 02, 18, 20, 18, 00), TimeZoneInfo.FindSystemTimeZoneById(_viewModel.Progeny.TimeZone));
            if (displayPicture.PictureTime != null && _viewModel.Progeny.BirthDay.HasValue)
            {
                DateTime picTimeBirthday = new DateTime(_viewModel.Progeny.BirthDay.Value.Ticks, DateTimeKind.Unspecified);

                picTime = new PictureTime(picTimeBirthday, displayPicture.PictureTime, TimeZoneInfo.FindSystemTimeZoneById(_viewModel.Progeny.TimeZone));
                _viewModel.PicTimeValid = true;
            }

            _viewModel.Tags = displayPicture.Tags;
            _viewModel.Location = displayPicture.Location;
            _viewModel.PicTime = picTime.PictureDateTime;
            _viewModel.PicYears = picTime.CalcYears();
            _viewModel.PicMonths = picTime.CalcMonths();
            _viewModel.PicWeeks = picTime.CalcWeeks();
            _viewModel.PicDays = picTime.CalcDays();
            _viewModel.PicHours = picTime.CalcHours();
            _viewModel.PicMinutes = picTime.CalcMinutes();
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
            _viewModel.EventsList = new List<CalendarItem>();
            List<CalendarItem> eventsList = await ProgenyService.GetUpcomingEventsList(_viewModel.Progeny.Id, _viewModel.UserAccessLevel);
            int eventListCurrent = 0;
            
            if (eventsList.Any())
            {
                
                UpcomingEventsStatckLayout.IsVisible = true;
                foreach (CalendarItem ev in eventsList)
                {
                    ev.StartTime = TimeZoneInfo.ConvertTimeFromUtc(ev.StartTime.Value, TimeZoneInfo.FindSystemTimeZoneById(_viewModel.UserInfo.Timezone));
                    ev.EndTime = TimeZoneInfo.ConvertTimeFromUtc(ev.EndTime.Value, TimeZoneInfo.FindSystemTimeZoneById(_viewModel.UserInfo.Timezone));
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
            _viewModel.TimeLineItems.Clear();
            _viewModel.LatestPosts = await ProgenyService.GetLatestPosts(_viewModel.Progeny.Id, _viewModel.UserAccessLevel, _viewModel.UserInfo.Timezone);
            
            if (_viewModel.LatestPosts.Any())
            {
                LatestPostsStackLayout.IsVisible = true;
                _viewModel.TimeLineItems.AddRange(_viewModel.LatestPosts);
            }
        }

        
        private async void ReloadToolbarButton_OnClicked(object sender, EventArgs e)
        {
            await Reload();
        }

        private async void ProgenyToolBarItem_OnClicked(object sender, EventArgs e)
        {
            SelectProgenyPage selProPage = new SelectProgenyPage(_viewModel.ProgenyCollection);
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

                        if (_viewModel != null)
                        {
                            _viewModel.ImageLinkWidth = (int)(_screenWidth * 0.4 - 20);
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
                        if (_viewModel != null)
                        {
                            _viewModel.ImageLinkWidth = (int)(_screenWidth - 20);
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
                        if (_viewModel != null)
                        {
                            _viewModel.ImageLinkWidth = (int)ProgenyDetailsStackLayout.WidthRequest - 20;
                        }
                        ProgenyDetailsStackLayout.Margin = new Thickness(5, 0, 2, 5);
                        UpcomingEventsStatckLayout.Margin = new Thickness(3, 0, 5, 5);
                        LatestPostsStackLayout.Margin = new Thickness(3, 0, 5, 5);
                        ContainerStackLayout.Orientation = StackOrientation.Horizontal;
                    }
                    else
                    {
                        ProgenyDetailsStackLayout.WidthRequest = width;
                        if (_viewModel != null)
                        {
                            _viewModel.ImageLinkWidth = (int)ProgenyDetailsStackLayout.WidthRequest - 20;
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
            if ( internetAccess != _viewModel.Online)
            {
                _viewModel.Online = internetAccess;
                await Reload();
            }
        }

        private async void AddItemToolbarButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PushModalAsync(new AddItemPage());
        }

        private async void RandomPhotoClickGestureRecognizer_OnClicked(object sender, EventArgs e)
        {
            PhotoDetailPage photoPage = new PhotoDetailPage(_viewModel.ImageId);
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
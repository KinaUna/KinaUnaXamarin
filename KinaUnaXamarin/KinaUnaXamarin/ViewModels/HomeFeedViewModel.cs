using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using MvvmHelpers;
using Xamarin.Forms;

namespace KinaUnaXamarin.ViewModels
{
    public class HomeFeedViewModel : BaseViewModel
    {
        private bool _online = true;
        private int _userAccessLevel;
        private Progeny _progeny;
        private int _imageId;
        private string _imageLink;
        private string _imageLink600;
        private string _currentTime;
        private string _years;
        private string _months;
        private string[] _weeks;
        private string _days;
        private string _hours;
        private string _minutes;
        private string _nextBirthday;
        private string[] _minutesMileStone;
        private string[] _hoursMileStone;
        private string[] _daysMileStone;
        private string[] _weeksMileStone;
        private bool _picTimeValid;
        private string _picTime;
        private string _picYears;
        private string _picMonths;
        private string[] _picWeeks;
        private string _picDays;
        private string _picHours;
        private string _picMinutes;
        private string _tags;
        private string _location;
        private List<CalendarItem> _eventsList;
        private List<TimeLineItem> _latestPosts;
        private bool _isLoggedIn;
        private ObservableRangeCollection<TimeLineItem> _timeLineItems;
        private bool _loggedOut;
        private int _imageLinkWidth;
        private bool _canUserAddItems;

        public HomeFeedViewModel()
        {
            LoginCommand = new Command(Login);
            ProgenyCollection = new ObservableCollection<Progeny>();
            _timeLineItems = new ObservableRangeCollection<TimeLineItem>();
            _latestPosts = new List<TimeLineItem>();
            _progeny = OfflineDefaultData.DefaultProgeny;
            _imageId = OfflineDefaultData.DefaultPicture.PictureId;
            _imageLink = OfflineDefaultData.DefaultPicture.PictureLink;
            _imageLink600 = OfflineDefaultData.DefaultPicture.PictureLink600;
            ProgenyCollection.Add(OfflineDefaultData.DefaultProgeny);
            _tags = OfflineDefaultData.DefaultPicture.Tags;
            _userAccessLevel = 5;
        }

        public bool Online
        {
            get => _online;
            set => SetProperty(ref _online, value);
        }

        public int ViewChild { get; set; }

        public UserInfo UserInfo { get; set; }

        public string AccessToken { get; set; }

        public ObservableCollection<Progeny> ProgenyCollection { get; set; }
        
        public Progeny Progeny
        {
            get => _progeny;
            set => SetProperty(ref _progeny, value);
        }

        public int UserAccessLevel
        {
            get => _userAccessLevel;
            set => SetProperty(ref _userAccessLevel, value);
        }

        public bool CanUserAddItems
        {
            get => _canUserAddItems;
            set => SetProperty(ref _canUserAddItems, value);
        }

        public List<CalendarItem> EventsList
        {
            get => _eventsList;
            set => SetProperty(ref _eventsList, value);
        }
        //public ObservableRangeCollection<CalendarItem> UpcomingEvents
        //{
        //    get => _upcomingEvents;
        //    set => _upcomingEvents = value;
        //}

        public ObservableRangeCollection<TimeLineItem> TimeLineItems
        {
            get => _timeLineItems;
            set => _timeLineItems = value;
        }

        public int ImageId
        {
            get => _imageId;
            set => SetProperty(ref _imageId, value);
        }

        public string ImageLink
        {
            get => _imageLink;
            set => SetProperty(ref _imageLink, value);
        }

        public string ImageLink600
        {
            get => _imageLink600;
            set => SetProperty(ref _imageLink600, value);
        }

        public int ImageLinkWidth
        {
            get => _imageLinkWidth;
            set => SetProperty(ref _imageLinkWidth, value);
        }

        public string CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        public string Years
        {
            get => _years;
            set => SetProperty(ref _years, value);
        }

        public string Months
        {
            get => _months;
            set => SetProperty(ref _months, value);
        }

        public string[] Weeks
        {
            get => _weeks;
            set => SetProperty(ref _weeks, value);
        }

        public string Days
        {
            get => _days;
            set => SetProperty(ref _days, value);
        }

        public string Hours
        {
            get => _hours;
            set => SetProperty(ref _hours, value);
        }

        public string Minutes
        {
            get => _minutes;
            set => SetProperty(ref _minutes, value);
        }

        public string NextBirthday
        {
            get => _nextBirthday;
            set => SetProperty(ref _nextBirthday, value);
        }

        public string[] MinutesMileStone
        {
            get => _minutesMileStone;
            set => SetProperty(ref _minutesMileStone, value);
        }

        public string[] HoursMileStone
        {
            get => _hoursMileStone;
            set => SetProperty(ref _hoursMileStone, value);
        }

        public string[] DaysMileStone
        {
            get => _daysMileStone;
            set => SetProperty(ref _daysMileStone, value);
        }

        public string[] WeeksMileStone
        {
            get => _weeksMileStone;
            set => SetProperty(ref _weeksMileStone, value);
        }

        public bool PicTimeValid
        {
            get => _picTimeValid;
            set => SetProperty(ref _picTimeValid, value);
        }

        public string PicTime
        {
            get => _picTime;
            set => SetProperty(ref _picTime, value);
        }

        public string PicYears
        {
            get => _picYears;
            set => SetProperty(ref _picYears, value);
        }

        public string PicMonths
        {
            get => _picMonths;
            set => SetProperty(ref _picMonths, value);
        }

        public string[] PicWeeks
        {
            get => _picWeeks;
            set => SetProperty(ref _picWeeks, value);
        }

        public string PicDays
        {
            get => _picDays;
            set => SetProperty(ref _picDays, value);
        }

        public string PicHours
        {
            get => _picHours;
            set => SetProperty(ref _picHours, value);
        }

        public string PicMinutes
        {
            get => _picMinutes;
            set => SetProperty(ref _picMinutes, value);
        }

        public string Tags
        {
            get => _tags;
            set => SetProperty(ref _tags, value);
        }

        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        public List<TimeLineItem> LatestPosts
        {
            get => _latestPosts;
            set => SetProperty(ref _latestPosts, value);
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }

        public bool LoggedOut
        {
            get => _loggedOut;
            set => SetProperty(ref _loggedOut, value);
        }

        public ICommand LoginCommand
        {
            get;
            private set;
        }

        private async void Login()
        {
            IsLoggedIn = await UserService.LoginIdsAsync();
            if (IsLoggedIn)
            {
                LoggedOut = !IsLoggedIn;
            }

            MessagingCenter.Send(this, "Reload");
        }
    }
}

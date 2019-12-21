﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using dotMorten.Xamarin.Forms;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using KinaUnaXamarin.ViewModels.Details;
using PanCardView;
using PanCardView.EventArgs;
using Plugin.Multilingual;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views.Details
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoDetailPage
    {
        readonly VideoDetailViewModel _viewModel;
        private UserInfo _userInfo;
        private string _accessToken;
        private int _viewChild = Constants.DefaultChildId;
        private bool _online = true;
        private bool _modalShowing;
        private bool _dataChanged;
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));
        private CommentsPageViewModel _commentsPageViewModel;

        public VideoDetailPage(int videoId)
        {
            InitializeComponent();
            _viewModel = new VideoDetailViewModel();
            _viewModel.CurrentVideoId = videoId;
            BindingContext = _viewModel;
            ContentGrid.BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
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

            if (!_modalShowing)
            {
                await Reload();
            }
            else
            {
                _modalShowing = false;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
            _modalShowing = true;
        }

        private async Task Reload()
        {
            _viewModel.IsBusy = true;
            _dataChanged = false;
            await CheckAccount();

            VideoViewModel videoViewModel = await ProgenyService.GetVideoViewModel(
                _viewModel.CurrentVideoId, _viewModel.UserAccessLevel, _userInfo.Timezone, 1);
            _viewModel.VideoItems.Add(videoViewModel);
            _viewModel.CurrentVideoViewModel = videoViewModel;

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

        private async Task LoadNewer()
        {
            _viewModel.CanLoadMore = false;
            VideoViewModel videoViewModel = _viewModel.VideoItems.FirstOrDefault();
            if (videoViewModel != null)
            {
                VideoViewModel videoViewModel2 = await ProgenyService.GetVideoViewModel(
                    videoViewModel.PrevVideo, _viewModel.UserAccessLevel, _userInfo.Timezone, 1);
                _viewModel.VideoItems.Insert(0, videoViewModel2);
                if (_viewModel.VideoItems.Count > 10)
                {
                    _viewModel.VideoItems.RemoveAt(_viewModel.VideoItems.Count - 1);
                }
            }

            _viewModel.CanLoadMore = true;
        }

        private async Task LoadOlder()
        {
            _viewModel.CanLoadMore = false;
            VideoViewModel videoViewModel = _viewModel.VideoItems.LastOrDefault();
            if (videoViewModel != null)
            {
                VideoViewModel videoViewModel2 = await ProgenyService.GetVideoViewModel(
                    videoViewModel.NextVideo, _viewModel.UserAccessLevel, _userInfo.Timezone, 1);
                _viewModel.VideoItems.Add(videoViewModel2);
                if (_viewModel.VideoItems.Count > 10)
                {
                    _viewModel.VideoItems.RemoveAt(0);
                }
            }

            _viewModel.CanLoadMore = true;
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

            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewChild);
            if (_viewModel.UserAccessLevel == 0)
            {
                _viewModel.CanUserEditItems = true;
            }
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

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called
            _viewModel.ImageHeight = height * 0.8;
            _viewModel.ImageWidth = width * 0.9;

            if (height > width)
            {
                LocationMap.WidthRequest = width * 0.9;
                DataStackLayout.WidthRequest = width * 0.9;
                InfoStackLayout.Orientation = StackOrientation.Vertical;
                _detailsHeight = LocationMap.Height + DataStackLayout.Height;
            }
            else
            {
                LocationMap.WidthRequest = width * 0.45;
                DataStackLayout.WidthRequest = width * 0.45;
                InfoStackLayout.Orientation = StackOrientation.Horizontal;
                _detailsHeight = LocationMap.Height;
            }
        }

        private async void CardsView_OnItemAppearing(CardsView view, ItemAppearingEventArgs args)
        {
            _viewModel.IsZoomed = false;
            _viewModel.IsBusy = true;
            
            if (_viewModel.CanLoadMore && _viewModel.CurrentIndex < 1)
            {
                await LoadNewer();

            }

            if (_viewModel.CanLoadMore && _viewModel.CurrentIndex > _viewModel.VideoItems.Count - 2)
            {
                await LoadOlder();

            }

            _viewModel.CurrentVideoViewModel = _viewModel.VideoItems[_viewModel.CurrentIndex];
            _viewModel.CurrentVideoId = _viewModel.CurrentVideoViewModel.VideoId;
            await UpdateEditInfo();

            if (_viewModel.CurrentVideoViewModel.VideoTime != null && _viewModel.Progeny.BirthDay.HasValue)
            {
                DateTime picTimeBirthday = new DateTime(_viewModel.Progeny.BirthDay.Value.Ticks, DateTimeKind.Unspecified);

                PictureTime picTime = new PictureTime(picTimeBirthday, _viewModel.CurrentVideoViewModel.VideoTime, TimeZoneInfo.FindSystemTimeZoneById(_viewModel.Progeny.TimeZone));
                _viewModel.PicTimeValid = true;
                _viewModel.PicYears = picTime.CalcYears();
                _viewModel.PicMonths = picTime.CalcMonths();
                _viewModel.PicWeeks = picTime.CalcWeeks();
                _viewModel.PicDays = picTime.CalcDays();
                _viewModel.PicHours = picTime.CalcHours();
                _viewModel.PicMinutes = picTime.CalcMinutes();
            }

            LocationMap.Pins.Clear();
            if (!string.IsNullOrEmpty(_viewModel.CurrentVideoViewModel.Latitude) &&
                !string.IsNullOrEmpty(_viewModel.CurrentVideoViewModel.Longtitude))
            {
                LocationMap.IsVisible = true;
                bool latParsed = double.TryParse(_viewModel.CurrentVideoViewModel.Latitude, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out double lat);
                bool lonParsed = double.TryParse(_viewModel.CurrentVideoViewModel.Longtitude, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out double lon);
                if (latParsed && lonParsed)
                {
                    Position position = new Position(lat, lon);
                    Pin pin = new Pin();
                    pin.Position = position;
                    pin.Label = _viewModel.CurrentVideoViewModel.Location;
                    pin.Type = PinType.SavedPin;
                    LocationMap.Pins.Add(pin);
                    LocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(2)));
                }
            }
            else
            {
                LocationMap.IsVisible = false;
            }
            _viewModel.IsBusy = false;
        }

        private double y;
        private double _detailsHeight;

        private void FrameOnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            // Source: https://github.com/rlingineni/Forms-BottomSheet/blob/master/XamJuly/MainPage.xaml.cs

            // Handle the pan
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    // Translate and ensure we don't y + e.TotalY pan beyond the wrapped user interface element bounds.
                    var translateY = Math.Max(Math.Min(0, y + e.TotalY), -Math.Abs((Height * .25) - Height));
                    BottomSheetFrame.TranslateTo(BottomSheetFrame.X, translateY, 20);
                    break;
                case GestureStatus.Completed:
                    // Store the translation applied during the pan
                    y = BottomSheetFrame.TranslationY;

                    //at the end of the event - snap to the closest location
                    var finalTranslation = Math.Max(Math.Min(0, -1000), -Math.Abs(GetClosestLockState(e.TotalY + y)));

                    //depending on Swipe Up or Down - change the snapping animation
                    if (isSwipeUp(e))
                    {
                        BottomSheetFrame.TranslateTo(BottomSheetFrame.X, finalTranslation, 250, Easing.SpringIn);
                    }
                    else
                    {
                        BottomSheetFrame.TranslateTo(BottomSheetFrame.X, finalTranslation, 250, Easing.SpringOut);
                    }


                    y = BottomSheetFrame.TranslationY;

                    break;
            }
        }

        private bool isSwipeUp(PanUpdatedEventArgs e)
        {
            if (e.TotalY < 0)
            {
                return true;
            }
            return false;
        }

        private double GetClosestLockState(double translationY)
        {
            //Play with these values to adjust the locking motions - this will change depending on the amount of content ona  apge
            var lockStates = new[] { 0, .1, .2, .3, .4, .5, .6, .7, .8, .9 };

            //get the current proportion of the sheet in relation to the screen
            var distance = Math.Abs(translationY);
            var currentProportion = distance / Height;

            //calculate which lockstate it's the closest to
            var smallestDistance = 10000.0;
            var closestIndex = 0;
            for (var i = 0; i < lockStates.Length; i++)
            {
                var state = lockStates[i];
                var absoluteDistance = Math.Abs(state - currentProportion);
                if (absoluteDistance < smallestDistance)
                {
                    smallestDistance = absoluteDistance;
                    closestIndex = i;
                }
            }

            var selectedLockState = lockStates[closestIndex];
            var translateToLockState = GetProportionCoordinate(selectedLockState);

            return translateToLockState;
        }

        private double GetProportionCoordinate(double proportion)
        {
            return proportion * Height;
        }

        private async void CommentsClicked(object sender, EventArgs e)
        {
            if (_viewModel.IsLoggedIn)
            {
                await GetComments();
                _viewModel.ShowComments = true;
            }
        }

        private async Task GetComments()
        {
            _commentsPageViewModel = new CommentsPageViewModel();
            _commentsPageViewModel.CommentsCollection = new ObservableCollection<Comment>();
            List<Comment> commentsList = await ProgenyService.GetComments(_viewModel.CurrentVideoViewModel.CommentThreadNumber);
            if (commentsList.Any())
            {
                foreach (var comment in commentsList)
                {
                    _commentsPageViewModel.CommentsCollection.Add(comment);
                }

                _viewModel.CurrentVideoViewModel.CommentsCount = commentsList.Count;
            }
            CommentsCollectionView.ItemsSource = _commentsPageViewModel.CommentsCollection;
        }

        private async void AddCommentButton_OnClicked(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(AddCommentEditor.Text))
            {
                AddCommentButton.IsEnabled = false;
                await ProgenyService.AddComment(_viewModel.CurrentVideoViewModel.CommentThreadNumber, AddCommentEditor.Text, _viewModel.Progeny, _viewModel.CurrentVideoViewModel.VideoId.ToString(), (int)KinaUnaTypes.TimeLineType.Video);
                AddCommentEditor.Text = "";
                await GetComments();
                AddCommentButton.IsEnabled = true;
            }
        }

        private async void DeleteCommentButton_OnClicked(object sender, EventArgs e)
        {
            Button deleteButton = (Button)sender;
            string commentIdString = deleteButton.CommandParameter.ToString();
            int.TryParse(commentIdString, out int commentId);
            Comment comment = _commentsPageViewModel.CommentsCollection.SingleOrDefault(c => c.CommentId == commentId);
            if (comment != null)
            {
                comment.Progeny = _viewModel.Progeny;
                comment.ItemId = _viewModel.CurrentVideoViewModel.VideoId.ToString();
                comment.ItemType = (int) KinaUnaTypes.TimeLineType.Video;
                var ci = CrossMultilingual.Current.CurrentCultureInfo;
                string deleteTitle = resmgr.Value.GetString("DeleteTitle", ci);
                string areYouSure = resmgr.Value.GetString("ConfirmCommentDelete", ci);
                string yes = resmgr.Value.GetString("Yes", ci);
                string no = resmgr.Value.GetString("No", ci);
                var confirmDelete = await DisplayAlert(deleteTitle, areYouSure, yes, no);
                if (confirmDelete)
                {
                    await ProgenyService.DeleteComment(comment);
                    await GetComments();
                }
            }
        }

        private void FrameOnTap(object sender, EventArgs e)
        {
            y = BottomSheetFrame.TranslationY;
            if (Math.Abs(y) < Height / 10)
            {
                var finalTranslation = Math.Max(Math.Min(0, -1000), -Math.Abs(GetClosestLockState(_detailsHeight + 15)));
                BottomSheetFrame.TranslateTo(BottomSheetFrame.X, finalTranslation, 350, Easing.SpringIn);
            }
            else
            {
                BottomSheetFrame.TranslateTo(BottomSheetFrame.X, 0, 350, Easing.SpringOut);
            }


            y = BottomSheetFrame.TranslationY;
        }

        private async void EditClicked(object sender, EventArgs e)
        {
            await UpdateEditInfo();
            VideoCarousel.VerticalOptions = LayoutOptions.Start;
            _viewModel.VideoVerticalOptions = LayoutOptions.Start;
            _viewModel.EditMode = true;
        }

        private async Task UpdateEditInfo()
        {
            MessageLabel.Text = "";
            MessageLabel.IsVisible = false;
            CancelButton.BackgroundColor = Color.DimGray;

            _viewModel.LocationAutoSuggestList = await ProgenyService.GetLocationAutoSuggestList(_viewModel.Progeny.Id, _viewModel.UserAccessLevel);
            _viewModel.TagsAutoSuggestList = await ProgenyService.GetTagsAutoSuggestList(_viewModel.Progeny.Id, _viewModel.UserAccessLevel);

            TagsEditor.Text = _viewModel.CurrentVideoViewModel.Tags;
            if (_viewModel.CurrentVideoViewModel.VideoTime.HasValue)
            {
                PhotoDatePicker.Date = _viewModel.CurrentVideoViewModel.VideoTime.Value.Date;
                PhotoTimePicker.Time = _viewModel.CurrentVideoViewModel.VideoTime.Value.TimeOfDay;
            }

            if (_viewModel.CurrentVideoViewModel.Duration.HasValue)
            {
                _viewModel.VideoHours = _viewModel.CurrentVideoViewModel.Duration.Value.Hours;
                _viewModel.VideoMinutes = _viewModel.CurrentVideoViewModel.Duration.Value.Minutes;
                _viewModel.VideoSeconds = _viewModel.CurrentVideoViewModel.Duration.Value.Seconds;
            }

            LocationEntry.Text = _viewModel.CurrentVideoViewModel.Location;
            LatitudeEntry.Text = _viewModel.CurrentVideoViewModel.Latitude;
            LongitudeEntry.Text = _viewModel.CurrentVideoViewModel.Longtitude;
            AltitudeEntry.Text = _viewModel.CurrentVideoViewModel.Altitude;
            AccessLevelPicker.SelectedIndex = _viewModel.CurrentVideoViewModel.AccessLevel;

            DeleteButton.IsVisible = true;
            CancelButton.IsVisible = true;
            CancelButton.Text = IconFont.Cancel;
            SaveButton.IsVisible = true;
            _dataChanged = false;
        }

        private async void CancelButton_OnClicked(object sender, EventArgs e)
        {
            DeleteButton.IsVisible = true;
            VideoCarousel.VerticalOptions = LayoutOptions.FillAndExpand;
            _viewModel.VideoVerticalOptions = LayoutOptions.FillAndExpand;
            _viewModel.EditMode = false;
            if (_dataChanged)
            {
                await Reload();
            }

        }

        private async void SaveButton_OnClicked(object sender, EventArgs e)
        {
            Video updatedVideo = await ProgenyService.GetVideo(_viewModel.CurrentVideoViewModel.VideoId, _accessToken, _userInfo.Timezone);
            updatedVideo.Progeny = _viewModel.Progeny;
            updatedVideo.Tags = TagsEditor.Text;
            updatedVideo.Location = LocationEntry.Text;
            if (!string.IsNullOrEmpty(LatitudeEntry.Text))
            {
                updatedVideo.Latitude = LatitudeEntry.Text.Replace(',', '.');
            }

            if (!string.IsNullOrEmpty(LongitudeEntry.Text))
            {
                updatedVideo.Longtitude = LongitudeEntry.Text.Replace(',', '.');
            }

            if (!string.IsNullOrEmpty(AltitudeEntry.Text))
            {
                updatedVideo.Altitude = AltitudeEntry.Text.Replace(',', '.');
            }

            updatedVideo.AccessLevel = AccessLevelPicker.SelectedIndex;
            int videoSeconds = 0;
            if (updatedVideo.VideoTime.HasValue)
            {
                videoSeconds = updatedVideo.VideoTime.Value.Second;
            }

            DateTime newVideoTime = new DateTime(PhotoDatePicker.Date.Year, PhotoDatePicker.Date.Month, PhotoDatePicker.Date.Day, PhotoTimePicker.Time.Hours, PhotoTimePicker.Time.Minutes, videoSeconds);
            updatedVideo.VideoTime = TimeZoneInfo.ConvertTimeToUtc(newVideoTime, TimeZoneInfo.FindSystemTimeZoneById(_userInfo.Timezone));

            Int32.TryParse(VideoHoursEntry.Text, out var durHours);
            Int32.TryParse(VideoMinutesEntry.Text, out var durMins);
            Int32.TryParse(VideoSecondsEntry.Text, out var durSecs);
            if (durHours + durMins + durSecs != 0)
            {
                updatedVideo.Duration = new TimeSpan(durHours, durMins, durSecs);
            }

            Video savedVideo = await ProgenyService.UpdateVideo(updatedVideo);

            if (savedVideo != null && savedVideo.VideoId != 0)
            {
                _dataChanged = true;
                TimeLineItem tItem = await ProgenyService.GetTimeLineItemByItemId(savedVideo.VideoId, KinaUnaTypes.TimeLineType.Video);
                if (tItem != null && tItem.TimeLineId != 0)
                {
                    tItem.AccessLevel = savedVideo.AccessLevel;
                    if (savedVideo.VideoTime.HasValue)
                    {
                        tItem.ProgenyTime = savedVideo.VideoTime.Value;
                    }
                    else
                    {
                        tItem.ProgenyTime = DateTime.UtcNow;
                    }

                    TimeLineItem updatedTimeLineItem = await ProgenyService.UpdateTimeLineItem(tItem);
                    if (updatedTimeLineItem != null && updatedTimeLineItem.TimeLineId != 0)
                    {
                        MessageLabel.IsVisible = true;
                        var ci = CrossMultilingual.Current.CurrentCultureInfo;
                        MessageLabel.Text = resmgr.Value.GetString("VideoSaved", ci) + savedVideo.VideoId;
                        MessageLabel.BackgroundColor = Color.Green;
                        SaveButton.IsVisible = false;
                        DeleteButton.IsVisible = false;
                        CancelButton.Text = "Ok";
                        CancelButton.BackgroundColor = Color.FromHex("#4caf50");
                        CancelButton.IsEnabled = true;
                        await Reload();
                    }
                    else
                    {
                        MessageLabel.IsVisible = true;
                        var ci = CrossMultilingual.Current.CurrentCultureInfo;
                        MessageLabel.Text = resmgr.Value.GetString("ErrorVideoNotSaved", ci);
                        MessageLabel.BackgroundColor = Color.Red;
                        SaveButton.IsEnabled = true;
                        CancelButton.IsEnabled = true;
                        DeleteButton.IsVisible = true;
                    }
                }
            }
        }

        private async void DeleteButton_OnClicked(object sender, EventArgs e)
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string confirmTitle = resmgr.Value.GetString("DeletePhoto", ci);
            string confirmMessage = resmgr.Value.GetString("DeletePhotoMessage", ci) + " ? ";
            string yes = resmgr.Value.GetString("Yes", ci);
            string no = resmgr.Value.GetString("No", ci);
            bool confirmDelete = await DisplayAlert(confirmTitle, confirmMessage, yes, no);
            if (confirmDelete)
            {
                _viewModel.IsBusy = true;
                int deleteId =_viewModel.CurrentVideoViewModel.VideoId;
                _viewModel.EditMode = false;
                Video deletedVideo = await ProgenyService.DeleteVideo(_viewModel.CurrentVideoViewModel.VideoId);
                if (deletedVideo.VideoId == 0)
                {
                    _dataChanged = true;

                    TimeLineItem tItem = await ProgenyService.GetTimeLineItemByItemId(deleteId, KinaUnaTypes.TimeLineType.Video);
                    if (tItem != null && tItem.TimeLineId != 0)
                    {
                        await ProgenyService.DeleteTimeLineItem(tItem);
                    }

                    if (_viewModel.VideoItems.Count > 1)
                    {
                        if (_viewModel.VideoItems.Count < _viewModel.CurrentIndex + 1)
                        {
                            VideoViewModel thisVideoViewModel = _viewModel.CurrentVideoViewModel;
                            VideoViewModel nextVideoViewModel = _viewModel.VideoItems[_viewModel.CurrentIndex + 1];
                            _viewModel.CurrentVideoId = nextVideoViewModel.VideoId;
                            _viewModel.CurrentIndex = _viewModel.CurrentIndex + 1;
                            _viewModel.VideoItems.Remove(thisVideoViewModel);
                        }
                        else
                        {
                            if (_viewModel.VideoItems.Count > _viewModel.CurrentIndex - 1)
                            {
                                VideoViewModel thisVideoViewModel = _viewModel.CurrentVideoViewModel;
                                VideoViewModel nextVideoViewModel = _viewModel.VideoItems[_viewModel.CurrentIndex - 1];
                                _viewModel.CurrentVideoId = nextVideoViewModel.VideoId;
                                _viewModel.CurrentIndex = _viewModel.CurrentIndex - 1;
                                _viewModel.VideoItems.Remove(thisVideoViewModel);
                            }
                        }
                    }

                    // Todo: Translate success message
                    MessageLabel.Text = "Video deleted.";
                    MessageLabel.IsVisible = true;
                    SaveButton.IsVisible = false;
                    DeleteButton.IsVisible = false;
                    CancelButton.Text = "Ok";
                    CancelButton.BackgroundColor = Color.FromHex("#4caf50");
                    CancelButton.IsEnabled = true;
                }
                else
                {
                    // Todo: Translate failed message
                    MessageLabel.Text = "Video deletion failed.";
                    MessageLabel.IsVisible = true;
                    MessageLabel.BackgroundColor = Color.Red;
                    SaveButton.IsEnabled = true;
                    DeleteButton.IsVisible = true;
                    CancelButton.IsEnabled = true;
                }



                _viewModel.IsBusy = false;
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (_viewModel.ShowComments)
            {
                _viewModel.ShowComments = false;
            }
            else
            {
                if (_viewModel.EditMode)
                {
                    VideoCarousel.VerticalOptions = LayoutOptions.FillAndExpand;
                    _viewModel.VideoVerticalOptions = LayoutOptions.FillAndExpand;
                    _viewModel.EditMode = false;
                }
                else
                {
                    return base.OnBackButtonPressed();
                }
            }

            return true;
        }

        private void LocationEntry_OnTextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
                if (autoSuggestBox != null && autoSuggestBox.Text.Length > 0)
                {
                    List<string> filteredLocations = new List<string>();
                    foreach (string locationString in _viewModel.LocationAutoSuggestList)
                    {
                        if (locationString.ToUpper().Contains(autoSuggestBox.Text.Trim().ToUpper()))
                        {
                            filteredLocations.Add(locationString);
                        }
                    }
                    //Set the ItemsSource to be your filtered dataset
                    autoSuggestBox.ItemsSource = filteredLocations;
                }
                else
                {
                    if (autoSuggestBox != null)
                    {
                        autoSuggestBox.ItemsSource = null;
                    }
                }
            }
        }

        private void LocationEntry_OnQuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            if (e.ChosenSuggestion != null)
            {
                AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
                if (autoSuggestBox != null)
                {
                    // User selected an item from the suggestion list, take an action on it here.
                    autoSuggestBox.Text = e.ChosenSuggestion.ToString();
                    autoSuggestBox.ItemsSource = null;
                }
            }
            else
            {
                // User hit Enter from the search box. Use e.QueryText to determine what to do.
            }
        }

        private void LocationEntry_OnSuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
            // Set sender.Text. You can use e.SelectedItem to build your text string.
            if (autoSuggestBox != null)
            {
                autoSuggestBox.Text = e.SelectedItem.ToString();
            }
        }

        private void TagsEditor_OnTextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
                if (autoSuggestBox != null && autoSuggestBox.Text.Length > 0)
                {
                    string lastTag = autoSuggestBox.Text.Split(',').LastOrDefault();
                    if (!string.IsNullOrEmpty(lastTag) && lastTag.Length > 0)
                    {
                        List<string> filteredTags = new List<string>();
                        foreach (string tagString in _viewModel.TagsAutoSuggestList)
                        {
                            if (tagString.Trim().ToUpper().Contains(lastTag.Trim().ToUpper()))
                            {
                                filteredTags.Add(tagString);
                            }
                        }
                        //Set the ItemsSource to be your filtered dataset
                        autoSuggestBox.ItemsSource = filteredTags;
                    }
                    else
                    {
                        autoSuggestBox.ItemsSource = null;
                    }

                }
                else
                {
                    if (autoSuggestBox != null)
                    {
                        autoSuggestBox.ItemsSource = null;
                    }
                }
            }
        }

        private void TagsEditor_OnQuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            if (e.ChosenSuggestion != null)
            {
                AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
                if (autoSuggestBox != null)
                {
                    // User selected an item from the suggestion list, take an action on it here.
                    List<string> existingTags = TagsEditor.Text.Split(',').ToList();
                    existingTags.Remove(existingTags.Last());
                    string newText = "";
                    if (existingTags.Any())
                    {
                        foreach (string tagString in existingTags)
                        {
                            newText = newText + tagString + ", ";
                        }
                    }
                    newText = newText + e.ChosenSuggestion + ", ";
                    autoSuggestBox.Text = newText;

                    autoSuggestBox.ItemsSource = null;
                }
            }
            else
            {
                // User hit Enter from the search box. Use e.QueryText to determine what to do.
            }
        }

        private void TagsEditor_OnSuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
            // Set sender.Text. You can use e.SelectedItem to build your text string.
            if (autoSuggestBox != null)
            {
                //List<string> existingTags = TagsEditor.Text.Split(',').ToList();
                //existingTags.Remove(existingTags.Last());
                //autoSuggestBox.Text = "";
                //foreach (string tagString in existingTags)
                //{
                //    autoSuggestBox.Text = autoSuggestBox.Text + ", " + tagString;
                //}
                //autoSuggestBox.Text = autoSuggestBox.Text + e.SelectedItem.ToString();
            }
        }

        private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            if (_viewModel.ShowComments)
            {
                _viewModel.ShowComments = false;
            }
            else
            {
                if (_viewModel.EditMode)
                {
                    VideoCarousel.VerticalOptions = LayoutOptions.FillAndExpand;
                    _viewModel.VideoVerticalOptions = LayoutOptions.FillAndExpand;
                    _viewModel.EditMode = false;
                }
                else
                {
                    await Shell.Current.Navigation.PopModalAsync();
                }
            }
        }
    }
}
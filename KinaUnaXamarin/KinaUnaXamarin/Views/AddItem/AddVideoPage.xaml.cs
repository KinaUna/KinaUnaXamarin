using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using dotMorten.Xamarin.Forms;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels.AddItem;
using Plugin.Multilingual;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views.AddItem
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddVideoPage
    {
        private readonly AddVideoViewModel _viewModel;
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public AddVideoPage()
        {
            InitializeComponent();
            _viewModel = new AddVideoViewModel();
            _viewModel.VideoItem = new Video();
            _viewModel.VideoItem.AccessLevel = 0;
            VideoDatePicker.Date = DateTime.Now.Date;
            VideoTimePicker.Time = DateTime.Now.TimeOfDay;
            BindingContext = _viewModel;
            ProgenyCollectionView.ItemsSource = _viewModel.ProgenyCollection;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await ProgenyService.GetProgenyList(await UserService.GetUserEmail());
            List<Progeny> progenyList = await ProgenyService.GetProgenyAdminList();
            if (progenyList.Any())
            {
                foreach (Progeny progeny in progenyList)
                {
                    _viewModel.ProgenyCollection.Add(progeny);
                }

                string userviewchild = await SecureStorage.GetAsync(Constants.UserViewChildKey);
                bool viewchildParsed = int.TryParse(userviewchild, out int viewChild);
                Progeny viewProgeny = new Progeny();
                if (viewchildParsed)
                {
                    viewProgeny = _viewModel.ProgenyCollection.SingleOrDefault(p => p.Id == viewChild);
                }
                
                if (viewProgeny != null)
                {
                    ProgenyCollectionView.SelectedItem =
                        _viewModel.ProgenyCollection.SingleOrDefault(p => p.Id == viewChild);
                    ProgenyCollectionView.ScrollTo(ProgenyCollectionView.SelectedItem);
                }
                else
                {
                    ProgenyCollectionView.SelectedItem = _viewModel.ProgenyCollection[0];
                }
            }
            _viewModel.VideoItem.ProgenyId = ((Progeny)ProgenyCollectionView.SelectedItem).Id;
        }
        
        private async void CancelVideoButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        private async void SaveVideoButton_OnClicked(object sender, EventArgs e)
        {
            _viewModel.IsBusy = true;
            Progeny progeny = ProgenyCollectionView.SelectedItem as Progeny;
            if (progeny != null)
            {
                
                Video saveVideo = new Video();
                saveVideo.ProgenyId = progeny.Id;
                saveVideo.AccessLevel = _viewModel.AccessLevel;
                saveVideo.Progeny = progeny;
                string userEmail = await UserService.GetUserEmail();
                UserInfo userinfo = await UserService.GetUserInfo(userEmail);
                saveVideo.Author = userinfo.UserId;
                saveVideo.Owners = userEmail;
                saveVideo.VideoType = 2; // Todo: Replace with Enum or constant. Assuming it is Youtube video for now.
                saveVideo.Tags = TagsEntry.Text;
                saveVideo.Location = LocationEntry.Text;

                Int32.TryParse(VideoHoursEntry.Text, out var durHours);
                Int32.TryParse(VideoMinutesEntry.Text, out var durMins);
                Int32.TryParse(VideoSecondsEntry.Text, out var durSecs);
                if (durHours + durMins + durSecs != 0)
                {
                    saveVideo.Duration = new TimeSpan(durHours, durMins, durSecs);
                }
                DateTime videoTime = new DateTime(VideoDatePicker.Date.Year, VideoDatePicker.Date.Month, VideoDatePicker.Date.Day, VideoTimePicker.Time.Hours, VideoTimePicker.Time.Minutes, 0);
                saveVideo.VideoTime = videoTime;

                try
                {
                    TimeZoneInfo.FindSystemTimeZoneById(userinfo.Timezone);
                }
                catch (Exception)
                {
                    userinfo.Timezone = TZConvert.WindowsToIana(userinfo.Timezone);
                }

                if (saveVideo.VideoTime != null)
                {
                    saveVideo.VideoTime = TimeZoneInfo.ConvertTimeToUtc(saveVideo.VideoTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userinfo.Timezone));
                }

                string fileLink = LinkEntry.Text;

                if (fileLink.Contains("<iframe"))
                {
                    string[] vLink1 = fileLink.Split('"');
                    foreach (string str in vLink1)
                    {
                        if (str.Contains("://"))
                        {
                            saveVideo.VideoLink = str;
                        }
                    }
                }

                if (fileLink.Contains("watch?v"))
                {
                    string str = fileLink.Split('=').Last();
                    saveVideo.VideoLink = "https://www.youtube.com/embed/" + str;
                }

                if (fileLink.StartsWith("https://youtu.be"))
                {
                    string str = fileLink.Split('/').Last();
                    saveVideo.VideoLink = "https://www.youtube.com/embed/" + str;
                }

                saveVideo.ThumbLink = "https://i.ytimg.com/vi/" + saveVideo.VideoLink.Split('/').Last() + "/hqdefault.jpg";

                if (ProgenyService.Online())
                {
                    // Todo: Translate messages.
                    saveVideo = await ProgenyService.SaveVideo(saveVideo);
                    if (saveVideo.VideoId == 0)
                    {
                        var ci = CrossMultilingual.Current.CurrentCultureInfo;
                        ErrorLabel.Text = resmgr.Value.GetString("ErrorVideoNotSaved", ci);
                        ErrorLabel.BackgroundColor = Color.Red;

                    }
                    else
                    {
                        TimeLineItem tItem = new TimeLineItem();
                        tItem.ProgenyId = saveVideo.ProgenyId;
                        tItem.AccessLevel = saveVideo.AccessLevel;
                        tItem.ItemType = (int)KinaUnaTypes.TimeLineType.Video;
                        tItem.ItemId = saveVideo.VideoId.ToString();
                        tItem.CreatedBy = userinfo.UserId;
                        tItem.CreatedTime = DateTime.UtcNow;
                        if (saveVideo.VideoTime.HasValue)
                        {
                            tItem.ProgenyTime = saveVideo.VideoTime.Value;
                        }
                        else
                        {
                            tItem.ProgenyTime = DateTime.UtcNow;
                        }

                        tItem = await ProgenyService.SaveTimeLineItem(tItem);
                        if (tItem.TimeLineId != 0)
                        {
                            var ci = CrossMultilingual.Current.CurrentCultureInfo;
                            ErrorLabel.Text = resmgr.Value.GetString("VideoSaved", ci) + saveVideo.VideoId;
                            ErrorLabel.BackgroundColor = Color.Green;
                            SaveVideoButton.IsVisible = false;
                            CancelVideoButton.Text = "Ok";
                            CancelVideoButton.BackgroundColor = Color.FromHex("#4caf50");
                        }
                        else
                        {
                            // Todo: Translate message.
                            ErrorLabel.Text = $"Error: Video for {progeny.NickName} was not saved. Try again later.";
                            ErrorLabel.BackgroundColor = Color.Red;
                        }
                    }
                }
                else
                {
                    // Todo: Translate message.
                    ErrorLabel.Text = $"Error: No internet connection. Video for {progeny.NickName} was not saved. Try again later.";
                    ErrorLabel.BackgroundColor = Color.Red;
                }
                
                ErrorLabel.IsVisible = true;
            }

            _viewModel.IsBusy = false;
        }

        private async void ProgenyCollectionView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Progeny viewProgeny = ProgenyCollectionView.SelectedItem as Progeny;
            if (viewProgeny != null)
            {
                _viewModel.LocationAutoSuggestList = await ProgenyService.GetLocationAutoSuggestList(viewProgeny.Id, 0);
                _viewModel.TagsAutoSuggestList = await ProgenyService.GetTagsAutoSuggestList(viewProgeny.Id, 0);
            }
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
                    List<string> existingTags = TagsEntry.Text.Split(',').ToList();
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
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
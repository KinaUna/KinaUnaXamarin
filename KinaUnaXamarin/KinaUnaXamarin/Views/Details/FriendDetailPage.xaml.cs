using System;
using System.Collections.Generic;
using System.IO;
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
using Plugin.Media;
using Plugin.Multilingual;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FriendDetailPage : ContentPage
    {
        private readonly FriendDetailViewModel _viewModel = new FriendDetailViewModel();
        private UserInfo _userInfo;
        private string _accessToken;
        private int _viewChild = Constants.DefaultChildId;
        private bool _online = true;
        private string _filePath;

        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public FriendDetailPage(Friend friendItem)
        {
            _viewModel = new FriendDetailViewModel();
            InitializeComponent();
            _viewModel.CurrentFriendId = friendItem.FriendId;
            _viewModel.AccessLevel = friendItem.AccessLevel;
            _viewModel.Description = friendItem.Description;
            _viewModel.FriendSince = friendItem.FriendSince;
            _viewModel.FriendType = friendItem.Type;
            _viewModel.Context = friendItem.Context;
            _viewModel.Name = friendItem.Name;
            _viewModel.Notes = friendItem.Notes;
            _viewModel.Tags = friendItem.Tags;
            FriendImage.Source = friendItem.PictureLink;
            BindingContext = _viewModel;
            
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

            await CheckAccount();
            await Reload();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
            
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

        private async Task Reload()
        {
            _viewModel.IsBusy = true;
            await CheckAccount();
            _viewModel.CurrentFriend =
                await ProgenyService.GetFriend(_viewModel.CurrentFriendId, _accessToken);

            _viewModel.AccessLevel = _viewModel.CurrentFriend.AccessLevel;
            _viewModel.CurrentFriend.Progeny = _viewModel.Progeny = await ProgenyService.GetProgeny(_viewModel.CurrentFriend.ProgenyId);

            if (_viewModel.CurrentFriend.FriendSince.HasValue)
            {
                _viewModel.DateYear = _viewModel.CurrentFriend.FriendSince.Value.Year;
                _viewModel.DateMonth = _viewModel.CurrentFriend.FriendSince.Value.Month;
                _viewModel.DateDay = _viewModel.CurrentFriend.FriendSince.Value.Day;
            }

            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewModel.CurrentFriend.ProgenyId);
            if (_viewModel.UserAccessLevel == 0)
            {
                _viewModel.CanUserEditItems = true;
            }
            else
            {
                _viewModel.CanUserEditItems = false;
            }

            _viewModel.CurrentFriendId = _viewModel.CurrentFriend.FriendId;
            _viewModel.AccessLevel = _viewModel.CurrentFriend.AccessLevel;
            _viewModel.Name = _viewModel.CurrentFriend.Name;
            _viewModel.FriendSince = _viewModel.CurrentFriend.FriendSince;
            _viewModel.FriendType = _viewModel.CurrentFriend.Type;
            _viewModel.Context = _viewModel.CurrentFriend.Context;
            _viewModel.Notes = _viewModel.CurrentFriend.Notes;
            _viewModel.Tags = _viewModel.CurrentFriend.Tags;
            _viewModel.Notes = _viewModel.CurrentFriend.Notes;
            FriendImage.Source = _viewModel.CurrentFriend.PictureLink;

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
        
        private async void EditButton_OnClicked(object sender, EventArgs e)
        {
            if (_viewModel.EditMode)
            {
                _viewModel.EditMode = false;
                _viewModel.IsBusy = true;

                DateTime frnDate = new DateTime(_viewModel.DateYear, _viewModel.DateMonth, _viewModel.DateDay);
                _viewModel.CurrentFriend.FriendSince = frnDate;
                _viewModel.CurrentFriend.Name = _viewModel.Name;
                _viewModel.CurrentFriend.Type = FriendTypePicker?.SelectedIndex ?? 0;
                _viewModel.CurrentFriend.Context = ContextEntry?.Text ?? ""; ;
                _viewModel.CurrentFriend.Notes = _viewModel.Notes;
                _viewModel.CurrentFriend.Tags = TagsEntry?.Text ?? "";
                _viewModel.CurrentFriend.AccessLevel = _viewModel.AccessLevel;
                
                if (string.IsNullOrEmpty(_filePath) || !File.Exists(_filePath))
                {
                    _viewModel.CurrentFriend.PictureLink = "[KeepExistingLink]";
                }
                else
                {
                    // Upload photo file, get a reference to the image.
                    string pictureLink = await ProgenyService.UploadFriendPicture(_filePath);
                    if (pictureLink == "")
                    {
                        _viewModel.IsBusy = false;
                        // Todo: Show error
                        _viewModel.CurrentFriend.PictureLink = "[KeepExistingLink]";
                        return;
                    }
                    _viewModel.CurrentFriend.PictureLink = pictureLink;
                }
                // Save changes.
                Friend resulFriend = await ProgenyService.UpdateFriend(_viewModel.CurrentFriend);
                _viewModel.IsBusy = false;
                EditButton.Text = IconFont.CalendarEdit;
                if (resulFriend != null)  // Todo: Error message if update fails.
                {
                    MessageLabel.Text = "Friend Updated"; // Todo: Translate
                    MessageLabel.BackgroundColor = Color.DarkGreen;
                    MessageLabel.IsVisible = true;
                    await Reload();
                }
            }
            else
            {
                EditButton.Text = IconFont.ContentSave;

                _viewModel.EditMode = true;
                _viewModel.TagsAutoSuggestList = await ProgenyService.GetTagsAutoSuggestList(_viewModel.CurrentFriend.ProgenyId, 0);
                _viewModel.ContextAutoSuggestList = await ProgenyService.GetContextAutoSuggestList(_viewModel.CurrentFriend.ProgenyId, 0);
            }
        }

        private async void SelectImageButton_OnClicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                return;
            }

            var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                RotateImage = false,
                SaveMetaData = true
            });

            if (file == null)
            {
                return;
            }

            _filePath = file.Path;
            FriendImage.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();
                return stream;
            });
        }

        private async void CancelButton_OnClicked(object sender, EventArgs e)
        {
            EditButton.Text = IconFont.AccountEdit;
            _viewModel.EditMode = false;
            await Reload();
        }

        private void FriendDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            _viewModel.DateYear = FriendDatePicker.Date.Year;
            _viewModel.DateMonth = FriendDatePicker.Date.Month;
            _viewModel.DateDay = FriendDatePicker.Date.Day;
        }
        
        private async void DeleteButton_OnClickedButton_OnClicked(object sender, EventArgs e)
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string confirmTitle = resmgr.Value.GetString("DeleteFriend", ci);
            string confirmMessage = resmgr.Value.GetString("DeleteFriendMessage", ci) + " ? ";
            string yes = resmgr.Value.GetString("Yes", ci);
            string no = resmgr.Value.GetString("No", ci); ;
            bool confirmDelete = await DisplayAlert(confirmTitle, confirmMessage, yes, no);
            if (confirmDelete)
            {
                _viewModel.IsBusy = true;
                _viewModel.EditMode = false;
                Friend deletedFriend = await ProgenyService.DeleteFriend(_viewModel.CurrentFriend);
                if (deletedFriend.FriendId == 0)
                {
                    _viewModel.EditMode = false;
                    // Todo: Show success message

                }
                else
                {
                    _viewModel.EditMode = true;
                    // Todo: Show failed message
                }
                _viewModel.IsBusy = false;
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
                    newText = newText + e.ChosenSuggestion.ToString() + ", ";
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

        private void ContextEntry_OnTextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
                if (autoSuggestBox != null && autoSuggestBox.Text.Length > 0)
                {
                    List<string> filteredContexts = new List<string>();
                    foreach (string contextString in _viewModel.ContextAutoSuggestList)
                    {
                        if (contextString.ToUpper().Contains(autoSuggestBox.Text.Trim().ToUpper()))
                        {
                            filteredContexts.Add(contextString);
                        }
                    }
                    //Set the ItemsSource to be your filtered dataset
                    autoSuggestBox.ItemsSource = filteredContexts;
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

        private void ContextEntry_OnQuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
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

        private void ContextEntry_OnSuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
            // Set sender.Text. You can use e.SelectedItem to build your text string.
            if (autoSuggestBox != null)
            {
                autoSuggestBox.Text = e.SelectedItem.ToString();
            }
        }

        private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
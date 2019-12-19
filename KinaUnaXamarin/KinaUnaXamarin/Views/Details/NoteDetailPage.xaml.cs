using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using dotMorten.Xamarin.Forms;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels.Details;
using Plugin.Multilingual;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views.Details
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NoteDetailPage
    {
        private readonly NoteDetailViewModel _viewModel;
        private UserInfo _userInfo;
        private string _accessToken;
        private int _viewChild = Constants.DefaultChildId;
        private bool _online = true;
        private double _webViewHeight;

        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public NoteDetailPage(Note note)
        {
            _viewModel = new NoteDetailViewModel();
            InitializeComponent();
            
            _viewModel.CurrentNoteId = note.NoteId;
            _viewModel.NoteTitle = note.Title;
            _viewModel.Content = note.Content;
            _viewModel.Category = note.Category;
            _viewModel.Date = note.CreatedDate;
            _viewModel.Time = note.CreatedDate.TimeOfDay;
            _viewModel.AccessLevel = note.AccessLevel;
            
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

        private async Task Reload()
        {
            _viewModel.IsBusy = true;
            await CheckAccount();
            _viewModel.CurrentNote =
                await ProgenyService.GetNote(_viewModel.CurrentNoteId, _accessToken, _userInfo.Timezone);

            _viewModel.AccessLevel = _viewModel.CurrentNote.AccessLevel;
            _viewModel.CurrentNote.Progeny = _viewModel.Progeny = await ProgenyService.GetProgeny(_viewModel.CurrentNote.ProgenyId);
            _viewModel.Date = _viewModel.CurrentNote.CreatedDate;
            _viewModel.Time = _viewModel.CurrentNote.CreatedDate.TimeOfDay;
            _viewModel.DateYear = _viewModel.CurrentNote.CreatedDate.Year;
            _viewModel.DateMonth = _viewModel.CurrentNote.CreatedDate.Month;
            _viewModel.DateDay = _viewModel.CurrentNote.CreatedDate.Day;
            NoteDatePicker.Date = new DateTime(_viewModel.DateYear, _viewModel.DateMonth, _viewModel.DateDay);
            _viewModel.DateHours = _viewModel.CurrentNote.CreatedDate.Hour;
            _viewModel.DateMinutes = _viewModel.CurrentNote.CreatedDate.Minute;
            NoteTimePicker.Time = new TimeSpan(_viewModel.DateHours, _viewModel.DateMinutes, 0);

            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewModel.CurrentNote.ProgenyId);
            if (_viewModel.UserAccessLevel == 0)
            {
                _viewModel.CanUserEditItems = true;
            }
            else
            {
                _viewModel.CanUserEditItems = false;
            }

            _viewModel.CurrentNoteId = _viewModel.CurrentNote.NoteId;
            _viewModel.AccessLevel = _viewModel.CurrentNote.AccessLevel;
            _viewModel.NoteTitle = _viewModel.CurrentNote.Title;
            _viewModel.Content = _viewModel.CurrentNote.Content;
            _viewModel.Category = _viewModel.CurrentNote.Category;
            _viewModel.Date = _viewModel.CurrentNote.CreatedDate;
            
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

                DateTime noteDate = new DateTime(_viewModel.DateYear, _viewModel.DateMonth, _viewModel.DateDay, _viewModel.DateHours, _viewModel.DateMinutes, 0);
                _viewModel.CurrentNote.CreatedDate = noteDate;
                _viewModel.CurrentNote.Title = _viewModel.NoteTitle;
                string noteContent = await ContentWebView.EvaluateJavaScriptAsync("getContent()");
                noteContent = noteContent.Replace(@"\u003C", "<"); // Todo: Proper string encoding/decoding.
                _viewModel.CurrentNote.Content = noteContent;
                _viewModel.CurrentNote.Category = CategoryEntry.Text;
                _viewModel.CurrentNote.AccessLevel = _viewModel.AccessLevel;

                // Save changes.
                Note resultNote = await ProgenyService.UpdateNote(_viewModel.CurrentNote);
                _viewModel.IsBusy = false;
                EditButton.Text = IconFont.CalendarEdit;
                if (resultNote != null)  // Todo: Error message if update fails.
                {
                    MessageLabel.Text = "Note Updated"; // Todo: Translate
                    MessageLabel.BackgroundColor = Color.DarkGreen;
                    MessageLabel.IsVisible = true;
                    await Reload();
                }
            }
            else
            {
                EditButton.Text = IconFont.ContentSave;
                
                HtmlWebViewSource htmlSource = new HtmlWebViewSource();
                // htmlSource.BaseUrl = DependencyService.Get<IBaseUrl>().Get();
                htmlSource.Html = DependencyService.Get<IBaseUrl>().GetQuillHtml().Replace("!!Content!!", _viewModel.Content);
                
                ContentWebView.Source = htmlSource;
                
                _viewModel.EditMode = true;
                _viewModel.CategoryAutoSuggestList = await ProgenyService.GetCategoryAutoSuggestList(_viewModel.CurrentNote.ProgenyId, 0);
            }
        }

        private async void CancelButton_OnClicked(object sender, EventArgs e)
        {
            EditButton.Text = IconFont.CalendarEdit;
            _viewModel.EditMode = false;
            await Reload();
        }

        private void NoteDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            _viewModel.DateYear = NoteDatePicker.Date.Year;
            _viewModel.DateMonth = NoteDatePicker.Date.Month;
            _viewModel.DateDay = NoteDatePicker.Date.Day;
        }

        private void NoteTimePicker_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _viewModel.DateHours = NoteTimePicker.Time.Hours;
            _viewModel.DateMinutes = NoteTimePicker.Time.Minutes;
        }

        private async void DeleteButton_OnClickedButton_OnClicked(object sender, EventArgs e)
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string confirmTitle = resmgr.Value.GetString("DeleteNote", ci);
            string confirmMessage = resmgr.Value.GetString("DeleteNoteMessage", ci) + " ? ";
            string yes = resmgr.Value.GetString("Yes", ci);
            string no = resmgr.Value.GetString("No", ci);
            bool confirmDelete = await DisplayAlert(confirmTitle, confirmMessage, yes, no);
            if (confirmDelete)
            {
                _viewModel.IsBusy = true;
                _viewModel.EditMode = false;
                Note deleteNote = await ProgenyService.DeleteNote(_viewModel.CurrentNote);
                if (deleteNote.NoteId == 0)
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

        private void CategoryEntry_OnTextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
                if (autoSuggestBox != null && autoSuggestBox.Text.Length > 0)
                {
                    List<string> filteredCategories = new List<string>();
                    foreach (string categoryString in _viewModel.CategoryAutoSuggestList)
                    {
                        if (categoryString.ToUpper().Contains(autoSuggestBox.Text.Trim().ToUpper()))
                        {
                            filteredCategories.Add(categoryString);
                        }
                    }
                    //Set the ItemsSource to be your filtered dataset
                    autoSuggestBox.ItemsSource = filteredCategories;
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

        private void CategoryEntry_OnQuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
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

        private void CategoryEntry_OnSuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            AutoSuggestBox autoSuggestBox = sender as AutoSuggestBox;
            // Set sender.Text. You can use e.SelectedItem to build your text string.
            if (autoSuggestBox != null)
            {
                autoSuggestBox.Text = e.SelectedItem.ToString();
            }
        }

        private async void ContentWebView_OnFocused(object sender, FocusEventArgs e)
        {
            string heightString = await ContentWebView.EvaluateJavaScriptAsync("getHeight()");
            
            bool heightParsed = double.TryParse(heightString, out _webViewHeight);
            if (heightParsed)
            {
                ContentWebView.HeightRequest = _webViewHeight + 25.0;
            }
        }

        private async void ContentWebView_OnNavigating(object sender, WebNavigatingEventArgs e)
        {
            e.Cancel = true;
            string heightString = await ContentWebView.EvaluateJavaScriptAsync("getHeight()");

            bool heightParsed = double.TryParse(heightString, out _webViewHeight);
            if (heightParsed)
            {
                ContentWebView.HeightRequest = _webViewHeight + 25.0;
            }

        }

        private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
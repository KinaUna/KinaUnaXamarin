using System;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using Plugin.Multilingual;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VocabularyDetailPage : ContentPage
    {
        private readonly VocabularyDetailViewModel _viewModel = new VocabularyDetailViewModel();
        private UserInfo _userInfo;
        private string _accessToken;
        private int _viewChild = Constants.DefaultChildId;
        private bool _online = true;

        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public VocabularyDetailPage(VocabularyItem vocabularyItem)
        {
            
            InitializeComponent();
            _viewModel.CurrentVocabularyItemId = vocabularyItem.WordId;
            _viewModel.Word = vocabularyItem.Word;
            _viewModel.AccessLevel = vocabularyItem.AccessLevel;
            _viewModel.Language = vocabularyItem.Language;
            _viewModel.SoundsLike = vocabularyItem.SoundsLike;
            _viewModel.Description = vocabularyItem.Description;
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
            _viewModel.CurrentVocabularyItem =
                await ProgenyService.GetVocabularyItem(_viewModel.CurrentVocabularyItemId, _accessToken, _userInfo.Timezone);

            _viewModel.AccessLevel = _viewModel.CurrentVocabularyItem.AccessLevel;
            _viewModel.CurrentVocabularyItem.Progeny = await ProgenyService.GetProgeny(_viewModel.CurrentVocabularyItem.ProgenyId);
            if (_viewModel.CurrentVocabularyItem.Date.HasValue)
            {
                _viewModel.DateYear = _viewModel.CurrentVocabularyItem.Date.Value.Year;
                _viewModel.DateMonth = _viewModel.CurrentVocabularyItem.Date.Value.Month;
                _viewModel.DateDay = _viewModel.CurrentVocabularyItem.Date.Value.Day;

                WordDatePicker.Date = new DateTime(_viewModel.DateYear, _viewModel.DateMonth, _viewModel.DateDay);
            }

            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewModel.CurrentVocabularyItem.ProgenyId);
            if (_viewModel.UserAccessLevel == 0)
            {
                _viewModel.CanUserEditItems = true;
            }
            else
            {
                _viewModel.CanUserEditItems = false;
            }

            _viewModel.CurrentVocabularyItemId = _viewModel.CurrentVocabularyItem.WordId;
            _viewModel.AccessLevel = _viewModel.CurrentVocabularyItem.AccessLevel;
            _viewModel.Word = _viewModel.CurrentVocabularyItem.Word;
            _viewModel.SoundsLike = _viewModel.CurrentVocabularyItem.SoundsLike;
            _viewModel.Description = _viewModel.CurrentVocabularyItem.Description;
            _viewModel.Language = _viewModel.CurrentVocabularyItem.Language;
            
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

                DateTime wordDate = new DateTime(_viewModel.DateYear, _viewModel.DateMonth, _viewModel.DateDay);
                _viewModel.CurrentVocabularyItem.Date = wordDate;
                _viewModel.CurrentVocabularyItem.Word = _viewModel.Word;
                _viewModel.CurrentVocabularyItem.SoundsLike = _viewModel.SoundsLike;
                _viewModel.CurrentVocabularyItem.Description = _viewModel.Description;
                _viewModel.CurrentVocabularyItem.Language = _viewModel.Language;
                _viewModel.CurrentVocabularyItem.AccessLevel = _viewModel.AccessLevel;

                // Save changes.
                VocabularyItem resultVocabularyItem = await ProgenyService.UpdateVocabularyItem(_viewModel.CurrentVocabularyItem);
                _viewModel.IsBusy = false;
                EditButton.Text = IconFont.CalendarEdit;
                if (resultVocabularyItem != null)  // Todo: Error message if update fails.
                {
                    MessageLabel.Text = "Word Updated"; // Todo: Translate
                    MessageLabel.BackgroundColor = Color.DarkGreen;
                    MessageLabel.IsVisible = true;
                    await Reload();
                }
            }
            else
            {
                EditButton.Text = IconFont.ContentSave;

                _viewModel.EditMode = true;
            }
        }

        private async void CancelButton_OnClicked(object sender, EventArgs e)
        {
            EditButton.Text = IconFont.CalendarEdit;
            _viewModel.EditMode = false;
            await Reload();
        }

        private void WordDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            _viewModel.DateYear = WordDatePicker.Date.Year;
            _viewModel.DateMonth = WordDatePicker.Date.Month;
            _viewModel.DateDay = WordDatePicker.Date.Day;
        }

        private async void DeleteButton_OnClickedButton_OnClicked(object sender, EventArgs e)
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string confirmTitle = resmgr.Value.GetString("DeleteWord", ci);
            string confirmMessage = resmgr.Value.GetString("DeleteWordMessage", ci) + " ? ";
            string yes = resmgr.Value.GetString("Yes", ci);
            string no = resmgr.Value.GetString("No", ci); ;
            bool confirmDelete = await DisplayAlert(confirmTitle, confirmMessage, yes, no);
            if (confirmDelete)
            {
                _viewModel.IsBusy = true;
                _viewModel.EditMode = false;
                VocabularyItem deleteVocabularyItem = await ProgenyService.DeleteVocabularyItem(_viewModel.CurrentVocabularyItem);
                if (deleteVocabularyItem.WordId == 0)
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
    }
}
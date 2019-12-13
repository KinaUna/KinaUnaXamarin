using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotesPage : ContentPage
    {
        private int _viewChild = Constants.DefaultChildId;
        private UserInfo _userInfo;
        private NotesViewModel _viewModel;
        private string _accessToken;
        private bool _reload = true;
        private bool _online = true;
        private double _screenWidth;
        private double _screenHeight;

        public NotesPage()
        {
            InitializeComponent();
            _viewModel = new NotesViewModel();
            _userInfo = OfflineDefaultData.DefaultUserInfo;
            ContainerGrid.BindingContext = _viewModel;
            BindingContext = _viewModel;

            MessagingCenter.Subscribe<SelectProgenyPage>(this, "Reload", async (sender) =>
            {
                _viewModel.PageNumber = 1;
                await Reload();
            });

            MessagingCenter.Subscribe<AccountViewModel>(this, "Reload", async (sender) =>
            {
                _viewModel.PageNumber = 1;
                await Reload();
            });
        }

        //protected override async void OnSizeAllocated(double width, double height)
        //{

        //    if (_screenWidth != width || _screenHeight != height)
        //    {
        //        _screenWidth = width;
        //        _screenHeight = height;
        //        if (Device.RuntimePlatform == Device.iOS)
        //        {
        //            _viewModel.NoteItems.Clear();
        //        }
        //        if (Device.RuntimePlatform == Device.iOS)
        //        {
        //            await UpdateNotes();
        //        }
        //    }
        //    base.OnSizeAllocated(width, height); //must be called
            
        //}

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            NotesListView.SelectedItem = null;

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

            if (_reload)
            {
                await Reload();
            }

            _reload = false;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }
        
        private async Task Reload()
        {
            _viewModel.IsBusy = true;
            await CheckAccount();
            
            await UpdateNotes();
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

            List<Progeny> progenyList = await ProgenyService.GetProgenyList(userEmail);
            _viewModel.ProgenyCollection.Clear();
            _viewModel.CanUserAddItems = false;
            foreach (Progeny prog in progenyList)
            {
                _viewModel.ProgenyCollection.Add(prog);
                if (prog.Admins.ToUpper().Contains(_userInfo.UserEmail.ToUpper()))
                {
                    _viewModel.CanUserAddItems = true;
                }
            }

            _viewModel.UserAccessLevel = await ProgenyService.GetAccessLevel(_viewChild);
        }

        private async Task UpdateNotes()
        {
            _viewModel.IsBusy = true;
            if (_viewModel.PageNumber < 1)
            {
                _viewModel.PageNumber = 1;
            }
            _viewModel.NoteItems.Clear();
            NotesListPage notesList = await ProgenyService.GetNotesPage(_viewModel.PageNumber, 5, _viewChild, _viewModel.UserAccessLevel, _userInfo.Timezone, 1);

            if (notesList.NotesList != null && notesList.NotesList.Count > 0)
            {
                foreach (Note note in notesList.NotesList)
                {
                    
                    note.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(note.CreatedDate,
                        TimeZoneInfo.FindSystemTimeZoneById(_userInfo.Timezone));
                }
                Device.BeginInvokeOnMainThread(() =>
                {
                    _viewModel.NoteItems.ReplaceRange(notesList.NotesList);
                });
                _viewModel.PageNumber = notesList.PageNumber;
                _viewModel.PageCount = notesList.TotalPages;
                
                Note firstNote = _viewModel.NoteItems.FirstOrDefault();
                if (firstNote != null)
                {
                    NotesListView.ScrollTo(firstNote, ScrollToPosition.MakeVisible, true);
                }
            }
            
            _viewModel.IsBusy = false;
        }

        private async void NewerButton_OnClicked(object sender, EventArgs e)
        {

            _viewModel.PageNumber--;
            if (_viewModel.PageNumber < 1)
            {
                _viewModel.PageNumber = _viewModel.PageCount;
            }
            await UpdateNotes();
        }

        private async void OlderButton_OnClicked(object sender, EventArgs e)
        {
            _viewModel.PageNumber++;
            if (_viewModel.PageNumber > _viewModel.PageCount)
            {
                _viewModel.PageNumber = 1;
            }
            await UpdateNotes();
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

        private async void ReloadToolbarButton_OnClicked(object sender, EventArgs e)
        {
            await Reload();
        }

        private async void AddItemToolbarButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PushModalAsync(new AddItemPage());
        }
        
        private async void NotesListView_OnItemSelected(object sender, SelectedItemChangedEventArgs selectedItemChangedEventArgs)
        {
            if (NotesListView.SelectedItem is Note noteItem)
            {
                NoteDetailPage noteDetailPage = new NoteDetailPage(noteItem);
                NotesListView.SelectedItem = null;
                await Shell.Current.Navigation.PushModalAsync(noteDetailPage);
            }
        }
    }
}
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
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views.AddItem
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddNotePage
    {
        private readonly AddNoteViewModel _viewModel;
        private bool _online = true;
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));
        private double _webViewHeight;

        public AddNotePage()
        {
            _viewModel = new AddNoteViewModel();
            _viewModel.NoteItem = new Note();
            _viewModel.NoteItem.CreatedDate = _viewModel.Date = DateTime.Now;
            _viewModel.Time = DateTime.Now.TimeOfDay;
            _viewModel.NoteItem.AccessLevel = 0;
            
            InitializeComponent();
            BindingContext = _viewModel;
            ProgenyCollectionView.ItemsSource = _viewModel.ProgenyCollection;
            HtmlWebViewSource htmlSource = new HtmlWebViewSource();
            htmlSource.BaseUrl = DependencyService.Get<IBaseUrl>().Get();
            htmlSource.Html = DependencyService.Get<IBaseUrl>().GetQuillHtml().Replace("!!Content!!", "");

            ContentWebView.Source = htmlSource;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            var networkAccess = Connectivity.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess)
            {
                _viewModel.Online = true;
                OfflineStackLayout.IsVisible = false;
            }
            else
            {
                _viewModel.Online = false;
                OfflineStackLayout.IsVisible = true;
            }

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
                    _viewModel.CategoryAutoSuggestList = await ProgenyService.GetCategoryAutoSuggestList(viewProgeny.Id, 0);
                }
                else
                {
                    ProgenyCollectionView.SelectedItem = _viewModel.ProgenyCollection[0];
                }
            }
            _viewModel.NoteItem.ProgenyId = ((Progeny)ProgenyCollectionView.SelectedItem).Id;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var networkAccess = e.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            if (internetAccess != _online)
            {
                _viewModel.Online = false;
                OfflineStackLayout.IsVisible = true;
                SaveNoteButton.IsEnabled = false;
            }
            else
            {
                _viewModel.Online = true;
                OfflineStackLayout.IsVisible = false;
                SaveNoteButton.IsEnabled = true;
            }
        }


        private async void CancelNoteButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        private async void SaveNoteButton_OnClicked(object sender, EventArgs e)
        {
            if (ProgenyCollectionView.SelectedItem is Progeny progeny)
            {
                _viewModel.IsBusy = true;
                _viewModel.IsSaving = true;
                Note saveNote = new Note();
                saveNote.ProgenyId = progeny.Id;
                saveNote.AccessLevel = _viewModel.AccessLevel;
                saveNote.Progeny = progeny;
                DateTime noteTime = new DateTime(NoteDatePicker.Date.Year, NoteDatePicker.Date.Month, NoteDatePicker.Date.Day, NoteTimePicker.Time.Hours, NoteTimePicker.Time.Minutes, 0);
                saveNote.CreatedDate = noteTime;
                
                string userEmail = await UserService.GetUserEmail();
                UserInfo userinfo = await UserService.GetUserInfo(userEmail);
                saveNote.Owner = userinfo.UserId;
                saveNote.Title = TitleEntry.Text;
                saveNote.Category = CategoryEntry.Text;
                string noteContent = await ContentWebView.EvaluateJavaScriptAsync("getContent()");
                noteContent = noteContent.Replace(@"\u003C", "<");  // Todo: Proper string encoding/decoding.
                saveNote.Content = noteContent;
                // saveNote.Content = ContentEditor.Text;
                
                if (ProgenyService.Online())
                {
                    saveNote = await ProgenyService.SaveNote(saveNote);
                    _viewModel.IsBusy = false;
                    _viewModel.IsSaving = false;
                    if (saveNote.NoteId == 0)
                    {
                        var ci = CrossMultilingual.Current.CurrentCultureInfo;
                        ErrorLabel.Text = resmgr.Value.GetString("ErrorNoteNotSaved", ci);
                        ErrorLabel.BackgroundColor = Color.Red;

                    }
                    else
                    {
                        var ci = CrossMultilingual.Current.CurrentCultureInfo;
                        ErrorLabel.Text = resmgr.Value.GetString("NoteSaved", ci) + saveNote.NoteId;
                        ErrorLabel.BackgroundColor = Color.Green;
                        SaveNoteButton.IsVisible = false;
                        CancelNoteButton.Text = "Ok";
                        CancelNoteButton.BackgroundColor = Color.FromHex("#4caf50");
                        await Shell.Current.Navigation.PopModalAsync();
                    }
                }
                else
                {
                    // Todo: Translate message.
                    ErrorLabel.Text = $"Error: No internet connection. Measurement for {progeny.NickName} was not saved. Try again later.";
                    ErrorLabel.BackgroundColor = Color.Red;
                }

                _viewModel.IsBusy = false;
                _viewModel.IsSaving = false;
                ErrorLabel.IsVisible = true;
            }
        }

        private async void ProgenyCollectionView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProgenyCollectionView.SelectedItem is Progeny viewProgeny)
            {
                _viewModel.CategoryAutoSuggestList = await ProgenyService.GetCategoryAutoSuggestList(viewProgeny.Id, 0);
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
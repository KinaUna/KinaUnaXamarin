using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using dotMorten.Xamarin.Forms;
using KinaUnaXamarin.Helpers;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels.AddItem;
using Plugin.Media;
using Plugin.Multilingual;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views.AddItem
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddFriendPage : ContentPage
    {
        private readonly AddFriendViewModel _viewModel;
        private bool _online = true;
        private string _filePath;
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public AddFriendPage()
        {
            InitializeComponent();
            _viewModel = new AddFriendViewModel();
            BindingContext = _viewModel;
            ProgenyCollectionView.ItemsSource = _viewModel.ProgenyCollection;
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
                Progeny viewProgeny = _viewModel.ProgenyCollection.SingleOrDefault(p => p.Id == viewChild);
                if (viewProgeny != null)
                {
                    ProgenyCollectionView.SelectedItem =
                        _viewModel.ProgenyCollection.SingleOrDefault(p => p.Id == viewChild);
                    ProgenyCollectionView.ScrollTo(ProgenyCollectionView.SelectedItem);
                    _viewModel.ContextAutoSuggestList = await ProgenyService.GetContextAutoSuggestList(viewProgeny.Id, 0);
                    _viewModel.TagsAutoSuggestList = await ProgenyService.GetTagsAutoSuggestList(viewProgeny.Id, 0);
                }
                else
                {
                    ProgenyCollectionView.SelectedItem = _viewModel.ProgenyCollection[0];
                }
            }

            _viewModel.FriendType = 0;
            _viewModel.AccessLevel = 0;
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
                SaveFriendButton.IsEnabled = false;
            }
            else
            {
                _viewModel.Online = true;
                OfflineStackLayout.IsVisible = false;
                SaveFriendButton.IsEnabled = true;
            }
        }

        private async void SaveFriendButton_OnClicked(object sender, EventArgs e)
        {
            Progeny progeny = ProgenyCollectionView.SelectedItem as Progeny;
            if (progeny == null)
            {
                return;
            }

            SaveFriendButton.IsEnabled = false;
            CancelFriendButton.IsEnabled = false;
            _viewModel.IsBusy = true;


            Friend friend = new Friend();
            
            friend.ProgenyId = progeny.Id;
            friend.AccessLevel = _viewModel?.AccessLevel ?? 0;
            string userEmail = await UserService.GetUserEmail();
            UserInfo userinfo = await UserService.GetUserInfo(userEmail);
            friend.Author = userinfo.UserId;
            friend.Context = ContextEntry?.Text ?? "";
            friend.Description = DescriptionEditor?.Text ?? "";
            friend.FriendAddedDate = DateTime.Now;
            friend.FriendSince = FriendSinceDatePicker.Date;
            friend.Name = NameEntry.Text;
            friend.Notes = NotesEditor?.Text ?? "";
            friend.Tags = TagsEntry?.Text ?? "";
            friend.Type = FriendTypePicker?.SelectedIndex ?? 0;

            if (string.IsNullOrEmpty(_filePath) || !File.Exists(_filePath))
            {
                friend.PictureLink = Constants.DefaultPictureLink;
            }
            else
            {
                // Upload photo file, get a reference to the image.
                string pictureLink = await ProgenyService.UploadFriendPicture(_filePath);
                if (pictureLink == "")
                {
                    SaveFriendButton.IsEnabled = true;
                    CancelFriendButton.IsEnabled = true;
                    _viewModel.IsBusy = false;
                    // Todo: Show error
                    return;
                }
                friend.PictureLink = pictureLink;
            }
            
            
            // Upload Friend object to add it to the database.
            
            Friend newFriend = await ProgenyService.SaveFriend(friend);
            
            ErrorLabel.IsVisible = true;
            if (newFriend.FriendId == 0)
            {
                var ci = CrossMultilingual.Current.CurrentCultureInfo;
                ErrorLabel.Text = resmgr.Value.GetString("ErrorFriendNotSaved", ci);
                ErrorLabel.BackgroundColor = Color.Red;
                SaveFriendButton.IsEnabled = true;
                CancelFriendButton.IsEnabled = true;

            }
            else
            {
                var ci = CrossMultilingual.Current.CurrentCultureInfo;
                ErrorLabel.Text = resmgr.Value.GetString("FriendSaved", ci) + newFriend.FriendId;
                ErrorLabel.BackgroundColor = Color.Green;
                SaveFriendButton.IsVisible = false;
                CancelFriendButton.Text = "Ok";
                CancelFriendButton.BackgroundColor = Color.FromHex("#4caf50");
                CancelFriendButton.IsEnabled = true;
            }

            _viewModel.IsBusy = false;
        }

        private async void CancelFriendButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
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
            UploadImage.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();
                return stream;
            });
        }

        private void NameEntry_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (NameEntry.Text.Length >= 1 && _viewModel.Online)
            {
                SaveFriendButton.IsEnabled = true;
            }
            else
            {
                SaveFriendButton.IsEnabled = false;
            }
        }

        private async void ProgenyCollectionView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Progeny viewProgeny = ProgenyCollectionView.SelectedItem as Progeny;
            if (viewProgeny != null)
            {
                _viewModel.TagsAutoSuggestList = await ProgenyService.GetTagsAutoSuggestList(viewProgeny.Id, 0);
                _viewModel.ContextAutoSuggestList = await ProgenyService.GetContextAutoSuggestList(viewProgeny.Id, 0);
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

        private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
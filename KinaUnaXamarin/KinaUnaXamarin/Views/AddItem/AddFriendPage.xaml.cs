using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
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
    }
}
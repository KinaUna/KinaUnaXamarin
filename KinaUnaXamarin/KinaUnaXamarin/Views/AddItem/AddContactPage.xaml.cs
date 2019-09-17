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
    public partial class AddContactPage : ContentPage
    {
        private readonly AddContactViewModel _viewModel;
        private bool _online = true;
        private string _filePath;
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public AddContactPage()
        {
            InitializeComponent();
            _viewModel = new AddContactViewModel();
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
                SaveContactButton.IsEnabled = false;
            }
            else
            {
                _viewModel.Online = true;
                OfflineStackLayout.IsVisible = false;
                SaveContactButton.IsEnabled = true;
            }
        }

        private async void SaveContactButton_OnClicked(object sender, EventArgs e)
        {
            Progeny progeny = ProgenyCollectionView.SelectedItem as Progeny;
            if (progeny == null)
            {
                return;
            }

            SaveContactButton.IsEnabled = false;
            CancelContactButton.IsEnabled = false;
            _viewModel.IsBusy = true;


            Contact contact = new Contact();
            
            contact.ProgenyId = progeny.Id;
            contact.AccessLevel = _viewModel?.AccessLevel ?? 0;
            string userEmail = await UserService.GetUserEmail();
            UserInfo userinfo = await UserService.GetUserInfo(userEmail);
            contact.Author = userinfo.UserId;
            contact.FirstName = FirstNameEntry?.Text ?? "";
            contact.MiddleName = MiddleNameEntry?.Text ?? "";
            contact.LastName = LastNameEntry?.Text ?? "";
            contact.DisplayName = DisplayNameEntry?.Text ?? "";
            contact.AddressIdNumber = 0; // Todo: Save address object.
            contact.Email1 = Email1Entry?.Text ?? "";
            contact.Email2 = Email2Entry?.Text ?? "";
            contact.PhoneNumber = PhoneNumberEntry?.Text ?? "";
            contact.MobileNumber = MobileNumberEntry?.Text ?? "";
            contact.Website = WebsiteEntry?.Text ?? "";
            contact.Context = ContextEntry?.Text ?? "";
            contact.Notes = NotesEditor?.Text ?? "";
            contact.Tags = TagsEntry?.Text ?? "";
            contact.Active = true;
            
            if (string.IsNullOrEmpty(_filePath) || !File.Exists(_filePath))
            {
                contact.PictureLink = Constants.DefaultPictureLink;
            }
            else
            {
                // Upload photo file, get a reference to the image.
                string pictureLink = await ProgenyService.UploadContactPicture(_filePath);
                if (pictureLink == "")
                {
                    SaveContactButton.IsEnabled = true;
                    CancelContactButton.IsEnabled = true;
                    _viewModel.IsBusy = false;
                    // Todo: Show error
                    return;
                }
                contact.PictureLink = pictureLink;
            }
            
            
            Address address = new Address();
            address.AddressLine1 = AddressLine1Entry.Text;
            address.AddressLine2 = AddressLine2Entry.Text;
            address.City = CityEntry.Text;
            address.Country = CountryEntry.Text;
            address.PostalCode = PostalCodeEntry.Text;
            address.State = StateEntry.Text;

            contact.Address = address;

            // Upload Contact object to add it to the database.
            Contact newContact = await ProgenyService.SaveContact(contact);
            
            ErrorLabel.IsVisible = true;
            if (newContact.ContactId == 0)
            {
                var ci = CrossMultilingual.Current.CurrentCultureInfo;
                ErrorLabel.Text = resmgr.Value.GetString("ErrorContactNotSaved", ci);
                ErrorLabel.BackgroundColor = Color.Red;
                SaveContactButton.IsEnabled = true;
                CancelContactButton.IsEnabled = true;

            }
            else
            {
                var ci = CrossMultilingual.Current.CurrentCultureInfo;
                ErrorLabel.Text = resmgr.Value.GetString("ContactSaved", ci) + newContact.ContactId;
                ErrorLabel.BackgroundColor = Color.Green;
                SaveContactButton.IsVisible = false;
                CancelContactButton.Text = "Ok";
                CancelContactButton.BackgroundColor = Color.FromHex("#4caf50");
                CancelContactButton.IsEnabled = true;
            }

            _viewModel.IsBusy = false;
        }

        private async void CancelContactButton_OnClicked(object sender, EventArgs e)
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
            if (DisplayNameEntry.Text.Length + FirstNameEntry.Text.Length + MiddleNameEntry.Text.Length + LastNameEntry.Text.Length >= 1 && _viewModel.Online)
            {
                SaveContactButton.IsEnabled = true;
            }
            else
            {
                SaveContactButton.IsEnabled = false;
            }
        }
    }
}
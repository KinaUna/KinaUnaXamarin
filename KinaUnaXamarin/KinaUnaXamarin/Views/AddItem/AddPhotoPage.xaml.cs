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
    public partial class AddPhotoPage : ContentPage
    {
        private readonly AddPhotoViewModel _addPhotoViewModel;
        private string _filePath;
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public AddPhotoPage()
        {
            InitializeComponent();
            _addPhotoViewModel = new AddPhotoViewModel();
            BindingContext = _addPhotoViewModel;
            ProgenyCollectionView.ItemsSource = _addPhotoViewModel.ProgenyCollection;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            List<Progeny> progenyList = await ProgenyService.GetProgenyAdminList();
            if (progenyList.Any())
            {
                foreach (Progeny progeny in progenyList)
                {
                    _addPhotoViewModel.ProgenyCollection.Add(progeny);
                }
            }
            string userviewchild = await SecureStorage.GetAsync(Constants.UserViewChildKey);
            bool viewchildParsed = int.TryParse(userviewchild, out int viewChild);
            ProgenyCollectionView.SelectedItem =
                _addPhotoViewModel.ProgenyCollection.SingleOrDefault(p => p.Id == viewChild);
            ProgenyCollectionView.ScrollTo(ProgenyCollectionView.SelectedItem);
        }

        private async void SavePhotoButton_OnClicked(object sender, EventArgs e)
        {
            Progeny progeny = ProgenyCollectionView.SelectedItem as Progeny;
            if (string.IsNullOrEmpty(_filePath) || progeny == null)
            {
                return;
            }

            if (!File.Exists(_filePath))
            {
                return;
            }

            SavePhotoButton.IsEnabled = false;
            CancelPhotoButton.IsEnabled = false;
            _addPhotoViewModel.IsBusy = true;

            // Upload photo file, get a reference to the image.
            string pictureLink = await ProgenyService.UploadPictureFile(progeny.Id, _filePath);
            if (pictureLink == "")
            {
                SavePhotoButton.IsEnabled = true;
                CancelPhotoButton.IsEnabled = true;
                _addPhotoViewModel.IsBusy = false;
                return;
            }
            // Upload Picture object to add it to the database.
            Picture picture = new Picture();
            picture.PictureLink = pictureLink;
            picture.ProgenyId = progeny.Id;
            picture.AccessLevel = _addPhotoViewModel.AccessLevel;
            string userEmail = await UserService.GetUserEmail();
            UserInfo userinfo = await UserService.GetUserInfo(userEmail);
            picture.Author = userinfo.UserId;
            picture.Owners = userEmail;
            picture.TimeZone = userinfo.Timezone;

            Picture newPicture = await ProgenyService.SavePicture(picture);
            if (newPicture.PictureId != 0)
            {
                TimeLineItem tItem = new TimeLineItem();
                tItem.ProgenyId = newPicture.ProgenyId;
                tItem.AccessLevel = newPicture.AccessLevel;
                tItem.ItemType = (int)KinaUnaTypes.TimeLineType.Photo;
                tItem.ItemId = newPicture.PictureId.ToString();
                tItem.CreatedBy = userinfo.UserId;
                tItem.CreatedTime = DateTime.UtcNow;
                if (newPicture.PictureTime.HasValue)
                {
                    tItem.ProgenyTime = newPicture.PictureTime.Value;
                }
                else
                {
                    tItem.ProgenyTime = DateTime.UtcNow;
                }

                tItem = await ProgenyService.SaveTimeLineItem(tItem);
            }

            ErrorLabel.IsVisible = true;
            if (newPicture.PictureId == 0)
            {
                var ci = CrossMultilingual.Current.CurrentCultureInfo;
                ErrorLabel.Text = resmgr.Value.GetString("ErrorPhotoNotSaved", ci);
                ErrorLabel.BackgroundColor = Color.Red;
                SavePhotoButton.IsEnabled = true;
                CancelPhotoButton.IsEnabled = true;

            }
            else
            {
                var ci = CrossMultilingual.Current.CurrentCultureInfo;
                ErrorLabel.Text = resmgr.Value.GetString("PhotoSaved", ci) + newPicture.PictureId;
                ErrorLabel.BackgroundColor = Color.Green;
                SavePhotoButton.IsVisible = false;
                CancelPhotoButton.Text = "Ok";
                CancelPhotoButton.BackgroundColor = Color.FromHex("#4caf50");
                CancelPhotoButton.IsEnabled = true;
            }

            _addPhotoViewModel.IsBusy = false;
        }

        private async void CancelPhotoButton_OnClicked(object sender, EventArgs e)
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

            SavePhotoButton.IsEnabled = true;
        }
    }
}
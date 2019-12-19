using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using KinaUnaXamarin.Extensions;
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
    public partial class AddUserPage
    {
        private readonly AddUserViewModel _addUserViewModel;
        const string ResourceId = "KinaUnaXamarin.Resources.Translations";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        public AddUserPage()
        {
            InitializeComponent();
            _addUserViewModel = new AddUserViewModel();
            BindingContext = _addUserViewModel;
            ProgenyCollectionView.ItemsSource = _addUserViewModel.ProgenyCollection;
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
                    _addUserViewModel.ProgenyCollection.Add(progeny);
                }

                string userviewchild = await SecureStorage.GetAsync(Constants.UserViewChildKey);
                bool viewchildParsed = int.TryParse(userviewchild, out int viewChild);
                Progeny viewProgeny = new Progeny();
                if (viewchildParsed)
                {
                    viewProgeny = _addUserViewModel.ProgenyCollection.SingleOrDefault(p => p.Id == viewChild);
                }
                
                if (viewProgeny != null)
                {
                    ProgenyCollectionView.SelectedItem =
                        _addUserViewModel.ProgenyCollection.SingleOrDefault(p => p.Id == viewChild);
                    ProgenyCollectionView.ScrollTo(ProgenyCollectionView.SelectedItem);
                }
                else
                {
                    ProgenyCollectionView.SelectedItem = _addUserViewModel.ProgenyCollection[0];
                }
            }
        }

        private async void SaveUserButton_OnClicked(object sender, EventArgs e)
        {
            Progeny progeny = ProgenyCollectionView.SelectedItem as Progeny;
            if (progeny != null)
            {
                SaveUserButton.IsEnabled = false;
                CancelUserButton.IsEnabled = false;
                _addUserViewModel.IsBusy = true;

                UserAccess userAccess = new UserAccess();
                userAccess.Progeny = progeny;
                userAccess.ProgenyId = progeny.Id;
                userAccess.UserId = EmailEntry.Text;
                userAccess.AccessLevel = _addUserViewModel.AccessLevel;

                UserAccess newUserAccess = await ProgenyService.AddUser(userAccess);


                MessageLabel.IsVisible = true;
                if (newUserAccess.AccessId == 0)
                {
                    var ci = CrossMultilingual.Current.CurrentCultureInfo;
                    MessageLabel.Text = resmgr.Value.GetString("ErrorUserNotSaved", ci);
                    MessageLabel.BackgroundColor = Color.Red;
                    SaveUserButton.IsEnabled = true;
                    CancelUserButton.IsEnabled = true;

                }
                else
                {
                    var ci = CrossMultilingual.Current.CurrentCultureInfo;
                    MessageLabel.Text = resmgr.Value.GetString("UserSaved", ci) + newUserAccess.AccessId;
                    MessageLabel.BackgroundColor = Color.Green;
                    SaveUserButton.IsVisible = false;
                    CancelUserButton.Text = "Ok";
                    CancelUserButton.BackgroundColor = Color.FromHex("#4caf50");
                    CancelUserButton.IsEnabled = true;
                }

                _addUserViewModel.IsBusy = false;
            }
        }

        private async void CancelUserButton_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        private void EmailEntry_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SaveUserButton.IsEnabled = false;
            if (EmailEntry.Text.IsValidEmail())
            {
                SaveUserButton.IsEnabled = true;
            }
        }

        private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
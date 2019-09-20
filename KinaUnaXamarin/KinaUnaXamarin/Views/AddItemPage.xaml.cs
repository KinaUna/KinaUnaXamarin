using System.Collections.Generic;
using System.Linq;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using KinaUnaXamarin.Views.AddItem;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddItemPage : ContentPage
    {
        private bool _online = true;
        private readonly AddItemViewModel _addItemModel;
        public AddItemPage()
        {
            InitializeComponent();
            _addItemModel = new AddItemViewModel();
            AddItemListCollectionView.ItemsSource = _addItemModel.ItemList;
            
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

            string accessToken = await UserService.GetAuthAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }
            List<Progeny> progenyList = await ProgenyService.GetProgenyAdminList();
            if (progenyList.Any())
            {
                _addItemModel.CanAddItems = true;
            }
            else
            {
                _addItemModel.CanAddItems = false;
            }
            _addItemModel.UpdateItemList();
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
                OfflineStackLayout.IsVisible = true;
            }
            else
            {
                OfflineStackLayout.IsVisible = false;
            }
        }

        private void AddItemListCollectionView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AddItemListCollectionView.SelectedItem is AddItemModel model)
            {
                // Reset the selected item.
                AddItemListCollectionView.SelectedItem = null;
                if (model.ItemType == (int)KinaUnaTypes.TimeLineType.Sleep)
                {
                    Shell.Current.Navigation.PushModalAsync(new AddSleepPage());
                }

                if (model.ItemType == (int)KinaUnaTypes.TimeLineType.Photo)
                {
                    Shell.Current.Navigation.PushModalAsync(new AddPhotoPage());
                }

                if (model.ItemType == (int)KinaUnaTypes.TimeLineType.Video)
                {
                    Shell.Current.Navigation.PushModalAsync(new AddVideoPage());
                }

                if (model.ItemType == (int)KinaUnaTypes.TimeLineType.Friend)
                {
                    Shell.Current.Navigation.PushModalAsync(new AddFriendPage());
                }

                if (model.ItemType == (int)KinaUnaTypes.TimeLineType.Contact)
                {
                    Shell.Current.Navigation.PushModalAsync(new AddContactPage());
                }

                if (model.ItemType == (int)KinaUnaTypes.TimeLineType.Measurement)
                {
                    Shell.Current.Navigation.PushModalAsync(new AddMeasurementPage());
                }

                if (model.ItemType == (int)KinaUnaTypes.TimeLineType.Skill)
                {
                    Shell.Current.Navigation.PushModalAsync(new AddSkillPage());
                }

                if (model.ItemType == (int)KinaUnaTypes.TimeLineType.Vocabulary)
                {
                    Shell.Current.Navigation.PushModalAsync(new AddVocabularyPage());
                }

                if (model.ItemType == (int)KinaUnaTypes.TimeLineType.Vaccination)
                {
                    Shell.Current.Navigation.PushModalAsync(new AddVaccinationPage());
                }

                if (model.ItemType == (int)KinaUnaTypes.TimeLineType.User)
                {
                    Shell.Current.Navigation.PushModalAsync(new AddUserPage());
                }

                if (model.ItemType == (int)KinaUnaTypes.TimeLineType.Child)
                {
                    Shell.Current.Navigation.PushModalAsync(new AddChildPage());
                }
            }
        }
    }
}
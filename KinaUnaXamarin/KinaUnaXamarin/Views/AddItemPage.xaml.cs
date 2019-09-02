using System.Collections.Generic;
using System.Linq;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
using KinaUnaXamarin.ViewModels;
using KinaUnaXamarin.Views.AddItem;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddItemPage : ContentPage
    {
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
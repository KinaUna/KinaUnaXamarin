using System;
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

            // Reset the selected item.
            AddItemListCollectionView.SelectedItem = null;

            List<Progeny> progenyList = await ProgenyService.GetProgenyAdminList();
            if (progenyList.Any())
            {
                _addItemModel.CanAddItems = true;
            }
            else
            {
                _addItemModel.CanAddItems = false;
            }
        }

        private void AddItemListCollectionView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddItemModel model = AddItemListCollectionView.SelectedItem as AddItemModel;
            if (model != null)
            {
                if (model.Name == "Sleep")
                {
                    Shell.Current.Navigation.PushModalAsync(new AddSleepPage());
                }

                if (model.Name == "Photo")
                {
                    Shell.Current.Navigation.PushModalAsync(new AddPhotoPage());
                }
            }
        }
    }
}
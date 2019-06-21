using System;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.ViewModels;
using KinaUnaXamarin.Views.AddItem;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KinaUnaXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddItemPage : ContentPage
    {
        private AddItemViewModel _addItemModel;
        public AddItemPage()
        {
            InitializeComponent();
            _addItemModel = new AddItemViewModel();
            AddItemListCollectionView.ItemsSource = _addItemModel.ItemList;
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
            }
        }
    }
}
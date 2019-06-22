using System;
using System.Collections.Generic;
using System.Text;
using KinaUnaXamarin.Models;
using MvvmHelpers;

namespace KinaUnaXamarin.ViewModels
{
    class AddItemViewModel: BaseViewModel
    {
        private bool _canAddItems;
        public AddItemViewModel()
        {
            ItemList = new ObservableRangeCollection<AddItemModel>();

            AddItemModel addSleepItem = new AddItemModel();
            addSleepItem.Name = "Sleep";
            addSleepItem.Icon = IconFont.Hotel;
            addSleepItem.Description = "Add Sleep";
            addSleepItem.BackgroundColor = "Green";
            ItemList.Add(addSleepItem);

            AddItemModel addPhoto = new AddItemModel();
            addPhoto.Name = "Photo";
            addPhoto.Icon = IconFont.Image;
            addPhoto.Description = "Add Photo";
            addPhoto.BackgroundColor = "#9c27b0";
            ItemList.Add(addPhoto);
        }
        public ObservableRangeCollection<AddItemModel> ItemList { get; set; }

        public bool CanAddItems
        {
            get => _canAddItems;
            set => SetProperty(ref _canAddItems, value);
        }
    }
}

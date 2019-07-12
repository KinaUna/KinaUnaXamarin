using System;
using System.Collections.Generic;
using System.Text;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;
using Plugin.Multilingual;

namespace KinaUnaXamarin.ViewModels
{
    class AddItemViewModel: BaseViewModel
    {
        private bool _canAddItems;
        public AddItemViewModel()
        {
            ItemList = new ObservableRangeCollection<AddItemModel>();

            var ci = CrossMultilingual.Current.CurrentCultureInfo.TwoLetterISOLanguageName;
            AddItemModel addSleepItem = new AddItemModel();
            addSleepItem.ItemType = (int)KinaUnaTypes.TimeLineType.Sleep;
            addSleepItem.Name = "Sleep";
            addSleepItem.Description = "Add Sleep";
            if (ci == "da")
            {
                addSleepItem.Name = "Søvn";
                addSleepItem.Description = "Tilføj søvn";
            }
            if (ci == "de")
            {
                addSleepItem.Name = "Schlaf";
                addSleepItem.Description = "Schlaf hinzufügen";
            }
            addSleepItem.Icon = IconFont.Hotel;
            addSleepItem.BackgroundColor = "Green";
            ItemList.Add(addSleepItem);

            AddItemModel addPhoto = new AddItemModel();
            addPhoto.ItemType = (int) KinaUnaTypes.TimeLineType.Photo;
            addPhoto.Name = "Photo";
            addPhoto.Description = "Add Photo";
            if (ci == "da")
            {
                addPhoto.Name = "Foto";
                addPhoto.Description = "Tilføj foto";
            }
            if (ci == "de")
            {
                addPhoto.Name = "Foto";
                addPhoto.Description = "Foto hinzufügen";
            }
            addPhoto.Icon = IconFont.Image;
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

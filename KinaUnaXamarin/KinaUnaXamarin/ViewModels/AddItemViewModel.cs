using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Services;
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
        }
        public ObservableRangeCollection<AddItemModel> ItemList { get; set; }

        public bool CanAddItems
        {
            get => _canAddItems;
            set => SetProperty(ref _canAddItems, value);
        }

        public void UpdateItemList()
        {
            ItemList.Clear();
            var ci = CrossMultilingual.Current.CurrentCultureInfo.TwoLetterISOLanguageName;

            if (_canAddItems)
            {
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
                addPhoto.ItemType = (int)KinaUnaTypes.TimeLineType.Photo;
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

                AddItemModel addFriendItem = new AddItemModel();
                addFriendItem.ItemType = (int)KinaUnaTypes.TimeLineType.Friend;
                addFriendItem.Name = "Friend";
                addFriendItem.Description = "Add Friend";
                if (ci == "da")
                {
                    addFriendItem.Name = "Ven";
                    addFriendItem.Description = "Tilføj ven";
                }
                if (ci == "de")
                {
                    addFriendItem.Name = "Freund";
                    addFriendItem.Description = "Freund hinzufügen";
                }
                addFriendItem.Icon = IconFont.EmoticonWink;
                addFriendItem.BackgroundColor = "BlueViolet";
                ItemList.Add(addFriendItem);

                AddItemModel addContactItem = new AddItemModel();
                addContactItem.ItemType = (int)KinaUnaTypes.TimeLineType.Contact;
                addContactItem.Name = "Contact";
                addContactItem.Description = "Add Contact";
                if (ci == "da")
                {
                    addContactItem.Name = "Kontakt";
                    addContactItem.Description = "Tilføj Kontakt";
                }
                if (ci == "de")
                {
                    addContactItem.Name = "Kontakte";
                    addContactItem.Description = "Kontakte hinzufügen";
                }
                addContactItem.Icon = IconFont.ContactMail;
                addContactItem.BackgroundColor = "Purple";
                ItemList.Add(addContactItem);

                AddItemModel addUser = new AddItemModel();
                addUser.ItemType = (int)KinaUnaTypes.TimeLineType.User;
                addUser.Name = "User";
                addUser.Description = "Add User";
                if (ci == "da")
                {
                    addUser.Name = "Bruger";
                    addUser.Description = "Tilføj bruger";
                }
                if (ci == "de")
                {
                    addUser.Name = "Benutzer";
                    addUser.Description = "Benutzer hinzufügen";
                }
                addUser.Icon = IconFont.AccountPlus;
                addUser.BackgroundColor = "#ff9800";
                ItemList.Add(addUser);
            }

            AddItemModel addChild = new AddItemModel();
            addChild.ItemType = (int)KinaUnaTypes.TimeLineType.Child;
            addChild.Name = "Child";
            addChild.Description = "Add a Child";
            if (ci == "da")
            {
                addChild.Name = "Barn";
                addChild.Description = "Tilføj et barn";
            }
            if (ci == "de")
            {
                addChild.Name = "Kind";
                addChild.Description = "Ein Kind hinzufügen";
            }
            addChild.Icon = IconFont.HumanChild;
            addChild.BackgroundColor = "#ff9800";
            ItemList.Add(addChild);
        }
    }
}

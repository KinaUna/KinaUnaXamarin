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

                AddItemModel addVideo = new AddItemModel();
                addVideo.ItemType = (int)KinaUnaTypes.TimeLineType.Video;
                addVideo.Name = "Video";
                addVideo.Description = "Add Video";
                if (ci == "da")
                {
                    addVideo.Name = "Video";
                    addVideo.Description = "Tilføj Video";
                }
                if (ci == "de")
                {
                    addVideo.Name = "Video";
                    addVideo.Description = "Video hinzufügen";
                }
                addVideo.Icon = IconFont.Video;
                addVideo.BackgroundColor = "MediumVioletRed";
                ItemList.Add(addVideo);

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

                AddItemModel addMeasurementItem = new AddItemModel();
                addMeasurementItem.ItemType = (int)KinaUnaTypes.TimeLineType.Measurement;
                addMeasurementItem.Name = "Measurement";
                addMeasurementItem.Description = "Add Measurement";
                if (ci == "da")
                {
                    addMeasurementItem.Name = "Måling";
                    addMeasurementItem.Description = "Tilføj Måling";
                }
                if (ci == "de")
                {
                    addMeasurementItem.Name = "Mässung";
                    addMeasurementItem.Description = "Mässung hinzufügen";
                }
                addMeasurementItem.Icon = IconFont.Ruler;
                addMeasurementItem.BackgroundColor = "DarkRed";
                ItemList.Add(addMeasurementItem);

                AddItemModel addSkillItem = new AddItemModel();
                addSkillItem.ItemType = (int)KinaUnaTypes.TimeLineType.Skill;
                addSkillItem.Name = "Skill";
                addSkillItem.Description = "Add Skill";
                if (ci == "da")
                {
                    addSkillItem.Name = "Færdighed";
                    addSkillItem.Description = "Tilføj Færdighed";
                }
                if (ci == "de")
                {
                    addSkillItem.Name = "Fähigkeit";
                    addSkillItem.Description = "Fähigkeit hinzufügen";
                }
                addSkillItem.Icon = IconFont.School;
                addSkillItem.BackgroundColor = "#787864";
                ItemList.Add(addSkillItem);

                AddItemModel addVocabularyItem = new AddItemModel();
                addVocabularyItem.ItemType = (int)KinaUnaTypes.TimeLineType.Vocabulary;
                addVocabularyItem.Name = "Word";
                addVocabularyItem.Description = "Add a Word to Vocabulary";
                if (ci == "da")
                {
                    addVocabularyItem.Name = "Ord";
                    addVocabularyItem.Description = "Tilføj et ord til ordforråd";
                }
                if (ci == "de")
                {
                    addVocabularyItem.Name = "Wort";
                    addVocabularyItem.Description = "Wort hinzufügen";
                }
                addVocabularyItem.Icon = IconFont.MessageProcessing;
                addVocabularyItem.BackgroundColor = "#641e1e";
                ItemList.Add(addVocabularyItem);

                AddItemModel addVaccinationItem = new AddItemModel();
                addVaccinationItem.ItemType = (int)KinaUnaTypes.TimeLineType.Vaccination;
                addVaccinationItem.Name = "Vaccination";
                addVaccinationItem.Description = "Add Vaccination";
                if (ci == "da")
                {
                    addVaccinationItem.Name = "Vaccination";
                    addVaccinationItem.Description = "Tilføj vaccination";
                }
                if (ci == "de")
                {
                    addVaccinationItem.Name = "Impfung";
                    addVaccinationItem.Description = "Impfung hinzufügen";
                }
                addVaccinationItem.Icon = IconFont.Needle;
                addVaccinationItem.BackgroundColor = "#c800c8";
                ItemList.Add(addVaccinationItem);

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
                addUser.BackgroundColor = "#dd6800";
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

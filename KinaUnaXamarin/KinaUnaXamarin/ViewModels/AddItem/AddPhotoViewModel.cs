using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;

namespace KinaUnaXamarin.ViewModels.AddItem
{
    class AddPhotoViewModel: BaseViewModel
    {
        private int _accessLevel;
        private List<string> _accessLevelList;

        public AddPhotoViewModel()
        {
            ProgenyCollection = new ObservableCollection<Progeny>();
            _accessLevelList = new List<string>();
            _accessLevelList.Add("Hidden/Private");
            _accessLevelList.Add("Family");
            _accessLevelList.Add("Caretakers/Special Access");
            _accessLevelList.Add("Friends");
            _accessLevelList.Add("Registered Users");
            _accessLevelList.Add("Public/Anyone");
        }

        public ObservableCollection<Progeny> ProgenyCollection { get; set; }

        public List<string> AccessLevelList
        {

            get => _accessLevelList;
            set => SetProperty(ref _accessLevelList, value);
        }

        public int AccessLevel
        {
            get => _accessLevel;
            set => SetProperty(ref _accessLevel, value);
        }
    }
}

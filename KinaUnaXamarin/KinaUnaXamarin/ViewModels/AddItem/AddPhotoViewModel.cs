using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;
using Plugin.Multilingual;

namespace KinaUnaXamarin.ViewModels.AddItem
{
    class AddPhotoViewModel: BaseViewModel
    {
        private int _accessLevel;
        private List<string> _accessLevelList;
        private string _tags;
        private string _location;
        private List<string> _locationAutoSuggestList;
        private List<string> _tagsAutoSuggestList;

        public AddPhotoViewModel()
        {
            ProgenyCollection = new ObservableCollection<Progeny>();
            _accessLevelList = new List<string>();
            var ci = CrossMultilingual.Current.CurrentCultureInfo.TwoLetterISOLanguageName;
            if (ci == "da")
            {
                _accessLevelList.Add("Administratorer");
                _accessLevelList.Add("Familie");
                _accessLevelList.Add("Omsorgspersoner/Speciel adgang");
                _accessLevelList.Add("Venner");
                _accessLevelList.Add("Registrerede brugere");
                _accessLevelList.Add("Offentlig/alle");
            }
            else
            {
                if (ci == "de")
                {
                    _accessLevelList.Add("Administratoren");
                    _accessLevelList.Add("Familie");
                    _accessLevelList.Add("Betreuer/Spezial");
                    _accessLevelList.Add("Freunde");
                    _accessLevelList.Add("Registrierte Benutzer");
                    _accessLevelList.Add("Allen zugänglich");
                }
                else
                {
                    _accessLevelList.Add("Hidden/Private");
                    _accessLevelList.Add("Family");
                    _accessLevelList.Add("Caretakers/Special Access");
                    _accessLevelList.Add("Friends");
                    _accessLevelList.Add("Registered Users");
                    _accessLevelList.Add("Public/Anyone");
                }
            }
            
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

        public List<string> LocationAutoSuggestList
        {
            get => _locationAutoSuggestList;
            set => SetProperty(ref _locationAutoSuggestList, value);
        }

        public List<string> TagsAutoSuggestList
        {
            get => _tagsAutoSuggestList;
            set => SetProperty(ref _tagsAutoSuggestList, value);
        }

        public string Tags
        {
            get => _tags;
            set => SetProperty(ref _tags, value);
        }

        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }
    }
}

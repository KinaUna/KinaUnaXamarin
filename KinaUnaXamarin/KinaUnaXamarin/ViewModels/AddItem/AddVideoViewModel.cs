using System.Collections.Generic;
using System.Collections.ObjectModel;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;
using Plugin.Multilingual;

namespace KinaUnaXamarin.ViewModels.AddItem
{
    class AddVideoViewModel: BaseViewModel
    {
        private int _accessLevel;
        private List<string> _accessLevelList;
        private Video _videoItem;
        private int _videoHours;
        private int _videoMinutes;
        private int _videoSeconds;
        private string _tags;
        private string _location;
        private List<string> _locationAutoSuggestList;
        private List<string> _tagsAutoSuggestList;
        private bool _isSaving;

        public AddVideoViewModel()
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

        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        public List<string> AccessLevelList
        {

            get => _accessLevelList;
            set => SetProperty(ref _accessLevelList, value);
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

        public int AccessLevel
        {
            get => _accessLevel;
            set => SetProperty(ref _accessLevel, value);
        }

        public int VideoHours
        {
            get => _videoHours;
            set => SetProperty(ref _videoHours, value);
        }

        public int VideoMinutes
        {
            get => _videoMinutes;
            set => SetProperty(ref _videoMinutes, value);
        }

        public int VideoSeconds
        {
            get => _videoSeconds;
            set => SetProperty(ref _videoSeconds, value);
        }

        public Video VideoItem
        {
            get => _videoItem;
            set => SetProperty(ref _videoItem, value);
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

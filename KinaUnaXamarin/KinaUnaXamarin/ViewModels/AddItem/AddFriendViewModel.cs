using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;
using Plugin.Multilingual;

namespace KinaUnaXamarin.ViewModels.AddItem
{
    class AddFriendViewModel: BaseViewModel
    {
        private int _accessLevel;
        private List<string> _accessLevelList;
        private List<string> _friendTypeList;
        private bool _online;
        private int _friendType;
        private List<string> _contextAutoSuggestList;
        private List<string> _tagsAutoSuggestList;

        public AddFriendViewModel()
        {
            ProgenyCollection = new ObservableCollection<Progeny>();
            _accessLevelList = new List<string>();
            _friendTypeList = new List<string>();

            var ci = CrossMultilingual.Current.CurrentCultureInfo.TwoLetterISOLanguageName;
            if (ci == "da")
            {
                _accessLevelList.Add("Administratorer");
                _accessLevelList.Add("Familie");
                _accessLevelList.Add("Omsorgspersoner/Speciel adgang");
                _accessLevelList.Add("Venner");
                _accessLevelList.Add("Registrerede brugere");
                _accessLevelList.Add("Offentlig/alle");

                _friendTypeList.Add("Personlige venner");
                _friendTypeList.Add("Legetøj/Dyr");
                _friendTypeList.Add("Forældre");
                _friendTypeList.Add("Familie");
                _friendTypeList.Add("Omsorgspersoner");
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

                    _friendTypeList.Add("Persönliche Freunde");
                    _friendTypeList.Add("Spielzeuge/Tiere");
                    _friendTypeList.Add("Eltern");
                    _friendTypeList.Add("Familie");
                    _friendTypeList.Add("Betreuer");
                }
                else
                {
                    _accessLevelList.Add("Hidden/Private");
                    _accessLevelList.Add("Family");
                    _accessLevelList.Add("Caretakers/Special Access");
                    _accessLevelList.Add("Friends");
                    _accessLevelList.Add("Registered Users");
                    _accessLevelList.Add("Public/Anyone");

                    _friendTypeList.Add("Personal Friends");
                    _friendTypeList.Add("Toy/Animal");
                    _friendTypeList.Add("Parents");
                    _friendTypeList.Add("Family");
                    _friendTypeList.Add("Caretakers");
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

        public List<string> FriendTypeList
        {

            get => _friendTypeList;
            set => SetProperty(ref _friendTypeList, value);
        }

        public int FriendType
        {
            get => _friendType;
            set => SetProperty(ref _friendType, value);
        }

        public bool Online
        {
            get => _online;
            set => SetProperty(ref _online, value);
        }

        public List<string> ContextAutoSuggestList
        {
            get => _contextAutoSuggestList;
            set => SetProperty(ref _contextAutoSuggestList, value);
        }

        public List<string> TagsAutoSuggestList
        {
            get => _tagsAutoSuggestList;
            set => SetProperty(ref _tagsAutoSuggestList, value);
        }
    }
}

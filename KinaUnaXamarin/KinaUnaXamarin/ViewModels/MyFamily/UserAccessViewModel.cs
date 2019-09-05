using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;
using Plugin.Multilingual;

namespace KinaUnaXamarin.ViewModels.MyFamily
{
    class UserAccessViewModel:BaseViewModel
    {
        private Progeny _progeny;
        private List<string> _accessLevelList;
        private bool _editMode;
        private UserAccess _selectedAccessLevel;
        private bool _anyChildren;

        public UserAccessViewModel()
        {
            ProgenyCollection = new ObservableCollection<Progeny>();
            UserAccessCollection = new ObservableCollection<UserAccess>();
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
                    _accessLevelList.Add("Administrator");
                    _accessLevelList.Add("Family");
                    _accessLevelList.Add("Caretakers/Special Access");
                    _accessLevelList.Add("Friends");
                    _accessLevelList.Add("Registered Users");
                    _accessLevelList.Add("Public/Anyone");
                }
            }
        }

        public ObservableCollection<Progeny> ProgenyCollection { get; set; }
        public ObservableCollection<UserAccess> UserAccessCollection { get; set; }

        public Progeny Progeny
        {
            get => _progeny;
            set => SetProperty(ref _progeny, value);
        }

        public UserAccess SelectedAccess
        {
            get => _selectedAccessLevel;
            set => SetProperty(ref _selectedAccessLevel, value);
        }

        public List<string> AccessLevelList
        {

            get => _accessLevelList;
            set => SetProperty(ref _accessLevelList, value);
        }

        public bool EditMode
        {
            get => _editMode;
            set => SetProperty(ref _editMode, value);
        }

        public bool AnyChildren
        {
            get => _anyChildren;
            set => SetProperty(ref _anyChildren, value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;

namespace KinaUnaXamarin.ViewModels.MyFamily
{
    class MyChildrenViewModel:BaseViewModel
    {
        private Progeny _progeny;
        private bool _editMode;
        private DateTime _progenyBirthDay;
        private TimeZoneInfo _selectedTimeZone;
        private string _profilePicture;
        private bool _anyChildren;

        public MyChildrenViewModel()
        {
            ProgenyCollection = new ObservableCollection<Progeny>();
            TimeZoneList = new ObservableCollection<TimeZoneInfo>();
            _progeny = OfflineDefaultData.DefaultProgeny;
            _progenyBirthDay = new DateTime(2018, 02, 18, 18, 02, 00);
        }

        public ObservableCollection<Progeny> ProgenyCollection { get; set; }
        public ObservableCollection<TimeZoneInfo> TimeZoneList { get; set; }

        public Progeny Progeny
        {
            get => _progeny;
            set
            {
                SetProperty(ref _progeny, value);
                if (_progeny.BirthDay.HasValue)
                {
                    ProgenyBirthDay = _progeny.BirthDay.Value;
                }

                ProfilePicture = _progeny.PictureLink;
            }
        }

        public bool EditMode
        {
            get => _editMode;
            set => SetProperty(ref _editMode, value);
        }

        public string ProfilePicture
        {
            get => _profilePicture;
            set => SetProperty(ref _profilePicture, value);
        }

        public DateTime ProgenyBirthDay
        {
            get => _progenyBirthDay;
            set => SetProperty(ref _progenyBirthDay, value);
        }

        public TimeZoneInfo SelectedTimeZone
        {
            get => _selectedTimeZone;
            set => SetProperty(ref _selectedTimeZone, value);
        }

        public bool AnyChildren
        {
            get => _anyChildren;
            set => SetProperty(ref _anyChildren, value);
        }
    }
}

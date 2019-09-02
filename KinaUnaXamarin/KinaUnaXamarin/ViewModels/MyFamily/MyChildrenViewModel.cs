using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using KinaUnaXamarin.Models.KinaUna;
using MvvmHelpers;

namespace KinaUnaXamarin.ViewModels.MyFamily
{
    class MyChildrenViewModel:BaseViewModel
    {
        private Progeny _progeny;
        private bool _editMode;

        public MyChildrenViewModel()
        {
            ProgenyCollection = new ObservableCollection<Progeny>();
            TimeZoneList = new ObservableCollection<TimeZoneInfo>();
        }

        public ObservableCollection<Progeny> ProgenyCollection { get; set; }
        public ObservableCollection<TimeZoneInfo> TimeZoneList { get; set; }

        public Progeny Progeny
        {
            get => _progeny;
            set => SetProperty(ref _progeny, value);
        }

        public bool EditMode
        {
            get => _editMode;
            set => SetProperty(ref _editMode, value);
        }
    }
}

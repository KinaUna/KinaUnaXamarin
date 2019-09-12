using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using MvvmHelpers;

namespace KinaUnaXamarin.ViewModels.AddItem
{
    class AddChildViewModel:BaseViewModel
    {
        private bool _online;

        public bool Online
        {
            get => _online;
            set => SetProperty(ref _online, value);
        }

        public AddChildViewModel()
        {
            TimeZoneList = new ObservableCollection<TimeZoneInfo>();
        }

        public ObservableCollection<TimeZoneInfo> TimeZoneList { get; set; }
    }
}

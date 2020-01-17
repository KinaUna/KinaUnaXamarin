using System;
using System.Collections.ObjectModel;
using MvvmHelpers;

namespace KinaUnaXamarin.ViewModels.AddItem
{
    class AddChildViewModel:BaseViewModel
    {
        private bool _online;
        private bool _isSaving;

        public bool Online
        {
            get => _online;
            set => SetProperty(ref _online, value);
        }

        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        public AddChildViewModel()
        {
            TimeZoneList = new ObservableCollection<TimeZoneInfo>();
        }

        public ObservableCollection<TimeZoneInfo> TimeZoneList { get; set; }
    }
}

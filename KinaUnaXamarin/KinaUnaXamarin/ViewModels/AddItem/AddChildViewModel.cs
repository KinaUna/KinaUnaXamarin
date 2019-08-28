using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using MvvmHelpers;

namespace KinaUnaXamarin.ViewModels.AddItem
{
    class AddChildViewModel:BaseViewModel
    {
        public AddChildViewModel()
        {
            timeZoneList = new ObservableCollection<TimeZoneInfo>();
        }

        public ObservableCollection<TimeZoneInfo> timeZoneList { get; set; }
    }
}

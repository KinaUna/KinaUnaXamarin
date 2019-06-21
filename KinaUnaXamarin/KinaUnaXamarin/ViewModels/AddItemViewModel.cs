using System;
using System.Collections.Generic;
using System.Text;
using KinaUnaXamarin.Models;
using MvvmHelpers;

namespace KinaUnaXamarin.ViewModels
{
    class AddItemViewModel: BaseViewModel
    {
        public AddItemViewModel()
        {
            ItemList = new ObservableRangeCollection<AddItemModel>();

            AddItemModel addSleepItem = new AddItemModel();
            addSleepItem.Name = "Sleep";
            addSleepItem.Icon = IconFont.Hotel;
            addSleepItem.Description = "Add Sleep";
            addSleepItem.BackgroundColor = "Green";
            ItemList.Add(addSleepItem);
        }
        public ObservableRangeCollection<AddItemModel> ItemList { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace KinaUnaXamarin.Models
{
    class AddItemModel
    {
        public int ItemType { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string BackgroundColor { get; set; }
    }
}

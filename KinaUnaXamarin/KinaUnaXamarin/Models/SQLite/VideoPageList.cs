using System;
using System.Collections.Generic;
using System.Text;

namespace KinaUnaXamarin.Models
{
    public class VideoPageList
    {
        public int ProgenyId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int SortBy { get; set; }
        public string TagFilter { get; set; }
        public string VideoItemsString { get; set; }
    }
}

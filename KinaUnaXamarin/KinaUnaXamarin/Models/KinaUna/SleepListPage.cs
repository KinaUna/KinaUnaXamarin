using System.Collections.Generic;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class SleepListPage
    {
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int SortBy { get; set; }
        public List<Sleep> SleepList { get; set; }
        public Progeny Progeny { get; set; }
        public bool IsAdmin { get; set; }

        public SleepListPage()
        {
            SleepList = new List<Sleep>();
        }
        
    }
}

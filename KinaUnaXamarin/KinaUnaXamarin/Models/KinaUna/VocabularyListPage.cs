using System.Collections.Generic;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class VocabularyListPage
    {
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int SortBy { get; set; }
        public List<VocabularyItem> VocabularyList { get; set; }
        public Progeny Progeny { get; set; }
        public bool IsAdmin { get; set; }

        public VocabularyListPage()
        {
            VocabularyList = new List<VocabularyItem>();
        }
    }
}

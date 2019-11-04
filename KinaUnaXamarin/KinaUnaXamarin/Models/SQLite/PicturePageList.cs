using SQLite;

namespace KinaUnaXamarin.Models
{
    public class PicturePageList
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int ProgenyId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int SortBy { get; set; }
        public string TagFilter { get; set; }
        public string PictureItemsString { get; set; }
    }
}

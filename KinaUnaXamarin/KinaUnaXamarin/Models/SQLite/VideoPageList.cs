using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class VideoPageList
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int ProgenyId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int SortBy { get; set; }
        public string TagFilter { get; set; }
        public string VideoItemsString { get; set; }
    }
}

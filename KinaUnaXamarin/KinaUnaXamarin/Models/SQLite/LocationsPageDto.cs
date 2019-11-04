using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class LocationsPageDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int ProgenyId { get; set; }
        public int AccessLevel { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int SortOrder { get; set; }
        public string LocationsPageString { get; set; }
    }
}

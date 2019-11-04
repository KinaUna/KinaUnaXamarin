using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class LocationDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int LocationId { get; set; }
        public string LocationString { get; set; }
    }
}

using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class EventDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int EventId { get; set; }
        public string EventString { get; set; }
    }
}

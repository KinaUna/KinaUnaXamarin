using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class UpcomingEvents
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int ProgenyId { get; set; }
        public int AccessLevel { get; set; }
        public string EventsListString { get; set; }
    }
}

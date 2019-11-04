using SQLite;

namespace KinaUnaXamarin.Models
{
    public class CalendarList
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int ProgenyId { get; set; }
        public int AccessLevel { get; set; }
        public string CalendarListString { get; set; }
    }
}

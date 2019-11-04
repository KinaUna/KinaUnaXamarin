using SQLite;

namespace KinaUnaXamarin.Models
{
    public class TimeLineLatest
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int ProgenyId { get; set; }
        public int AccessLevel { get; set; }
        public string TimeLineItemsString { get; set; }
    }
}

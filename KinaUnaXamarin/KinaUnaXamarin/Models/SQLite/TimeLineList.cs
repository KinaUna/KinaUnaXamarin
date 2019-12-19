using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class TimeLineList
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int ProgenyId { get; set; }
        public int AccessLevel { get; set; }
        public int Count { get; set; }
        public int Start { get; set; }
        public string TimeLineItemsString { get; set; }
    }
}

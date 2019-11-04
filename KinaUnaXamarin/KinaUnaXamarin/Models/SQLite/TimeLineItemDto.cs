using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class TimeLineItemDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int TimeLineId { get; set; }
        public int ItemId { get; set; }
        public int ItemType { get; set; }
        public string TimeLineItemString { get; set; }
    }
}

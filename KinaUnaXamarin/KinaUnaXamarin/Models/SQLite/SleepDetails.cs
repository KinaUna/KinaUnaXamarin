using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class SleepDetails
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int SleepId { get; set; }
        public int AccessLevel { get; set; }
        public int SortOrder { get; set; }
        public string SleepItemsString { get; set; }
    }
}

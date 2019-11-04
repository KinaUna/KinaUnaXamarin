using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class SleepDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int SleepId { get; set; }
        public string SleepString { get; set; }
    }
}

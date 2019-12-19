using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class ProgenyList
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public string UserEmail { get; set; }
        public string ProgenyListString { get; set; }
    }
}

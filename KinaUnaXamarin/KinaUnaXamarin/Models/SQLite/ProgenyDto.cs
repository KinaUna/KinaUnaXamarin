using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class ProgenyDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int ProgenyId { get; set; }
        public string ProgenyString { get; set; }
    }
}

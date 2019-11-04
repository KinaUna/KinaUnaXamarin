using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class WordDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int WordId { get; set; }
        public string WordString { get; set; }
    }
}

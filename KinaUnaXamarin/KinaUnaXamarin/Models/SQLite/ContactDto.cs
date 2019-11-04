using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class ContactDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int ContactId { get; set; }
        public string ContactString { get; set; }
    }
}

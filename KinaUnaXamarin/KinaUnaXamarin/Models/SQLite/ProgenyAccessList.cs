using SQLite;

namespace KinaUnaXamarin.Models
{
    public class ProgenyAccessList
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int ProgenyId { get; set; }
        
        public string AccessListString { get; set; }
    }
}

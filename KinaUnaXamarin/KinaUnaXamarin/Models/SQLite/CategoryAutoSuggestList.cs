using SQLite;

namespace KinaUnaXamarin.Models
{
    public class CategoryAutoSuggestList
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int ProgenyId { get; set; }
        public int AccessLevel { get; set; }
        public string AutoSuggestListString { get; set; }
    }
}

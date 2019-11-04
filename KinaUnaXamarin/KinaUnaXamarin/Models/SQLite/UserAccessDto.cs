using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class UserAccessDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public string Email { get; set; }
        public int ProgenyId { get; set; }
        public string UserAccessString { get; set; }
    }
}

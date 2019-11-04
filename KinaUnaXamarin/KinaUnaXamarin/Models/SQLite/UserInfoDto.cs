using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class UserInfoDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public string Email { get; set; }
        public string UserInfoString { get; set; }
    }
}

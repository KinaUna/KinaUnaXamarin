using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class FriendDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int FriendId { get; set; }
        public string FriendString { get; set; }
    }
}

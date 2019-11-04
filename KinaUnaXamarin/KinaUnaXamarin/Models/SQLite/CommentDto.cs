using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class CommentDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int CommentId { get; set; }
        public string CommentString { get; set; }
    }
}

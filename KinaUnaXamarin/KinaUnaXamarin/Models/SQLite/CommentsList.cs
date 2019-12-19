using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class CommentsList
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int CommentThread { get; set; }
        public string CommentsListString { get; set; }
    }
}

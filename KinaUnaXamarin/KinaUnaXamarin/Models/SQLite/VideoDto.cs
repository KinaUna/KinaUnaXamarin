using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class VideoDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int VideoId { get; set; }
        public string VideoString { get; set; }
    }
}

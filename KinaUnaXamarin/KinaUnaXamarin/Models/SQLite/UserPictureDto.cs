using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class UserPictureDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public string PictureId { get; set; }
        public string PictureString { get; set; }
    }
}

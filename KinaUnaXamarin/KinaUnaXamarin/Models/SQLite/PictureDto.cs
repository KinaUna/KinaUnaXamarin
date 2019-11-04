using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class PictureDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int PictureId { get; set; }
        public string PictureString { get; set; }
    }
}

using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class PictureViewModelDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int PictureId { get; set; }
        public string PictureViewModelString { get; set; }
    }
}

using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class NoteDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int NoteId { get; set; }
        public string NoteString { get; set; }
    }
}

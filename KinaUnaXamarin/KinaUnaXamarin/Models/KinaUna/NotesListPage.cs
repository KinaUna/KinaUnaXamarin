using System.Collections.Generic;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class NotesListPage
    {
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int SortBy { get; set; }
        public List<Note> NotesList { get; set; }
        public Progeny Progeny { get; set; }
        public bool IsAdmin { get; set; }

        public NotesListPage()
        {
            NotesList = new List<Note>();
        }
    }
}

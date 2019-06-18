using System;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class Note
    {
        public int NoteId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Category { get; set; }
        public DateTime CreatedDate { get; set; }
        public int AccessLevel { get; set; }
        public int ProgenyId { get; set; }
        public string Owner { get; set; }

        public Progeny Progeny { get; set; }
    }
}

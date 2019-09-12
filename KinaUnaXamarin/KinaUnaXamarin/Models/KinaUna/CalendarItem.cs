using System;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class CalendarItem
    {
        public int EventId { get; set; }
        public int ProgenyId { get; set; }
        public string Title { get; set; }
        public string Notes { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Location { get; set; }
        public string Context { get; set; }
        public bool AllDay { get; set; }
        public int AccessLevel { get; set; }

        public string StartString { get; set; }
        public string EndString { get; set; }
        public string Author { get; set; }

        public Progeny Progeny { get; set; }

        public CalendarItem()
        {
            Progeny = OfflineDefaultData.DefaultProgeny;
        }
    }
}

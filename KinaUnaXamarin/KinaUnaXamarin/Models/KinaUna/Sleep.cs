using System;
using Newtonsoft.Json;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class Sleep
    {
        public int SleepId { get; set; }
        public int ProgenyId { get; set; }
        public DateTime SleepStart { get; set; }
        public DateTime SleepEnd { get; set; }
        public DateTime CreatedDate { get; set; }
        public int SleepRating { get; set; }
        public string SleepNotes { get; set; }
        public int AccessLevel { get; set; }
        public string Author { get; set; }
        public TimeSpan SleepDuration { get; set; }
        public string StartString { get; set; }
        public string EndString { get; set; }
        public Progeny Progeny { get; set; }

        [JsonIgnore]
        public double SleepDurDouble { get; set; }

        public int SleepNumber { get; set; }

        public Sleep()
        {
            Progeny = OfflineDefaultData.DefaultProgeny;
        }
    }
}

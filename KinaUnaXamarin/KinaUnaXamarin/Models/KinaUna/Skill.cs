using System;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class Skill
    {
        public int SkillId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public DateTime? SkillFirstObservation { get; set; }
        public DateTime SkillAddedDate { get; set; }
        public string Author { get; set; }
        public int ProgenyId { get; set; }
        public Progeny Progeny { get; set; }
        public int AccessLevel { get; set; } // 0 = Hidden/Parents only, 1=Family, 2= Friends, 3=DefaultUSers, 4= public.
    }
}

using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class SkillDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int SkillId { get; set; }
        public string SkillString { get; set; }
    }
}

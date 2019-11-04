using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class VaccinationDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int VaccinationId { get; set; }
        public string VaccinationString { get; set; }
    }
}

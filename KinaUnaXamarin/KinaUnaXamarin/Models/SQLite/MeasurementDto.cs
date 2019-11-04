using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class MeasurementDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int MeasurementId { get; set; }
        public string MeasurementString { get; set; }
    }
}

using System;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class Vaccination
    {
        public int VaccinationId { get; set; }
        public string VaccinationName { get; set; }
        public string VaccinationDescription { get; set; }
        public DateTime VaccinationDate { get; set; }
        public string Notes { get; set; }
        public int ProgenyId { get; set; }
        public int AccessLevel { get; set; }
        public string Author { get; set; }

        public Progeny Progeny { get; set; }
    }
}

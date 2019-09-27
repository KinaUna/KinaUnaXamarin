using System.Collections.Generic;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class LocationsListPage
    {
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int SortBy { get; set; }
        public List<Location> LocationsList { get; set; }
        public Progeny Progeny { get; set; }
        public bool IsAdmin { get; set; }

        public LocationsListPage()
        {
            LocationsList = new List<Location>();
        }
    }
}

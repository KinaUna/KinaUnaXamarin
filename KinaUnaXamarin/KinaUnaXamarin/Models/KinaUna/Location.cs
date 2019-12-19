using System;
using Newtonsoft.Json;
using Xamarin.Forms.GoogleMaps;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class Location
    {
        public int LocationId { get; set; }
        public int ProgenyId { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string StreetName { get; set; }
        public string HouseNumber { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string County { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public DateTime? Date { get; set; }
        public string Notes { get; set; }
        public int AccessLevel { get; set; }
        public string Tags { get; set; }
        public DateTime? DateAdded { get; set; }
        public string Author { get; set; }

        public int LocationNumber { get; set; }

        [JsonIgnore]
        public Position Position { get; set; }

        public Progeny Progeny { get; set; }
    }
}

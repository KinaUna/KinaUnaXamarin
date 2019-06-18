using System;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class ApplicationUser // : IdentityUser
    {
        // Properties from IdentityUser not implemented, as they are not needed here.

        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public int ViewChild { get; set; }
        public string TimeZone { get; set; }
        public DateTime JoinDate { get; set; }

        public string Email { get; set; }
        public string Id { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
    }
}

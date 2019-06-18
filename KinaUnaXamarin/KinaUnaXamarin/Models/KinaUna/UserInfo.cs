using System.Collections.Generic;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class UserInfo
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public int ViewChild { get; set; }
        public string Timezone { get; set; }
        public string ProfilePicture { get; set; }

        public List<Progeny> ProgenyList { get; set; }
        public bool CanUserAddItems { get; set; }
        public List<UserAccess> AccessList { get; set; }
    }
}

using System;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class Friend
    {
        public int FriendId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? FriendSince { get; set; }
        public DateTime FriendAddedDate { get; set; }
        public string PictureLink { get; set; }
        public int ProgenyId { get; set; }
        public Progeny Progeny { get; set; }
        public int Type { get; set; } // 0= Personal friend, 1= toy friend, 2= parent, 3= family, 4= caretaker
        public string Context { get; set; } // 
        public string Notes { get; set; }
        public string Tags { get; set; }
        public int AccessLevel { get; set; } // 0 = Hidden/Parents only, 1=Family, 2= Friends, 3=DefaultUSers, 4= public.
        public string Author { get; set; }

        public Friend()
        {
            Progeny = OfflineDefaultData.DefaultProgeny;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class PictureViewModel
    {
        public int PictureId { get; set; }

        public string PictureLink { get; set; }
        public DateTime? PictureTime { get; set; }
        public int? PictureRotation { get; set; }
        public int PictureWidth { get; set; }
        public int PictureHeight { get; set; }

        public int ProgenyId { get; set; }
        public Progeny Progeny { get; set; }
        public string Owners { get; set; } // Comma separated list of emails.
        public int AccessLevel { get; set; } // 0 = Hidden/Parents only, 1=Family, 2= Friends, 3=DefaultUSers, 4= public.
        public string Author { get; set; }
        public bool IsAdmin { get; set; }
        public int CommentThreadNumber { get; set; }
        public List<Comment> CommentsList { get; set; }
        public int CommentsCount { get; set; }
        public string Tags { get; set; }
        public string TagsList { get; set; }
        public string TagFilter { get; set; }
        public string Location { get; set; }
        public string Longtitude { get; set; }
        public string Latitude { get; set; }
        public string Altitude { get; set; }
        public List<Location> ProgenyLocations { get; set; }
        public int PictureNumber { get; set; }
        public int PictureCount { get; set; }
        public int PrevPicture { get; set; }
        public int NextPicture { get; set; }

        public bool Zoomed { get; set; }

        public PictureViewModel()
        {
            CommentsList = new List<Comment>();
        }

    }
}

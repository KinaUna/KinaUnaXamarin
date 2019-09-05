using System;
using System.Collections.Generic;
using System.Text;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class VideoViewModel
    {
        public int VideoId { get; set; }

        public string VideoLink { get; set; }
        public string ThumbLink { get; set; }
        public DateTime? VideoTime { get; set; }
        public int? VideoRotation { get; set; }
        public int VideoNumber { get; set; }
        public int ProgenyId { get; set; }
        public Progeny Progeny { get; set; }
        public string Owners { get; set; } // Comma separated list of emails.
        public int AccessLevel { get; set; } // 0 = Hidden/Parents only, 1=Family, 2= Friends, 3=DefaultUSers, 4= public.
        public string Author { get; set; }
        
        public bool IsAdmin { get; set; }
        public int CommentThreadNumber { get; set; }
        public List<Comment> CommentsList { get; set; }
        public int CommentsCount { get; set; }
        public int VideoType { get; set; }
        public string Tags { get; set; }
        public string TagFilter { get; set; }
        public string TagsList { get; set; }
        public string DurationHours { get; set; }
        public string DurationMinutes { get; set; }
        public string DurationSeconds { get; set; }
        public int SortBy { get; set; }
        public string UserId { get; set; }


        public TimeSpan? Duration { get; set; }

        public string Location { get; set; }
        public string Longtitude { get; set; }
        public string Latitude { get; set; }
        public string Altitude { get; set; }
        public List<Location> ProgenyLocations { get; set; }
        public int VideoCount { get; set; }
        public int PrevVideo { get; set; }
        public int NextVideo { get; set; }

    }
}

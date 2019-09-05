using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class Video
    {
        public int VideoId { get; set; }
        public DateTime? VideoTime { get; set; }
        public string VideoLink { get; set; }
        public string ThumbLink { get; set; }
        
        public int ProgenyId { get; set; }
        public string Owners { get; set; } // Comma separated list of emails.
        public int AccessLevel { get; set; } // 0 = Hidden/Parents only, 1=Family, 2= Friends, 3=DefaultUSers, 4= public.
        public int CommentThreadNumber { get; set; }
        public int VideoType { get; set; } // 0 = file upload, 1 = OneDrive, 2 = Youtube
        public string Tags { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Author { get; set; }
        public string Location { get; set; }
        public string Longtitude { get; set; }
        public string Latitude { get; set; }
        public string Altitude { get; set; }

        public Progeny Progeny { get; set; }
        public List<Comment> Comments { get; set; }
        public string TimeZone { get; set; }
        public int VideoNumber { get; set; }
        public string DurationHours { get; set; }
        public string DurationMinutes { get; set; }
        public string DurationSeconds { get; set; }

        [JsonIgnore]
        public int CommentsCount { get; set; }
    }
}

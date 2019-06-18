using System;
using Newtonsoft.Json;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class Comment
    {
        public int CommentId { get; set; }
        public int CommentThreadNumber { get; set; }
        public string CommentText { get; set; }
        public string Author { get; set; }
        public string DisplayName { get; set; }
        public DateTime Created { get; set; }

        [JsonIgnore]
        public string AuthorImage { get; set; }
    }
}

using System;
using Newtonsoft.Json;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class TimeLineItem
    {
        public int TimeLineId { get; set; }
        public int ProgenyId { get; set; }
        public DateTime ProgenyTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public int ItemType { get; set; }
        public string ItemId { get; set; }
        public string CreatedBy { get; set; }
        public int AccessLevel { get; set; }

        [JsonIgnore]
        public Object ItemObject { get; set; }
        [JsonIgnore]
        public bool VisibleBefore { get; set; }
    }
}

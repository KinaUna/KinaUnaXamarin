using System;
using Newtonsoft.Json;
using Xamarin.Forms;

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

        [JsonIgnore]
        public Picture PictureObject
        {
            get
            {
                if (ItemType == (int) KinaUnaTypes.TimeLineType.Photo)
                {
                    return ItemObject as Picture;
                }

                return null;
            }
        }

        [JsonIgnore]
        public Video VideoObject
        {
            get
            {
                if (ItemType == (int)KinaUnaTypes.TimeLineType.Video)
                {
                    return ItemObject as Video;
                }

                return null;
            }
        }

        [JsonIgnore]
        public CalendarItem CalendarObject
        {
            get
            {
                if (ItemType == (int)KinaUnaTypes.TimeLineType.Calendar)
                {
                    return ItemObject as CalendarItem;
                }

                return null;
            }
        }

        [JsonIgnore]
        public Location LocationObject
        {
            get
            {
                if (ItemType == (int)KinaUnaTypes.TimeLineType.Location)
                {
                    return ItemObject as Location;
                }

                return null;
            }
        }

        [JsonIgnore]
        public VocabularyItem VocabularyObject
        {
            get
            {
                if (ItemType == (int)KinaUnaTypes.TimeLineType.Vocabulary)
                {
                    return ItemObject as VocabularyItem;
                }

                return null;
            }
        }

        [JsonIgnore]
        public Skill SkillObject
        {
            get
            {
                if (ItemType == (int)KinaUnaTypes.TimeLineType.Skill)
                {
                    return ItemObject as Skill;
                }

                return null;
            }
        }

        [JsonIgnore]
        public Friend FriendObject
        {
            get
            {
                if (ItemType == (int)KinaUnaTypes.TimeLineType.Friend)
                {
                    return ItemObject as Friend;
                }

                return null;
            }
        }

        [JsonIgnore]
        public Measurement MeasurementObject
        {
            get
            {
                if (ItemType == (int)KinaUnaTypes.TimeLineType.Measurement)
                {
                    return ItemObject as Measurement;
                }

                return null;
            }
        }

        [JsonIgnore]
        public Sleep SleepObject
        {
            get
            {
                if (ItemType == (int)KinaUnaTypes.TimeLineType.Sleep)
                {
                    return ItemObject as Sleep;
                }

                return null;
            }
        }

        [JsonIgnore]
        public Note NoteObject
        {
            get
            {
                if (ItemType == (int)KinaUnaTypes.TimeLineType.Note)
                {
                    return ItemObject as Note;
                }

                return null;
            }
        }

        [JsonIgnore]
        public Contact ContactObject
        {
            get
            {
                if (ItemType == (int)KinaUnaTypes.TimeLineType.Contact)
                {
                    return ItemObject as Contact;
                }

                return null;
            }
        }

        [JsonIgnore]
        public Vaccination VaccinationObject
        {
            get
            {
                if (ItemType == (int)KinaUnaTypes.TimeLineType.Vaccination)
                {
                    return ItemObject as Vaccination;
                }

                return null;
            }
        }

        [JsonIgnore]
        public DateHeader DateHeaderObject
        {
            get
            {
                if (ItemObject is DateHeader)
                {
                    return ItemObject as DateHeader;
                }

                return null;
            }
        }
    }
}

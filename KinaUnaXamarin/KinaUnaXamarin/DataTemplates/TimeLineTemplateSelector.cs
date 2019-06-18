using KinaUnaXamarin.Models.KinaUna;
using Xamarin.Forms;

namespace KinaUnaXamarin.DataTemplates
{
    public class TimeLineTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PhotoTemplate { get; set; }
        public DataTemplate PhotoTemplateLandscape { get; set; }
        public DataTemplate VideoTemplate { get; set; }
        public DataTemplate CalendarTemplate { get; set; }
        public DataTemplate LocationTemplate { get; set; }
        public DataTemplate VocabularyTemplate { get; set; }
        public DataTemplate SkillTemplate { get; set; }
        public DataTemplate FriendTemplate { get; set; }

        public DataTemplate MeasurementTemplate { get; set; }
        public DataTemplate SleepTemplate { get; set; }
        public DataTemplate NoteTemplate { get; set; }
        public DataTemplate ContactTemplate { get; set; }
        public DataTemplate VaccinationTemplate { get; set; }
        public DataTemplate DateHeaderTemplate { get; set; }

        public DataTemplate InvalidTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item == null)
            {
                return null;
            }
            TimeLineItem timelineItem = item as TimeLineItem;

            if (timelineItem != null && timelineItem.ItemType == 9999)
            {
                return DateHeaderTemplate;
            }

            if (timelineItem != null && timelineItem.ItemType == (int)KinaUnaTypes.TimeLineType.Photo)
            {
                Picture pic = timelineItem.ItemObject as Picture;
                if (pic != null && pic.PictureRotation != null)
                {
                    if (pic.PictureRotation == 90 || pic.PictureRotation == 270)
                    {
                        return PhotoTemplate;
                    }
                }
                return PhotoTemplateLandscape;
            }

            if (timelineItem != null && timelineItem.ItemType == (int)KinaUnaTypes.TimeLineType.Video)
            {
                return VideoTemplate;
            }

            if (timelineItem != null && timelineItem.ItemType == (int)KinaUnaTypes.TimeLineType.Calendar)
            {
                return CalendarTemplate;
            }

            if (timelineItem != null && timelineItem.ItemType == (int)KinaUnaTypes.TimeLineType.Location)
            {
                return LocationTemplate;
            }

            if (timelineItem != null && timelineItem.ItemType == (int)KinaUnaTypes.TimeLineType.Vocabulary)
            {
                return VocabularyTemplate;
            }

            if (timelineItem != null && timelineItem.ItemType == (int)KinaUnaTypes.TimeLineType.Skill)
            {
                return SkillTemplate;
            }

            if (timelineItem != null && timelineItem.ItemType == (int)KinaUnaTypes.TimeLineType.Friend)
            {
                return FriendTemplate;
            }

            if (timelineItem != null && timelineItem.ItemType == (int)KinaUnaTypes.TimeLineType.Measurement)
            {
                return MeasurementTemplate;
            }

            if (timelineItem != null && timelineItem.ItemType == (int)KinaUnaTypes.TimeLineType.Sleep)
            {
                return SleepTemplate;
            }

            if (timelineItem != null && timelineItem.ItemType == (int)KinaUnaTypes.TimeLineType.Note)
            {
                return NoteTemplate;
            }

            if (timelineItem != null && timelineItem.ItemType == (int)KinaUnaTypes.TimeLineType.Contact)
            {
                return ContactTemplate;
            }

            if (timelineItem != null && timelineItem.ItemType == (int)KinaUnaTypes.TimeLineType.Vaccination)
            {
                return VaccinationTemplate;
            }

            return InvalidTemplate;
        }
    }
}

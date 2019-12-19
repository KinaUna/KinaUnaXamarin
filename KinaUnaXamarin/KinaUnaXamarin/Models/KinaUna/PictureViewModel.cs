using System;
using System.Collections.Generic;
using MvvmHelpers;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class PictureViewModel: BaseViewModel
    {
        private int _commentsCount;
        private int _pictureId;
        private string _pictureLink;
        private DateTime? _pictureTime;
        private int _accessLevel;
        private bool _isAdmin;
        private string _tags;
        private string _location;
        private string _longitude;
        private string _latitude;
        private string _altitude;
        private bool _zoomed;

        public int CommentsCount
        {
            get => _commentsCount;
            set => SetProperty(ref _commentsCount, value);
        }

        public int PictureId
        {
            get => _pictureId;
            set => SetProperty(ref _pictureId, value);
        }

        public string PictureLink
        {
            get => _pictureLink;
            set => SetProperty(ref _pictureLink, value);
        }

        public DateTime? PictureTime
        {
            get => _pictureTime;
            set => SetProperty(ref _pictureTime, value);
        }
        public int? PictureRotation { get; set; }
        public int PictureWidth { get; set; }
        public int PictureHeight { get; set; }

        public int ProgenyId { get; set; }
        public Progeny Progeny { get; set; }
        public string Owners { get; set; } // Comma separated list of emails.

        public int AccessLevel
        {
            get => _accessLevel;
            set => SetProperty(ref _accessLevel, value);
        } // 0 = Hidden/Parents only, 1=Family, 2= Friends, 3=DefaultUSers, 4= public.

        public string Author { get; set; }

        public bool IsAdmin
        {
            get => _isAdmin;
            set => SetProperty(ref _isAdmin, value);
        }

        public int CommentThreadNumber { get; set; }
        public List<Comment> CommentsList { get; set; }

        public string Tags
        {
            get => _tags;
            set => SetProperty(ref _tags, value);
        }
        public string TagsList { get; set; }
        public string TagFilter { get; set; }

        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        public string Longtitude
        {
            get => _longitude;
            set => SetProperty(ref _longitude, value);
        }
        public string Latitude {
            get => _latitude;
            set => SetProperty(ref _latitude, value);
        }

        public string Altitude
        {
            get => _altitude;
            set => SetProperty(ref _altitude, value);
        }
        public List<Location> ProgenyLocations { get; set; }
        public int PictureNumber { get; set; }
        public int PictureCount { get; set; }
        public int PrevPicture { get; set; }
        public int NextPicture { get; set; }

        public bool Zoomed {
            get => _zoomed;
            set => SetProperty(ref _zoomed, value);
        }

        public PictureViewModel()
        {
            CommentsList = new List<Comment>();
        }

    }
}

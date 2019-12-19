using System;
using System.Collections.Generic;
using KinaUnaXamarin.Models.KinaUna;

namespace KinaUnaXamarin.Models
{
    public static class OfflineDefaultData
    {
        static OfflineDefaultData()
        {
            DateTime? bday = new DateTime(2018, 2, 18, 18, 2, 0, kind: DateTimeKind.Utc);
            DefaultProgeny = new Progeny();
            DefaultProgeny.Admins = Constants.DummyEmail;
            DefaultProgeny.BirthDay = bday;
            DefaultProgeny.Id = Constants.DefaultChildId;
            DefaultProgeny.Name = Constants.DefaultChildName;
            DefaultProgeny.NickName = Constants.DefaultChildNickName;
            DefaultProgeny.PictureLink = Constants.DefaultChildPictureLink;
            DefaultProgeny.TimeZone = Constants.DefaultTimeZone;

            DefaultPicture = new Picture();
            DefaultPicture.PictureLink = Constants.DefaultPictureLink;
            DefaultPicture.AccessLevel = 5;
            DefaultPicture.Altitude = "0";
            DefaultPicture.Author = Constants.DummyEmail;
            DefaultPicture.CommentThreadNumber = 0;
            DefaultPicture.Comments = new List<Comment>();
            DefaultPicture.Latitude = "48.9873699722222";
            DefaultPicture.Longtitude = "12.0561299722222";
            DefaultPicture.Location = "Bavaria, Germany";
            DefaultPicture.Owners = Constants.DummyEmail;
            DefaultPicture.PictureId = 2450;
            DefaultPicture.PictureHeight = 3008;
            DefaultPicture.PictureWidth = 5344;
            DefaultPicture.PictureLink1200 = Constants.DefaultPictureLink;
            DefaultPicture.PictureLink600 = Constants.DefaultPictureLink;
            DefaultPicture.PictureRotation = 0;
            DefaultPicture.Tags = "Clouds";
            DefaultPicture.TimeZone = Constants.DefaultTimeZone;
            DefaultPicture.Progeny = DefaultProgeny;

            DefaultUser = new ApplicationUser();
            DefaultUser.Email = Constants.DummyEmail;
            DefaultUser.FirstName = "Kina";
            DefaultUser.MiddleName = "";
            DefaultUser.LastName = "Una";
            DefaultUser.Id = "0";
            DefaultUser.JoinDate = new DateTime(2018, 02, 18, 18, 02, 0);
            DefaultUser.TimeZone = Constants.DefaultTimeZone;
            DefaultUser.ViewChild = Constants.DefaultChildId;

            DefaultUserInfo = new UserInfo();
            DefaultUserInfo.Id = 0;
            DefaultUserInfo.CanUserAddItems = false;
            DefaultUserInfo.FirstName = "Kina";
            DefaultUserInfo.MiddleName = "";
            DefaultUserInfo.LastName = "Una";
            DefaultUserInfo.ViewChild = Constants.DefaultChildId;
            DefaultUserInfo.Timezone = Constants.DefaultTimeZone;
            DefaultUserInfo.CanUserAddItems = false;
            DefaultUserInfo.ProfilePicture = Constants.DefaultChildPictureLink;
            DefaultUserInfo.UserEmail = Constants.DummyEmail;
            DefaultUserInfo.UserId = "0";
            DefaultUserInfo.ProgenyList = new List<Progeny> {DefaultProgeny};
            DefaultUserInfo.ViewChild = Constants.DefaultChildId;
            DefaultUserInfo.UserName = "Kina Una";
            UserAccess tempAccess = new UserAccess();
            tempAccess.Progeny = DefaultProgeny;
            tempAccess.AccessId = 0;
            tempAccess.AccessLevel = 5;
            tempAccess.CanContribute = false;
            tempAccess.ProgenyId = Constants.DefaultChildId;
            tempAccess.UserId = "0";
            tempAccess.User = DefaultUser;
            DefaultUserInfo.AccessList = new List<UserAccess>{ tempAccess};
        }
        public static Progeny DefaultProgeny { get; set; }
        public static Picture DefaultPicture { get; set; }

        public static ApplicationUser DefaultUser { get; set; }
        public static UserInfo DefaultUserInfo { get; set; }
    }
}

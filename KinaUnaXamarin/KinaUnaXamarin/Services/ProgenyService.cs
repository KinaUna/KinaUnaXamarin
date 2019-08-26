using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FFImageLoading;
using FFImageLoading.Forms;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using Newtonsoft.Json;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Location = KinaUnaXamarin.Models.KinaUna.Location;

namespace KinaUnaXamarin.Services
{
    public static class ProgenyService
    {
        public static bool Online()
        {
            var networkAccess = Connectivity.NetworkAccess;
            bool internetAccess = networkAccess == NetworkAccess.Internet;
            return internetAccess;
        }

        public static async Task<Progeny> GetProgeny(int progenyId)
        {
            if (progenyId == 0)
            {
                return OfflineDefaultData.DefaultProgeny;
            }

            bool online = Online();
            if (!online)
            {

                string progenyString = await SecureStorage.GetAsync("ProgenyObject" + progenyId);
                if (string.IsNullOrEmpty(progenyString))
                {
                    return OfflineDefaultData.DefaultProgeny;
                }
                Progeny progeny = JsonConvert.DeserializeObject<Progeny>(progenyString);
                //string documentsPath = FileSystem.CacheDirectory;
                //string localFilename = "progenyprofile" + progeny.Id + ".jpg";
                //string progenyProfileFile = Path.Combine(documentsPath, localFilename);
                //progeny.PictureLink = progenyProfileFile;
                return progeny;
            }
            else
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();
                // If the user is not logged in
                if (String.IsNullOrEmpty(accessToken))
                {
                    var result = await client.GetAsync("api/publicaccess/" + progenyId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var progenyString = await result.Content.ReadAsStringAsync();
                        Progeny progeny = JsonConvert.DeserializeObject<Progeny>(progenyString);
                        try
                        {
                            TimeZoneInfo.FindSystemTimeZoneById(progeny.TimeZone);
                        }
                        catch (Exception)
                        {
                            progeny.TimeZone = TZConvert.WindowsToIana(progeny.TimeZone);
                        }

                        await SecureStorage.SetAsync("ProgenyObject" + progenyId, JsonConvert.SerializeObject(progeny));
                        ImageService.Instance.LoadUrl(progeny.PictureLink).Preload();
                        return progeny;
                    }
                    else
                    {
                        return OfflineDefaultData.DefaultProgeny;
                    }
                }
                else // User is logged in
                {
                    client.SetBearerToken(accessToken);
                    var result = await client.GetAsync("api/progeny/mobile/" + progenyId).ConfigureAwait(false);
                    Progeny progeny = new Progeny();
                    if (result.IsSuccessStatusCode)
                    {
                        var progenyString = await result.Content.ReadAsStringAsync();
                        progeny = JsonConvert.DeserializeObject<Progeny>(progenyString);
                        try
                        {
                            TimeZoneInfo.FindSystemTimeZoneById(progeny.TimeZone);
                        }
                        catch (Exception)
                        {
                            progeny.TimeZone = TZConvert.WindowsToIana(progeny.TimeZone);
                        }
                        if (!progeny.PictureLink.ToLower().StartsWith("https"))
                        {
                            progeny.PictureLink = Constants.ProfilePicture;
                        }
                        else
                        {
                            ImageService.Instance.LoadUrl(progeny.PictureLink).Preload();
                        }
                        await SecureStorage.SetAsync("ProgenyObject" + progenyId, JsonConvert.SerializeObject(progeny));
                        return progeny;
                    }
                    else
                    {

                        string progenyString = await SecureStorage.GetAsync("ProgenyObject" + progenyId);
                        progeny = JsonConvert.DeserializeObject<Progeny>(progenyString);
                        //string documentsPath = FileSystem.CacheDirectory;
                        //string localFilename = "progenyprofile" + progeny.Id + ".jpg";
                        //string progenyProfileFile = Path.Combine(documentsPath, localFilename);
                        //progeny.PictureLink = progenyProfileFile;
                        return progeny;
                    }

                }
            }
        }

        public static async Task<Comment> AddComment(int commentThread, string text)
        {
            Comment cmnt = new Comment();

            cmnt.CommentThreadNumber = commentThread;
            cmnt.CommentText = text;
            cmnt.Author = await UserService.GetUserId();
            cmnt.DisplayName = await UserService.GetFullname();
            cmnt.Created = DateTime.UtcNow;

            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.MediaApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                var result = await client.PostAsync("api/comments/", new StringContent(JsonConvert.SerializeObject(cmnt), System.Text.Encoding.UTF8, "application/json")).ConfigureAwait(false);
                if (result.IsSuccessStatusCode)
                {
                    string resultString = await result.Content.ReadAsStringAsync();
                    Comment resultComment = JsonConvert.DeserializeObject<Comment>(resultString);
                    return resultComment;
                }
            }

            return cmnt;
        }

        public static async Task<List<Comment>> GetComments(int commentThread)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.MediaApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                // If user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {

                    return new List<Comment>();
                    
                }
                else // If user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    string commentsApiPath = "api/comments/getcommentsbythread/" + commentThread;
                    var result = await client.GetAsync(commentsApiPath).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var commentsListString = await result.Content.ReadAsStringAsync();
                        List<Comment> commentsList = JsonConvert.DeserializeObject<List<Comment>>(commentsListString);
                        string userEmail = await UserService.GetUserEmail();
                        UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(await SecureStorage.GetAsync("UserInfo" + userEmail));
                        string userTimezone = userInfo.Timezone;

                        try
                        {
                            TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
                        }
                        catch (Exception)
                        {
                            userTimezone = TZConvert.WindowsToIana(userTimezone);
                        }

                        foreach (Comment comment in commentsList)
                        {
                            comment.Created = TimeZoneInfo.ConvertTimeFromUtc(comment.Created, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                            if (comment.Author == userInfo.UserId)
                            {
                                comment.IsAuthor = true;
                            }
                            await SecureStorage.SetAsync("Comment" + comment.CommentId, JsonConvert.SerializeObject(comment));
                        }

                        await SecureStorage.SetAsync("CommentThread" + commentThread, JsonConvert.SerializeObject(commentsList));
                        return commentsList;
                    }
                    else
                    {
                        return new List<Comment>();
                    }
                }
            }
            else
            {
                string commentsListString = await SecureStorage.GetAsync("CommentThread" + commentThread);
                List<Comment> commentsList = JsonConvert.DeserializeObject<List<Comment>>(commentsListString);
                return commentsList;
            }
        }

        public static async Task<Comment> DeleteComment(Comment comment)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.MediaApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                // If user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {

                    return new Comment();

                }
                else // If user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    string commentsApiPath = "api/comments/" + comment.CommentId;
                    var result = await client.DeleteAsync(commentsApiPath).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        SecureStorage.Remove("Comment" + comment.CommentId);
                        return comment;
                    }
                    
                }
            }
            return new Comment();
        }

        public static async Task<List<Progeny>> GetProgenyList(string userEmail)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                // If the user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {
                    var result = await client.GetAsync("api/publicaccess/progenylistbyuser/" + userEmail).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var progenyListString = await result.Content.ReadAsStringAsync();
                        List<Progeny> progenyList = JsonConvert.DeserializeObject<List<Progeny>>(progenyListString);
                        await SecureStorage.SetAsync("ProgenyList" + userEmail, JsonConvert.SerializeObject(progenyList));
                        return progenyList;
                    }
                    else
                    {
                        List<Progeny> progenyList = new List<Progeny>();
                        progenyList.Add(OfflineDefaultData.DefaultProgeny);
                        return progenyList;
                    }
                }
                else // If the user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/access/progenylistbyusermobile/" + userEmail)
                        .ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var progenyListString = await result.Content.ReadAsStringAsync();
                        List<Progeny> progenyList = JsonConvert.DeserializeObject<List<Progeny>>(progenyListString);
                        await SecureStorage.SetAsync("ProgenyList" + userEmail,
                            JsonConvert.SerializeObject(progenyList));
                        return progenyList;
                    }
                    else
                    {
                        string progenyListString = await SecureStorage.GetAsync("ProgenyList" + userEmail);
                        List<Progeny> progenyList = JsonConvert.DeserializeObject<List<Progeny>>(progenyListString);
                        return progenyList;
                    }
                }
            }
            else
            {
                string progenyListString = await SecureStorage.GetAsync("ProgenyList" + userEmail);
                List<Progeny> progenyList = JsonConvert.DeserializeObject<List<Progeny>>(progenyListString);
                return progenyList;
            }
        }

        public static async Task<List<Progeny>> GetProgenyAdminList()
        {
            List<Progeny> progenyListResult = new List<Progeny>();
            string userEmail = await UserService.GetUserEmail();
            string progenyListString = await SecureStorage.GetAsync("ProgenyList" + userEmail);
            List<Progeny> progenyList = JsonConvert.DeserializeObject<List<Progeny>>(progenyListString);
            foreach (Progeny progeny in progenyList)
            {
                int al = 5;
                try
                {
                    al = await GetAccessLevel(progeny.Id);
                }
                catch (Exception)
                {
                    al = 5;
                }
                if (al == 0)
                {
                    progenyListResult.Add(progeny);
                }
            }

            return progenyListResult;
        }

        public static async Task<int> GetAccessLevel(int progenyId)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                // If user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {
                    var result = await client.GetAsync("api/publicaccess/access/" + progenyId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var accessListString = await result.Content.ReadAsStringAsync();
                        List<UserAccess> accessList = JsonConvert.DeserializeObject<List<UserAccess>>(accessListString);
                        string email = await SecureStorage.GetAsync(Constants.UserEmailKey);
                        if (String.IsNullOrEmpty(email))
                        {
                            email = Constants.DummyEmail;
                        }

                        if (accessList != null)
                        {
                            UserAccess ua = accessList.SingleOrDefault(a => a.UserId.ToUpper() == email.ToUpper());
                            if (ua != null)
                            {
                                return ua.AccessLevel;
                            }
                        }

                    }
                }
                else // If user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/access/progeny/" + progenyId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var accessListString = await result.Content.ReadAsStringAsync();
                        List<UserAccess> accessList = JsonConvert.DeserializeObject<List<UserAccess>>(accessListString);
                        string email = await SecureStorage.GetAsync(Constants.UserEmailKey);
                        if (accessList != null)
                        {
                            UserAccess ua = accessList.SingleOrDefault(a => a.UserId.ToUpper() == email.ToUpper());
                            if (ua != null)
                            {
                                await SecureStorage.SetAsync("AccessLevel" + progenyId + email, ua.AccessLevel.ToString());
                                return ua.AccessLevel;
                            }
                        }
                    }
                    else
                    {
                        string email = await SecureStorage.GetAsync(Constants.UserEmailKey);
                        int al = 5;
                        int.TryParse(await SecureStorage.GetAsync("AccessLevel" + progenyId + email), out al);
                        return al;
                    }
                }
                return 5;
            }
            else
            {
                string email = await SecureStorage.GetAsync(Constants.UserEmailKey);
                int al = 5;
                int.TryParse(await SecureStorage.GetAsync("AccessLevel" + progenyId + email), out al);
                return al;
            }
        }

        public static async Task<Picture> GetRandomPicture(int progenyId, int userAccessLevel, string userTimeZone)
        {
            bool online = Online();
            if (!online)
            {
                return OfflineDefaultData.DefaultPicture;
                // Todo: Replace with random picture from cached data.

            }
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);
            }
            catch (Exception)
            {
                userTimeZone = TZConvert.WindowsToIana(userTimeZone);
            }

            var client = new HttpClient();
            client.BaseAddress = new Uri(Constants.MediaApiUrl);

            string accessToken = await UserService.GetAuthAccessToken();

            // If the user is not logged in.
            if (String.IsNullOrEmpty(accessToken))
            {
                var result = await client.GetAsync("api/publicaccess/randompicturemobile/" + progenyId + "/" + userAccessLevel).ConfigureAwait(false);

                if (result.IsSuccessStatusCode)
                {
                    var pictureString = await result.Content.ReadAsStringAsync();
                    Picture picture = JsonConvert.DeserializeObject<Picture>(pictureString);
                    if (userTimeZone != "")
                    {
                        if (picture.PictureTime.HasValue)
                        {
                            picture.PictureTime = TimeZoneInfo.ConvertTimeFromUtc(picture.PictureTime.Value,
                                TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                        }

                    }
                    ImageService.Instance.LoadUrl(picture.PictureLink).Preload();
                    return picture;
                }
                else
                {
                    Picture tempPicture = new Picture();
                    tempPicture.ProgenyId = 0;
                    tempPicture.Progeny = new Progeny();
                    tempPicture.AccessLevel = 5;
                    tempPicture.PictureLink600 = Constants.WebUrl + "/photodb/0/default_temp.jpg";
                    tempPicture.ProgenyId = progenyId;
                    tempPicture.PictureTime = new DateTime(2018, 9, 1, 12, 00, 00);
                    return tempPicture;
                }
            }
            else // If the user is logged in.
            {
                client.SetBearerToken(accessToken);
                
                var result = await client.GetAsync("api/pictures/randommobile/" + progenyId + "/" + userAccessLevel).ConfigureAwait(false);

                if (result.IsSuccessStatusCode)
                {
                    var pictureString = await result.Content.ReadAsStringAsync();
                    Picture picture = JsonConvert.DeserializeObject<Picture>(pictureString);
                    if (userTimeZone != "")
                    {
                        if (picture.PictureTime.HasValue)
                        {
                            picture.PictureTime = TimeZoneInfo.ConvertTimeFromUtc(picture.PictureTime.Value,
                                TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                        }

                    }
                    ImageService.Instance.LoadUrl(picture.PictureLink).Preload();
                    return picture;
                }
                else
                {
                    Picture tempPicture = new Picture();
                    tempPicture.Progeny = new Progeny();
                    tempPicture.AccessLevel = 5;
                    tempPicture.PictureLink600 = Constants.WebUrl + "/photodb/0/default_temp.jpg";
                    tempPicture.ProgenyId = progenyId;
                    tempPicture.PictureTime = new DateTime(2018, 9, 1, 12, 00, 00);
                    ImageService.Instance.LoadUrl(tempPicture.PictureLink).Preload();
                    return tempPicture;
                }
            }
        }

        public static async Task<List<CalendarItem>> GetUpcommingEventsList(int progenyId, int accessLevel)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                // If the user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {

                    var result = await client.GetAsync("api/publicaccess/eventlist/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var eventsString = await result.Content.ReadAsStringAsync();
                        List<CalendarItem> events = JsonConvert.DeserializeObject<List<CalendarItem>>(eventsString);

                        return events;
                    }
                    else
                    {
                        // Todo: Handle errors
                        return new List<CalendarItem>();
                    }
                }
                else // If the user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/calendar/eventlist/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var eventsString = await result.Content.ReadAsStringAsync();
                        List<CalendarItem> events = JsonConvert.DeserializeObject<List<CalendarItem>>(eventsString);
                        await SecureStorage.SetAsync("UpcommingEvents" + progenyId + "Al" + accessLevel, JsonConvert.SerializeObject(events));
                        return events;
                    }
                    else
                    {
                        // Todo: Handle errors
                        return new List<CalendarItem>();
                    }
                }
            }
            else
            {
                string eventListString =
                    await SecureStorage.GetAsync("UpcommingEvents" + progenyId + "Al" + accessLevel);
                List<CalendarItem> evtList = JsonConvert.DeserializeObject<List<CalendarItem>>(eventListString);
                return evtList;
            }
        }

        public static async Task<TimeLineItem> SaveTimeLineItem(TimeLineItem timeLineItem)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                var result = await client.PostAsync("api/timeline/", new StringContent(JsonConvert.SerializeObject(timeLineItem), System.Text.Encoding.UTF8, "application/json")).ConfigureAwait(false);
                if (result.IsSuccessStatusCode)
                {
                    string resultString = await result.Content.ReadAsStringAsync();
                    TimeLineItem resultTimelineItem = JsonConvert.DeserializeObject<TimeLineItem>(resultString);
                    return resultTimelineItem;
                }
            }

            return timeLineItem;
        }

        public static async Task<List<TimeLineItem>> GetTimeLine(int progenyId, int accessLevel, int count, int start, string timezone, DateTime startTime, string lastItemDateText)
        {
            bool online = Online();
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(timezone);
            }
            catch (Exception)
            {
                timezone = TZConvert.WindowsToIana(timezone);
            }
            string accessToken = await UserService.GetAuthAccessToken();
            List<TimeLineItem> timeLineLatest = new List<TimeLineItem>();
            
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                

                // If user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {
                    var result = await client.GetAsync("api/publicaccess/progenylatest/" + progenyId + "/" + accessLevel + "/" + count + "/" + start).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var timelineString = await result.Content.ReadAsStringAsync();
                        timeLineLatest = JsonConvert.DeserializeObject<List<TimeLineItem>>(timelineString);
                        await SecureStorage.SetAsync("Timeline" + progenyId + "Al" + accessLevel + "Cnt" + count + "Strt" + start, JsonConvert.SerializeObject(timeLineLatest));
                    }
                }
                else // If user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/timeline/progenylatestmobile/" + progenyId + "/" + accessLevel + "/" + count + "/" + start + "/" + startTime.Year + "/" + startTime.Month + "/" + startTime.Day).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var timelineString = await result.Content.ReadAsStringAsync();
                        timeLineLatest = JsonConvert.DeserializeObject<List<TimeLineItem>>(timelineString);
                        await SecureStorage.SetAsync("Timeline" + progenyId + "Al" + accessLevel + "Cnt" + count + "Strt" + start, JsonConvert.SerializeObject(timeLineLatest));
                    }
                }
            }
            else
            {
                string timlineListString = await SecureStorage.GetAsync("Timeline" + progenyId + "Al" + accessLevel + "Cnt" + count + "Strt" + start);
                timeLineLatest = JsonConvert.DeserializeObject<List<TimeLineItem>>(timlineListString);
            }
            string currentDate = lastItemDateText;
            List<TimeLineItem> resultList = new List<TimeLineItem>();
            foreach (TimeLineItem tItem in timeLineLatest)
            {
                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Photo)
                {
                    int.TryParse(tItem.ItemId, out int picId);
                    tItem.ItemObject = await GetPicture(picId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Video)
                {
                    int.TryParse(tItem.ItemId, out int vidId);
                    tItem.ItemObject = await GetVideo(vidId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Calendar)
                {
                    int.TryParse(tItem.ItemId, out int calId);
                    tItem.ItemObject = await GetCalendarItem(calId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Location)
                {
                    int.TryParse(tItem.ItemId, out int locId);
                    tItem.ItemObject = await GetLocation(locId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Vocabulary)
                {
                    int.TryParse(tItem.ItemId, out int vocId);
                    tItem.ItemObject = await GetVocabularyItem(vocId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Skill)
                {
                    int.TryParse(tItem.ItemId, out int skillId);
                    tItem.ItemObject = await GetSkill(skillId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Friend)
                {
                    int.TryParse(tItem.ItemId, out int frnId);
                    tItem.ItemObject = await GetFriend(frnId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Measurement)
                {
                    int.TryParse(tItem.ItemId, out int mesId);
                    tItem.ItemObject = await GetMeasurement(mesId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Sleep)
                {
                    int.TryParse(tItem.ItemId, out int slpId);
                    tItem.ItemObject = await GetSleep(slpId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Note)
                {
                    int.TryParse(tItem.ItemId, out int nteId);
                    Note note = await GetNote(nteId, accessToken, timezone);
                    note.Content = "<html><body>" + note.Content + "</body></html>";
                    tItem.ItemObject = note;

                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Contact)
                {
                    int.TryParse(tItem.ItemId, out int contId);
                    tItem.ItemObject = await GetContact(contId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Vaccination)
                {
                    int.TryParse(tItem.ItemId, out int vacId);
                    tItem.ItemObject = await GetVaccination(vacId, accessToken, timezone);
                }

                string itemDate = tItem.ProgenyTime.ToLongDateString();
                if (itemDate != currentDate)
                {
                    TimeLineItem headerItem = new TimeLineItem();
                    headerItem.ProgenyTime = tItem.ProgenyTime.Date;
                    headerItem.TimeLineId = 0;
                    headerItem.ItemType = 9999;
                    headerItem.ProgenyId = tItem.ProgenyId;
                    DateHeader dateHeader = new DateHeader();
                    dateHeader.DateText = itemDate;
                    headerItem.ItemObject = dateHeader;
                    currentDate = itemDate;
                    resultList.Add(headerItem);
                }
                resultList.Add(tItem);
            }

            return resultList;
        }

        public static async Task<List<TimeLineItem>> GetTimeLineYearAgo(int progenyId, int accessLevel, string timezone)
        {
            bool online = Online();
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(timezone);
            }
            catch (Exception)
            {
                timezone = TZConvert.WindowsToIana(timezone);
            }
            string accessToken = await UserService.GetAuthAccessToken();
            List<TimeLineItem> timeLineYearAgo = new List<TimeLineItem>();

            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);



                // If user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {
                    var result = await client.GetAsync("api/publicaccess/progenyyearago/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var timelineString = await result.Content.ReadAsStringAsync();
                        timeLineYearAgo = JsonConvert.DeserializeObject<List<TimeLineItem>>(timelineString);
                        await SecureStorage.SetAsync("YearAgo" + progenyId + "Al" + accessLevel + "Date" + DateTime.Today.DayOfYear, JsonConvert.SerializeObject(timeLineYearAgo));
                    }
                }
                else // If user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/timeline/progenyyearago/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var timelineString = await result.Content.ReadAsStringAsync();
                        timeLineYearAgo = JsonConvert.DeserializeObject<List<TimeLineItem>>(timelineString);
                        await SecureStorage.SetAsync("YearAgo" + progenyId + "Al" + accessLevel + "Date" + DateTime.Today.DayOfYear, JsonConvert.SerializeObject(timeLineYearAgo));
                    }
                }
            }
            else
            {
                string timlineListString = await SecureStorage.GetAsync("YearAgo" + progenyId + "Al" + accessLevel + "Date" + DateTime.Today.DayOfYear);
                timeLineYearAgo = JsonConvert.DeserializeObject<List<TimeLineItem>>(timlineListString);
            }

            List<TimeLineItem> resultList = new List<TimeLineItem>();
            foreach (TimeLineItem tItem in timeLineYearAgo)
            {
                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Photo)
                {
                    int.TryParse(tItem.ItemId, out int picId);
                    tItem.ItemObject = await GetPicture(picId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Video)
                {
                    int.TryParse(tItem.ItemId, out int vidId);
                    tItem.ItemObject = await GetVideo(vidId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Calendar)
                {
                    int.TryParse(tItem.ItemId, out int calId);
                    tItem.ItemObject = await GetCalendarItem(calId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Location)
                {
                    int.TryParse(tItem.ItemId, out int locId);
                    tItem.ItemObject = await GetLocation(locId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Vocabulary)
                {
                    int.TryParse(tItem.ItemId, out int vocId);
                    tItem.ItemObject = await GetVocabularyItem(vocId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Skill)
                {
                    int.TryParse(tItem.ItemId, out int skillId);
                    tItem.ItemObject = await GetSkill(skillId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Friend)
                {
                    int.TryParse(tItem.ItemId, out int frnId);
                    tItem.ItemObject = await GetFriend(frnId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Measurement)
                {
                    int.TryParse(tItem.ItemId, out int mesId);
                    tItem.ItemObject = await GetMeasurement(mesId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Sleep)
                {
                    int.TryParse(tItem.ItemId, out int slpId);
                    tItem.ItemObject = await GetSleep(slpId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Note)
                {
                    int.TryParse(tItem.ItemId, out int nteId);
                    Note note = await GetNote(nteId, accessToken, timezone);
                    note.Content = "<html><body>" + note.Content + "</body></html>";
                    tItem.ItemObject = note;

                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Contact)
                {
                    int.TryParse(tItem.ItemId, out int contId);
                    tItem.ItemObject = await GetContact(contId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Vaccination)
                {
                    int.TryParse(tItem.ItemId, out int vacId);
                    tItem.ItemObject = await GetVaccination(vacId, accessToken, timezone);
                }
                
                resultList.Add(tItem);
            }

            return resultList;
        }
        
        public static async Task<List<TimeLineItem>> GetLatestPosts(int progenyId, int accessLevel, string timezone)
        {
            bool online = Online();

            List<TimeLineItem> timeLineLatest = new List<TimeLineItem>();

            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(timezone);
            }
            catch (Exception)
            {
                timezone = TZConvert.WindowsToIana(timezone);
            }

            var client = new HttpClient();
            client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

            string accessToken = await UserService.GetAuthAccessToken();
            DateTime startTime = DateTime.UtcNow;
            // If user is not logged in.
            if (String.IsNullOrEmpty(accessToken))
            {
                if (online)
                {
                    var result = await client
                        .GetAsync("api/publicaccess/progenylatest/" + progenyId + "/" + accessLevel + "/" + 5 + "/" + 0 + "/" + startTime.Year + "/" + startTime.Month + "/" + startTime.Day)
                        .ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var timelineString = await result.Content.ReadAsStringAsync();
                        timeLineLatest = JsonConvert.DeserializeObject<List<TimeLineItem>>(timelineString);
                        await SecureStorage.SetAsync("TimelineLatest" + progenyId + "Al" + accessLevel, timelineString);
                    }
                }
                else
                {
                    string timelineString =
                        await SecureStorage.GetAsync("TimelineLatest" + progenyId + "Al" + accessLevel);
                    timeLineLatest = JsonConvert.DeserializeObject<List<TimeLineItem>>(timelineString);
                }
            }
            else // If user is logged in.
            {

                if (online)
                {
                    client.SetBearerToken(accessToken);

                    var result = await client
                        .GetAsync("api/timeline/progenylatestmobile/" + progenyId + "/" + accessLevel + "/" + 5 + "/" + 0 + "/" + startTime.Year + "/" + startTime.Month + "/" + startTime.Day)
                        .ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var timelineString = await result.Content.ReadAsStringAsync();
                        timeLineLatest = JsonConvert.DeserializeObject<List<TimeLineItem>>(timelineString);
                        await SecureStorage.SetAsync("TimelineLatest" + progenyId + "Al" + accessLevel, timelineString);

                    }
                }
                else
                {
                    string timelineString =
                        await SecureStorage.GetAsync("TimelineLatest" + progenyId + "Al" + accessLevel);
                    timeLineLatest = JsonConvert.DeserializeObject<List<TimeLineItem>>(timelineString);
                }

            }
            
            foreach (TimeLineItem tItem in timeLineLatest)
            {
                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Photo)
                {
                    int.TryParse(tItem.ItemId, out int picId);
                    tItem.ItemObject = await GetPicture(picId, accessToken, timezone).ConfigureAwait(false);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Video)
                {
                    int.TryParse(tItem.ItemId, out int vidId);
                    tItem.ItemObject = await GetVideo(vidId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Calendar)
                {
                    int.TryParse(tItem.ItemId, out int calId);
                    tItem.ItemObject = await GetCalendarItem(calId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Location)
                {
                    int.TryParse(tItem.ItemId, out int locId);
                    tItem.ItemObject = await GetLocation(locId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Vocabulary)
                {
                    int.TryParse(tItem.ItemId, out int vocId);
                    tItem.ItemObject = await GetVocabularyItem(vocId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Skill)
                {
                    int.TryParse(tItem.ItemId, out int skillId);
                    tItem.ItemObject = await GetSkill(skillId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Friend)
                {
                    int.TryParse(tItem.ItemId, out int frnId);
                    tItem.ItemObject = await GetFriend(frnId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Measurement)
                {
                    int.TryParse(tItem.ItemId, out int mesId);
                    tItem.ItemObject = await GetMeasurement(mesId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Sleep)
                {
                    int.TryParse(tItem.ItemId, out int slpId);
                    tItem.ItemObject = await GetSleep(slpId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Note)
                {
                    int.TryParse(tItem.ItemId, out int nteId);
                    Note note = await GetNote(nteId, accessToken, timezone);
                    note.Content = "<html><body>" + note.Content + "</body></html>";
                    tItem.ItemObject = note;
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Contact)
                {
                    int.TryParse(tItem.ItemId, out int contId);
                    tItem.ItemObject = await GetContact(contId, accessToken, timezone);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Vaccination)
                {
                    int.TryParse(tItem.ItemId, out int vacId);
                    tItem.ItemObject = await GetVaccination(vacId, accessToken, timezone);
                }
            }
            return timeLineLatest;
        }

        public static async Task<Picture> GetPicture(int pictureId, string accessToken, string userTimezone)
        {
            bool online = Online();
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
            }
            catch (Exception)
            {
                userTimezone = TZConvert.WindowsToIana(userTimezone);
            }

            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.MediaApiUrl);

                // If the user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {

                    var result = await client.GetAsync("api/publicaccess/getpicturemobile/" + pictureId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var pictureString = await result.Content.ReadAsStringAsync();
                        Picture picture = JsonConvert.DeserializeObject<Picture>(pictureString);
                        if (picture.PictureTime.HasValue)
                        {
                            picture.PictureTime = TimeZoneInfo.ConvertTimeFromUtc(picture.PictureTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        }
                        ImageService.Instance.LoadUrl(picture.PictureLink600).DownSample(height: 440, allowUpscale: true).Preload();
                        await SecureStorage.SetAsync("Picture" + pictureId,
                            JsonConvert.SerializeObject(picture));
                        return picture;
                    }
                    else
                    {
                        return new Picture();
                    }
                }
                else // If the user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/pictures/getpicturemobile/" + pictureId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var pictureString = await result.Content.ReadAsStringAsync();
                        Picture picture = JsonConvert.DeserializeObject<Picture>(pictureString);
                        if (picture.PictureTime.HasValue)
                        {
                            picture.PictureTime = TimeZoneInfo.ConvertTimeFromUtc(picture.PictureTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        }
                        ImageService.Instance.LoadUrl(picture.PictureLink600).DownSample(height: 440, allowUpscale: true).Preload();
                        await SecureStorage.SetAsync("Picture" + pictureId,
                            JsonConvert.SerializeObject(picture));
                        return picture;
                    }
                    else
                    {
                        return new Picture();
                    }
                }
            }
            else
            {
                string picturestring = await SecureStorage.GetAsync("Picture" + pictureId);
                Picture picture = JsonConvert.DeserializeObject<Picture>(picturestring);
                return picture;
            }
        }

        public static async Task<string> UploadPictureFile(int progenyId, string fileName)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.MediaApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
             
                var fileBytes = File.ReadAllBytes(fileName);
                MemoryStream stream = new MemoryStream(fileBytes);
                HttpContent fileStreamContent = new StreamContent(stream);

                fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "file", FileName = "file" };
                fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Add(fileStreamContent);
                var result = await client.PostAsync("api/pictures/uploadpicture/", content).ConfigureAwait(false);

                if (result.IsSuccessStatusCode)
                {
                    string resultString = await result.Content.ReadAsStringAsync();
                    string pictureResultString = Regex.Replace(resultString, @"([|""|])", "");
                    return pictureResultString;
                }
            }

            return "";
        }

        public static async Task<Picture> SavePicture(Picture picture)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.MediaApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                var result = await client.PostAsync("api/pictures/", new StringContent(JsonConvert.SerializeObject(picture), System.Text.Encoding.UTF8, "application/json")).ConfigureAwait(false);
                if (result.IsSuccessStatusCode)
                {
                    string resultString = await result.Content.ReadAsStringAsync();
                    Picture resultPicture = JsonConvert.DeserializeObject<Picture>(resultString);
                    return resultPicture;
                }
            }

            return picture;
        }

        public static async Task<PicturePage> GetPicturePage(int pageNumber, int pageSize, int progenyId, int userAccessLevel, string userTimezone, int sortBy, string tagFilter)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
            }
            catch (Exception)
            {
                userTimezone = TZConvert.WindowsToIana(userTimezone);
            }

            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.MediaApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                // If user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {

                    string pageApiPath = "api/publicaccess/pagemobile?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + progenyId + "&accessLevel=" + userAccessLevel + "&tagFilter=" + tagFilter + "&sortBy=" + sortBy;
                    var result = await client.GetAsync(pageApiPath).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var picturePageString = await result.Content.ReadAsStringAsync();
                        PicturePage picturePage = JsonConvert.DeserializeObject<PicturePage>(picturePageString);
                        foreach (Picture picture in picturePage.PicturesList)
                        {
                            if (picture.PictureTime.HasValue)
                            {
                                picture.PictureTime = TimeZoneInfo.ConvertTimeFromUtc(picture.PictureTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                            }

                            picture.CommentsCount = picture.Comments.Count;
                            ImageService.Instance.LoadUrl(picture.PictureLink600).DownSample(height: 440, allowUpscale: true).Preload();
                            await SecureStorage.SetAsync("Picture" + picture.PictureId, JsonConvert.SerializeObject(picture));
                        }
                        await SecureStorage.SetAsync("PicturePage" + progenyId + "Page" + pageNumber + "Size" + pageSize, JsonConvert.SerializeObject(picturePage));
                        return picturePage;
                    }
                    else
                    {
                        return new PicturePage();
                    }
                }
                else // If user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    string pageApiPath = "api/pictures/pagemobile?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + progenyId + "&accessLevel=" + userAccessLevel + "&tagFilter=" + tagFilter + "&sortBy=" + sortBy;
                    var result = await client.GetAsync(pageApiPath).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var picturePageString = await result.Content.ReadAsStringAsync();
                        PicturePage picturePage = JsonConvert.DeserializeObject<PicturePage>(picturePageString);
                        foreach (Picture picture in picturePage.PicturesList)
                        {
                            if (picture.PictureTime.HasValue)
                            {
                                picture.PictureTime = TimeZoneInfo.ConvertTimeFromUtc(picture.PictureTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                            }
                            picture.CommentsCount = picture.Comments.Count;
                            ImageService.Instance.LoadUrl(picture.PictureLink600).DownSample(height: 440, allowUpscale: true).Preload();
                            await SecureStorage.SetAsync("Picture" + picture.PictureId, JsonConvert.SerializeObject(picture));
                        }
                        
                        await SecureStorage.SetAsync("PicturePage" + progenyId + "Page" + pageNumber + "Size" + pageSize, JsonConvert.SerializeObject(picturePage));
                        return picturePage;
                    }
                    else
                    {
                        return new PicturePage();
                    }
                }
            }
            else
            {
                string picturePageString = await SecureStorage.GetAsync("PicturePage" + progenyId + "Page" + pageNumber + "Size" + pageSize);
                PicturePage picturePage = JsonConvert.DeserializeObject<PicturePage>(picturePageString);
                return picturePage;
            }
        }

        public static async Task<PictureViewModel> GetPictureViewModel(int pictureId, int userAccessLevel, string userTimezone, int sortBy)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
            }
            catch (Exception)
            {
                userTimezone = TZConvert.WindowsToIana(userTimezone);
            }

            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.MediaApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                // If user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {

                    string pictureViewApiPath = "api/publicaccess/pictureviewmodelmobile/" + pictureId + "/" + userAccessLevel + "?sortBy=" + sortBy;
                    var result = await client.GetAsync(pictureViewApiPath).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var pictureViewModelString = await result.Content.ReadAsStringAsync();
                        PictureViewModel pictureViewModel = JsonConvert.DeserializeObject<PictureViewModel>(pictureViewModelString);
                        if (pictureViewModel.PictureTime.HasValue)
                        {
                            pictureViewModel.PictureTime = TimeZoneInfo.ConvertTimeFromUtc(pictureViewModel.PictureTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        }

                        pictureViewModel.CommentsCount = pictureViewModel.CommentsList.Count;
                        ImageService.Instance.LoadUrl(pictureViewModel.PictureLink).DownSample(height: 440, allowUpscale: true).Preload();
                        await SecureStorage.SetAsync("PictureViewModel" + pictureId, JsonConvert.SerializeObject(pictureViewModel));


                        return pictureViewModel;
                    }
                    else
                    {
                        return new PictureViewModel();
                    }
                }
                else // If user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    string pictureViewApiPath = "api/pictures/pictureviewmodelmobile/" + pictureId + "/" + userAccessLevel + "?sortBy=" + sortBy;
                    var result = await client.GetAsync(pictureViewApiPath).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var pictureViewModelString = await result.Content.ReadAsStringAsync();
                        PictureViewModel pictureViewModel = JsonConvert.DeserializeObject<PictureViewModel>(pictureViewModelString);
                        if (pictureViewModel.PictureTime.HasValue)
                        {
                            pictureViewModel.PictureTime = TimeZoneInfo.ConvertTimeFromUtc(pictureViewModel.PictureTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        }

                        pictureViewModel.CommentsCount = pictureViewModel.CommentsList.Count;
                        ImageService.Instance.LoadUrl(pictureViewModel.PictureLink).DownSample(height: 440, allowUpscale: true).Preload();
                        await SecureStorage.SetAsync("PictureViewModel" + pictureId, JsonConvert.SerializeObject(pictureViewModel));


                        return pictureViewModel;
                    }
                    else
                    {
                        return new PictureViewModel();
                    }
                }
            }
            else
            {
                string pictureViewString = await SecureStorage.GetAsync("PictureViewModel" + pictureId);
                PictureViewModel pictureViewModel = JsonConvert.DeserializeObject<PictureViewModel>(pictureViewString);
                return pictureViewModel;
            }
        }

        public static async Task<Video> GetVideo(int videoId, string accessToken, string userTimezone)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
            }
            catch (Exception)
            {
                userTimezone = TZConvert.WindowsToIana(userTimezone);
            }
            bool online = Online();

            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.MediaApiUrl);

                // If user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {

                    var result = await client.GetAsync("api/publicaccess/getvideomobile/" + videoId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var videoString = await result.Content.ReadAsStringAsync();
                        Video video = JsonConvert.DeserializeObject<Video>(videoString);
                        if (video.VideoTime.HasValue)
                        {
                            video.VideoTime = TimeZoneInfo.ConvertTimeFromUtc(video.VideoTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        }
                        await SecureStorage.SetAsync("Video" + videoId, JsonConvert.SerializeObject(video));
                        return video;
                    }
                    else
                    {
                        return new Video();
                    }
                }
                else // If user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/videos/getvideomobile/" + videoId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var videoString = await result.Content.ReadAsStringAsync();
                        Video video = JsonConvert.DeserializeObject<Video>(videoString);
                        if (video.VideoTime.HasValue)
                        {
                            video.VideoTime = TimeZoneInfo.ConvertTimeFromUtc(video.VideoTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        }
                        await SecureStorage.SetAsync("Video" + videoId, JsonConvert.SerializeObject(video));
                        return video;
                    }
                    else
                    {
                        return new Video();
                    }
                }
            }
            else
            {
                string videostring = await SecureStorage.GetAsync("Video" + videoId);
                Video video = JsonConvert.DeserializeObject<Video>(videostring);
                return video;
            }
        }

        public static async Task<VideosPageModel> GetVideoPage(int pageNumber, int pageSize, int progenyId, int userAccessLevel, string userTimezone, int sortBy)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
            }
            catch (Exception)
            {
                userTimezone = TZConvert.WindowsToIana(userTimezone);
            }

            var client = new HttpClient();
            client.BaseAddress = new Uri(Constants.MediaApiUrl);

            string accessToken = await UserService.GetAuthAccessToken();

            // If user is not logged in.
            if (String.IsNullOrEmpty(accessToken))
            {
                
                string pageApiPath = "api/publicaccess/videopagemobile?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + progenyId + "&accessLevel=" + userAccessLevel + "&sortBy=" + sortBy;
                var result = await client.GetAsync(pageApiPath).ConfigureAwait(false); 

                if (result.IsSuccessStatusCode)
                {
                    var videoPageString = await result.Content.ReadAsStringAsync();
                    VideosPageModel videoPage = JsonConvert.DeserializeObject<VideosPageModel>(videoPageString);
                    foreach (Video video in videoPage.VideosList)
                    {
                        if (video.VideoTime.HasValue)
                        {
                            video.VideoTime = TimeZoneInfo.ConvertTimeFromUtc(video.VideoTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        }
                    }

                    return videoPage;
                }
                else
                {
                    return new VideosPageModel();
                }
            }
            else // User is logged in.
            {
                client.SetBearerToken(accessToken);

                string pageApiPath = "api/videos/pagemobile?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + progenyId + "&accessLevel=" + userAccessLevel + "&sortBy=" + sortBy;
                var result = await client.GetAsync(pageApiPath).ConfigureAwait(false); 

                if (result.IsSuccessStatusCode)
                {
                    var videoPageString = await result.Content.ReadAsStringAsync();
                    VideosPageModel videoPage = JsonConvert.DeserializeObject<VideosPageModel>(videoPageString);
                    foreach (Video video in videoPage.VideosList)
                    {
                        if (video.VideoTime.HasValue)
                        {
                            video.VideoTime = TimeZoneInfo.ConvertTimeFromUtc(video.VideoTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        }
                    }

                    return videoPage;
                }
                else
                {
                    return new VideosPageModel();
                }
            }
        }

        public static async Task<CalendarItem> GetCalendarItem(int calendarId, string accessToken, string userTimeZone)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);
            }
            catch (Exception)
            {
                userTimeZone = TZConvert.WindowsToIana(userTimeZone);
            }
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                // User is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {
                    var result = await client.GetAsync("api/publicaccess/getcalendaritemmobile/" + calendarId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var calendarString = await result.Content.ReadAsStringAsync();
                        CalendarItem calItem = JsonConvert.DeserializeObject<CalendarItem>(calendarString);
                        if (calItem.StartTime != null)
                        {
                            calItem.StartTime = TimeZoneInfo.ConvertTimeFromUtc(calItem.StartTime.Value,
                                TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                            if (calItem.EndTime != null)
                            {
                                calItem.EndTime = TimeZoneInfo.ConvertTimeFromUtc(calItem.EndTime.Value,
                                    TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                                calItem.StartString = calItem.StartTime.Value.ToString("dd-MMM-yyyy HH:mm") + " - " +
                                                      calItem.EndTime.Value.ToString("dd-MMM-yyyy HH:mm");
                            }
                        }
                        await SecureStorage.SetAsync("CalendarItem" + calendarId, JsonConvert.SerializeObject(calItem));
                        return calItem;
                    }
                    else
                    {
                        return new CalendarItem();
                    }
                }
                else // User is logged in.
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/calendar/" + calendarId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var calendarString = await result.Content.ReadAsStringAsync();
                        CalendarItem calItem = JsonConvert.DeserializeObject<CalendarItem>(calendarString);
                        if (calItem.StartTime != null)
                        {
                            calItem.StartTime = TimeZoneInfo.ConvertTimeFromUtc(calItem.StartTime.Value,
                                TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                            if (calItem.EndTime != null)
                            {
                                calItem.EndTime = TimeZoneInfo.ConvertTimeFromUtc(calItem.EndTime.Value,
                                    TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                                calItem.StartString = calItem.StartTime.Value.ToString("dd-MMM-yyyy HH:mm") + " - " +
                                                      calItem.EndTime.Value.ToString("dd-MMM-yyyy HH:mm");
                            }
                        }
                        await SecureStorage.SetAsync("CalendarItem" + calendarId, JsonConvert.SerializeObject(calItem));
                        return calItem;
                    }
                    else
                    {
                        return new CalendarItem();
                    }
                }
            }
            else
            {
                string calendarString = await SecureStorage.GetAsync("CalendarItem" + calendarId);
                CalendarItem calItem = JsonConvert.DeserializeObject<CalendarItem>(calendarString);
                return calItem;
            }

        }

        public static async Task<List<CalendarItem>> GetProgenyCalendar(int progenyId, int accessLevel, string userTimeZone)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);
            }
            catch (Exception)
            {
                userTimeZone = TZConvert.WindowsToIana(userTimeZone);
            }

            var client = new HttpClient();
            client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

            string accessToken = await UserService.GetAuthAccessToken();

            // If user is not logged in.
            if (String.IsNullOrEmpty(accessToken))
            {
                
                var result = await client.GetAsync("api/publicaccess/progenycalendarmobile/" + progenyId + "/" + accessLevel).ConfigureAwait(false); 

                if (result.IsSuccessStatusCode)
                {
                    var calendarString = await result.Content.ReadAsStringAsync();
                    List<CalendarItem> calList = JsonConvert.DeserializeObject<List<CalendarItem>>(calendarString);
                    if (calList.Any())
                    {
                        foreach (CalendarItem calItem in calList)
                        {
                            if (calItem.StartTime != null)
                            {
                                calItem.StartTime = TimeZoneInfo.ConvertTimeFromUtc(calItem.StartTime.Value,
                                    TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                                if (calItem.EndTime != null)
                                {
                                    calItem.EndTime = TimeZoneInfo.ConvertTimeFromUtc(calItem.EndTime.Value,
                                        TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                                    calItem.StartString =
                                        calItem.StartTime.Value.ToString("dd-MMM-yyyy HH:mm") + " - " +
                                        calItem.EndTime.Value.ToString("dd-MMM-yyyy HH:mm");
                                }
                            }
                        }
                    }
                    return calList;
                }
                else
                {
                    return new List<CalendarItem>();
                }
            }
            else // User is logged in.
            {
                client.SetBearerToken(accessToken);
                
                var result = await client.GetAsync("api/calendar/progeny/" + progenyId + "/" + accessLevel).ConfigureAwait(false); 

                if (result.IsSuccessStatusCode)
                {
                    var calendarString = await result.Content.ReadAsStringAsync();
                    List<CalendarItem> calList = JsonConvert.DeserializeObject<List<CalendarItem>>(calendarString);
                    if (calList.Any())
                    {
                        foreach (CalendarItem calItem in calList)
                        {
                            if (calItem.StartTime != null)
                            {
                                calItem.StartTime = TimeZoneInfo.ConvertTimeFromUtc(calItem.StartTime.Value,
                                    TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                                if (calItem.EndTime != null)
                                {
                                    calItem.EndTime = TimeZoneInfo.ConvertTimeFromUtc(calItem.EndTime.Value,
                                        TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                                    calItem.StartString =
                                        calItem.StartTime.Value.ToString("dd-MMM-yyyy HH:mm") + " - " +
                                        calItem.EndTime.Value.ToString("dd-MMM-yyyy HH:mm");
                                }
                            }
                        }
                    }
                    return calList;
                }
                else
                {
                    return new List<CalendarItem>();
                }
            }
        }

        public static async Task<Location> GetLocation(int locationId, string accessToken, string userTimezone)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
            }
            catch (Exception)
            {
                userTimezone = TZConvert.WindowsToIana(userTimezone);
            }
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                // If the user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {

                    var result = await client.GetAsync("api/publicaccess/getlocationmobile/" + locationId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var locationString = await result.Content.ReadAsStringAsync();
                        Location locItem = JsonConvert.DeserializeObject<Location>(locationString);
                        if (locItem.Date.HasValue)
                        {
                            locItem.Date = TimeZoneInfo.ConvertTimeFromUtc(locItem.Date.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        }
                        await SecureStorage.SetAsync("LocationItem" + locationId, JsonConvert.SerializeObject(locItem));
                        return locItem;
                    }
                    else
                    {
                        return new Location();
                    }
                }
                else // User is logged in.
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/locations/" + locationId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var locationString = await result.Content.ReadAsStringAsync();
                        Location locItem = JsonConvert.DeserializeObject<Location>(locationString);
                        if (locItem.Date.HasValue)
                        {
                            locItem.Date = TimeZoneInfo.ConvertTimeFromUtc(locItem.Date.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        }
                        await SecureStorage.SetAsync("LocationItem" + locationId, JsonConvert.SerializeObject(locItem));
                        return locItem;
                    }
                    else
                    {
                        return new Location();
                    }
                }
            }
            else
            {
                string locationString = await SecureStorage.GetAsync("LocationItem" + locationId);
                Location locItem = JsonConvert.DeserializeObject<Location>(locationString);
                return locItem;
            }
            
        }

        public static async Task<VocabularyItem> GetVocabularyItem(int vocabularyId, string accessToken, string userTimezone)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
            }
            catch (Exception)
            {
                userTimezone = TZConvert.WindowsToIana(userTimezone);
            }
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                // If user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {

                    var result = await client.GetAsync("api/publicaccess/getvocabularyitemmobile/" + vocabularyId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var vocabularyString = await result.Content.ReadAsStringAsync();
                        VocabularyItem vocItem = JsonConvert.DeserializeObject<VocabularyItem>(vocabularyString);
                        if (vocItem.Date.HasValue)
                        {
                            vocItem.Date = TimeZoneInfo.ConvertTimeFromUtc(vocItem.Date.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        }
                        await SecureStorage.SetAsync("VocabularyItem" + vocabularyId, JsonConvert.SerializeObject(vocItem));
                        return vocItem;
                    }
                    else
                    {
                        return new VocabularyItem();
                    }
                }
                else  // User is logged in.
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/vocabulary/" + vocabularyId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var vocabularyString = await result.Content.ReadAsStringAsync();
                        VocabularyItem vocItem = JsonConvert.DeserializeObject<VocabularyItem>(vocabularyString);
                        if (vocItem.Date.HasValue)
                        {
                            vocItem.Date = TimeZoneInfo.ConvertTimeFromUtc(vocItem.Date.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        }
                        await SecureStorage.SetAsync("VocabularyItem" + vocabularyId, JsonConvert.SerializeObject(vocItem));
                        return vocItem;
                    }
                    else
                    {
                        return new VocabularyItem();
                    }
                }
            }
            else
            {
                string vocabularyString = await SecureStorage.GetAsync("VocabularyItem" + vocabularyId);
                VocabularyItem vocItem = JsonConvert.DeserializeObject<VocabularyItem>(vocabularyString);
                return vocItem;
            }
        }

        public static async Task<Skill> GetSkill(int skillId, string accessToken, string userTimezone)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
            }
            catch (Exception)
            {
                userTimezone = TZConvert.WindowsToIana(userTimezone);
            }
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                // User is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {
                    var result = await client.GetAsync("api/publicaccess/getskillmobile/" + skillId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var skillString = await result.Content.ReadAsStringAsync();
                        Skill skillItem = JsonConvert.DeserializeObject<Skill>(skillString);
                        if (skillItem.SkillFirstObservation.HasValue)
                        {
                            skillItem.SkillFirstObservation = TimeZoneInfo.ConvertTimeFromUtc(skillItem.SkillFirstObservation.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        }
                        await SecureStorage.SetAsync("Skill" + skillId, JsonConvert.SerializeObject(skillItem));
                        return skillItem;
                    }
                    else
                    {
                        return new Skill();
                    }
                }
                else // User is logged in.
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/skills/" + skillId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var skillString = await result.Content.ReadAsStringAsync();
                        Skill skillItem = JsonConvert.DeserializeObject<Skill>(skillString);
                        if (skillItem.SkillFirstObservation.HasValue)
                        {
                            skillItem.SkillFirstObservation = TimeZoneInfo.ConvertTimeFromUtc(skillItem.SkillFirstObservation.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        }
                        await SecureStorage.SetAsync("Skill" + skillId, JsonConvert.SerializeObject(skillItem));
                        return skillItem;
                    }
                    else
                    {
                        return new Skill();
                    }
                }
            }
            else
            {
                string skillString = await SecureStorage.GetAsync("Skill" + skillId);
                Skill skillItem = JsonConvert.DeserializeObject<Skill>(skillString);
                return skillItem;
            }
        }

        public static async Task<Friend> GetFriend(int frnId, string accessToken, string userTimezone)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
            }
            catch (Exception)
            {
                userTimezone = TZConvert.WindowsToIana(userTimezone);
            }
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                // User is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {

                    var result = await client.GetAsync("api/publicaccess/getfriendmobile/" + frnId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var friendString = await result.Content.ReadAsStringAsync();
                        Friend friendItem = JsonConvert.DeserializeObject<Friend>(friendString);
                        if (friendItem.FriendSince.HasValue)
                        {
                            friendItem.FriendSince = TimeZoneInfo.ConvertTimeFromUtc(friendItem.FriendSince.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        }
                        await SecureStorage.SetAsync("Friend" + frnId, JsonConvert.SerializeObject(friendItem));
                        ImageService.Instance.LoadUrl(friendItem.PictureLink).Preload();
                        return friendItem;
                    }
                    else
                    {
                        return new Friend();
                    }
                }
                else // User is logged in.
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/friends/getfriendmobile/" + frnId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var friendString = await result.Content.ReadAsStringAsync();
                        Friend friendItem = JsonConvert.DeserializeObject<Friend>(friendString);
                        if (friendItem.FriendSince.HasValue)
                        {
                            friendItem.FriendSince = TimeZoneInfo.ConvertTimeFromUtc(friendItem.FriendSince.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        }
                        
                        await SecureStorage.SetAsync("Friend" + frnId, JsonConvert.SerializeObject(friendItem));
                        ImageService.Instance.LoadUrl(friendItem.PictureLink).Preload();
                        return friendItem;
                    }
                    else
                    {
                        return new Friend();
                    }
                }
            }
            else
            {
                string friendString = await SecureStorage.GetAsync("Friend" + frnId);
                Friend friendItem = JsonConvert.DeserializeObject<Friend>(friendString);
                return friendItem;
            }
        }

        public static async Task<Measurement> GetMeasurement(int mesId, string accessToken, string userTimezone)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
            }
            catch (Exception)
            {
                userTimezone = TZConvert.WindowsToIana(userTimezone);
            }
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                // User is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {

                    var result = await client.GetAsync("api/publicaccess/getmeasurementmobile/" + mesId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var measurementString = await result.Content.ReadAsStringAsync();
                        Measurement mesItem = JsonConvert.DeserializeObject<Measurement>(measurementString);
                        mesItem.Date =
                            TimeZoneInfo.ConvertTimeFromUtc(mesItem.Date, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        await SecureStorage.SetAsync("Measurement" + mesId, JsonConvert.SerializeObject(mesItem));
                        return mesItem;
                    }
                    else
                    {
                        return new Measurement();
                    }
                }
                else // User is logged in.
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/measurements/" + mesId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var measurementString = await result.Content.ReadAsStringAsync();
                        Measurement mesItem = JsonConvert.DeserializeObject<Measurement>(measurementString);
                        mesItem.Date =
                            TimeZoneInfo.ConvertTimeFromUtc(mesItem.Date, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        await SecureStorage.SetAsync("Measurement" + mesId, JsonConvert.SerializeObject(mesItem));
                        return mesItem;
                    }
                    else
                    {
                        return new Measurement();
                    }
                }
            }
            else
            {
                string measurementString = await SecureStorage.GetAsync("Measurement" + mesId);
                Measurement mesItem = JsonConvert.DeserializeObject<Measurement>(measurementString);
                return mesItem;
            }
        }

        public static async Task<Sleep> GetSleep(int slpId, string accessToken, string userTimeZone)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);
            }
            catch (Exception)
            {
                userTimeZone = TZConvert.WindowsToIana(userTimeZone);
            }
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                // User is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {
                    var result = await client.GetAsync("api/publicaccess/getsleepmobile/" + slpId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var sleepString = await result.Content.ReadAsStringAsync();
                        Sleep slpItem = JsonConvert.DeserializeObject<Sleep>(sleepString);

                        slpItem.SleepStart = TimeZoneInfo.ConvertTimeFromUtc(slpItem.SleepStart, TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                        slpItem.SleepEnd = TimeZoneInfo.ConvertTimeFromUtc(slpItem.SleepEnd, TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                        slpItem.StartString = slpItem.SleepStart.ToString("dd-MMM-yyyy HH:mm") + " - " + slpItem.SleepEnd.ToString("dd-MMM-yyyy HH:mm");
                        DateTimeOffset sOffset = new DateTimeOffset(slpItem.SleepStart,
                            TimeZoneInfo.FindSystemTimeZoneById(userTimeZone).GetUtcOffset(slpItem.SleepStart));
                        DateTimeOffset eOffset = new DateTimeOffset(slpItem.SleepEnd,
                            TimeZoneInfo.FindSystemTimeZoneById(userTimeZone).GetUtcOffset(slpItem.SleepEnd));
                        slpItem.SleepDuration = eOffset - sOffset;
                        slpItem.EndString = slpItem.SleepDuration.ToString();
                        await SecureStorage.SetAsync("Sleep" + slpId, JsonConvert.SerializeObject(slpItem));
                        return slpItem;
                    }
                    else
                    {
                        return new Sleep();
                    }
                }
                else // User is logged in.
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/sleep/" + slpId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var sleepString = await result.Content.ReadAsStringAsync();
                        Sleep slpItem = JsonConvert.DeserializeObject<Sleep>(sleepString);

                        slpItem.SleepStart = TimeZoneInfo.ConvertTimeFromUtc(slpItem.SleepStart, TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                        slpItem.SleepEnd = TimeZoneInfo.ConvertTimeFromUtc(slpItem.SleepEnd, TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                        slpItem.StartString = slpItem.SleepStart.ToString("dd-MMM-yyyy HH:mm") + " - " + slpItem.SleepEnd.ToString("dd-MMM-yyyy HH:mm");
                        DateTimeOffset sOffset = new DateTimeOffset(slpItem.SleepStart,
                            TimeZoneInfo.FindSystemTimeZoneById(userTimeZone).GetUtcOffset(slpItem.SleepStart));
                        DateTimeOffset eOffset = new DateTimeOffset(slpItem.SleepEnd,
                            TimeZoneInfo.FindSystemTimeZoneById(userTimeZone).GetUtcOffset(slpItem.SleepEnd));
                        slpItem.SleepDuration = eOffset - sOffset;
                        slpItem.EndString = slpItem.SleepDuration.ToString();
                        await SecureStorage.SetAsync("Sleep" + slpId, JsonConvert.SerializeObject(slpItem));
                        return slpItem;
                    }
                    else
                    {
                        return new Sleep();
                    }
                }
            }
            else
            {
                string sleepString = await SecureStorage.GetAsync("Sleep" + slpId);
                Sleep slpItem = JsonConvert.DeserializeObject<Sleep>(sleepString);
                return slpItem;
            }
        }

        public static async Task<Sleep> SaveSleep(Sleep sleep)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(sleep.Progeny.TimeZone);
            }
            catch (Exception)
            {
                sleep.Progeny.TimeZone = TZConvert.WindowsToIana(sleep.Progeny.TimeZone);
            }
            sleep.SleepStart = TimeZoneInfo.ConvertTimeToUtc(sleep.SleepStart, TimeZoneInfo.FindSystemTimeZoneById(sleep.Progeny.TimeZone));
            sleep.SleepEnd = TimeZoneInfo.ConvertTimeToUtc(sleep.SleepEnd, TimeZoneInfo.FindSystemTimeZoneById(sleep.Progeny.TimeZone));
            var client = new HttpClient();
            client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
            string accessToken = await UserService.GetAuthAccessToken();
            client.SetBearerToken(accessToken);
            var result = await client.PostAsync("api/sleep/", new StringContent(JsonConvert.SerializeObject(sleep), System.Text.Encoding.UTF8, "application/json")).ConfigureAwait(false);
            if (result.IsSuccessStatusCode)
            {
                string resultString = await result.Content.ReadAsStringAsync();
                Sleep resultSleep = JsonConvert.DeserializeObject<Sleep>(resultString);
                return resultSleep;
            }

            return sleep;
        }
        public static async Task<List<Sleep>> GetSleepList(int progenyId, int accessLevel, string userTimeZone, int start)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);
            }
            catch (Exception)
            {
                userTimeZone = TZConvert.WindowsToIana(userTimeZone);
            }
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                // User is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {

                    var result = await client.GetAsync("api/publicaccess/getsleeplistmobile/" + progenyId + "/" + accessLevel + "/" + start).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var sleepString = await result.Content.ReadAsStringAsync();
                        List<Sleep> sleepList = JsonConvert.DeserializeObject<List<Sleep>>(sleepString);

                        foreach (Sleep slpItem in sleepList)
                        {
                            slpItem.SleepStart = TimeZoneInfo.ConvertTimeFromUtc(slpItem.SleepStart, TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                            slpItem.SleepEnd = TimeZoneInfo.ConvertTimeFromUtc(slpItem.SleepEnd, TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                            slpItem.StartString = slpItem.SleepStart.ToString("dd-MMM-yyyy HH:mm") + " - " + slpItem.SleepEnd.ToString("dd-MMM-yyyy HH:mm");
                            slpItem.EndString = "Sleep Duration: ";
                            DateTimeOffset sOffset = new DateTimeOffset(slpItem.SleepStart,
                                TimeZoneInfo.FindSystemTimeZoneById(userTimeZone).GetUtcOffset(slpItem.SleepStart));
                            DateTimeOffset eOffset = new DateTimeOffset(slpItem.SleepEnd,
                                TimeZoneInfo.FindSystemTimeZoneById(userTimeZone).GetUtcOffset(slpItem.SleepEnd));
                            slpItem.SleepDuration = eOffset - sOffset;
                            slpItem.EndString = slpItem.EndString + slpItem.SleepDuration;
                        }
                        await SecureStorage.SetAsync("SleepList" + progenyId + "Al" + accessLevel, JsonConvert.SerializeObject(sleepList));
                        return sleepList;
                    }
                    else
                    {
                        return new List<Sleep>();
                    }
                }
                else // User is logged in.
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/sleep/getsleeplistmobile/" + progenyId + "/" + accessLevel + "/" + start).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var sleepString = await result.Content.ReadAsStringAsync();
                        List<Sleep> sleepList = JsonConvert.DeserializeObject<List<Sleep>>(sleepString);

                        foreach (Sleep slpItem in sleepList)
                        {
                            slpItem.SleepStart = TimeZoneInfo.ConvertTimeFromUtc(slpItem.SleepStart, TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                            slpItem.SleepEnd = TimeZoneInfo.ConvertTimeFromUtc(slpItem.SleepEnd, TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                            DateTimeOffset sOffset = new DateTimeOffset(slpItem.SleepStart,
                                TimeZoneInfo.FindSystemTimeZoneById(userTimeZone).GetUtcOffset(slpItem.SleepStart));
                            DateTimeOffset eOffset = new DateTimeOffset(slpItem.SleepEnd,
                                TimeZoneInfo.FindSystemTimeZoneById(userTimeZone).GetUtcOffset(slpItem.SleepEnd));
                            await SecureStorage.SetAsync("SleepList" + progenyId + "Al" + accessLevel, JsonConvert.SerializeObject(sleepList));
                            slpItem.SleepDuration = eOffset - sOffset;
                        }
                        await SecureStorage.SetAsync("SleepList" + progenyId + "Al" + accessLevel, JsonConvert.SerializeObject(sleepList));
                        return sleepList;
                    }
                    else
                    {
                        return new List<Sleep>();
                    }
                }
            }
            else
            {
                string sleepString = await SecureStorage.GetAsync("SleepList" + progenyId + "Al" + accessLevel);
                List<Sleep> sleepList = JsonConvert.DeserializeObject<List<Sleep>>(sleepString);
                return sleepList;
            }
            
        }

        public static async Task<SleepStatsModel> GetSleepStats(int progenyId, int accessLevel)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                if (String.IsNullOrEmpty(accessToken))
                {

                    var result = await client.GetAsync("api/publicaccess/getsleepstatsmobile/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var sleepString = await result.Content.ReadAsStringAsync();
                        SleepStatsModel sleepList = JsonConvert.DeserializeObject<SleepStatsModel>(sleepString);
                        await SecureStorage.SetAsync("SleepStats" + progenyId + "Al" + accessLevel, JsonConvert.SerializeObject(sleepList));
                        return sleepList;
                    }
                    else
                    {
                        return new SleepStatsModel();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/sleep/getsleepstatsmobile/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var sleepString = await result.Content.ReadAsStringAsync();
                        SleepStatsModel sleepList = JsonConvert.DeserializeObject<SleepStatsModel>(sleepString);
                        await SecureStorage.SetAsync("SleepStats" + progenyId + "Al" + accessLevel, JsonConvert.SerializeObject(sleepList));
                        return sleepList;
                    }
                    else
                    {
                        return new SleepStatsModel();
                    }
                }
            }
            else
            {
                string sleepString = await SecureStorage.GetAsync("SleepStats" + progenyId + "Al" + accessLevel);
                SleepStatsModel sleepList = JsonConvert.DeserializeObject<SleepStatsModel>(sleepString);
                return sleepList;
            }
            
        }

        public static async Task<List<Sleep>> GetSleepChartData(int progenyId, int accessLevel)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                if (String.IsNullOrEmpty(accessToken))
                {

                    var result = await client.GetAsync("api/publicaccess/getsleepchartdatamobile/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var sleepString = await result.Content.ReadAsStringAsync();
                        List<Sleep> sleepList = JsonConvert.DeserializeObject<List<Sleep>>(sleepString);
                        await SecureStorage.SetAsync("SleepChart" + progenyId + "Al" + accessLevel, JsonConvert.SerializeObject(sleepList));
                        return sleepList;
                    }
                    else
                    {
                        return new List<Sleep>();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/sleep/getsleepchartdatamobile/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var sleepString = await result.Content.ReadAsStringAsync();
                        List<Sleep> sleepList = JsonConvert.DeserializeObject<List<Sleep>>(sleepString);
                        await SecureStorage.SetAsync("SleepChart" + progenyId + "Al" + accessLevel, JsonConvert.SerializeObject(sleepList));
                        return sleepList;
                    }
                    else
                    {
                        return new List<Sleep>();
                    }
                }
            }
            else
            {
                string sleepString = await SecureStorage.GetAsync("SleepChart" + progenyId + "Al" + accessLevel);
                List<Sleep> sleepList = JsonConvert.DeserializeObject<List<Sleep>>(sleepString);
                return sleepList;
            }
        }

        public static async Task<Note> GetNote(int nteId, string accessToken, string userTimezone)
        {
            bool online = Online();
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
            }
            catch (Exception)
            {
                userTimezone = TZConvert.WindowsToIana(userTimezone);
            }

            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                if (String.IsNullOrEmpty(accessToken))
                {

                    var result = await client.GetAsync("api/publicaccess/getnotemobile/" + nteId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var noteString = await result.Content.ReadAsStringAsync();
                        Note nteItem = JsonConvert.DeserializeObject<Note>(noteString);

                        nteItem.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(nteItem.CreatedDate,
                            TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        await SecureStorage.SetAsync("Note" + nteId, JsonConvert.SerializeObject(nteItem));
                        return nteItem;
                    }
                    else
                    {
                        return new Note();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/notes/" + nteId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var noteString = await result.Content.ReadAsStringAsync();
                        Note nteItem = JsonConvert.DeserializeObject<Note>(noteString);

                        nteItem.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(nteItem.CreatedDate,
                            TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        await SecureStorage.SetAsync("Note" + nteId, JsonConvert.SerializeObject(nteItem));
                        return nteItem;
                    }
                    else
                    {
                        return new Note();
                    }
                }
            }
            else
            {
                string noteString = await SecureStorage.GetAsync("Note" + nteId);
                Note nteItem = JsonConvert.DeserializeObject<Note>(noteString);
                return nteItem;
            }
        }

        public static async Task<Contact> GetContact(int contId, string accessToken, string userTimezone)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
            }
            catch (Exception)
            {
                userTimezone = TZConvert.WindowsToIana(userTimezone);
            }
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                if (String.IsNullOrEmpty(accessToken))
                {
                    var result = await client.GetAsync("api/publicaccess/getcontactmobile/" + contId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var contactString = await result.Content.ReadAsStringAsync();
                        Contact contItem = JsonConvert.DeserializeObject<Contact>(contactString);
                        if (contItem.DateAdded.HasValue)
                        {
                            contItem.DateAdded = TimeZoneInfo.ConvertTimeFromUtc(contItem.DateAdded.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        }
                        await SecureStorage.SetAsync("Contact" + contId, JsonConvert.SerializeObject(contItem));
                        ImageService.Instance.LoadUrl(contItem.PictureLink).Preload();
                        return contItem;
                    }
                    else
                    {
                        return new Contact();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/contacts/getcontactmobile/" + contId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var contactString = await result.Content.ReadAsStringAsync();
                        Contact contItem = JsonConvert.DeserializeObject<Contact>(contactString);
                        if (contItem.DateAdded.HasValue)
                        {
                            contItem.DateAdded = TimeZoneInfo.ConvertTimeFromUtc(contItem.DateAdded.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        }
                        await SecureStorage.SetAsync("Contact" + contId, JsonConvert.SerializeObject(contItem));
                        ImageService.Instance.LoadUrl(contItem.PictureLink).Preload();
                        return contItem;
                    }
                    else
                    {
                        return new Contact();
                    }
                }
            }
            else
            {
                string contactString = await SecureStorage.GetAsync("Contact" + contId);
                Contact contItem = JsonConvert.DeserializeObject<Contact>(contactString);
                return contItem;
            }
        }

        public static async Task<List<Contact>> GetProgenyContacts(int progenyId, int accessLevel, string userTimeZone)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);
            }
            catch (Exception)
            {
                userTimeZone = TZConvert.WindowsToIana(userTimeZone);
            }
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                if (String.IsNullOrEmpty(accessToken))
                {

                    var result = await client.GetAsync("api/publicaccess/progenycontactsmobile/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var contactsString = await result.Content.ReadAsStringAsync();
                        List<Contact> contList = JsonConvert.DeserializeObject<List<Contact>>(contactsString);
                        if (contList.Any())
                        {
                            foreach (Contact contItem in contList)
                            {
                                if (contItem.DateAdded.HasValue)
                                {
                                    contItem.DateAdded = TimeZoneInfo.ConvertTimeFromUtc(contItem.DateAdded.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                                }
                            }
                        }
                        await SecureStorage.SetAsync("ContactList" + progenyId + "Al" + accessLevel, JsonConvert.SerializeObject(contList));
                        return contList;
                    }
                    else
                    {
                        return new List<Contact>();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/contacts/progeny/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var contactsString = await result.Content.ReadAsStringAsync();
                        List<Contact> contList = JsonConvert.DeserializeObject<List<Contact>>(contactsString);
                        if (contList.Any())
                        {
                            foreach (Contact contItem in contList)
                            {
                                if (contItem.DateAdded.HasValue)
                                {
                                    contItem.DateAdded = TimeZoneInfo.ConvertTimeFromUtc(contItem.DateAdded.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimeZone));
                                }
                            }
                        }
                        await SecureStorage.SetAsync("ContactList" + progenyId + "Al" + accessLevel, JsonConvert.SerializeObject(contList));
                        return contList;
                    }
                    else
                    {
                        return new List<Contact>();
                    }
                }
            }
            else
            {
                string contactsString = await SecureStorage.GetAsync("ContactList" + progenyId + "Al" + accessLevel);
                List<Contact> contList = JsonConvert.DeserializeObject<List<Contact>>(contactsString);
                return contList;
            }
        }

        public static async Task<Vaccination> GetVaccination(int vacId, string accessToken, string userTimezone)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
            }
            catch (Exception)
            {
                userTimezone = TZConvert.WindowsToIana(userTimezone);
            }
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                if (String.IsNullOrEmpty(accessToken))
                {

                    var result = await client.GetAsync("api/publicaccess/getvaccinationmobile/" + vacId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var vaccinationString = await result.Content.ReadAsStringAsync();
                        Vaccination vacItem = JsonConvert.DeserializeObject<Vaccination>(vaccinationString);
                        vacItem.VaccinationDate = TimeZoneInfo.ConvertTimeFromUtc(vacItem.VaccinationDate, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        await SecureStorage.SetAsync("Vaccination" + vacId, JsonConvert.SerializeObject(vacItem));
                        return vacItem;
                    }
                    else
                    {
                        return new Vaccination();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/vaccinations/" + vacId).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var vaccinationString = await result.Content.ReadAsStringAsync();
                        Vaccination vacItem = JsonConvert.DeserializeObject<Vaccination>(vaccinationString);
                        vacItem.VaccinationDate = TimeZoneInfo.ConvertTimeFromUtc(vacItem.VaccinationDate, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                        await SecureStorage.SetAsync("Vaccination" + vacId, JsonConvert.SerializeObject(vacItem));
                        return vacItem;
                    }
                    else
                    {
                        return new Vaccination();
                    }
                }
            }
            else
            {
                string vaccinationString = await SecureStorage.GetAsync("Vaccination" + vacId);
                Vaccination vacItem = JsonConvert.DeserializeObject<Vaccination>(vaccinationString);
                return vacItem;
            }
        }
    }
}

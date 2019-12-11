using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FFImageLoading;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using Newtonsoft.Json;
using TimeZoneConverter;
using Xamarin.Essentials;
using Xamarin.Forms.GoogleMaps;
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
                Progeny progeny = await App.Database.GetProgenyAsync(progenyId);
                if (progeny == null)
                {
                    return OfflineDefaultData.DefaultProgeny;
                }

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
                    try
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


                            await App.Database.SaveProgenyAsync(progeny);
                            ImageService.Instance.LoadUrl(progeny.PictureLink).Preload();
                            return progeny;
                        }
                        else
                        {
                            return OfflineDefaultData.DefaultProgeny;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return OfflineDefaultData.DefaultProgeny;
                    }
                }
                else // User is logged in
                {
                    try
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
                            await App.Database.SaveProgenyAsync(progeny);
                            return progeny;
                        }
                        else
                        {
                            progeny = await App.Database.GetProgenyAsync(progenyId);
                            return progeny;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return OfflineDefaultData.DefaultProgeny;
                    }
                }
            }
        }

        public static async Task<Progeny> AddProgeny(Progeny progeny)
        {
            if (Online())
            {
                try
                {
                    var client = new HttpClient();
                    client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                    string accessToken = await UserService.GetAuthAccessToken();
                    client.SetBearerToken(accessToken);
                    var result = await client.PostAsync("api/progeny/", new StringContent(JsonConvert.SerializeObject(progeny), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Progeny resultProgeny = JsonConvert.DeserializeObject<Progeny>(resultString);
                        return resultProgeny;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return progeny;
                }
            }

            return progeny;
        }

        public static async Task<Progeny> UpdateProgeny(Progeny progeny)
        {
            if (Online())
            {
                try
                {
                    var client = new HttpClient();
                    client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                    string accessToken = await UserService.GetAuthAccessToken();
                    client.SetBearerToken(accessToken);
                    var result = await client.PutAsync("api/progeny/" + progeny.Id, new StringContent(JsonConvert.SerializeObject(progeny), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Progeny resultProgeny = JsonConvert.DeserializeObject<Progeny>(resultString);
                        return resultProgeny;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return progeny;
                }
            }

            return progeny;
        }

        public static async Task<Comment> AddComment(int commentThread, string text, Progeny progeny, string itemId, int itemType)
        {
            Comment cmnt = new Comment();

            cmnt.CommentThreadNumber = commentThread;
            cmnt.CommentText = text;
            cmnt.Author = await UserService.GetUserId();
            cmnt.DisplayName = await UserService.GetFullname();
            cmnt.Created = DateTime.UtcNow;
            cmnt.Progeny = progeny;
            cmnt.ItemId = itemId;
            cmnt.ItemType = itemType;

            if (Online())
            {
                try
                {
                    var client = new HttpClient();
                    client.BaseAddress = new Uri(Constants.MediaApiUrl);
                    string accessToken = await UserService.GetAuthAccessToken();
                    client.SetBearerToken(accessToken);
                    var result = await client.PostAsync("api/comments/", new StringContent(JsonConvert.SerializeObject(cmnt), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Comment resultComment = JsonConvert.DeserializeObject<Comment>(resultString);
                        return resultComment;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return cmnt;
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
                    try
                    {
                        client.SetBearerToken(accessToken);

                        string commentsApiPath = "api/comments/getcommentsbythread/" + commentThread;
                        var result = await client.GetAsync(commentsApiPath).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var commentsListString = await result.Content.ReadAsStringAsync();
                            List<Comment> commentsList = JsonConvert.DeserializeObject<List<Comment>>(commentsListString);
                            string userEmail = await UserService.GetUserEmail();
                            UserInfo userInfo = await App.Database.GetUserInfoAsync(userEmail);
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
                                await App.Database.SaveCommentAsync(comment);
                            }

                            await App.Database.SaveCommentThreadAsync(commentThread, commentsList);
                            return commentsList;
                        }
                        else
                        {
                            return new List<Comment>();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new List<Comment>();
                    }
                }
            }
            else
            {
                string commentsListString = await App.Database.GetCommentThreadAsync(commentThread);
                if (string.IsNullOrEmpty(commentsListString))
                {
                    return new List<Comment>();
                }
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
                    try
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
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new Comment();
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
                try
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
                            await App.Database.SaveProgenyListAsync(userEmail, progenyList);
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
                            await App.Database.SaveProgenyListAsync(userEmail, progenyList);
                            return progenyList;
                        }
                        else
                        {
                            string progenyListString = await App.Database.GetProgenyListAsync(userEmail);
                            List<Progeny> progenyList = JsonConvert.DeserializeObject<List<Progeny>>(progenyListString);
                            return progenyList;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return new List<Progeny>();
                }
            }
            else
            {
                string progenyListString = await App.Database.GetProgenyListAsync(userEmail);
                if (string.IsNullOrEmpty(progenyListString))
                {
                    return new List<Progeny>();
                }
                List<Progeny> progenyList = JsonConvert.DeserializeObject<List<Progeny>>(progenyListString);
                return progenyList;
            }
        }

        public static async Task<List<Progeny>> GetProgenyAdminList()
        {
            List<Progeny> progenyListResult = new List<Progeny>();
            string userEmail = await UserService.GetUserEmail();
            string progenyListString = await App.Database.GetProgenyListAsync(userEmail);
            List<Progeny> progenyList = JsonConvert.DeserializeObject<List<Progeny>>(progenyListString);
            if (progenyList != null)
            {
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
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/access/" + progenyId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var accessListString = await result.Content.ReadAsStringAsync();
                            List<UserAccess> accessList = JsonConvert.DeserializeObject<List<UserAccess>>(accessListString);
                            string email = await UserService.GetUserEmail();
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
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return 5;
                    }
                }
                else // If user is logged in.
                {
                    try
                    {
                        client.SetBearerToken(accessToken);

                        var result = await client.GetAsync("api/access/progeny/" + progenyId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var accessListString = await result.Content.ReadAsStringAsync();
                            List<UserAccess> accessList = JsonConvert.DeserializeObject<List<UserAccess>>(accessListString);
                            string email = await UserService.GetUserEmail();
                            if (accessList != null)
                            {
                                UserAccess ua = accessList.SingleOrDefault(a => a.UserId.ToUpper() == email.ToUpper());
                                if (ua != null)
                                {
                                    await App.Database.SaveUserAccessAsync(ua);
                                    return ua.AccessLevel;
                                }
                            }
                        }
                        else
                        {
                            string email = await UserService.GetUserEmail();
                            int al = 5;
                            UserAccess ua = await App.Database.GetUserAccessAsync(email, progenyId);
                            if (ua != null)
                            {
                                al = ua.AccessLevel;
                            }
                            return al;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return 5;
                    }
                }
                return 5;
            }
            else
            {
                string email = await UserService.GetUserEmail();
                int al = 5;
                UserAccess ua = await App.Database.GetUserAccessAsync(email, progenyId);
                if (ua != null)
                {
                    al = ua.AccessLevel;
                }
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
                try
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
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);

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
                try
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
                        tempPicture.PictureLink = Constants.WebUrl + "/photodb/0/default_temp.jpg";
                        tempPicture.PictureLink600 = Constants.WebUrl + "/photodb/0/default_temp.jpg";
                        tempPicture.PictureLink1200 = Constants.WebUrl + "/photodb/0/default_temp.jpg";
                        tempPicture.ProgenyId = progenyId;
                        tempPicture.PictureTime = new DateTime(2018, 9, 1, 12, 00, 00);
                        ImageService.Instance.LoadUrl(tempPicture.PictureLink).Preload();
                        return tempPicture;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);

                    Picture tempPicture = new Picture();
                    tempPicture.Progeny = new Progeny();
                    tempPicture.AccessLevel = 5;
                    tempPicture.PictureLink = Constants.WebUrl + "/photodb/0/default_temp.jpg";
                    tempPicture.PictureLink600 = Constants.WebUrl + "/photodb/0/default_temp.jpg";
                    tempPicture.PictureLink1200 = Constants.WebUrl + "/photodb/0/default_temp.jpg";
                    tempPicture.ProgenyId = progenyId;
                    tempPicture.PictureTime = new DateTime(2018, 9, 1, 12, 00, 00);
                    ImageService.Instance.LoadUrl(tempPicture.PictureLink).Preload();
                    return tempPicture;
                }
            }
        }

        public static async Task<List<CalendarItem>> GetUpcomingEventsList(int progenyId, int accessLevel)
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

                    try
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
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new List<CalendarItem>();
                    }
                }
                else // If the user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/calendar/eventlist/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var eventsString = await result.Content.ReadAsStringAsync();
                            List<CalendarItem> events = JsonConvert.DeserializeObject<List<CalendarItem>>(eventsString);
                            await App.Database.SaveUpcomingEventsAsync(progenyId, accessLevel, events);
                            return events;
                        }
                        else
                        {
                            // Todo: Handle errors
                            return new List<CalendarItem>();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new List<CalendarItem>();
                    }
                }
            }
            else
            {
                string eventListString = await App.Database.GetUpcomingEventsAsync(progenyId, accessLevel);
                if (string.IsNullOrEmpty(eventListString))
                {
                    return new List<CalendarItem>();
                }
                List<CalendarItem> evtList = JsonConvert.DeserializeObject<List<CalendarItem>>(eventListString);
                return evtList;
            }
        }

        public static async Task<TimeLineItem> SaveTimeLineItem(TimeLineItem timeLineItem)
        {
            if (Online())
            {
                try
                {
                    var client = new HttpClient();
                    client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                    string accessToken = await UserService.GetAuthAccessToken();
                    client.SetBearerToken(accessToken);
                    var result = await client.PostAsync("api/timeline/", new StringContent(JsonConvert.SerializeObject(timeLineItem), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        TimeLineItem resultTimelineItem = JsonConvert.DeserializeObject<TimeLineItem>(resultString);
                        return resultTimelineItem;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return timeLineItem;
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
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/progenylatest/" + progenyId + "/" + accessLevel + "/" + count + "/" + start).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var timelineString = await result.Content.ReadAsStringAsync();
                            timeLineLatest = JsonConvert.DeserializeObject<List<TimeLineItem>>(timelineString);
                            await App.Database.SaveTimeLineListAsync(progenyId, accessLevel, count, start, timeLineLatest);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new List<TimeLineItem>();
                    }
                }
                else // If user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/timeline/progenylatestmobile/" + progenyId + "/" + accessLevel + "/" + count + "/" + start + "/" + startTime.Year + "/" + startTime.Month + "/" + startTime.Day).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var timelineString = await result.Content.ReadAsStringAsync();
                            timeLineLatest = JsonConvert.DeserializeObject<List<TimeLineItem>>(timelineString);
                            await App.Database.SaveTimeLineListAsync(progenyId, accessLevel, count, start, timeLineLatest);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new List<TimeLineItem>();
                    }
                }
            }
            else
            {
                string timlineListString =
                    await App.Database.GetTimeLineListAsync(progenyId, accessLevel, count, start);
                if (string.IsNullOrEmpty(timlineListString))
                {
                    return new List<TimeLineItem>();
                }
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
                    tItem.ItemObject = await GetFriend(frnId, accessToken);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Measurement)
                {
                    int.TryParse(tItem.ItemId, out int mesId);
                    tItem.ItemObject = await GetMeasurement(mesId, accessToken);
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
                    tItem.ItemObject = await GetContact(contId, accessToken);
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
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/progenyyearago/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var timelineString = await result.Content.ReadAsStringAsync();
                            timeLineYearAgo = JsonConvert.DeserializeObject<List<TimeLineItem>>(timelineString);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new List<TimeLineItem>();
                    }
                }
                else // If user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/timeline/progenyyearago/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var timelineString = await result.Content.ReadAsStringAsync();
                            timeLineYearAgo = JsonConvert.DeserializeObject<List<TimeLineItem>>(timelineString);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new List<TimeLineItem>();
                    }
                }
            }
            else
            {
               return new List<TimeLineItem>();
            }

            string currentDate = "";

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
                    tItem.ItemObject = await GetFriend(frnId, accessToken);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Measurement)
                {
                    int.TryParse(tItem.ItemId, out int mesId);
                    tItem.ItemObject = await GetMeasurement(mesId, accessToken);
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
                    tItem.ItemObject = await GetContact(contId, accessToken);
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
                    dateHeader.DateText = tItem.ProgenyTime.Year.ToString();
                    headerItem.ItemObject = dateHeader;
                    currentDate = itemDate;
                    resultList.Add(headerItem);
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
                    try
                    {
                        var result = await client
                            .GetAsync("api/publicaccess/progenylatest/" + progenyId + "/" + accessLevel + "/" + 5 + "/" + 0 )
                            .ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var timelineString = await result.Content.ReadAsStringAsync();
                            timeLineLatest = JsonConvert.DeserializeObject<List<TimeLineItem>>(timelineString);
                            await App.Database.SaveTimeLineLatestAsync(progenyId, accessLevel, timeLineLatest);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new List<TimeLineItem>();
                    }
                }
                else
                {
                    string timelineString = await App.Database.GetTimeLineLatestAsync(progenyId, accessLevel);
                    if (string.IsNullOrEmpty(timelineString))
                    {
                        return new List<TimeLineItem>();
                    }
                    timeLineLatest = JsonConvert.DeserializeObject<List<TimeLineItem>>(timelineString);
                }
            }
            else // If user is logged in.
            {

                if (online)
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client
                            .GetAsync("api/timeline/progenylatestmobile/" + progenyId + "/" + accessLevel + "/" + 5 + "/" + 0 + "/" + startTime.Year + "/" + startTime.Month + "/" + startTime.Day)
                            .ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var timelineString = await result.Content.ReadAsStringAsync();
                            timeLineLatest = JsonConvert.DeserializeObject<List<TimeLineItem>>(timelineString);
                            await App.Database.SaveTimeLineLatestAsync(progenyId, accessLevel, timeLineLatest);

                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new List<TimeLineItem>();
                    }
                }
                else
                {
                    string timelineString = await App.Database.GetTimeLineLatestAsync(progenyId, accessLevel);
                    if (string.IsNullOrEmpty(timelineString))
                    {
                        return new List<TimeLineItem>();
                    }
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
                    tItem.ItemObject = await GetFriend(frnId, accessToken);
                }

                if (tItem.ItemType == (int)KinaUnaTypes.TimeLineType.Measurement)
                {
                    int.TryParse(tItem.ItemId, out int mesId);
                    tItem.ItemObject = await GetMeasurement(mesId, accessToken);
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
                    tItem.ItemObject = await GetContact(contId, accessToken);
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

                    try
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
                            await App.Database.SavePictureAsync(picture);
                            return picture;
                        }
                        else
                        {
                            return new Picture();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new Picture();
                    }
                }
                else // If the user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
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
                            await App.Database.SavePictureAsync(picture);
                            return picture;
                        }
                        else
                        {
                            return new Picture();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new Picture();
                    }
                }
            }
            else
            {
                Picture picture = await App.Database.GetPictureAsync(pictureId); // await SecureStorage.GetAsync("Picture" + pictureId);
                if (picture == null)
                {
                    return new Picture();
                }
                return picture;
            }
        }

        public static async Task<Picture> GetPictureWithOriginalImageLink(int pictureId, string accessToken, string userTimezone)
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
                    return new Picture();
                }
                else // If the user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/pictures/" + pictureId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var pictureString = await result.Content.ReadAsStringAsync();
                            Picture picture = JsonConvert.DeserializeObject<Picture>(pictureString);
                            if (picture.PictureTime.HasValue)
                            {
                                picture.PictureTime = TimeZoneInfo.ConvertTimeFromUtc(picture.PictureTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                            }
                            return picture;
                        }
                        else
                        {
                            return new Picture();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new Picture();
                    }
                }
            }
            
            return new Picture();
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

                try
                {
                    var result = await client.PostAsync("api/pictures/uploadpicture/", content).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        string pictureResultString = Regex.Replace(resultString, @"([|""|])", "");
                        return pictureResultString;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return "";
                }
            }

            return "";
        }

        public static async Task<string> UploadProgenyPicture(string fileName)
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
                try
                {
                    var result = await client.PostAsync("api/pictures/uploadprogenypicture/", content).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        string pictureResultString = Regex.Replace(resultString, @"([|""|])", "");
                        return pictureResultString;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return "";
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
                try
                {
                    var result = await client.PostAsync("api/pictures/", new StringContent(JsonConvert.SerializeObject(picture), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Picture resultPicture = JsonConvert.DeserializeObject<Picture>(resultString);
                        return resultPicture;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return picture;
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
                    try
                    {
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
                                await App.Database.SavePictureAsync(picture);
                            }
                            await App.Database.SavePicturePageListAsync(progenyId, pageNumber, pageSize, sortBy, tagFilter, picturePage);
                            return picturePage;
                        }
                        else
                        {
                            return new PicturePage();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new PicturePage();
                    }
                }
                else // If user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    string pageApiPath = "api/pictures/pagemobile?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + progenyId + "&accessLevel=" + userAccessLevel + "&tagFilter=" + tagFilter + "&sortBy=" + sortBy;
                    try
                    {
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
                                await App.Database.SavePictureAsync(picture);
                            }

                            await App.Database.SavePicturePageListAsync(progenyId, pageNumber, pageSize, sortBy, tagFilter, picturePage);
                            return picturePage;
                        }
                        else
                        {
                            return new PicturePage();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new PicturePage();
                    }
                }
            }
            else
            {
                string picturePageString = await App.Database.GetPicturePageListAsync(progenyId, pageNumber, pageSize, sortBy, tagFilter);
                if (string.IsNullOrEmpty(picturePageString))
                {
                    return new PicturePage();
                }
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
                    try
                    {
                        var result = await client.GetAsync(pictureViewApiPath).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var pictureViewModelString = await result.Content.ReadAsStringAsync();
                            PictureViewModel pictureViewModel = JsonConvert.DeserializeObject<PictureViewModel>(pictureViewModelString);
                            if (pictureViewModel.PictureTime.HasValue)
                            {
                                pictureViewModel.PictureTime = TimeZoneInfo.ConvertTimeFromUtc(pictureViewModel.PictureTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                            }

                            if (string.IsNullOrEmpty(pictureViewModel.Location))
                            {
                                pictureViewModel.Location = "";
                            }

                            if (string.IsNullOrEmpty(pictureViewModel.Tags))
                            {
                                pictureViewModel.Tags = "";
                            }
                        
                            pictureViewModel.CommentsCount = pictureViewModel.CommentsList.Count;
                            ImageService.Instance.LoadUrl(pictureViewModel.PictureLink).DownSample(height: 440, allowUpscale: true).Preload();
                            await App.Database.SavePictureViewModelAsync(pictureViewModel);

                            return pictureViewModel;
                        }
                        else
                        {
                            return new PictureViewModel();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new PictureViewModel();
                    }
                }
                else // If user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    string pictureViewApiPath = "api/pictures/pictureviewmodelmobile/" + pictureId + "/" + userAccessLevel + "?sortBy=" + sortBy;
                    try
                    {
                        var result = await client.GetAsync(pictureViewApiPath).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var pictureViewModelString = await result.Content.ReadAsStringAsync();
                            PictureViewModel pictureViewModel = JsonConvert.DeserializeObject<PictureViewModel>(pictureViewModelString);
                            if (pictureViewModel.PictureTime.HasValue)
                            {
                                pictureViewModel.PictureTime = TimeZoneInfo.ConvertTimeFromUtc(pictureViewModel.PictureTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                            }
                            if (string.IsNullOrEmpty(pictureViewModel.Tags))
                            {
                                pictureViewModel.Tags = "";
                            }

                            pictureViewModel.CommentsCount = pictureViewModel.CommentsList.Count;
                            ImageService.Instance.LoadUrl(pictureViewModel.PictureLink).DownSample(height: 440, allowUpscale: true).Preload();
                            await App.Database.SavePictureViewModelAsync(pictureViewModel);

                            return pictureViewModel;
                        }
                        else
                        {
                            return new PictureViewModel();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new PictureViewModel();
                    }
                }
            }
            else
            {
                PictureViewModel pictureViewModel = await App.Database.GetPictureViewModelAsync(pictureId);
                if (pictureViewModel != null)
                {
                    pictureViewModel = new PictureViewModel();
                }
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

                    try
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
                            await App.Database.SaveVideoAsync(video);
                            return video;
                        }
                        else
                        {
                            return new Video();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new Video();
                    }
                }
                else // If user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/videos/getvideomobile/" + videoId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var videoString = await result.Content.ReadAsStringAsync();
                            Video video = JsonConvert.DeserializeObject<Video>(videoString);
                            if (video.VideoTime.HasValue)
                            {
                                video.VideoTime = TimeZoneInfo.ConvertTimeFromUtc(video.VideoTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                            }
                            await App.Database.SaveVideoAsync(video);
                            return video;
                        }
                        else
                        {
                            return new Video();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new Video();
                    }
                }
            }
            else
            {
                Video video = await App.Database.GetVideoAsync(videoId);
                if (video == null)
                {
                    video = new Video();
                }
                return video;
            }
        }

        public static async Task<VideoPage> GetVideoPage(int pageNumber, int pageSize, int progenyId, int userAccessLevel, string userTimezone, int sortBy, string tagFilter)
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

                    string pageApiPath = "api/publicaccess/videopagemobile?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + progenyId + "&accessLevel=" + userAccessLevel + "&sortBy=" + sortBy;
                    try
                    {
                        var result = await client.GetAsync(pageApiPath).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var videoPageString = await result.Content.ReadAsStringAsync();
                            VideoPage videoPage = JsonConvert.DeserializeObject<VideoPage>(videoPageString);
                            foreach (Video video in videoPage.VideosList)
                            {
                                if (video.VideoTime.HasValue)
                                {
                                    video.VideoTime = TimeZoneInfo.ConvertTimeFromUtc(video.VideoTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                                }

                                video.CommentsCount = video.Comments.Count;
                            }
                            await App.Database.SaveVideoPageListAsync(progenyId, pageNumber, pageSize, sortBy, tagFilter, videoPage);
                            return videoPage;
                        }
                        else
                        {
                            return new VideoPage();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new VideoPage();
                    }
                }
                else // User is logged in.
                {
                    client.SetBearerToken(accessToken);

                    string pageApiPath = "api/videos/pagemobile?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + progenyId + "&accessLevel=" + userAccessLevel + "&tagFilter=" + tagFilter + "&sortBy=" + sortBy;
                    try
                    {
                        var result = await client.GetAsync(pageApiPath).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var videoPageString = await result.Content.ReadAsStringAsync();
                            VideoPage videoPage = JsonConvert.DeserializeObject<VideoPage>(videoPageString);
                            foreach (Video video in videoPage.VideosList)
                            {
                                if (video.VideoTime.HasValue)
                                {
                                    video.VideoTime = TimeZoneInfo.ConvertTimeFromUtc(video.VideoTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                                }

                                video.CommentsCount = video.Comments.Count;
                            }
                            await App.Database.SaveVideoPageListAsync(progenyId, pageNumber, pageSize, sortBy, tagFilter, videoPage);
                            return videoPage;
                        }
                        else
                        {
                            return new VideoPage();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new VideoPage();
                    }
                }
            }
            else
            {
                string videoPageString = await App.Database.GetVideoPageListAsync(progenyId, pageNumber, pageSize, sortBy, tagFilter);
                if (string.IsNullOrEmpty(videoPageString))
                {
                    return new VideoPage();
                }
                VideoPage videoPage = JsonConvert.DeserializeObject<VideoPage>(videoPageString);
                return videoPage;
            }
            
        }

        public static async Task<VideoViewModel> GetVideoViewModel(int videoId, int userAccessLevel, string userTimezone, int sortBy)
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

                    string pictureViewApiPath = "api/publicaccess/videoviewmodelmobile/" + videoId + "/" + userAccessLevel + "?sortBy=" + sortBy;
                    try
                    {
                        var result = await client.GetAsync(pictureViewApiPath).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var videoViewModelString = await result.Content.ReadAsStringAsync();
                            VideoViewModel videoViewModel = JsonConvert.DeserializeObject<VideoViewModel>(videoViewModelString);
                            if (videoViewModel.VideoTime.HasValue)
                            {
                                videoViewModel.VideoTime = TimeZoneInfo.ConvertTimeFromUtc(videoViewModel.VideoTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                            }

                            videoViewModel.CommentsCount = videoViewModel.CommentsList.Count;
                            if (videoViewModel.VideoType == 2)
                            {
                                string[] linkSplit = videoViewModel.VideoLink.Split('/');
                                if (linkSplit.Count() > 1)
                                {
                                    videoViewModel.VideoLink = linkSplit.LastOrDefault();

                                    videoViewModel.VideoLink = "https://web.kinauna.com/videos/youtube?link=" + videoViewModel.VideoLink;
                                }
                            }
                        
                            if (string.IsNullOrEmpty(videoViewModel.Location))
                            {
                                videoViewModel.Location = "";
                            }

                            if (string.IsNullOrEmpty(videoViewModel.Tags))
                            {
                                videoViewModel.Tags = "";
                            }
                            await App.Database.SaveVideoViewModelAsync(videoViewModel);

                            return videoViewModel;
                        }
                        else
                        {
                            return new VideoViewModel();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new VideoViewModel();
                    }
                }
                else // If user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    string pictureViewApiPath = "api/videos/videoviewmodel/" + videoId + "/" + userAccessLevel + "?sortBy=" + sortBy;
                    try
                    {
                        var result = await client.GetAsync(pictureViewApiPath).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var videoViewModelString = await result.Content.ReadAsStringAsync();
                            VideoViewModel videoViewModel = JsonConvert.DeserializeObject<VideoViewModel>(videoViewModelString);
                            if (videoViewModel.VideoTime.HasValue)
                            {
                                videoViewModel.VideoTime = TimeZoneInfo.ConvertTimeFromUtc(videoViewModel.VideoTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                            }

                            videoViewModel.CommentsCount = videoViewModel.CommentsList.Count;
                            if (videoViewModel.VideoType == 2)
                            {
                                string[] linkSplit = videoViewModel.VideoLink.Split('/');
                                if (linkSplit.Count() > 1)
                                {
                                    videoViewModel.VideoLink = linkSplit.LastOrDefault();
                                
                                    videoViewModel.VideoLink = "https://web.kinauna.com/videos/youtube?link=" + videoViewModel.VideoLink;
                                }
                            }

                            if (string.IsNullOrEmpty(videoViewModel.Location))
                            {
                                videoViewModel.Location = "";
                            }

                            if (string.IsNullOrEmpty(videoViewModel.Tags))
                            {
                                videoViewModel.Tags = "";
                            }
                            await App.Database.SaveVideoViewModelAsync(videoViewModel);

                            return videoViewModel;
                        }
                        else
                        {
                            return new VideoViewModel();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new VideoViewModel();
                    }
                }
            }
            else
            {
                VideoViewModel videoViewModel = await App.Database.GetVideoViewModelAsync(videoId);
                if (videoViewModel == null)
                {
                    videoViewModel = new VideoViewModel();
                }
                return videoViewModel;
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
                    try
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
                            // await SecureStorage.SetAsync("CalendarItem" + calendarId, JsonConvert.SerializeObject(calItem));
                            await App.Database.SaveCalendarItemAsync(calItem);
                            return calItem;
                        }
                        else
                        {
                            return new CalendarItem();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new CalendarItem();
                    }
                }
                else // User is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
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
                            // await SecureStorage.SetAsync("CalendarItem" + calendarId, JsonConvert.SerializeObject(calItem));
                            await App.Database.SaveCalendarItemAsync(calItem);
                            return calItem;
                        }
                        else
                        {
                            return new CalendarItem();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new CalendarItem();
                    }
                }
            }
            else
            {
                CalendarItem calItem = await App.Database.GetCalendarItemAsync(calendarId);
                if (calItem == null)
                {
                    calItem = new CalendarItem();
                }
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

            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                // If user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {
                    try
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
                            await App.Database.SaveCalendarListAsync(progenyId, accessLevel, calList);
                            return calList;
                        }
                        else
                        {
                            return new List<CalendarItem>();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new List<CalendarItem>();
                    }
                }
                else // User is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
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
                            await App.Database.SaveCalendarListAsync(progenyId, accessLevel, calList);
                            return calList;
                        }
                        else
                        {
                            return new List<CalendarItem>();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        return new List<CalendarItem>();
                    }
                }
            }
            else
            {
                List<CalendarItem> calItems = await App.Database.GetCalendarListAsync(progenyId, accessLevel);
                if (calItems == null)
                {
                    calItems = new List<CalendarItem>();
                }
                return calItems;
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

                    try
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
                            await App.Database.SaveLocationAsync(locItem);
                            return locItem;
                        }
                        else
                        {
                            return new Location();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new Location();
                    }
                }
                else // User is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/locations/" + locationId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var locationString = await result.Content.ReadAsStringAsync();
                            Location locItem = JsonConvert.DeserializeObject<Location>(locationString);
                            if (locItem.Date.HasValue)
                            {
                                locItem.Date = TimeZoneInfo.ConvertTimeFromUtc(locItem.Date.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                            }
                            await App.Database.SaveLocationAsync(locItem);
                            return locItem;
                        }
                        else
                        {
                            return new Location();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new Location();
                    }
                }
            }
            else
            {
                Location locItem = await App.Database.GetLocationAsync(locationId);
                if (locItem == null)
                {
                    return new Location();
                }
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

                    try
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
                            await App.Database.SaveVocabularyItemAsync(vocItem);
                            return vocItem;
                        }
                        else
                        {
                            return new VocabularyItem();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new VocabularyItem();
                    }
                }
                else  // User is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/vocabulary/" + vocabularyId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var vocabularyString = await result.Content.ReadAsStringAsync();
                            VocabularyItem vocItem = JsonConvert.DeserializeObject<VocabularyItem>(vocabularyString);
                            if (vocItem.Date.HasValue)
                            {
                                vocItem.Date = TimeZoneInfo.ConvertTimeFromUtc(vocItem.Date.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                            }
                            await App.Database.SaveVocabularyItemAsync(vocItem);
                            return vocItem;
                        }
                        else
                        {
                            return new VocabularyItem();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new VocabularyItem();
                    }
                }
            }
            else
            {
                VocabularyItem vocItem = await App.Database.GetVocabularyItemAsync(vocabularyId);
                if (vocItem == null)
                {
                    vocItem = new VocabularyItem();
                }
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
                    try
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
                            await App.Database.SaveSkillAsync(skillItem);
                            return skillItem;
                        }
                        else
                        {
                            return new Skill();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new Skill();
                    }
                }
                else // User is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/skills/" + skillId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var skillString = await result.Content.ReadAsStringAsync();
                            Skill skillItem = JsonConvert.DeserializeObject<Skill>(skillString);
                            if (skillItem.SkillFirstObservation.HasValue)
                            {
                                skillItem.SkillFirstObservation = TimeZoneInfo.ConvertTimeFromUtc(skillItem.SkillFirstObservation.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                            }
                            await App.Database.SaveSkillAsync(skillItem);
                            return skillItem;
                        }
                        else
                        {
                            return new Skill();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new Skill();
                    }
                }
            }
            else
            {
                Skill skillItem = await App.Database.GetSkillAsync(skillId);
                if (skillItem == null)
                {
                    skillItem = new Skill();
                }
                return skillItem;
            }
        }

        public static async Task<Friend> GetFriend(int frnId, string accessToken)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                // User is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {

                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/getfriendmobile/" + frnId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var friendString = await result.Content.ReadAsStringAsync();
                            Friend friendItem = JsonConvert.DeserializeObject<Friend>(friendString);
                            await App.Database.SaveFriendAsync(friendItem);
                            ImageService.Instance.LoadUrl(friendItem.PictureLink).Preload();
                            return friendItem;
                        }
                        else
                        {
                            return new Friend();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new Friend();
                    }
                }
                else // User is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/friends/getfriendmobile/" + frnId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var friendString = await result.Content.ReadAsStringAsync();
                            Friend friendItem = JsonConvert.DeserializeObject<Friend>(friendString);
                            await App.Database.SaveFriendAsync(friendItem);
                            ImageService.Instance.LoadUrl(friendItem.PictureLink).Preload();
                            return friendItem;
                        }
                        else
                        {
                            return new Friend();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new Friend();
                    }
                }
            }
            else
            {
                Friend friendItem = await App.Database.GetFriendAsync(frnId);
                if (friendItem == null)
                {
                    friendItem = new Friend();
                }
                return friendItem;
            }
        }

        public static async Task<Measurement> GetMeasurement(int mesId, string accessToken)
        {
            
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                // User is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/getmeasurementmobile/" + mesId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var measurementString = await result.Content.ReadAsStringAsync();
                            Measurement mesItem = JsonConvert.DeserializeObject<Measurement>(measurementString);
                            await App.Database.SaveMeasurementAsync(mesItem);
                            return mesItem;
                        }
                        else
                        {
                            return new Measurement();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new Measurement();
                    }
                }
                else // User is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/measurements/" + mesId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var measurementString = await result.Content.ReadAsStringAsync();
                            Measurement mesItem = JsonConvert.DeserializeObject<Measurement>(measurementString);
                            await App.Database.SaveMeasurementAsync(mesItem);
                            return mesItem;
                        }
                        else
                        {
                            return new Measurement();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new Measurement();
                    }
                }
            }
            else
            {
                Measurement mesItem = await App.Database.GetMeasurementAsync(mesId);
                if (mesItem == null)
                {
                    mesItem = new Measurement();
                }
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
                    try
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
                            await App.Database.SaveSleepAsync(slpItem);
                            return slpItem;
                        }
                        else
                        {
                            return new Sleep();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new Sleep();
                    }
                }
                else // User is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
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
                            await App.Database.SaveSleepAsync(slpItem);
                            return slpItem;
                        }
                        else
                        {
                            return new Sleep();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new Sleep();
                    }
                }
            }
            else
            {
                Sleep slpItem = await App.Database.GetSleepAsync(slpId);
                if (slpItem == null)
                {
                    slpItem = new Sleep();
                }
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

            try
            {
                var result = await client.PostAsync("api/sleep/", new StringContent(JsonConvert.SerializeObject(sleep), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                if (result.IsSuccessStatusCode)
                {
                    string resultString = await result.Content.ReadAsStringAsync();
                    Sleep resultSleep = JsonConvert.DeserializeObject<Sleep>(resultString);
                    return resultSleep;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return sleep;
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

                    try
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
                            await App.Database.SaveSleepListAsync(progenyId, accessLevel, sleepList);
                            return sleepList;
                        }
                        else
                        {
                            return new List<Sleep>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<Sleep>();
                    }
                }
                else // User is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
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
                            await App.Database.SaveSleepListAsync(progenyId, accessLevel, sleepList);
                            return sleepList;
                        }
                        else
                        {
                            return new List<Sleep>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<Sleep>();
                    }
                }
            }
            else
            {
                List<Sleep> sleepList = await App.Database.GetSleepListAsync(progenyId, accessLevel);
                if (sleepList == null)
                {
                    sleepList = new List<Sleep>();
                }
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
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/getsleepstatsmobile/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var sleepString = await result.Content.ReadAsStringAsync();
                            SleepStatsModel sleepList = JsonConvert.DeserializeObject<SleepStatsModel>(sleepString);
                            await App.Database.SaveSleepStatsModelAsync(progenyId, accessLevel, sleepList);
                            return sleepList;
                        }
                        else
                        {
                            return new SleepStatsModel();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new SleepStatsModel();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/sleep/getsleepstatsmobile/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var sleepString = await result.Content.ReadAsStringAsync();
                            SleepStatsModel sleepList = JsonConvert.DeserializeObject<SleepStatsModel>(sleepString);
                            await App.Database.SaveSleepStatsModelAsync(progenyId, accessLevel, sleepList);
                            return sleepList;
                        }
                        else
                        {
                            return new SleepStatsModel();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new SleepStatsModel();
                    }
                }
            }
            else
            {
                SleepStatsModel sleepList = await App.Database.GetSleepStatsModelAsync(progenyId, accessLevel);
                if (sleepList == null)
                {
                    sleepList = new SleepStatsModel();
                }
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
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/getsleepchartdatamobile/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var sleepString = await result.Content.ReadAsStringAsync();
                            List<Sleep> sleepList = JsonConvert.DeserializeObject<List<Sleep>>(sleepString);
                            await App.Database.SaveSleepChartAsync(progenyId, accessLevel, sleepList);
                            return sleepList;
                        }
                        else
                        {
                            return new List<Sleep>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<Sleep>();
                    }
                    
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/sleep/getsleepchartdatamobile/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var sleepString = await result.Content.ReadAsStringAsync();
                            List<Sleep> sleepList = JsonConvert.DeserializeObject<List<Sleep>>(sleepString);
                            await App.Database.SaveSleepChartAsync(progenyId, accessLevel, sleepList);
                            return sleepList;
                        }
                        else
                        {
                            return new List<Sleep>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<Sleep>();
                    }
                }
            }
            else
            {
                List<Sleep> sleepList = await App.Database.GetSleepChartAsync(progenyId, accessLevel);
                if (sleepList == null)
                {
                    sleepList = new List<Sleep>();
                }
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
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/getnotemobile/" + nteId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var noteString = await result.Content.ReadAsStringAsync();
                            Note nteItem = JsonConvert.DeserializeObject<Note>(noteString);

                            nteItem.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(nteItem.CreatedDate,
                                TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                            await App.Database.SaveNoteAsync(nteItem);
                            return nteItem;
                        }
                        else
                        {
                            return new Note();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new Note();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/notes/" + nteId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var noteString = await result.Content.ReadAsStringAsync();
                            Note nteItem = JsonConvert.DeserializeObject<Note>(noteString);

                            nteItem.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(nteItem.CreatedDate,
                                TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                            await App.Database.SaveNoteAsync(nteItem);
                            return nteItem;
                        }
                        else
                        {
                            return new Note();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new Note();
                    }
                }
            }
            else
            {
                Note nteItem = await App.Database.GetNoteAsync(nteId);
                if (nteItem == null)
                {
                    nteItem = new Note();
                }
                return nteItem;
            }
        }

        public static async Task<Contact> GetContact(int contId, string accessToken)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                if (String.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/getcontactmobile/" + contId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var contactString = await result.Content.ReadAsStringAsync();
                            Contact contItem = JsonConvert.DeserializeObject<Contact>(contactString);
                            await App.Database.SaveContactAsync(contItem);
                            ImageService.Instance.LoadUrl(contItem.PictureLink).Preload();
                            return contItem;
                        }
                        else
                        {
                            return new Contact();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new Contact();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/contacts/getcontactmobile/" + contId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var contactString = await result.Content.ReadAsStringAsync();
                            Contact contItem = JsonConvert.DeserializeObject<Contact>(contactString);
                            await App.Database.SaveContactAsync(contItem);
                            ImageService.Instance.LoadUrl(contItem.PictureLink).Preload();
                            return contItem;
                        }
                        else
                        {
                            return new Contact();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new Contact();
                    }
                }
            }
            else
            {
                Contact contItem = await App.Database.GetContactAsync(contId);
                if (contItem == null)
                {
                    contItem = new Contact();
                }
                return contItem;
            }
        }

        public static async Task<List<Contact>> GetProgenyContacts(int progenyId, int accessLevel)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                if (String.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/progenycontactsmobile/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var contactsString = await result.Content.ReadAsStringAsync();
                            List<Contact> contList = JsonConvert.DeserializeObject<List<Contact>>(contactsString);
                            await App.Database.SaveContactListAsync(progenyId, accessLevel, contList);
                            return contList;
                        }
                        else
                        {
                            return new List<Contact>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<Contact>();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/contacts/progenymobile/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var contactsString = await result.Content.ReadAsStringAsync();
                            List<Contact> contList = JsonConvert.DeserializeObject<List<Contact>>(contactsString);
                            await App.Database.SaveContactListAsync(progenyId, accessLevel, contList);
                            return contList;
                        }
                        else
                        {
                            return new List<Contact>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<Contact>();
                    }
                }
            }
            else
            {
                List<Contact> contList = await App.Database.GetContactListAsync(progenyId, accessLevel);
                if (contList == null)
                {
                    contList = new List<Contact>();
                }
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
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/getvaccinationmobile/" + vacId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var vaccinationString = await result.Content.ReadAsStringAsync();
                            Vaccination vacItem = JsonConvert.DeserializeObject<Vaccination>(vaccinationString);
                            vacItem.VaccinationDate = TimeZoneInfo.ConvertTimeFromUtc(vacItem.VaccinationDate, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                            await App.Database.SaveVaccinationAsync(vacItem);
                            return vacItem;
                        }
                        else
                        {
                            return new Vaccination();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new Vaccination();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/vaccinations/" + vacId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var vaccinationString = await result.Content.ReadAsStringAsync();
                            Vaccination vacItem = JsonConvert.DeserializeObject<Vaccination>(vaccinationString);
                            vacItem.VaccinationDate = TimeZoneInfo.ConvertTimeFromUtc(vacItem.VaccinationDate, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                            await App.Database.SaveVaccinationAsync(vacItem);
                            return vacItem;
                        }
                        else
                        {
                            return new Vaccination();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new Vaccination();
                    }
                }
            }
            else
            {
                Vaccination vacItem = await App.Database.GetVaccinationAsync(vacId);
                if (vacItem == null)
                {
                    vacItem = new Vaccination();
                }
                return vacItem;
            }
        }

        public static async Task<UserAccess> AddUser(UserAccess userAccess)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PostAsync("api/access/", new StringContent(JsonConvert.SerializeObject(userAccess), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        UserAccess resultAccess = JsonConvert.DeserializeObject<UserAccess>(resultString);
                        return resultAccess;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return userAccess;
                }
            }
            return userAccess;
        }

        public static async Task<List<UserAccess>> GetProgenyAccessList(int progenyId)
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
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/access/" + progenyId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var accessListString = await result.Content.ReadAsStringAsync();
                            List<UserAccess> accessList = JsonConvert.DeserializeObject<List<UserAccess>>(accessListString);
                            if (accessList != null)
                            {
                                await App.Database.SaveProgenyAccessListAsync(progenyId, accessList);
                                return accessList;
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<UserAccess>();
                    }
                }
                else // If user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/access/progeny/" + progenyId).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var accessListString = await result.Content.ReadAsStringAsync();
                            List<UserAccess> accessList = JsonConvert.DeserializeObject<List<UserAccess>>(accessListString);
                            if (accessList != null)
                            {
                                await App.Database.SaveProgenyAccessListAsync(progenyId, accessList);
                                return accessList;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<UserAccess>();
                    }
                }
            }

            List<UserAccess> offlineList = await App.Database.GetProgenyAccessListAsync(progenyId);
            if (offlineList == null)
            {
                offlineList = new List<UserAccess>();
            }
            return offlineList;
            
        }

        public static async Task<UserAccess> UpdateUserAccess(UserAccess userAccess)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PutAsync("api/access/" + userAccess.AccessId, new StringContent(JsonConvert.SerializeObject(userAccess), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        UserAccess resultUserAccess = JsonConvert.DeserializeObject<UserAccess>(resultString);
                        return resultUserAccess;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    userAccess.AccessId = 0;
                    return userAccess;
                }
            }

            userAccess.AccessId = 0;
            return userAccess;
        }

        public static async Task<UserAccess> DeleteUserAccess(UserAccess userAccess)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.DeleteAsync("api/access/" + userAccess.AccessId).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        return userAccess;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    userAccess.AccessId = 0;
                    return userAccess;
                }
            }

            userAccess.AccessId = 0;
            return userAccess;
        }

        public static async Task<SleepListPage> GetSleepListPage(int pageNumber, int pageSize, int progenyId, int accessLevel, string timezone, int sortOrder)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                if (String.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/getsleeplistpage?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + Constants.DefaultChildId + "&accessLevel=" + accessLevel + "&sortBy=" + sortOrder).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var sleepString = await result.Content.ReadAsStringAsync();
                            SleepListPage sleepList = JsonConvert.DeserializeObject<SleepListPage>(sleepString);
                            foreach (Sleep slpItem in sleepList.SleepList)
                            {
                                slpItem.SleepStart = TimeZoneInfo.ConvertTimeFromUtc(slpItem.SleepStart, TimeZoneInfo.FindSystemTimeZoneById(timezone));
                                slpItem.SleepEnd = TimeZoneInfo.ConvertTimeFromUtc(slpItem.SleepEnd, TimeZoneInfo.FindSystemTimeZoneById(timezone));
                                DateTimeOffset sOffset = new DateTimeOffset(slpItem.SleepStart,
                                    TimeZoneInfo.FindSystemTimeZoneById(timezone).GetUtcOffset(slpItem.SleepStart));
                                DateTimeOffset eOffset = new DateTimeOffset(slpItem.SleepEnd,
                                    TimeZoneInfo.FindSystemTimeZoneById(timezone).GetUtcOffset(slpItem.SleepEnd));
                                slpItem.SleepDuration = eOffset - sOffset;
                            }
                            await App.Database.SaveSleepListPageAsync(progenyId, accessLevel, pageNumber, pageSize, sortOrder, sleepList);
                            return sleepList;
                        }
                        else
                        {
                            return new SleepListPage();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new SleepListPage();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/sleep/getsleeplistpage?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + progenyId + "&accessLevel=" + accessLevel + "&sortBy=" + sortOrder).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var sleepString = await result.Content.ReadAsStringAsync();
                            SleepListPage sleepList = JsonConvert.DeserializeObject<SleepListPage>(sleepString);
                            foreach (Sleep slpItem in sleepList.SleepList)
                            {
                                slpItem.SleepStart = TimeZoneInfo.ConvertTimeFromUtc(slpItem.SleepStart, TimeZoneInfo.FindSystemTimeZoneById(timezone));
                                slpItem.SleepEnd = TimeZoneInfo.ConvertTimeFromUtc(slpItem.SleepEnd, TimeZoneInfo.FindSystemTimeZoneById(timezone));
                                DateTimeOffset sOffset = new DateTimeOffset(slpItem.SleepStart,
                                    TimeZoneInfo.FindSystemTimeZoneById(timezone).GetUtcOffset(slpItem.SleepStart));
                                DateTimeOffset eOffset = new DateTimeOffset(slpItem.SleepEnd,
                                    TimeZoneInfo.FindSystemTimeZoneById(timezone).GetUtcOffset(slpItem.SleepEnd));
                                slpItem.SleepDuration = eOffset - sOffset;
                            }
                            await App.Database.SaveSleepListPageAsync(progenyId, accessLevel, pageNumber, pageSize, sortOrder, sleepList);
                            return sleepList;
                        }
                        else
                        {
                            return new SleepListPage();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new SleepListPage();
                    }
                }
            }
            else
            {
                SleepListPage sleepList = await App.Database.GetSleepListPageAsync(progenyId, accessLevel, pageNumber, pageSize, sortOrder);
                if (sleepList == null)
                {
                    sleepList = new SleepListPage();
                }
                return sleepList;
            }
        }

        public static async Task<List<Friend>> GetFriendsList(int progenyId, int accessLevel)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                if (String.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/progenyfriendsmobile/" + Constants.DefaultChildId + "/" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var friendsString = await result.Content.ReadAsStringAsync();
                            List<Friend> frnList = JsonConvert.DeserializeObject<List<Friend>>(friendsString);
                            await App.Database.SaveFriendsListAsync(progenyId, accessLevel, frnList);
                            return frnList;
                        }
                        else
                        {
                            return new List<Friend>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<Friend>();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/friends/progenymobile/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var friendsString = await result.Content.ReadAsStringAsync();
                            List<Friend> frnList = JsonConvert.DeserializeObject<List<Friend>>(friendsString);
                            await App.Database.SaveFriendsListAsync(progenyId, accessLevel, frnList);
                            return frnList;
                        }
                        else
                        {
                            return new List<Friend>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<Friend>();
                    }
                }
            }
            else
            {
                List<Friend> frnList = await App.Database.GetFriendsListAsync(progenyId, accessLevel);
                return frnList;
            }
        }

        public static async Task<string> UploadFriendPicture(string fileName)
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
                try
                {
                    var result = await client.PostAsync("api/pictures/uploadfriendpicture/", content).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        string pictureResultString = Regex.Replace(resultString, @"([|""|])", "");
                        return pictureResultString;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return "";
                }
            }

            return "";
        }

        public static async Task<Friend> SaveFriend(Friend friend)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PostAsync("api/friends/", new StringContent(JsonConvert.SerializeObject(friend), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Friend resultFriend = JsonConvert.DeserializeObject<Friend>(resultString);
                        return resultFriend;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return friend;
                }
            }
            
            return friend;
        }

        public static async Task<string> UploadContactPicture(string fileName)
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
                try
                {
                    var result = await client.PostAsync("api/pictures/uploadcontactpicture/", content).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        string pictureResultString = Regex.Replace(resultString, @"([|""|])", "");
                        return pictureResultString;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return "";
                }
            }

            return "";
        }

        public static async Task<Address> SaveAddress(Address address)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PostAsync("api/addresses/", new StringContent(JsonConvert.SerializeObject(address), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Address resultAddress = JsonConvert.DeserializeObject<Address>(resultString);
                        return resultAddress;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return address;
                }
            }

            return address;
        }

        public static async Task<Contact> SaveContact(Contact contact)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PostAsync("api/contacts/", new StringContent(JsonConvert.SerializeObject(contact), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Contact resultContact = JsonConvert.DeserializeObject<Contact>(resultString);
                        return resultContact;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return contact;
                }
            }

            return contact;
        }

        public static async Task<MeasurementsListPage> GetMeasurementsListPage(int pageNumber, int pageSize, int progenyId, int accessLevel, int sortOrder)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                if (String.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/getmeasurementslistpage?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + Constants.DefaultChildId + "&accessLevel=" + accessLevel + "&sortBy=" + sortOrder).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var measurementsString = await result.Content.ReadAsStringAsync();
                            MeasurementsListPage measurementsList = JsonConvert.DeserializeObject<MeasurementsListPage>(measurementsString);
                            await App.Database.SaveMeasurementsListPageAsync(progenyId, accessLevel, pageNumber, pageSize, sortOrder, measurementsList);
                            return measurementsList;
                        }
                        else
                        {
                            return new MeasurementsListPage();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new MeasurementsListPage();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/measurements/getmeasurementslistpage?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + progenyId + "&accessLevel=" + accessLevel + "&sortBy=" + sortOrder).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var measurementsString = await result.Content.ReadAsStringAsync();
                            MeasurementsListPage measurementsList = JsonConvert.DeserializeObject<MeasurementsListPage>(measurementsString);
                            await App.Database.SaveMeasurementsListPageAsync(progenyId, accessLevel, pageNumber, pageSize, sortOrder, measurementsList);
                            return measurementsList;
                        }
                        else
                        {
                            return new MeasurementsListPage();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new MeasurementsListPage();
                    }
                }
            }
            else
            {
                MeasurementsListPage measurementsList = await App.Database.GetMeasurementsListPageAsync(progenyId, accessLevel, pageNumber, pageSize, sortOrder);
                if (measurementsList == null)
                {
                    measurementsList = new MeasurementsListPage();
                }
                return measurementsList;
            }
        }

        public static async Task<List<Measurement>> GetMeasurementsList(int progenyId, int accessLevel)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                if (String.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/measurementslist/" + Constants.DefaultChildId + "?accessLevel=" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var measurementsString = await result.Content.ReadAsStringAsync();
                            List<Measurement> measurementsList = JsonConvert.DeserializeObject<List<Measurement>>(measurementsString);
                            await App.Database.SaveMeasurementsListAsync(progenyId, accessLevel, measurementsList);
                            return measurementsList;
                        }
                        else
                        {
                            return new List<Measurement>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<Measurement>();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/measurements/progeny/" + progenyId + "?accessLevel=" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var measurementsString = await result.Content.ReadAsStringAsync();
                            List<Measurement> measurementsList = JsonConvert.DeserializeObject<List<Measurement>>(measurementsString);
                            await App.Database.SaveMeasurementsListAsync(progenyId, accessLevel, measurementsList);
                            return measurementsList;
                        }
                        else
                        {
                            return new List<Measurement>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<Measurement>();
                    }
                }
            }
            else
            {
                List<Measurement> measurementsList = await App.Database.GetMeasurementsListAsync(progenyId, accessLevel);
                return measurementsList;
            }
        }

        public static async Task<Measurement> SaveMeasurement(Measurement measurement)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PostAsync("api/measurements/", new StringContent(JsonConvert.SerializeObject(measurement), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Measurement resultMeasurement = JsonConvert.DeserializeObject<Measurement>(resultString);
                        return resultMeasurement;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return measurement;
                }
            }

            return measurement;
        }

        public static async Task<SkillsListPage> GetSkillsListPage(int pageNumber, int pageSize, int progenyId, int accessLevel, int sortOrder)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                if (String.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/getskillslistpage?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + Constants.DefaultChildId + "&accessLevel=" + accessLevel + "&sortBy=" + sortOrder).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var skillsString = await result.Content.ReadAsStringAsync();
                            SkillsListPage skillsListPage = JsonConvert.DeserializeObject<SkillsListPage>(skillsString);
                            await App.Database.SaveSkillsListPageAsync(progenyId, accessLevel, pageNumber, pageSize, sortOrder, skillsListPage);
                            return skillsListPage;
                        }
                        else
                        {
                            return new SkillsListPage();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new SkillsListPage();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/skills/getskillslistpage?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + progenyId + "&accessLevel=" + accessLevel + "&sortBy=" + sortOrder).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var skillsString = await result.Content.ReadAsStringAsync();
                            SkillsListPage skillsListPage = JsonConvert.DeserializeObject<SkillsListPage>(skillsString);
                            await App.Database.SaveSkillsListPageAsync(progenyId, accessLevel, pageNumber, pageSize, sortOrder, skillsListPage);
                            return skillsListPage;
                        }
                        else
                        {
                            return new SkillsListPage();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new SkillsListPage();
                    }
                }
            }
            else
            {
                SkillsListPage skillsListPage = await App.Database.GetSkillsListPageAsync(progenyId, accessLevel, pageNumber, pageSize, sortOrder);
                if (skillsListPage == null)
                {
                    skillsListPage = new SkillsListPage();
                }
                return skillsListPage;
            }
        }

        public static async Task<Skill> SaveSkill(Skill skill)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PostAsync("api/skills/", new StringContent(JsonConvert.SerializeObject(skill), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Skill resultSkill = JsonConvert.DeserializeObject<Skill>(resultString);
                        return resultSkill;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return skill;
                }
            }

            return skill;
        }

        public static async Task<VocabularyListPage> GetVocabularyListPage(int pageNumber, int pageSize, int progenyId, int accessLevel, int sortOrder)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                if (String.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/getvocabularylistpage?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + Constants.DefaultChildId + "&accessLevel=" + accessLevel + "&sortBy=" + sortOrder).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var vocabularyString = await result.Content.ReadAsStringAsync();
                            VocabularyListPage vocabularyListPage = JsonConvert.DeserializeObject<VocabularyListPage>(vocabularyString);
                            await App.Database.SaveVocabularyListPageAsync(progenyId, accessLevel, pageNumber, pageSize, sortOrder, vocabularyListPage);
                            return vocabularyListPage;
                        }
                        else
                        {
                            return new VocabularyListPage();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new VocabularyListPage();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/vocabulary/getvocabularylistpage?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + progenyId + "&accessLevel=" + accessLevel + "&sortBy=" + sortOrder).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var vocabularyString = await result.Content.ReadAsStringAsync();
                            VocabularyListPage vocabularyListPage = JsonConvert.DeserializeObject<VocabularyListPage>(vocabularyString);
                            await App.Database.SaveVocabularyListPageAsync(progenyId, accessLevel, pageNumber, pageSize, sortOrder, vocabularyListPage);
                            return vocabularyListPage;
                        }
                        else
                        {
                            return new VocabularyListPage();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new VocabularyListPage();
                    }
                }
            }
            else
            {
                VocabularyListPage vocabularyListPage = await App.Database.GetVocabularyListPageAsync(progenyId, accessLevel, pageNumber, pageSize, sortOrder);
                if (vocabularyListPage == null)
                {
                    vocabularyListPage = new VocabularyListPage();
                }
                return vocabularyListPage;
            }
        }

        public static async Task<List<VocabularyItem>> GetVocabularyList(int progenyId, int accessLevel, string timeZone)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                if (String.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/vocabularylist/" + Constants.DefaultChildId + "?accessLevel=" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var vocabularyString = await result.Content.ReadAsStringAsync();
                            List<VocabularyItem> vocabularyList = JsonConvert.DeserializeObject<List<VocabularyItem>>(vocabularyString);
                            foreach (VocabularyItem vocabItem in vocabularyList)
                            {
                                if (vocabItem.Date.HasValue)
                                {
                                    vocabItem.Date = TimeZoneInfo.ConvertTimeFromUtc(vocabItem.Date.Value, TimeZoneInfo.FindSystemTimeZoneById(timeZone));
                                }
                            }
                            await App.Database.SaveVocabularyListAsync(progenyId, accessLevel, vocabularyList);
                            return vocabularyList;
                        }
                        else
                        {
                            return new List<VocabularyItem>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<VocabularyItem>();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/vocabulary/progeny/" + progenyId + "?accessLevel=" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var vocabularyString = await result.Content.ReadAsStringAsync();
                            List<VocabularyItem> vocabularyList = JsonConvert.DeserializeObject<List<VocabularyItem>>(vocabularyString);
                            foreach (VocabularyItem vocabItem in vocabularyList)
                            {
                                if (vocabItem.Date.HasValue)
                                {
                                    vocabItem.Date = TimeZoneInfo.ConvertTimeFromUtc(vocabItem.Date.Value, TimeZoneInfo.FindSystemTimeZoneById(timeZone));
                                }
                            }
                            await App.Database.SaveVocabularyListAsync(progenyId, accessLevel, vocabularyList);
                            return vocabularyList;
                        }
                        else
                        {
                            return new List<VocabularyItem>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<VocabularyItem>();
                    }
                }
            }
            else
            {
                List<VocabularyItem> vocabularyList = await App.Database.GetVocabularyListAsync(progenyId, accessLevel);
                return vocabularyList;
            }
        }

        public static async Task<VocabularyItem> SaveVocabularyItem(VocabularyItem vocabularyItem)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PostAsync("api/vocabulary/", new StringContent(JsonConvert.SerializeObject(vocabularyItem), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        VocabularyItem resultVocabularyItem = JsonConvert.DeserializeObject<VocabularyItem>(resultString);
                        return resultVocabularyItem;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return vocabularyItem;
                }
            }

            return vocabularyItem;
        }

        public static async Task<Video> SaveVideo(Video video)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.MediaApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PostAsync("api/videos/", new StringContent(JsonConvert.SerializeObject(video), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Video resultVideo = JsonConvert.DeserializeObject<Video>(resultString);
                        return resultVideo;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return video;
                }
            }

            return video;
        }

        public static async Task<List<Vaccination>> GetVaccinationsList(int progenyId, int accessLevel)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                if (String.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/vaccinationslist/" + Constants.DefaultChildId + "?accessLevel=" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var vaccinationsString = await result.Content.ReadAsStringAsync();
                            List<Vaccination> vaccinationsList = JsonConvert.DeserializeObject<List<Vaccination>>(vaccinationsString);
                            await App.Database.SaveVaccinationsListAsync(progenyId, accessLevel, vaccinationsList);
                            return vaccinationsList;
                        }
                        else
                        {
                            return new List<Vaccination>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<Vaccination>();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/vaccinations/progeny/" + progenyId + "?accessLevel=" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var vaccinationsString = await result.Content.ReadAsStringAsync();
                            List<Vaccination> vaccinationsList = JsonConvert.DeserializeObject<List<Vaccination>>(vaccinationsString);
                            await App.Database.SaveVaccinationsListAsync(progenyId, accessLevel, vaccinationsList);
                            return vaccinationsList;
                        }
                        else
                        {
                            return new List<Vaccination>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<Vaccination>();
                    }
                }
            }
            else
            {
                List<Vaccination> vaccinationsList = await App.Database.GetVaccinationsListAsync(progenyId,accessLevel);
                if (vaccinationsList == null)
                {
                    vaccinationsList = new List<Vaccination>();
                }
                return vaccinationsList;
            }
        }

        public static async Task<Vaccination> SaveVaccination(Vaccination vaccination)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PostAsync("api/vaccinations/", new StringContent(JsonConvert.SerializeObject(vaccination), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Vaccination resultVaccination = JsonConvert.DeserializeObject<Vaccination>(resultString);
                        return resultVaccination;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return vaccination;
                }
            }

            return vaccination;
        }

        public static async Task<CalendarItem> SaveCalendarEvent(CalendarItem calendarItem)
        {
            if (Online())
            {
                try
                {
                    calendarItem.Progeny.TimeZone = await UserService.GetUserTimezone();
                }
                catch (Exception)
                {
                    calendarItem.Progeny.TimeZone = TZConvert.WindowsToIana(await UserService.GetUserTimezone());
                }

                if (calendarItem.StartTime.HasValue && calendarItem.EndTime.HasValue)
                {
                    calendarItem.StartTime = TimeZoneInfo.ConvertTimeToUtc(calendarItem.StartTime.Value, TimeZoneInfo.FindSystemTimeZoneById(calendarItem.Progeny.TimeZone));
                    calendarItem.EndTime = TimeZoneInfo.ConvertTimeToUtc(calendarItem.EndTime.Value, TimeZoneInfo.FindSystemTimeZoneById(calendarItem.Progeny.TimeZone));
                }

                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PostAsync("api/calendar/", new StringContent(JsonConvert.SerializeObject(calendarItem), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        CalendarItem resultEvent = JsonConvert.DeserializeObject<CalendarItem>(resultString);
                        return resultEvent;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return calendarItem;
                }
            }

            return calendarItem;
        }

        public static async Task<Note> SaveNote(Note note)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PostAsync("api/notes/", new StringContent(JsonConvert.SerializeObject(note), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Note resultNote = JsonConvert.DeserializeObject<Note>(resultString);
                        return resultNote;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return note;
                }
            }

            return note;
        }

        public static async Task<Location> SaveLocation(Location location)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PostAsync("api/locations/", new StringContent(JsonConvert.SerializeObject(location), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Location resultLocation = JsonConvert.DeserializeObject<Location>(resultString);
                        return resultLocation;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return location;
                }
            }

            return location;
        }

        public static async Task<NotesListPage> GetNotesPage(int pageNumber, int pageSize, int progenyId, int accessLevel, string timezone, int sortOrder)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                if (String.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/getnoteslistpage?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + Constants.DefaultChildId + "&accessLevel=" + accessLevel + "&sortBy=" + sortOrder).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var notesString = await result.Content.ReadAsStringAsync();
                            NotesListPage notesListPage = JsonConvert.DeserializeObject<NotesListPage>(notesString);
                            await App.Database.SaveNotesListPageAsync(progenyId, accessLevel, pageNumber, pageSize, sortOrder, notesListPage);
                            return notesListPage;
                        }
                        else
                        {
                            return new NotesListPage();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new NotesListPage();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/notes/getnoteslistpage?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + progenyId + "&accessLevel=" + accessLevel + "&sortBy=" + sortOrder).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var notesString = await result.Content.ReadAsStringAsync();
                            NotesListPage notesListPage = JsonConvert.DeserializeObject<NotesListPage>(notesString);
                            await App.Database.SaveNotesListPageAsync(progenyId, accessLevel, pageNumber, pageSize, sortOrder, notesListPage);
                            return notesListPage;
                        }
                        else
                        {
                            return new NotesListPage();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new NotesListPage();
                    }
                }
            }
            else
            {
                NotesListPage notesListPage = await App.Database.GetNotesListPageAsync(progenyId, accessLevel, pageNumber, pageSize, sortOrder);
                return notesListPage;
            }
        }

        public static async Task<LocationsListPage> GetLocationsPage(int pageNumber, int pageSize, int progenyId, int accessLevel, string timeZone, int sortOrder)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                if (String.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/getlocationslistpage?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + Constants.DefaultChildId + "&accessLevel=" + accessLevel + "&sortBy=" + sortOrder).ConfigureAwait(false); // Todo: Change to PublicAccess API

                        if (result.IsSuccessStatusCode)
                        {
                            var locationsString = await result.Content.ReadAsStringAsync();
                            LocationsListPage locationsListPage = JsonConvert.DeserializeObject<LocationsListPage>(locationsString);
                            if (locationsListPage != null && locationsListPage.LocationsList.Count > 0)
                            {
                                foreach (Location location in locationsListPage.LocationsList)
                                {
                                    if (location.Latitude != 0 && location.Longitude != 0)
                                    {
                                        location.Position = new Position(location.Latitude, location.Longitude);
                                    }
                                }
                            }
                            
                            await App.Database.SaveLocationsListPageAsync(progenyId, accessLevel, pageNumber, pageSize, sortOrder, locationsListPage);
                            return locationsListPage;
                        }
                        else
                        {
                            return new LocationsListPage();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new LocationsListPage();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/locations/getlocationslistpage?pageSize=" + pageSize + "&pageIndex=" + pageNumber + "&progenyId=" + progenyId + "&accessLevel=" + accessLevel + "&sortBy=" + sortOrder).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var locationsString = await result.Content.ReadAsStringAsync();
                            LocationsListPage locationsListPage = JsonConvert.DeserializeObject<LocationsListPage>(locationsString);
                            if (locationsListPage != null && locationsListPage.LocationsList.Count > 0)
                            {
                                foreach(Location location in locationsListPage.LocationsList)
                                {
                                    if (location.Latitude != 0 && location.Longitude != 0)
                                    {
                                        location.Position = new Position(location.Latitude, location.Longitude);
                                    }
                                }
                            }
                            await App.Database.SaveLocationsListPageAsync(progenyId, accessLevel, pageNumber, pageSize, sortOrder, locationsListPage);
                            return locationsListPage;
                        }
                        else
                        {
                            return new LocationsListPage();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new LocationsListPage();
                    }
                }
            }
            else
            {
                LocationsListPage locationsListPage = await App.Database.GetLocationsListPageAsync(progenyId, accessLevel, pageNumber, pageSize, sortOrder);
                if (locationsListPage == null)
                {
                    locationsListPage = new LocationsListPage();
                }
                return locationsListPage;
            }
        }

        public static async Task<List<Location>> GetLocationsList(int progenyId, int accessLevel)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                if (String.IsNullOrEmpty(accessToken))
                {

                    var result = await client.GetAsync("api/publicaccess/locationslist/" + progenyId + "?accessLevel=" + accessLevel).ConfigureAwait(false); // Todo: Change to PublicAccess API

                    if (result.IsSuccessStatusCode)
                    {
                        var locationsString = await result.Content.ReadAsStringAsync();
                        List<Location> locationsList = JsonConvert.DeserializeObject<List<Location>>(locationsString);
                        if (locationsList != null && locationsList.Count > 0)
                        {
                            foreach (Location location in locationsList)
                            {
                                if (location.Latitude != 0 && location.Longitude != 0)
                                {
                                    location.Position = new Position(location.Latitude, location.Longitude);
                                }
                            }
                        }
                        await App.Database.SaveLocationsListAsync(progenyId, accessLevel, locationsList);
                        return locationsList;
                    }
                    else
                    {
                        return new List<Location>();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    var result = await client.GetAsync("api/locations/progeny/" + progenyId + "?accessLevel=" + accessLevel).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var locationsString = await result.Content.ReadAsStringAsync();
                        List<Location> locationsList = JsonConvert.DeserializeObject<List<Location>>(locationsString);
                        if (locationsList != null && locationsList.Count > 0)
                        {
                            foreach (Location location in locationsList)
                            {
                                if (location.Latitude != 0 && location.Longitude != 0)
                                {
                                    location.Position = new Position(location.Latitude, location.Longitude);
                                }
                            }
                        }
                        await App.Database.SaveLocationsListAsync(progenyId, accessLevel, locationsList);
                        return locationsList;
                    }
                    else
                    {
                        return new List<Location>();
                    }
                }
            }
            else
            {
                List<Location> locationsList = await App.Database.GeLocationsListAsync(progenyId, accessLevel);
                return locationsList;
            }
        }

        public static async Task<List<Picture>> GetPicturesList(int progenyId, int userAccessLevel, string userTimezone)
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

                    string pageApiPath = "api/publicaccess/picturelist/" + progenyId + "?accessLevel=" + userAccessLevel;
                    try
                    {
                        var result = await client.GetAsync(pageApiPath).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var pictureListString = await result.Content.ReadAsStringAsync();
                            List<Picture> pictureList = JsonConvert.DeserializeObject<List<Picture>>(pictureListString);
                            foreach (Picture picture in pictureList)
                            {
                                if (picture.PictureTime.HasValue)
                                {
                                    picture.PictureTime = TimeZoneInfo.ConvertTimeFromUtc(picture.PictureTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                                }

                                picture.CommentsCount = picture.Comments.Count;
                                await App.Database.SavePictureAsync(picture);
                            }
                            await App.Database.SavePicturesListAsync(progenyId, userAccessLevel, pictureList);
                            return pictureList;
                        }
                        else
                        {
                            return new List<Picture>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<Picture>();
                    }
                }
                else // If user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    string pageApiPath = "api/pictures/progeny/" + progenyId + "/" + userAccessLevel;
                    try
                    {
                        var result = await client.GetAsync(pageApiPath).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var pictureListString = await result.Content.ReadAsStringAsync();
                            List<Picture> pictureList = JsonConvert.DeserializeObject<List<Picture>>(pictureListString);
                            foreach (Picture picture in pictureList)
                            {
                                if (picture.PictureTime.HasValue)
                                {
                                    picture.PictureTime = TimeZoneInfo.ConvertTimeFromUtc(picture.PictureTime.Value, TimeZoneInfo.FindSystemTimeZoneById(userTimezone));
                                }
                                picture.CommentsCount = picture.Comments.Count;
                                await App.Database.SavePictureAsync(picture);
                            }

                            await App.Database.SavePicturesListAsync(progenyId, userAccessLevel, pictureList);
                            return pictureList;
                        }
                        else
                        {
                            return new List<Picture>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<Picture>();
                    }
                }
            }
            else
            {
                List<Picture> pictureList = await App.Database.GetPicturesListAsync(progenyId, userAccessLevel);
                return pictureList;
            }
        }

        public static async Task<List<Sleep>> GetSleepDetails(int sleepId, int accessLevel, string timezone, int sortOrder)
        {
            bool online = Online();
            if (online)
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                string accessToken = await UserService.GetAuthAccessToken();

                if (String.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        var result = await client.GetAsync("api/publicaccess/getsleepdetails?sleepId=" + sleepId + "&accessLevel=" + accessLevel + "&sortBy=" + sortOrder).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var sleepString = await result.Content.ReadAsStringAsync();
                            List<Sleep> sleepList = JsonConvert.DeserializeObject<List<Sleep>>(sleepString);
                            await App.Database.SaveSleepDetailsAsync( accessLevel, sleepId, sortOrder, sleepList);
                            return sleepList;
                        }
                        else
                        {
                            return new List<Sleep>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<Sleep>();
                    }
                }
                else
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/sleep/getsleepdetails/" + sleepId + "/" + accessLevel + "/" + sortOrder).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var sleepString = await result.Content.ReadAsStringAsync();
                            List<Sleep> sleepList = JsonConvert.DeserializeObject<List<Sleep>>(sleepString);
                            foreach (Sleep slpItem in sleepList)
                            {
                                slpItem.SleepStart = TimeZoneInfo.ConvertTimeFromUtc(slpItem.SleepStart, TimeZoneInfo.FindSystemTimeZoneById(timezone));
                                slpItem.SleepEnd = TimeZoneInfo.ConvertTimeFromUtc(slpItem.SleepEnd, TimeZoneInfo.FindSystemTimeZoneById(timezone));
                            
                            }
                            await App.Database.SaveSleepDetailsAsync(accessLevel, sleepId, sortOrder, sleepList);
                            return sleepList;
                        }
                        else
                        {
                            return new List<Sleep>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<Sleep>();
                    }
                }
            }
            else
            {
                List<Sleep> sleepList = await App.Database.GetSleepDetailsAsync(accessLevel, sleepId, sortOrder);
                if (sleepList == null)
                {
                    sleepList = new List<Sleep>();
                }
                return sleepList;
            }
        }

        public static async Task<Sleep> UpdateSleep(Sleep sleep)
        {
            if (Online())
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
                try
                {
                    var result = await client.PutAsync("api/sleep/" + sleep.SleepId, new StringContent(JsonConvert.SerializeObject(sleep), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Sleep resultSleep = JsonConvert.DeserializeObject<Sleep>(resultString);
                        return resultSleep;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return sleep;
                }
            }

            return sleep;
        }

        public static async Task<CalendarItem> UpdateCalendarItem(CalendarItem calendarItem)
        {
            if (Online())
            {
                try
                {
                    TimeZoneInfo.FindSystemTimeZoneById(await UserService.GetUserTimezone());
                }
                catch (Exception)
                {
                    calendarItem.Progeny.TimeZone = TZConvert.WindowsToIana(await UserService.GetUserTimezone());
                }

                if (calendarItem.StartTime.HasValue && calendarItem.EndTime.HasValue)
                {
                    calendarItem.StartTime = TimeZoneInfo.ConvertTimeToUtc(calendarItem.StartTime.Value, TimeZoneInfo.FindSystemTimeZoneById(calendarItem.Progeny.TimeZone));
                    calendarItem.EndTime = TimeZoneInfo.ConvertTimeToUtc(calendarItem.EndTime.Value, TimeZoneInfo.FindSystemTimeZoneById(calendarItem.Progeny.TimeZone));
                }

                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PutAsync("api/calendar/" + calendarItem.EventId, new StringContent(JsonConvert.SerializeObject(calendarItem), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        CalendarItem resultCalendarItem = JsonConvert.DeserializeObject<CalendarItem>(resultString);
                        return resultCalendarItem;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return calendarItem;
                }
            }

            return calendarItem;
        }

        public static async Task<Sleep> DeleteSleep(Sleep sleep)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.DeleteAsync("api/sleep/" + sleep.SleepId).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        Sleep deletedSleep = new Sleep();
                        deletedSleep.SleepId = 0;
                        return deletedSleep;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return sleep;
                }
            }

            return sleep;
        }

        public static async Task<CalendarItem> DeleteCalendarItem(CalendarItem calendarItem)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.DeleteAsync("api/calendar/" + calendarItem.EventId).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        CalendarItem deletedCalendarItem = new CalendarItem();
                        deletedCalendarItem.EventId = 0;
                        return deletedCalendarItem;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return calendarItem;
                }
            }

            return calendarItem;
        }

        public static async Task<Picture> UpdatePicture(Picture picture)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.MediaApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PutAsync("api/pictures/" + picture.PictureId, new StringContent(JsonConvert.SerializeObject(picture), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Picture resultPicture = JsonConvert.DeserializeObject<Picture>(resultString);
                        return resultPicture;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return new Picture();
                }
            }

            return new Picture();
        }

        public static async Task<TimeLineItem> GetTimeLineItemByItemId(int itemId, KinaUnaTypes.TimeLineType timeLineType)
        {
            bool online = Online();
            
            if (online)
            {
                string accessToken = await UserService.GetAuthAccessToken();
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                // If the user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {
                    return new TimeLineItem();
                }
                else // If the user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/timeline/gettimelineitembyitemid/" + itemId + "/" + (int)timeLineType).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var timeLineItemString = await result.Content.ReadAsStringAsync();
                            TimeLineItem timeLineItem = JsonConvert.DeserializeObject<TimeLineItem>(timeLineItemString);
                            // await SecureStorage.SetAsync("TimeLineItem" + itemId + "Type" + (int)timeLineType, JsonConvert.SerializeObject(timeLineItem));
                            await App.Database.SaveTimeLineItemByItemIdAsync(itemId, (int)timeLineType, timeLineItem);
                            return timeLineItem;
                        }
                        else
                        {
                            return new TimeLineItem();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new TimeLineItem();
                    }
                }
            }
            else
            {
                TimeLineItem timeLineItem = await App.Database.GetTimeLineItemByItemIdAsync(itemId, (int)timeLineType);
                return timeLineItem;
            }
        }

        public static async Task<TimeLineItem> UpdateTimeLineItem(TimeLineItem timeLineItem)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PutAsync("api/timeline/" + timeLineItem.TimeLineId, new StringContent(JsonConvert.SerializeObject(timeLineItem), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        TimeLineItem resultTimeLineItem = JsonConvert.DeserializeObject<TimeLineItem>(resultString);
                        return resultTimeLineItem;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return new TimeLineItem();
                }
            }

            return new TimeLineItem();
        }

        public static async Task<Picture> DeletePicture(int pictureId)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.MediaApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.DeleteAsync("api/pictures/" + pictureId).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        Picture deletedPicture = new Picture();
                        deletedPicture.PictureId = 0;
                        return deletedPicture;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Picture failPicture1 = new Picture();
                    failPicture1.PictureId = pictureId;
                    return failPicture1;
                }
            }

            Picture failPicture = new Picture();
            failPicture.PictureId = pictureId;
            return failPicture;
        }

        public static async Task<Video> UpdateVideo(Video video)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.MediaApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PutAsync("api/videos/" + video.VideoId, new StringContent(JsonConvert.SerializeObject(video), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Video resultVideo = JsonConvert.DeserializeObject<Video>(resultString);
                        return resultVideo;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return new Video();
                }
            }

            return new Video();
        }

        public static async Task<Video> DeleteVideo(int videoId)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.MediaApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.DeleteAsync("api/videos/" + videoId).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        Video deletedVideo = new Video();
                        deletedVideo.VideoId = 0;
                        return deletedVideo;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Video failVideo1 = new Video();
                    failVideo1.VideoId = videoId;
                    return failVideo1;
                }
            }

            Video failVideo = new Video();
            failVideo.VideoId = videoId;
            return failVideo;
        }

        public static async Task<TimeLineItem> DeleteTimeLineItem(TimeLineItem timeLineItem)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.DeleteAsync("api/timeline/" + timeLineItem.TimeLineId).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        TimeLineItem deletedTimeLineItem = new TimeLineItem();
                        deletedTimeLineItem.TimeLineId = 0;
                        return deletedTimeLineItem;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    TimeLineItem failTimeLineItem1 = new TimeLineItem();
                    failTimeLineItem1.TimeLineId = timeLineItem.TimeLineId;
                    return failTimeLineItem1;
                }
            }

            TimeLineItem failTimeLineItem = new TimeLineItem();
            failTimeLineItem.TimeLineId = timeLineItem.TimeLineId;
            return failTimeLineItem;
        }

        public static async Task<Location> UpdateLocation(Location location)
        {
            if (Online())
            {
                try
                {
                    TimeZoneInfo.FindSystemTimeZoneById(location.Progeny.TimeZone);
                }
                catch (Exception)
                {
                    location.Progeny.TimeZone = TZConvert.WindowsToIana(location.Progeny.TimeZone);
                }

                if (location.Date.HasValue)
                {
                    location.Date = TimeZoneInfo.ConvertTimeToUtc(location.Date.Value, TimeZoneInfo.FindSystemTimeZoneById(location.Progeny.TimeZone));
                }

                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PutAsync("api/locations/" + location.LocationId, new StringContent(JsonConvert.SerializeObject(location), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Location resultLocationItem = JsonConvert.DeserializeObject<Location>(resultString);
                        return resultLocationItem;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return location;
                }
            }

            return location;
        }

        public static async Task<Location> DeleteLocation(Location location)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.DeleteAsync("api/locations/" + location.LocationId).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        Location deletedLocation = new Location();
                        deletedLocation.LocationId = 0;
                        return deletedLocation;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return location;
                }
            }

            return location;
        }

        public static async Task<Vaccination> UpdateVaccination(Vaccination vaccination)
        {
            if (Online())
            {
                try
                {
                    TimeZoneInfo.FindSystemTimeZoneById(vaccination.Progeny.TimeZone);
                }
                catch (Exception)
                {
                    vaccination.Progeny.TimeZone = TZConvert.WindowsToIana(vaccination.Progeny.TimeZone);
                }

                vaccination.VaccinationDate = TimeZoneInfo.ConvertTimeToUtc(vaccination.VaccinationDate, TimeZoneInfo.FindSystemTimeZoneById(vaccination.Progeny.TimeZone));

                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PutAsync("api/vaccinations/" + vaccination.VaccinationId, new StringContent(JsonConvert.SerializeObject(vaccination), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Vaccination resultVaccinationItem = JsonConvert.DeserializeObject<Vaccination>(resultString);
                        return resultVaccinationItem;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return vaccination;
                }
            }

            return vaccination;
        }

        public static async Task<Vaccination> DeleteVaccination(Vaccination vaccination)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.DeleteAsync("api/vaccinations/" + vaccination.VaccinationId).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        Vaccination deletedVaccination = new Vaccination();
                        deletedVaccination.VaccinationId = 0;
                        return deletedVaccination;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return vaccination;
                }
            }

            return vaccination;
        }

        public static async Task<List<string>> GetLocationAutoSuggestList(int progenyId, int accessLevel)
        {
            bool online = Online();

            if (online)
            {
                string accessToken = await UserService.GetAuthAccessToken();
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.MediaApiUrl);

                // If the user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {
                    return new List<string>();
                }
                else // If the user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/autosuggests/getlocationautosuggestlist/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var autoSuggestListString = await result.Content.ReadAsStringAsync();
                            List<string> autoSuggestList = JsonConvert.DeserializeObject<List<string>>(autoSuggestListString);
                            // await SecureStorage.SetAsync("LocationAutoSuggestList" + progenyId + "AL" + accessLevel, JsonConvert.SerializeObject(autoSuggestList));
                            await App.Database.SaveLocationAutoSuggestListAsync(progenyId, accessLevel, autoSuggestList);
                            return autoSuggestList;
                        }
                        else
                        {
                            return new List<string>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<string>();
                    }
                }
            }
            else
            {
                List<string> autoSuggestList = await App.Database.GetLocationAutoSuggestListAsync(progenyId, accessLevel);
                if (autoSuggestList == null)
                {
                    autoSuggestList = new List<string>();
                }
                return autoSuggestList;
            }
        }

        public static async Task<List<string>> GetTagsAutoSuggestList(int progenyId, int accessLevel)
        {
            bool online = Online();

            if (online)
            {
                string accessToken = await UserService.GetAuthAccessToken();
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.MediaApiUrl);

                // If the user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {
                    return new List<string>();
                }
                else // If the user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/autosuggests/gettagsautosuggestlist/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var autoSuggestListString = await result.Content.ReadAsStringAsync();
                            List<string> autoSuggestList = JsonConvert.DeserializeObject<List<string>>(autoSuggestListString);
                            // await SecureStorage.SetAsync("TagsAutoSuggestList" + progenyId + "AL" + accessLevel, JsonConvert.SerializeObject(autoSuggestList));
                            await App.Database.SaveTagsAutoSuggestListAsync(progenyId, accessLevel, autoSuggestList);
                            return autoSuggestList;
                        }
                        else
                        {
                            return new List<string>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<string>();
                    }
                }
            }
            else
            {
                List<string> autoSuggestList = await App.Database.GetLocationAutoSuggestListAsync(progenyId, accessLevel);
                if (autoSuggestList == null)
                {
                    autoSuggestList = new List<string>();
                }
                return autoSuggestList;
            }
        }

        public static async Task<List<string>> GetCategoryAutoSuggestList(int progenyId, int accessLevel)
        {
            bool online = Online();

            if (online)
            {
                string accessToken = await UserService.GetAuthAccessToken();
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                // If the user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {
                    return new List<string>();
                }
                else // If the user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/autosuggests/getcategoryautosuggestlist/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var autoSuggestListString = await result.Content.ReadAsStringAsync();
                            List<string> autoSuggestList = JsonConvert.DeserializeObject<List<string>>(autoSuggestListString);
                            await App.Database.SaveCategoryAutoSuggestListAsync(progenyId, accessLevel, autoSuggestList);
                            return autoSuggestList;
                        }
                        else
                        {
                            return new List<string>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<string>();
                    }
                }
            }
            else
            {
                List<string> autoSuggestList = await App.Database.GetLocationAutoSuggestListAsync(progenyId, accessLevel);
                return autoSuggestList;
            }
        }

        public static async Task<List<string>> GetContextAutoSuggestList(int progenyId, int accessLevel)
        {
            bool online = Online();

            if (online)
            {
                string accessToken = await UserService.GetAuthAccessToken();
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);

                // If the user is not logged in.
                if (String.IsNullOrEmpty(accessToken))
                {
                    return new List<string>();
                }
                else // If the user is logged in.
                {
                    client.SetBearerToken(accessToken);

                    try
                    {
                        var result = await client.GetAsync("api/autosuggests/getcontextautosuggestlist/" + progenyId + "/" + accessLevel).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var autoSuggestListString = await result.Content.ReadAsStringAsync();
                            List<string> autoSuggestList = JsonConvert.DeserializeObject<List<string>>(autoSuggestListString);
                            await App.Database.SaveContextAutoSuggestListAsync(progenyId, accessLevel, autoSuggestList);
                            return autoSuggestList;
                        }
                        else
                        {
                            return new List<string>();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return new List<string>();
                    }
                }
            }
            else
            {
                List<string> autoSuggestList = await App.Database.GetLocationAutoSuggestListAsync(progenyId, accessLevel);
                return autoSuggestList;
            }
        }

        public static async Task<VocabularyItem> UpdateVocabularyItem(VocabularyItem vocabularyItem)
        {
            if (Online())
            {
                try
                {
                    TimeZoneInfo.FindSystemTimeZoneById(vocabularyItem.Progeny.TimeZone);
                }
                catch (Exception)
                {
                    vocabularyItem.Progeny.TimeZone = TZConvert.WindowsToIana(vocabularyItem.Progeny.TimeZone);
                }

                if (vocabularyItem.Date.HasValue)
                {
                    vocabularyItem.Date = TimeZoneInfo.ConvertTimeToUtc(vocabularyItem.Date.Value,
                        TimeZoneInfo.FindSystemTimeZoneById(vocabularyItem.Progeny.TimeZone));
                }

                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PutAsync("api/vocabulary/" + vocabularyItem.WordId, new StringContent(JsonConvert.SerializeObject(vocabularyItem), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        VocabularyItem resultItem = JsonConvert.DeserializeObject<VocabularyItem>(resultString);
                        return resultItem;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return vocabularyItem;
                }
            }

            return vocabularyItem;
        }

        public static async Task<VocabularyItem> DeleteVocabularyItem(VocabularyItem vocabularyItem)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.DeleteAsync("api/vocabulary/" + vocabularyItem.WordId).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        VocabularyItem deleteVocabularyItem = new VocabularyItem();
                        deleteVocabularyItem.WordId = 0;
                        return deleteVocabularyItem;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return vocabularyItem;
                }
            }

            return vocabularyItem;
        }

        public static async Task<Skill> UpdateSkill(Skill skill)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PutAsync("api/skills/" + skill.SkillId, new StringContent(JsonConvert.SerializeObject(skill), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Skill resultItem = JsonConvert.DeserializeObject<Skill>(resultString);
                        return resultItem;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return skill;
                }
            }

            return skill;
        }

        public static async Task<Skill> DeleteSkill(Skill skill)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.DeleteAsync("api/skills/" + skill.SkillId).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        Skill deleteSkillItem = new Skill();
                        deleteSkillItem.SkillId = 0;
                        return deleteSkillItem;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return skill;
                }
            }

            return skill;
        }

        public static async Task<Measurement> UpdateMeasurement(Measurement measurement)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PutAsync("api/measurements/" + measurement.MeasurementId, new StringContent(JsonConvert.SerializeObject(measurement), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Measurement resultItem = JsonConvert.DeserializeObject<Measurement>(resultString);
                        return resultItem;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return measurement;
                }
            }

            return measurement;
        }

        public static async Task<Measurement> DeleteMeasurement(Measurement measurement)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.DeleteAsync("api/measurements/" + measurement.MeasurementId).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        Measurement deleteMeasurementItem = new Measurement();
                        deleteMeasurementItem.MeasurementId = 0;
                        return deleteMeasurementItem;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return measurement;
                }
            }

            return measurement;
        }

        public static async Task<Friend> UpdateFriend(Friend friend)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PutAsync("api/friends/" + friend.FriendId, new StringContent(JsonConvert.SerializeObject(friend), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Friend resultItem = JsonConvert.DeserializeObject<Friend>(resultString);
                        return resultItem;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return friend;
                }
            }

            return friend;
        }

        public static async Task<Friend> DeleteFriend(Friend friend)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.DeleteAsync("api/friends/" + friend.FriendId).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        Friend deleteFriendItem = new Friend();
                        deleteFriendItem.FriendId = 0;
                        return deleteFriendItem;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return friend;
                }
            }

            return friend;
        }

        public static async Task<Contact> UpdateContact(Contact contact)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PutAsync("api/contacts/" + contact.ContactId, new StringContent(JsonConvert.SerializeObject(contact), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Contact resultItem = JsonConvert.DeserializeObject<Contact>(resultString);
                        return resultItem;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return contact;
                }
            }

            return contact;
        }

        public static async Task<Contact> DeleteContact(Contact contact)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.DeleteAsync("api/contacts/" + contact.ContactId).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        Contact deleteContactItem = new Contact();
                        deleteContactItem.ContactId = 0;
                        return deleteContactItem;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return contact;
                }
            }

            return contact;
        }

        public static async Task<Note> UpdateNote(Note note)
        {
            if (Online())
            {
                string timeZone = await UserService.GetUserTimezone();
                try
                {
                    TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                }
                catch (Exception)
                {
                    timeZone = TZConvert.WindowsToIana(timeZone);
                }

                note.CreatedDate = TimeZoneInfo.ConvertTimeToUtc(note.CreatedDate, TimeZoneInfo.FindSystemTimeZoneById(timeZone));
                

                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.PutAsync("api/notes/" + note.NoteId, new StringContent(JsonConvert.SerializeObject(note), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        string resultString = await result.Content.ReadAsStringAsync();
                        Note resultItem = JsonConvert.DeserializeObject<Note>(resultString);
                        return resultItem;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return note;
                }
            }

            return note;
        }

        public static async Task<Note> DeleteNote(Note note)
        {
            if (Online())
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Constants.ProgenyApiUrl);
                string accessToken = await UserService.GetAuthAccessToken();
                client.SetBearerToken(accessToken);
                try
                {
                    var result = await client.DeleteAsync("api/notes/" + note.NoteId).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        Note deleteNote = new Note();
                        deleteNote.NoteId = 0;
                        return deleteNote;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return note;
                }
            }

            return note;
        }
    }
}

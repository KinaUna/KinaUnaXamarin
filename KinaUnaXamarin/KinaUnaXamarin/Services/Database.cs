using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Models.SQLite;
using Newtonsoft.Json;
using Polly;
using SQLite;

namespace KinaUnaXamarin.Services
{
    public class Database
    {
        readonly SQLiteAsyncConnection _database;
        
        public Database(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
            _database.EnableWriteAheadLoggingAsync().Wait();
            _database.CreateTableAsync<ProgenyDto>().Wait();
            _database.CreateTableAsync<CommentDto>().Wait();
            _database.CreateTableAsync<CommentsList>().Wait();
            _database.CreateTableAsync<ProgenyList>().Wait();
            _database.CreateTableAsync<UserAccessDto>().Wait();
            _database.CreateTableAsync<UpcomingEvents>().Wait();
            _database.CreateTableAsync<TimeLineList>().Wait();
            _database.CreateTableAsync<TimeLineLatest>().Wait();
            _database.CreateTableAsync<PictureDto>().Wait();
            _database.CreateTableAsync<PicturePageList>().Wait();
            _database.CreateTableAsync<PictureViewModelDto>().Wait();
            _database.CreateTableAsync<VideoDto>().Wait();
            _database.CreateTableAsync<VideoPageList>().Wait();
            _database.CreateTableAsync<VideoViewModelDto>().Wait();
            _database.CreateTableAsync<EventDto>().Wait();
            _database.CreateTableAsync<CalendarList>().Wait();
            _database.CreateTableAsync<LocationDto>().Wait();
            _database.CreateTableAsync<WordDto>().Wait();
            _database.CreateTableAsync<SkillDto>().Wait();
            _database.CreateTableAsync<FriendDto>().Wait();
            _database.CreateTableAsync<MeasurementDto>().Wait();
            _database.CreateTableAsync<SleepDto>().Wait();
            _database.CreateTableAsync<SleepList>().Wait();
            _database.CreateTableAsync<SleepStatsModelDto>().Wait();
            _database.CreateTableAsync<SleepChartDto>().Wait();
            _database.CreateTableAsync<NoteDto>().Wait();
            _database.CreateTableAsync<ContactDto>().Wait();
            _database.CreateTableAsync<ContactList>().Wait();
            _database.CreateTableAsync<VaccinationDto>().Wait();
            _database.CreateTableAsync<ProgenyAccessList>().Wait();
            _database.CreateTableAsync<SleepListPageDto>().Wait();
            _database.CreateTableAsync<FriendsList>().Wait();
            _database.CreateTableAsync<MeasurementsListPageDto>().Wait();
            _database.CreateTableAsync<MeasurementsList>().Wait();
            _database.CreateTableAsync<SkillsListPageDto>().Wait();
            _database.CreateTableAsync<VocabularyListPageDto>().Wait();
            _database.CreateTableAsync<VocabularyList>().Wait();
            _database.CreateTableAsync<VaccinationsList>().Wait();
            _database.CreateTableAsync<NotesListPageDto>().Wait();
            _database.CreateTableAsync<LocationsPageDto>().Wait();
            _database.CreateTableAsync<LocationsList>().Wait();
            _database.CreateTableAsync<PicturesList>().Wait();
            _database.CreateTableAsync<SleepDetails>().Wait();
            _database.CreateTableAsync<TimeLineItemDto>().Wait();
            _database.CreateTableAsync<LocationAutoSuggestList>().Wait();
            _database.CreateTableAsync<TagsAutoSuggestList>().Wait();
            _database.CreateTableAsync<CategoryAutoSuggestList>().Wait();
            _database.CreateTableAsync<ContextAutoSuggestList>().Wait();
            _database.CreateTableAsync<UserInfoDto>().Wait();
            _database.CreateTableAsync<UserPictureDto>().Wait();
        }

        public void ResetAll()
        {

            _database.DeleteAllAsync<ProgenyDto>().Wait();
            _database.DeleteAllAsync<CommentDto>().Wait();
            _database.DeleteAllAsync<CommentsList>().Wait();
            _database.DeleteAllAsync<ProgenyList>().Wait();
            _database.DeleteAllAsync<UserAccessDto>().Wait();
            _database.DeleteAllAsync<UpcomingEvents>().Wait();
            _database.DeleteAllAsync<TimeLineList>().Wait();
            _database.DeleteAllAsync<TimeLineLatest>().Wait();
            _database.DeleteAllAsync<PictureDto>().Wait();
            _database.DeleteAllAsync<PicturePageList>().Wait();
            _database.DeleteAllAsync<PictureViewModelDto>().Wait();
            _database.DeleteAllAsync<VideoDto>().Wait();
            _database.DeleteAllAsync<VideoPageList>().Wait();
            _database.DeleteAllAsync<VideoViewModelDto>().Wait();
            _database.DeleteAllAsync<EventDto>().Wait();
            _database.DeleteAllAsync<CalendarList>().Wait();
            _database.DeleteAllAsync<LocationDto>().Wait();
            _database.DeleteAllAsync<WordDto>().Wait();
            _database.DeleteAllAsync<SkillDto>().Wait();
            _database.DeleteAllAsync<FriendDto>().Wait();
            _database.DeleteAllAsync<MeasurementDto>().Wait();
            _database.DeleteAllAsync<SleepDto>().Wait();
            _database.DeleteAllAsync<SleepList>().Wait();
            _database.DeleteAllAsync<SleepStatsModelDto>().Wait();
            _database.DeleteAllAsync<SleepChartDto>().Wait();
            _database.DeleteAllAsync<NoteDto>().Wait();
            _database.DeleteAllAsync<ContactDto>().Wait();
            _database.DeleteAllAsync<ContactList>().Wait();
            _database.DeleteAllAsync<VaccinationDto>().Wait();
            _database.DeleteAllAsync<ProgenyAccessList>().Wait();
            _database.DeleteAllAsync<SleepListPageDto>().Wait();
            _database.DeleteAllAsync<FriendsList>().Wait();
            _database.DeleteAllAsync<MeasurementsListPageDto>().Wait();
            _database.DeleteAllAsync<MeasurementsList>().Wait();
            _database.DeleteAllAsync<SkillsListPageDto>().Wait();
            _database.DeleteAllAsync<VocabularyListPageDto>().Wait();
            _database.DeleteAllAsync<VocabularyList>().Wait();
            _database.DeleteAllAsync<VaccinationsList>().Wait();
            _database.DeleteAllAsync<NotesListPageDto>().Wait();
            _database.DeleteAllAsync<LocationsPageDto>().Wait();
            _database.DeleteAllAsync<LocationsList>().Wait();
            _database.DeleteAllAsync<PicturesList>().Wait();
            _database.DeleteAllAsync<SleepDetails>().Wait();
            _database.DeleteAllAsync<TimeLineItemDto>().Wait();
            _database.DeleteAllAsync<LocationAutoSuggestList>().Wait();
            _database.DeleteAllAsync<TagsAutoSuggestList>().Wait();
            _database.DeleteAllAsync<CategoryAutoSuggestList>().Wait();
            _database.DeleteAllAsync<ContextAutoSuggestList>().Wait();
            _database.DeleteAllAsync<UserInfoDto>().Wait();
            _database.DeleteAllAsync<UserPictureDto>().Wait();
        }

        protected static Task<T> AttemptAndRetry<T>(Func<Task<T>> action, int numRetries = 3)
        {
            return Policy.Handle<SQLiteException>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).ExecuteAsync(action);

            TimeSpan pollyRetryAttempt(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));
        }

        public async Task<List<Progeny>> GetProgenyListAsync()
        {
            List<ProgenyDto> dtos = await AttemptAndRetry(() => _database.Table<ProgenyDto>().ToListAsync()).ConfigureAwait(false);
            List<Progeny> progenyList = new List<Progeny>();
            foreach (ProgenyDto progDto in dtos)
            {
                Progeny prog = JsonConvert.DeserializeObject<Progeny>(progDto.ProgenyString);
                progenyList.Add(prog);
            }

            return progenyList;
        }

        public async Task<Progeny> GetProgenyAsync(int progenyId)
        {
            ProgenyDto progDto = await AttemptAndRetry(() => _database.Table<ProgenyDto>().FirstOrDefaultAsync(p => p.ProgenyId == progenyId)).ConfigureAwait(false);
            if (progDto == null)
            {
                return OfflineDefaultData.DefaultProgeny;
            }
            Progeny prog = JsonConvert.DeserializeObject<Progeny>(progDto.ProgenyString);
            return prog;
        }
        public async Task<int> SaveProgenyAsync(Progeny progeny)
        {
            ProgenyDto progDto = await AttemptAndRetry(() => _database.Table<ProgenyDto>().FirstOrDefaultAsync(p => p.ProgenyId == progeny.Id)).ConfigureAwait(false);
            if (progDto == null)
            {
                progDto = new ProgenyDto();
                progDto.ProgenyId = progeny.Id;
                progDto.ProgenyString = JsonConvert.SerializeObject(progeny);
                return await AttemptAndRetry(() => _database.InsertAsync(progDto)).ConfigureAwait(false);
            }

            progDto.ProgenyString = JsonConvert.SerializeObject(progeny);
            return await AttemptAndRetry(() => _database.UpdateAsync(progDto)).ConfigureAwait(false);
        }

        public async Task<Comment> GetCommentAsync(int commentId)
        {
            CommentDto cmntDto = await AttemptAndRetry(() => _database.Table<CommentDto>().FirstOrDefaultAsync(c => c.CommentId == commentId)).ConfigureAwait(false);
            Comment comment = JsonConvert.DeserializeObject<Comment>(cmntDto.CommentString);
            return comment;
        }
        public async Task<int> SaveCommentAsync(Comment comment)
        {
            CommentDto cmntDto = await AttemptAndRetry(() => _database.Table<CommentDto>().FirstOrDefaultAsync(c => c.CommentId == comment.CommentId)).ConfigureAwait(false);
            if (cmntDto == null)
            {
                cmntDto = new CommentDto();
                cmntDto.CommentId = comment.CommentId;
                cmntDto.CommentString = JsonConvert.SerializeObject(comment);
                return await AttemptAndRetry(() => _database.InsertAsync(cmntDto)).ConfigureAwait(false);
            }

            cmntDto.CommentString = JsonConvert.SerializeObject(comment);
            return await AttemptAndRetry(() => _database.UpdateAsync(cmntDto)).ConfigureAwait(false);
        }

        public async Task<int> DeleteCommentAsync(Comment comment)
        {
            CommentDto cmntDto = await AttemptAndRetry(() => _database.Table<CommentDto>().FirstOrDefaultAsync(c => c.CommentId == comment.CommentId)).ConfigureAwait(false);
            if (cmntDto == null)
            {
                return 1;
            }
            cmntDto.CommentString = JsonConvert.SerializeObject(comment);
            return await AttemptAndRetry(() => _database.DeleteAsync<CommentDto>(cmntDto)).ConfigureAwait(false);
        }

        public async Task<string> GetCommentThreadAsync(int commentThread)
        {
            CommentsList comList = await AttemptAndRetry(() => _database.Table<CommentsList>().FirstOrDefaultAsync(c => c.CommentThread == commentThread)).ConfigureAwait(false);
            return comList?.CommentsListString ?? "";
        }

        public async Task<int> SaveCommentThreadAsync(int commentThread, List<Comment> commentsList)
        {
            CommentsList comList = await AttemptAndRetry(() => _database.Table<CommentsList>().FirstOrDefaultAsync(c => c.CommentThread == commentThread)).ConfigureAwait(false);
            if (comList == null)
            {
                comList = new CommentsList();
                comList.CommentThread = commentThread;
                comList.CommentsListString = JsonConvert.SerializeObject(commentsList);
                return await AttemptAndRetry(() => _database.InsertAsync(comList)).ConfigureAwait(false);
            }
            comList.CommentsListString = JsonConvert.SerializeObject(commentsList);
            return await AttemptAndRetry(() => _database.UpdateAsync(comList)).ConfigureAwait(false);
        }

        public async Task<string> GetProgenyListAsync(string userEmail)
        {
            ProgenyList progList = await AttemptAndRetry(() => _database.Table<ProgenyList>().FirstOrDefaultAsync(p => p.UserEmail.ToUpper() == userEmail.ToUpper())).ConfigureAwait(false);
            return progList?.ProgenyListString ?? "";
        }

        public async Task<int> SaveProgenyListAsync(string userEmail, List<Progeny> progenyList)
        {
            ProgenyList progList = await AttemptAndRetry(() => _database.Table<ProgenyList>().FirstOrDefaultAsync(p => p.UserEmail.ToUpper() == userEmail.ToUpper())).ConfigureAwait(false);
            if (progList == null)
            {
                progList = new ProgenyList();
                progList.UserEmail = userEmail;
                progList.ProgenyListString = JsonConvert.SerializeObject(progenyList);
                return await AttemptAndRetry(() => _database.InsertAsync(progList)).ConfigureAwait(false);
            }
            progList.ProgenyListString = JsonConvert.SerializeObject(progenyList);
            return await AttemptAndRetry(() => _database.UpdateAsync(progList)).ConfigureAwait(false);
        }

        public async Task<UserAccess> GetUserAccessAsync(string email, int progenyId)
        {
            UserAccessDto uaDto = await AttemptAndRetry(() => _database.Table<UserAccessDto>().FirstOrDefaultAsync(u => u.Email.ToUpper() == email.ToUpper() && u.ProgenyId == progenyId)).ConfigureAwait(false);
            UserAccess ua = new UserAccess();
            if (uaDto != null)
            {
                ua = JsonConvert.DeserializeObject<UserAccess>(uaDto.UserAccessString);
            }

            return ua;
        }
        public async Task<int> SaveUserAccessAsync(UserAccess userAccess)
        {
            UserAccessDto uaDto = await AttemptAndRetry(() => _database.Table<UserAccessDto>().FirstOrDefaultAsync(u => u.Email.ToUpper() == userAccess.UserId.ToUpper() && u.ProgenyId == userAccess.ProgenyId)).ConfigureAwait(false);
            if (uaDto == null)
            {
                uaDto = new UserAccessDto();
                uaDto.Email = userAccess.UserId;
                uaDto.ProgenyId = userAccess.ProgenyId;
                uaDto.UserAccessString = JsonConvert.SerializeObject(userAccess);
                return await AttemptAndRetry(() => _database.InsertAsync(uaDto)).ConfigureAwait(false);
            }
            uaDto.UserAccessString = JsonConvert.SerializeObject(userAccess);
            return await AttemptAndRetry(() => _database.UpdateAsync(uaDto)).ConfigureAwait(false);
        }

        public async Task<string> GetUpcomingEventsAsync(int progenyId, int accessLevel)
        {
            UpcomingEvents eventsList = await AttemptAndRetry(() => _database.Table<UpcomingEvents>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.AccessLevel == accessLevel)).ConfigureAwait(false);
            return eventsList?.EventsListString ?? "";
        }

        public async Task<int> SaveUpcomingEventsAsync(int progenyId, int accessLevel, List<CalendarItem> eventsList)
        {
            UpcomingEvents upcomingList = await AttemptAndRetry(() => _database.Table<UpcomingEvents>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.AccessLevel == accessLevel)).ConfigureAwait(false);
            if (upcomingList == null)
            {
                upcomingList = new UpcomingEvents();
                upcomingList.ProgenyId = progenyId;
                upcomingList.AccessLevel = accessLevel;
                upcomingList.EventsListString = JsonConvert.SerializeObject(eventsList);
                return await AttemptAndRetry(() => _database.InsertAsync(upcomingList)).ConfigureAwait(false);
            }
            upcomingList.EventsListString = JsonConvert.SerializeObject(eventsList);
            return await AttemptAndRetry(() => _database.UpdateAsync(upcomingList)).ConfigureAwait(false);
        }

        public async Task<string> GetTimeLineListAsync(int progenyId, int accessLevel, int count, int start)
        {
            TimeLineList timeLineItems = await AttemptAndRetry(() => _database.Table<TimeLineList>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.AccessLevel == accessLevel && e.Count == count && e.Start == start)).ConfigureAwait(false);
            return timeLineItems?.TimeLineItemsString ?? "";
        }

        public async Task<int> SaveTimeLineListAsync(int progenyId, int accessLevel, int count, int start, List<TimeLineItem> timeLineItemsList)
        {
            TimeLineList timeLineList = await AttemptAndRetry(() => _database.Table<TimeLineList>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.AccessLevel == accessLevel && e.Count == count && e.Start == start)).ConfigureAwait(false);
            if (timeLineList == null)
            {
                timeLineList = new TimeLineList();
                timeLineList.ProgenyId = progenyId;
                timeLineList.AccessLevel = accessLevel;
                timeLineList.Count = count;
                timeLineList.Start = start;
                timeLineList.TimeLineItemsString = JsonConvert.SerializeObject(timeLineItemsList);
                return await AttemptAndRetry(() => _database.InsertAsync(timeLineList)).ConfigureAwait(false);
            }
            timeLineList.TimeLineItemsString = JsonConvert.SerializeObject(timeLineItemsList);
            return await AttemptAndRetry(() => _database.UpdateAsync(timeLineList)).ConfigureAwait(false);
        }

        public async Task<string> GetTimeLineLatestAsync(int progenyId, int accessLevel)
        {
            TimeLineLatest timeLineLatestItems = await AttemptAndRetry(() => _database.Table<TimeLineLatest>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.AccessLevel == accessLevel)).ConfigureAwait(false);
            return timeLineLatestItems?.TimeLineItemsString ?? "";
        }

        public async Task<int> SaveTimeLineLatestAsync(int progenyId, int accessLevel, List<TimeLineItem> timeLineItemsList)
        {
            TimeLineLatest timeLineList = await AttemptAndRetry(() => _database.Table<TimeLineLatest>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.AccessLevel == accessLevel)).ConfigureAwait(false);
            if (timeLineList == null)
            {
                timeLineList = new TimeLineLatest();
                timeLineList.ProgenyId = progenyId;
                timeLineList.AccessLevel = accessLevel;
                timeLineList.TimeLineItemsString = JsonConvert.SerializeObject(timeLineItemsList);
                return await AttemptAndRetry(() => _database.InsertAsync(timeLineList)).ConfigureAwait(false);
            }
            timeLineList.TimeLineItemsString = JsonConvert.SerializeObject(timeLineItemsList);
            return await AttemptAndRetry(() => _database.UpdateAsync(timeLineList)).ConfigureAwait(false);
        }

        public async Task<Picture> GetPictureAsync(int pictureId)
        {
            PictureDto pictureDto = await AttemptAndRetry(() => _database.Table<PictureDto>().FirstOrDefaultAsync(p => p.PictureId == pictureId)).ConfigureAwait(false);
            Picture picture = JsonConvert.DeserializeObject<Picture>(pictureDto.PictureString);
            return picture;
        }
        public async Task<int> SavePictureAsync(Picture picture)
        {
            PictureDto pictureDto = await AttemptAndRetry(() => _database.Table<PictureDto>().FirstOrDefaultAsync(p => p.PictureId == picture.PictureId)).ConfigureAwait(false);
            if (pictureDto == null)
            {
                pictureDto = new PictureDto();
                pictureDto.PictureId = picture.PictureId;
                pictureDto.PictureString = JsonConvert.SerializeObject(picture);
                return await AttemptAndRetry(() => _database.InsertAsync(pictureDto)).ConfigureAwait(false);
            }
            pictureDto.PictureString = JsonConvert.SerializeObject(picture);
            return await AttemptAndRetry(() => _database.UpdateAsync(pictureDto)).ConfigureAwait(false);
        }

        public async Task<string> GetPicturePageListAsync(int progenyId, int pageNumber, int pageSize, int sortBy, string tagFilter)
        {
            PicturePageList picturePageList = await AttemptAndRetry(() => _database.Table<PicturePageList>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.PageNumber == pageNumber && e.PageSize == pageSize && e.SortBy == sortBy && e.TagFilter.ToUpper() == tagFilter.ToUpper())).ConfigureAwait(false);
            return picturePageList?.PictureItemsString ?? "";
        }

        public async Task<int> SavePicturePageListAsync(int progenyId, int pageNumber, int pageSize, int sortBy, string tagFilter, PicturePage picturePage)
        {
            PicturePageList picturePageList = await AttemptAndRetry(() => _database.Table<PicturePageList>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.PageNumber == pageNumber && e.PageSize == pageSize && e.SortBy == sortBy && e.TagFilter.ToUpper() == tagFilter.ToUpper())).ConfigureAwait(false);
            if (picturePageList == null)
            {
                picturePageList = new PicturePageList();
                picturePageList.ProgenyId = progenyId;
                picturePageList.PageNumber = pageNumber;
                picturePageList.PageSize = pageSize;
                picturePageList.SortBy = sortBy;
                picturePageList.TagFilter = tagFilter;
                picturePageList.PictureItemsString = JsonConvert.SerializeObject(picturePage);
                return await AttemptAndRetry(() => _database.InsertAsync(picturePageList)).ConfigureAwait(false);
            }
            picturePageList.PictureItemsString = JsonConvert.SerializeObject(picturePage);
            return await AttemptAndRetry(() => _database.UpdateAsync(picturePageList)).ConfigureAwait(false);
        }

        public async Task<PictureViewModel> GetPictureViewModelAsync(int pictureId)
        {
            PictureViewModelDto pictureViewModelDto = await AttemptAndRetry(() => _database.Table<PictureViewModelDto>().FirstOrDefaultAsync(p => p.PictureId == pictureId)).ConfigureAwait(false);
            PictureViewModel pictureViewModel =
                JsonConvert.DeserializeObject<PictureViewModel>(pictureViewModelDto.PictureViewModelString);
            return pictureViewModel;
        }
        public async Task<int> SavePictureViewModelAsync(PictureViewModel pictureViewModel)
        {
            PictureViewModelDto pictureViewModelDto = await AttemptAndRetry(() => _database.Table<PictureViewModelDto>().FirstOrDefaultAsync(p => p.PictureId == pictureViewModel.PictureId)).ConfigureAwait(false);
            if (pictureViewModelDto == null)
            {
                pictureViewModelDto = new PictureViewModelDto();
                pictureViewModelDto.PictureId = pictureViewModel.PictureId;
                pictureViewModelDto.PictureViewModelString = JsonConvert.SerializeObject(pictureViewModel);
                return await AttemptAndRetry(() => _database.InsertAsync(pictureViewModelDto)).ConfigureAwait(false);
            }
            pictureViewModelDto.PictureViewModelString = JsonConvert.SerializeObject(pictureViewModel);
            return await AttemptAndRetry(() => _database.UpdateAsync(pictureViewModelDto)).ConfigureAwait(false);
        }

        public async Task<Video> GetVideoAsync(int videoId)
        {
            VideoDto videoDto = await AttemptAndRetry(() => _database.Table<VideoDto>().FirstOrDefaultAsync(v => v.VideoId == videoId)).ConfigureAwait(false);
            Video video = JsonConvert.DeserializeObject<Video>(videoDto.VideoString);
            return video;
        }
        public async Task<int> SaveVideoAsync(Video video)
        {
            VideoDto videoDto = await AttemptAndRetry(() => _database.Table<VideoDto>().FirstOrDefaultAsync(v => v.VideoId == video.VideoId)).ConfigureAwait(false);
            if (videoDto == null)
            {
                videoDto = new VideoDto();
                videoDto.VideoId = video.VideoId;
                videoDto.VideoString = JsonConvert.SerializeObject(video);
                return await AttemptAndRetry(() => _database.InsertAsync(videoDto)).ConfigureAwait(false);
            }
            videoDto.VideoString = JsonConvert.SerializeObject(video);
            return await AttemptAndRetry(() => _database.UpdateAsync(videoDto)).ConfigureAwait(false);
        }

        public async Task<string> GetVideoPageListAsync(int progenyId, int pageNumber, int pageSize, int sortBy, string tagFilter)
        {
            VideoPageList videoPageList = await AttemptAndRetry(() => _database.Table<VideoPageList>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.PageNumber == pageNumber && e.PageSize == pageSize && e.SortBy == sortBy && e.TagFilter.ToUpper() == tagFilter.ToUpper())).ConfigureAwait(false);
            return videoPageList?.VideoItemsString ?? "";
        }

        public async Task<int> SaveVideoPageListAsync(int progenyId, int pageNumber, int pageSize, int sortBy, string tagFilter, VideoPage videoPage)
        {
            VideoPageList videoPageList = await AttemptAndRetry(() => _database.Table<VideoPageList>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.PageNumber == pageNumber && e.PageSize == pageSize && e.SortBy == sortBy && e.TagFilter.ToUpper() == tagFilter.ToUpper())).ConfigureAwait(false);
            if (videoPageList == null)
            {
                videoPageList = new VideoPageList();
                videoPageList.ProgenyId = progenyId;
                videoPageList.PageNumber = pageNumber;
                videoPageList.PageSize = pageSize;
                videoPageList.SortBy = sortBy;
                videoPageList.TagFilter = tagFilter;
                videoPageList.VideoItemsString = JsonConvert.SerializeObject(videoPage);
                return await AttemptAndRetry(() => _database.InsertAsync(videoPageList)).ConfigureAwait(false);
            }
            videoPageList.VideoItemsString = JsonConvert.SerializeObject(videoPage);
            return await AttemptAndRetry(() => _database.UpdateAsync(videoPageList)).ConfigureAwait(false);
        }

        public async Task<VideoViewModel> GetVideoViewModelAsync(int videoId)
        {
            VideoViewModelDto videoViewModelDto = await AttemptAndRetry(() => _database.Table<VideoViewModelDto>().FirstOrDefaultAsync(v => v.VideoId == videoId)).ConfigureAwait(false);
            VideoViewModel videoViewModel =
                JsonConvert.DeserializeObject<VideoViewModel>(videoViewModelDto.VideoViewModelString);
            return videoViewModel;
        }
        public async Task<int> SaveVideoViewModelAsync(VideoViewModel videoViewModel)
        {
            VideoViewModelDto videoViewModelDto = await AttemptAndRetry(() => _database.Table<VideoViewModelDto>().FirstOrDefaultAsync(v => v.VideoId == videoViewModel.VideoId)).ConfigureAwait(false);
            if (videoViewModelDto == null)
            {
                videoViewModelDto = new VideoViewModelDto();
                videoViewModelDto.VideoId = videoViewModelDto.VideoId;
                videoViewModelDto.VideoViewModelString = JsonConvert.SerializeObject(videoViewModel);
                return await AttemptAndRetry(() => _database.InsertAsync(videoViewModelDto)).ConfigureAwait(false);
            }
            videoViewModelDto.VideoViewModelString = JsonConvert.SerializeObject(videoViewModel);
            return await AttemptAndRetry(() => _database.UpdateAsync(videoViewModelDto)).ConfigureAwait(false);
        }

        public async Task<CalendarItem> GetCalendarItemAsync(int eventId)
        {
            EventDto evtDto = await AttemptAndRetry(() => _database.Table<EventDto>().FirstOrDefaultAsync(c => c.EventId == eventId)).ConfigureAwait(false);
            CalendarItem calendarItem = JsonConvert.DeserializeObject<CalendarItem>(evtDto.EventString);
            return calendarItem;
        }
        public async Task<int> SaveCalendarItemAsync(CalendarItem calendarItem)
        {
            EventDto eventDto = await AttemptAndRetry(() => _database.Table<EventDto>().FirstOrDefaultAsync(c => c.EventId == calendarItem.EventId)).ConfigureAwait(false);
            if (eventDto == null)
            {
                eventDto = new EventDto();
                eventDto.EventId = calendarItem.EventId;
                eventDto.EventString = JsonConvert.SerializeObject(calendarItem);
                return await AttemptAndRetry(() => _database.InsertAsync(eventDto)).ConfigureAwait(false);
            }
            eventDto.EventString = JsonConvert.SerializeObject(calendarItem);
            return await AttemptAndRetry(() => _database.UpdateAsync(eventDto)).ConfigureAwait(false);
        }

        public async Task<List<CalendarItem>> GetCalendarListAsync(int progenyId, int accessLevel)
        {
            CalendarList calList = await AttemptAndRetry(() => _database.Table<CalendarList>().FirstOrDefaultAsync(c => c.ProgenyId == progenyId && c.AccessLevel == accessLevel)).ConfigureAwait(false);
            List<CalendarItem> calendarItems =
                JsonConvert.DeserializeObject<List<CalendarItem>>(calList.CalendarListString);
            return calendarItems;
        }

        public async Task<int> SaveCalendarListAsync(int progenyId, int accessLevel, List<CalendarItem> calendarItems)
        {
            CalendarList calList = await AttemptAndRetry(() => _database.Table<CalendarList>().FirstOrDefaultAsync(c => c.ProgenyId == progenyId && c.AccessLevel == accessLevel)).ConfigureAwait(false);
            if (calList == null)
            {
                calList = new CalendarList();
                calList.ProgenyId = progenyId;
                calList.AccessLevel = accessLevel;
                calList.CalendarListString = JsonConvert.SerializeObject(calendarItems);
                return await AttemptAndRetry(() => _database.InsertAsync(calList)).ConfigureAwait(false);
            }
            calList.CalendarListString = JsonConvert.SerializeObject(calendarItems);
            return await AttemptAndRetry(() => _database.UpdateAsync(calList)).ConfigureAwait(false);
        }

        public async Task<Location> GetLocationAsync(int locationId)
        {
            LocationDto locDto = await AttemptAndRetry(() => _database.Table<LocationDto>().FirstOrDefaultAsync(c => c.LocationId == locationId)).ConfigureAwait(false);
            Location location = JsonConvert.DeserializeObject<Location>(locDto.LocationString);
            return location;
        }
        public async Task<int> SaveLocationAsync(Location location)
        {
            LocationDto locDto = await AttemptAndRetry(() => _database.Table<LocationDto>().FirstOrDefaultAsync(c => c.LocationId == location.LocationId)).ConfigureAwait(false);
            if (locDto == null)
            {
                locDto = new LocationDto();
                locDto.LocationId = location.LocationId;
                locDto.LocationString = JsonConvert.SerializeObject(location);
                return await AttemptAndRetry(() => _database.InsertAsync(locDto)).ConfigureAwait(false);
            }
            locDto.LocationString = JsonConvert.SerializeObject(location);
            return await AttemptAndRetry(() => _database.UpdateAsync(locDto)).ConfigureAwait(false);
        }

        public async Task<VocabularyItem> GetVocabularyItemAsync(int wordId)
        {
            WordDto wordDto = await AttemptAndRetry(() => _database.Table<WordDto>().FirstOrDefaultAsync(w => w.WordId == wordId)).ConfigureAwait(false);
            VocabularyItem vocabularyItem = JsonConvert.DeserializeObject<VocabularyItem>(wordDto.WordString);
            return vocabularyItem;
        }
        public async Task<int> SaveVocabularyItemAsync(VocabularyItem vocabularyItem)
        {
            WordDto wordDto = await AttemptAndRetry(() => _database.Table<WordDto>().FirstOrDefaultAsync(w => w.WordId == vocabularyItem.WordId)).ConfigureAwait(false);
            if (wordDto == null)
            {
                wordDto = new WordDto();
                wordDto.WordId = vocabularyItem.WordId;
                wordDto.WordString = JsonConvert.SerializeObject(vocabularyItem);
                return await AttemptAndRetry(() => _database.InsertAsync(wordDto)).ConfigureAwait(false);
            }
            wordDto.WordString = JsonConvert.SerializeObject(vocabularyItem);
            return await AttemptAndRetry(() => _database.UpdateAsync(wordDto)).ConfigureAwait(false);
        }

        public async Task<Skill> GetSkillAsync(int skillId)
        {
            SkillDto skillDto = await AttemptAndRetry(() => _database.Table<SkillDto>().FirstOrDefaultAsync(s => s.SkillId == skillId)).ConfigureAwait(false);
            Skill skillItem = JsonConvert.DeserializeObject<Skill>(skillDto.SkillString);
            return skillItem;
        }
        public async Task<int> SaveSkillAsync(Skill skill)
        {
            SkillDto skillDto = await AttemptAndRetry(() => _database.Table<SkillDto>().FirstOrDefaultAsync(s => s.SkillId == skill.SkillId)).ConfigureAwait(false);
            if (skillDto == null)
            {
                skillDto = new SkillDto();
                skillDto.SkillId = skill.SkillId;
                skillDto.SkillString = JsonConvert.SerializeObject(skill);
                return await AttemptAndRetry(() => _database.InsertAsync(skillDto)).ConfigureAwait(false);
            }
            skillDto.SkillString = JsonConvert.SerializeObject(skill);
            return await AttemptAndRetry(() => _database.UpdateAsync(skillDto)).ConfigureAwait(false);
        }

        public async Task<Friend> GetFriendAsync(int friendId)
        {
            FriendDto friendDto = await AttemptAndRetry(() => _database.Table<FriendDto>().FirstOrDefaultAsync(f => f.FriendId == friendId)).ConfigureAwait(false);
            Friend friendItem = JsonConvert.DeserializeObject<Friend>(friendDto.FriendString);
            return friendItem;
        }
        public async Task<int> SaveFriendAsync(Friend friend)
        {
            FriendDto friendDto = await AttemptAndRetry(() => _database.Table<FriendDto>().FirstOrDefaultAsync(f => f.FriendId == friend.FriendId)).ConfigureAwait(false);
            if (friendDto == null)
            {
                friendDto = new FriendDto();
                friendDto.FriendId = friend.FriendId;
                friendDto.FriendString = JsonConvert.SerializeObject(friend);
                return await AttemptAndRetry(() => _database.InsertAsync(friendDto)).ConfigureAwait(false);
            }
            friendDto.FriendString = JsonConvert.SerializeObject(friend);
            return await AttemptAndRetry(() => _database.UpdateAsync(friendDto)).ConfigureAwait(false);
        }

        public async Task<Measurement> GetMeasurementAsync(int measurementId)
        {
            MeasurementDto measurementDto = await AttemptAndRetry(() => _database.Table<MeasurementDto>().FirstOrDefaultAsync(m => m.MeasurementId == measurementId)).ConfigureAwait(false);
            Measurement measurementItem = JsonConvert.DeserializeObject<Measurement>(measurementDto.MeasurementString);
            return measurementItem;
        }
        public async Task<int> SaveMeasurementAsync(Measurement measurement)
        {
            MeasurementDto measurementDto = await AttemptAndRetry(() => _database.Table<MeasurementDto>().FirstOrDefaultAsync(m => m.MeasurementId == measurement.MeasurementId)).ConfigureAwait(false);
            if (measurementDto == null)
            {
                measurementDto = new MeasurementDto();
                measurementDto.MeasurementId = measurement.MeasurementId;
                measurementDto.MeasurementString = JsonConvert.SerializeObject(measurement);
                return await AttemptAndRetry(() => _database.InsertAsync(measurementDto)).ConfigureAwait(false);
            }
            measurementDto.MeasurementString = JsonConvert.SerializeObject(measurement);
            return await AttemptAndRetry(() => _database.UpdateAsync(measurementDto)).ConfigureAwait(false);
        }

        public async Task<Sleep> GetSleepAsync(int sleepId)
        {
            SleepDto sleepDto = await AttemptAndRetry(() => _database.Table<SleepDto>().FirstOrDefaultAsync(s => s.SleepId == sleepId)).ConfigureAwait(false);
            Sleep sleepItem = JsonConvert.DeserializeObject<Sleep>(sleepDto.SleepString);
            return sleepItem;
        }

        public async Task<int> SaveSleepAsync(Sleep sleep)
        {
            SleepDto sleepDto = await AttemptAndRetry(() => _database.Table<SleepDto>().FirstOrDefaultAsync(s => s.SleepId == sleep.SleepId)).ConfigureAwait(false);
            if (sleepDto == null)
            {
                sleepDto = new SleepDto();
                sleepDto.SleepId = sleep.SleepId;
                sleepDto.SleepString = JsonConvert.SerializeObject(sleep);
                return await AttemptAndRetry(() => _database.InsertAsync(sleepDto)).ConfigureAwait(false);
            }
            sleepDto.SleepString = JsonConvert.SerializeObject(sleep);
            return await AttemptAndRetry(() => _database.UpdateAsync(sleepDto)).ConfigureAwait(false);
        }

        public async Task<List<Sleep>> GetSleepListAsync(int progenyId, int accessLevel)
        {
            SleepList sleepList = await AttemptAndRetry(() => _database.Table<SleepList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel)).ConfigureAwait(false);
            List<Sleep> sleepItems = JsonConvert.DeserializeObject<List<Sleep>>(sleepList.SleepItemsString);
            return sleepItems;
        }
        public async Task<int> SaveSleepListAsync(int progenyId, int accessLevel, List<Sleep> sleepItemsList)
        {
            SleepList sleepList = await AttemptAndRetry(() => _database.Table<SleepList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel)).ConfigureAwait(false);
            if (sleepList == null)
            {
                sleepList = new SleepList();
                sleepList.ProgenyId = progenyId;
                sleepList.AccessLevel = accessLevel;
                sleepList.SleepItemsString = JsonConvert.SerializeObject(sleepItemsList);
                return await AttemptAndRetry(() => _database.InsertAsync(sleepList)).ConfigureAwait(false);
            }
            sleepList.SleepItemsString = JsonConvert.SerializeObject(sleepItemsList);
            return await AttemptAndRetry(() => _database.UpdateAsync(sleepList)).ConfigureAwait(false);
        }

        public async Task<SleepStatsModel> GetSleepStatsModelAsync(int progenyId, int accessLevel)
        {
            SleepStatsModelDto sleepStatsModelDto = await AttemptAndRetry(() => _database.Table<SleepStatsModelDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel)).ConfigureAwait(false);
            SleepStatsModel sleepStats = JsonConvert.DeserializeObject<SleepStatsModel>(sleepStatsModelDto.SleepStatsString);
            return sleepStats;
        }
        public async Task<int> SaveSleepStatsModelAsync(int progenyId, int accessLevel, SleepStatsModel sleepStats)
        {
            SleepStatsModelDto sleepStatsModelDto = await AttemptAndRetry(() => _database.Table<SleepStatsModelDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel)).ConfigureAwait(false);
            if (sleepStatsModelDto == null)
            {
                sleepStatsModelDto = new SleepStatsModelDto();
                sleepStatsModelDto.ProgenyId = progenyId;
                sleepStatsModelDto.AccessLevel = accessLevel;
                sleepStatsModelDto.SleepStatsString = JsonConvert.SerializeObject(sleepStats);
                return await AttemptAndRetry(() => _database.InsertAsync(sleepStatsModelDto)).ConfigureAwait(false);
            }
            sleepStatsModelDto.SleepStatsString = JsonConvert.SerializeObject(sleepStats);
            return await AttemptAndRetry(() => _database.UpdateAsync(sleepStatsModelDto)).ConfigureAwait(false);
        }

        public async Task<List<Sleep>> GetSleepChartAsync(int progenyId, int accessLevel)
        {
            SleepChartDto sleepList = await AttemptAndRetry(() => _database.Table<SleepChartDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel)).ConfigureAwait(false);
            List<Sleep> sleepItems = JsonConvert.DeserializeObject<List<Sleep>>(sleepList.SleepChartString);
            return sleepItems;
        }
        public async Task<int> SaveSleepChartAsync(int progenyId, int accessLevel, List<Sleep> sleepItemsList)
        {
            SleepChartDto sleepChart = await AttemptAndRetry(() => _database.Table<SleepChartDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel)).ConfigureAwait(false);
            if (sleepChart == null)
            {
                sleepChart = new SleepChartDto();
                sleepChart.ProgenyId = progenyId;
                sleepChart.AccessLevel = accessLevel;
                sleepChart.SleepChartString = JsonConvert.SerializeObject(sleepItemsList);
                return await AttemptAndRetry(() => _database.InsertAsync(sleepChart)).ConfigureAwait(false);
            }
            sleepChart.SleepChartString = JsonConvert.SerializeObject(sleepItemsList);
            return await AttemptAndRetry(() => _database.UpdateAsync(sleepChart)).ConfigureAwait(false);
        }

        public async Task<Note> GetNoteAsync(int noteId)
        {
            NoteDto noteDto = await AttemptAndRetry(() => _database.Table<NoteDto>().FirstOrDefaultAsync(n => n.NoteId == noteId)).ConfigureAwait(false);
            Note noteItem = JsonConvert.DeserializeObject<Note>(noteDto.NoteString);
            return noteItem;
        }
        public async Task<int> SaveNoteAsync(Note note)
        {
            NoteDto noteDto = await AttemptAndRetry(() => _database.Table<NoteDto>().FirstOrDefaultAsync(n => n.NoteId == note.NoteId)).ConfigureAwait(false);
            if (noteDto == null)
            {
                noteDto = new NoteDto();
                noteDto.NoteId = note.NoteId;
                noteDto.NoteString = JsonConvert.SerializeObject(note);
                return await AttemptAndRetry(() => _database.InsertAsync(noteDto)).ConfigureAwait(false);
            }
            noteDto.NoteString = JsonConvert.SerializeObject(note);
            return await AttemptAndRetry(() => _database.UpdateAsync(noteDto)).ConfigureAwait(false);
        }

        public async Task<Contact> GetContactAsync(int contactId)
        {
            ContactDto contactDto = await AttemptAndRetry(() => _database.Table<ContactDto>().FirstOrDefaultAsync(c => c.ContactId == contactId)).ConfigureAwait(false);
            Contact contactItem = JsonConvert.DeserializeObject<Contact>(contactDto.ContactString);
            return contactItem;
        }
        public async Task<int> SaveContactAsync(Contact contact)
        {
            ContactDto contactDto = await AttemptAndRetry(() => _database.Table<ContactDto>().FirstOrDefaultAsync(c => c.ContactId == contact.ContactId)).ConfigureAwait(false);
            if (contactDto == null)
            {
                contactDto = new ContactDto();
                contactDto.ContactId = contact.ContactId;
                contactDto.ContactString = JsonConvert.SerializeObject(contact);
                return await AttemptAndRetry(() => _database.InsertAsync(contactDto)).ConfigureAwait(false);
            }
            contactDto.ContactString = JsonConvert.SerializeObject(contact);
            return await AttemptAndRetry(() => _database.UpdateAsync(contactDto)).ConfigureAwait(false);
        }

        public async Task<List<Contact>> GetContactListAsync(int progenyId, int accessLevel)
        {
            ContactList contactList = await AttemptAndRetry(() => _database.Table<ContactList>().FirstOrDefaultAsync(c => c.ProgenyId == progenyId && c.AccessLevel == accessLevel)).ConfigureAwait(false);
            List<Contact> contactItems = JsonConvert.DeserializeObject<List<Contact>>(contactList.ContactItemsString);
            return contactItems;
        }
        public async Task<int> SaveContactListAsync(int progenyId, int accessLevel, List<Contact> contactItemsList)
        {
            ContactList contactList = await AttemptAndRetry(() => _database.Table<ContactList>().FirstOrDefaultAsync(c => c.ProgenyId == progenyId && c.AccessLevel == accessLevel)).ConfigureAwait(false);
            if (contactList == null)
            {
                contactList = new ContactList();
                contactList.ProgenyId = progenyId;
                contactList.AccessLevel = accessLevel;
                contactList.ContactItemsString = JsonConvert.SerializeObject(contactItemsList);
                return await AttemptAndRetry(() => _database.InsertAsync(contactList)).ConfigureAwait(false);
            }
            contactList.ContactItemsString = JsonConvert.SerializeObject(contactItemsList);
            return await AttemptAndRetry(() => _database.UpdateAsync(contactList)).ConfigureAwait(false);
        }

        public async Task<Vaccination> GetVaccinationAsync(int vaccinationId)
        {
            VaccinationDto vacDto = await AttemptAndRetry(() => _database.Table<VaccinationDto>().FirstOrDefaultAsync(s => s.VaccinationId == vaccinationId)).ConfigureAwait(false);
            Vaccination vacItem = JsonConvert.DeserializeObject<Vaccination>(vacDto.VaccinationString);
            return vacItem;
        }
        public async Task<int> SaveVaccinationAsync(Vaccination vaccination)
        {
            VaccinationDto vaccinationDto = await AttemptAndRetry(() => _database.Table<VaccinationDto>().FirstOrDefaultAsync(s => s.VaccinationId == vaccination.VaccinationId)).ConfigureAwait(false);
            if (vaccinationDto == null)
            {
                vaccinationDto = new VaccinationDto();
                vaccinationDto.VaccinationId = vaccination.VaccinationId;
                vaccinationDto.VaccinationString = JsonConvert.SerializeObject(vaccination);
                return await AttemptAndRetry(() => _database.InsertAsync(vaccinationDto)).ConfigureAwait(false);
            }
            vaccinationDto.VaccinationString = JsonConvert.SerializeObject(vaccination);
            return await AttemptAndRetry(() => _database.UpdateAsync(vaccinationDto)).ConfigureAwait(false);
        }

        public async Task<List<UserAccess>> GetProgenyAccessListAsync(int progenyId)
        {
            ProgenyAccessList progenyAccessList = await AttemptAndRetry(() => _database.Table<ProgenyAccessList>().FirstOrDefaultAsync(p => p.ProgenyId == progenyId)).ConfigureAwait(false);
            List<UserAccess> userAccessList =
                JsonConvert.DeserializeObject<List<UserAccess>>(progenyAccessList.AccessListString);
            return userAccessList;
        }

        public async Task<int> SaveProgenyAccessListAsync(int progenyId, List<UserAccess> userAccessList)
        {
            ProgenyAccessList progenyAccessList = await AttemptAndRetry(() => _database.Table<ProgenyAccessList>().FirstOrDefaultAsync(p => p.ProgenyId == progenyId)).ConfigureAwait(false);
            if (progenyAccessList == null)
            {
                progenyAccessList = new ProgenyAccessList();
                progenyAccessList.ProgenyId = progenyId;
                progenyAccessList.AccessListString = JsonConvert.SerializeObject(userAccessList);
                return await AttemptAndRetry(() => _database.InsertAsync(progenyAccessList)).ConfigureAwait(false);
            }
            progenyAccessList.AccessListString = JsonConvert.SerializeObject(userAccessList);
            return await AttemptAndRetry(() => _database.UpdateAsync(progenyAccessList)).ConfigureAwait(false);
        }

        public async Task<SleepListPage> GetSleepListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder)
        {
            SleepListPageDto sleepListPageDto = await AttemptAndRetry(() => _database.Table<SleepListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder)).ConfigureAwait(false);
            SleepListPage sleepListPage = JsonConvert.DeserializeObject<SleepListPage>(sleepListPageDto.SleepListPageString);
            return sleepListPage;
        }
        public async Task<int> SaveSleepListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder, SleepListPage sleepListPage)
        {
            SleepListPageDto sleepListPageDto = await AttemptAndRetry(() => _database.Table<SleepListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder)).ConfigureAwait(false);
            if (sleepListPageDto == null)
            {
                sleepListPageDto = new SleepListPageDto();
                sleepListPageDto.ProgenyId = progenyId;
                sleepListPageDto.AccessLevel = accessLevel;
                sleepListPageDto.PageNumber = pageNumber;
                sleepListPageDto.PageSize = pageSize;
                sleepListPageDto.SortOrder = sortOrder;
                sleepListPageDto.SleepListPageString = JsonConvert.SerializeObject(sleepListPage);
                return await AttemptAndRetry(() => _database.InsertAsync(sleepListPageDto)).ConfigureAwait(false);
            }
            sleepListPageDto.SleepListPageString = JsonConvert.SerializeObject(sleepListPage);
            return await AttemptAndRetry(() => _database.UpdateAsync(sleepListPageDto)).ConfigureAwait(false);
        }

        public async Task<List<Friend>> GetFriendsListAsync(int progenyId, int accessLevel)
        {
            FriendsList friendsList = await AttemptAndRetry(() => _database.Table<FriendsList>().FirstOrDefaultAsync(f => f.ProgenyId == progenyId && f.AccessLevel == accessLevel)).ConfigureAwait(false);
            List<Friend> friendItems = JsonConvert.DeserializeObject<List<Friend>>(friendsList.FriendItemsString);
            return friendItems;
        }
        public async Task<int> SaveFriendsListAsync(int progenyId, int accessLevel, List<Friend> friendItemsList)
        {
            FriendsList friendsList = await AttemptAndRetry(() => _database.Table<FriendsList>().FirstOrDefaultAsync(f => f.ProgenyId == progenyId && f.AccessLevel == accessLevel)).ConfigureAwait(false);
            if (friendsList == null)
            {
                friendsList = new FriendsList();
                friendsList.ProgenyId = progenyId;
                friendsList.AccessLevel = accessLevel;
                friendsList.FriendItemsString = JsonConvert.SerializeObject(friendItemsList);
                return await AttemptAndRetry(() => _database.InsertAsync(friendsList)).ConfigureAwait(false);
            }
            friendsList.FriendItemsString = JsonConvert.SerializeObject(friendItemsList);
            return await AttemptAndRetry(() => _database.UpdateAsync(friendsList)).ConfigureAwait(false);
        }

        public async Task<MeasurementsListPage> GetMeasurementsListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder)
        {
            MeasurementsListPageDto measurementsListPageDto = await AttemptAndRetry(() => _database.Table<MeasurementsListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder)).ConfigureAwait(false);
            MeasurementsListPage sleepListPage = JsonConvert.DeserializeObject<MeasurementsListPage>(measurementsListPageDto.MeasurementsListPageString);
            return sleepListPage;
        }
        public async Task<int> SaveMeasurementsListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder, MeasurementsListPage measurementsListPage)
        {
            MeasurementsListPageDto measurementsListPageDto = await AttemptAndRetry(() => _database.Table<MeasurementsListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder)).ConfigureAwait(false);
            if (measurementsListPageDto == null)
            {
                measurementsListPageDto = new MeasurementsListPageDto();
                measurementsListPageDto.ProgenyId = progenyId;
                measurementsListPageDto.AccessLevel = accessLevel;
                measurementsListPageDto.PageNumber = pageNumber;
                measurementsListPageDto.PageSize = pageSize;
                measurementsListPageDto.SortOrder = sortOrder;
                measurementsListPageDto.MeasurementsListPageString = JsonConvert.SerializeObject(measurementsListPage);
                return await AttemptAndRetry(() => _database.InsertAsync(measurementsListPageDto)).ConfigureAwait(false);
            }
            measurementsListPageDto.MeasurementsListPageString = JsonConvert.SerializeObject(measurementsListPage);
            return await AttemptAndRetry(() => _database.UpdateAsync(measurementsListPageDto)).ConfigureAwait(false);
        }

        public async Task<List<Measurement>> GetMeasurementsListAsync(int progenyId, int accessLevel)
        {
            MeasurementsList measurementsList = await AttemptAndRetry(() => _database.Table<MeasurementsList>().FirstOrDefaultAsync(m => m.ProgenyId == progenyId && m.AccessLevel == accessLevel)).ConfigureAwait(false);
            List<Measurement> measurements = JsonConvert.DeserializeObject<List<Measurement>>(measurementsList.MeasurementsItemsString);
            return measurements;
        }
        public async Task<int> SaveMeasurementsListAsync(int progenyId, int accessLevel, List<Measurement> measurementItemsList)
        {
            MeasurementsList measurementsList = await AttemptAndRetry(() => _database.Table<MeasurementsList>().FirstOrDefaultAsync(m => m.ProgenyId == progenyId && m.AccessLevel == accessLevel)).ConfigureAwait(false);
            if (measurementsList == null)
            {
                measurementsList = new MeasurementsList();
                measurementsList.ProgenyId = progenyId;
                measurementsList.AccessLevel = accessLevel;
                measurementsList.MeasurementsItemsString = JsonConvert.SerializeObject(measurementItemsList);
                return await AttemptAndRetry(() => _database.InsertAsync(measurementsList)).ConfigureAwait(false);
            }
            measurementsList.MeasurementsItemsString = JsonConvert.SerializeObject(measurementItemsList);
            return await AttemptAndRetry(() => _database.UpdateAsync(measurementsList)).ConfigureAwait(false);
        }

        public async Task<SkillsListPage> GetSkillsListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder)
        {
            SkillsListPageDto skillsListPageDto = await AttemptAndRetry(() => _database.Table<SkillsListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder)).ConfigureAwait(false);
            SkillsListPage skillsListPage = JsonConvert.DeserializeObject<SkillsListPage>(skillsListPageDto.SkillsListPageString);
            return skillsListPage;
        }
        public async Task<int> SaveSkillsListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder, SkillsListPage skillsListPage)
        {
            SkillsListPageDto skillsListPageDto = await AttemptAndRetry(() => _database.Table<SkillsListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder)).ConfigureAwait(false);
            if (skillsListPageDto == null)
            {
                skillsListPageDto = new SkillsListPageDto();
                skillsListPageDto.ProgenyId = progenyId;
                skillsListPageDto.AccessLevel = accessLevel;
                skillsListPageDto.PageNumber = pageNumber;
                skillsListPageDto.PageSize = pageSize;
                skillsListPageDto.SortOrder = sortOrder;
                skillsListPageDto.SkillsListPageString = JsonConvert.SerializeObject(skillsListPage);
                return await AttemptAndRetry(() => _database.InsertAsync(skillsListPageDto)).ConfigureAwait(false);
            }
            skillsListPageDto.SkillsListPageString = JsonConvert.SerializeObject(skillsListPage);
            return await AttemptAndRetry(() => _database.UpdateAsync(skillsListPageDto)).ConfigureAwait(false);
        }

        public async Task<VocabularyListPage> GetVocabularyListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder)
        {
            VocabularyListPageDto vocabularyListPageDto = await AttemptAndRetry(() => _database.Table<VocabularyListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder)).ConfigureAwait(false);
            VocabularyListPage vocabularyListPage = JsonConvert.DeserializeObject<VocabularyListPage>(vocabularyListPageDto.VocabularyListPageString);
            return vocabularyListPage;
        }
        public async Task<int> SaveVocabularyListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder, VocabularyListPage vocabularyListPage)
        {
            VocabularyListPageDto vocabularyListPageDto = await AttemptAndRetry(() => _database.Table<VocabularyListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder)).ConfigureAwait(false);
            if (vocabularyListPageDto == null)
            {
                vocabularyListPageDto = new VocabularyListPageDto();
                vocabularyListPageDto.ProgenyId = progenyId;
                vocabularyListPageDto.AccessLevel = accessLevel;
                vocabularyListPageDto.PageNumber = pageNumber;
                vocabularyListPageDto.PageSize = pageSize;
                vocabularyListPageDto.SortOrder = sortOrder;
                vocabularyListPageDto.VocabularyListPageString = JsonConvert.SerializeObject(vocabularyListPage);
                return await AttemptAndRetry(() => _database.InsertAsync(vocabularyListPageDto)).ConfigureAwait(false);
            }
            vocabularyListPageDto.VocabularyListPageString = JsonConvert.SerializeObject(vocabularyListPage);
            return await AttemptAndRetry(() => _database.UpdateAsync(vocabularyListPageDto)).ConfigureAwait(false);
        }

        public async Task<List<VocabularyItem>> GetVocabularyListAsync(int progenyId, int accessLevel)
        {
            VocabularyList vocabularyList = await AttemptAndRetry(() => _database.Table<VocabularyList>().FirstOrDefaultAsync(v => v.ProgenyId == progenyId && v.AccessLevel == accessLevel)).ConfigureAwait(false);
            List<VocabularyItem> vocabularyItems = JsonConvert.DeserializeObject<List<VocabularyItem>>(vocabularyList.VocabularyItemsString);
            return vocabularyItems;
        }
        public async Task<int> SaveVocabularyListAsync(int progenyId, int accessLevel, List<VocabularyItem> vocabularyItemsList)
        {
            VocabularyList vocabularyList = await AttemptAndRetry(() => _database.Table<VocabularyList>().FirstOrDefaultAsync(v => v.ProgenyId == progenyId && v.AccessLevel == accessLevel)).ConfigureAwait(false);
            if (vocabularyList == null)
            {
                vocabularyList = new VocabularyList();
                vocabularyList.ProgenyId = progenyId;
                vocabularyList.AccessLevel = accessLevel;
                vocabularyList.VocabularyItemsString = JsonConvert.SerializeObject(vocabularyItemsList);
                return await AttemptAndRetry(() => _database.InsertAsync(vocabularyList)).ConfigureAwait(false);
            }
            vocabularyList.VocabularyItemsString = JsonConvert.SerializeObject(vocabularyItemsList);
            return await AttemptAndRetry(() => _database.UpdateAsync(vocabularyList)).ConfigureAwait(false);
        }

        public async Task<List<Vaccination>> GetVaccinationsListAsync(int progenyId, int accessLevel)
        {
            VaccinationsList vaccinationsList = await AttemptAndRetry(() => _database.Table<VaccinationsList>().FirstOrDefaultAsync(v => v.ProgenyId == progenyId && v.AccessLevel == accessLevel)).ConfigureAwait(false);
            List<Vaccination> vaccinations = JsonConvert.DeserializeObject<List<Vaccination>>(vaccinationsList.VaccinationItemsString);
            return vaccinations;
        }
        public async Task<int> SaveVaccinationsListAsync(int progenyId, int accessLevel, List<Vaccination> vaccinationItemsList)
        {
            VaccinationsList vaccinationsList = await AttemptAndRetry(() => _database.Table<VaccinationsList>().FirstOrDefaultAsync(v => v.ProgenyId == progenyId && v.AccessLevel == accessLevel)).ConfigureAwait(false);
            if (vaccinationsList == null)
            {
                vaccinationsList = new VaccinationsList();
                vaccinationsList.ProgenyId = progenyId;
                vaccinationsList.AccessLevel = accessLevel;
                vaccinationsList.VaccinationItemsString = JsonConvert.SerializeObject(vaccinationItemsList);
                return await AttemptAndRetry(() => _database.InsertAsync(vaccinationsList)).ConfigureAwait(false);
            }
            vaccinationsList.VaccinationItemsString = JsonConvert.SerializeObject(vaccinationItemsList);
            return await AttemptAndRetry(() => _database.UpdateAsync(vaccinationsList)).ConfigureAwait(false);
        }

        public async Task<NotesListPage> GetNotesListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder)
        {
            NotesListPageDto notesListPageDto = await AttemptAndRetry(() => _database.Table<NotesListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder)).ConfigureAwait(false);
            NotesListPage notesListPage = JsonConvert.DeserializeObject<NotesListPage>(notesListPageDto.NotesListPageString);
            return notesListPage;
        }
        public async Task<int> SaveNotesListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder, NotesListPage notesListPage)
        {
            NotesListPageDto notesListPageDto = await AttemptAndRetry(() => _database.Table<NotesListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder)).ConfigureAwait(false);
            if (notesListPageDto == null)
            {
                notesListPageDto = new NotesListPageDto();
                notesListPageDto.ProgenyId = progenyId;
                notesListPageDto.AccessLevel = accessLevel;
                notesListPageDto.PageNumber = pageNumber;
                notesListPageDto.PageSize = pageSize;
                notesListPageDto.SortOrder = sortOrder;
                notesListPageDto.NotesListPageString = JsonConvert.SerializeObject(notesListPage);
                return await AttemptAndRetry(() => _database.InsertAsync(notesListPageDto)).ConfigureAwait(false);
            }
            notesListPageDto.NotesListPageString = JsonConvert.SerializeObject(notesListPage);
            return await AttemptAndRetry(() => _database.UpdateAsync(notesListPageDto)).ConfigureAwait(false);
        }

        public async Task<LocationsListPage> GetLocationsListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder)
        {
            LocationsPageDto locationsPageDto = await AttemptAndRetry(() => _database.Table<LocationsPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder)).ConfigureAwait(false);
            LocationsListPage locationsListPage = JsonConvert.DeserializeObject<LocationsListPage>(locationsPageDto.LocationsPageString);
            return locationsListPage;
        }
        public async Task<int> SaveLocationsListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder, LocationsListPage locationsListPage)
        {
            LocationsPageDto locationsPageDto = await AttemptAndRetry(() => _database.Table<LocationsPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder)).ConfigureAwait(false);
            if (locationsPageDto == null)
            {
                locationsPageDto = new LocationsPageDto();
                locationsPageDto.ProgenyId = progenyId;
                locationsPageDto.AccessLevel = accessLevel;
                locationsPageDto.PageNumber = pageNumber;
                locationsPageDto.PageSize = pageSize;
                locationsPageDto.SortOrder = sortOrder;
                locationsPageDto.LocationsPageString = JsonConvert.SerializeObject(locationsListPage);
                return await AttemptAndRetry(() => _database.InsertAsync(locationsPageDto)).ConfigureAwait(false);
            }
            locationsPageDto.LocationsPageString = JsonConvert.SerializeObject(locationsListPage);
            return await AttemptAndRetry(() => _database.UpdateAsync(locationsPageDto)).ConfigureAwait(false);
        }

        public async Task<List<Location>> GeLocationsListAsync(int progenyId, int accessLevel)
        {
            LocationsList locationsList = await AttemptAndRetry(() => _database.Table<LocationsList>().FirstOrDefaultAsync(m => m.ProgenyId == progenyId && m.AccessLevel == accessLevel)).ConfigureAwait(false);
            List<Location> locations = JsonConvert.DeserializeObject<List<Location>>(locationsList.LocationItemsString);
            return locations;
        }
        public async Task<int> SaveLocationsListAsync(int progenyId, int accessLevel, List<Location> locationItemsList)
        {
            LocationsList locationsList = await AttemptAndRetry(() => _database.Table<LocationsList>().FirstOrDefaultAsync(m => m.ProgenyId == progenyId && m.AccessLevel == accessLevel)).ConfigureAwait(false);
            if (locationsList == null)
            {
                locationsList = new LocationsList();
                locationsList.ProgenyId = progenyId;
                locationsList.AccessLevel = accessLevel;
                locationsList.LocationItemsString = JsonConvert.SerializeObject(locationItemsList);
                return await AttemptAndRetry(() => _database.InsertAsync(locationsList)).ConfigureAwait(false);
            }
            locationsList.LocationItemsString = JsonConvert.SerializeObject(locationItemsList);
            return await AttemptAndRetry(() => _database.UpdateAsync(locationsList)).ConfigureAwait(false);
        }

        public async Task<List<Picture>> GetPicturesListAsync(int progenyId, int accessLevel)
        {
            PicturesList picturesList = await AttemptAndRetry(() => _database.Table<PicturesList>().FirstOrDefaultAsync(p => p.ProgenyId == progenyId && p.AccessLevel == accessLevel)).ConfigureAwait(false);
            List<Picture> pictures = JsonConvert.DeserializeObject<List<Picture>>(picturesList.PictureItemsString);
            return pictures;
        }
        public async Task<int> SavePicturesListAsync(int progenyId, int accessLevel, List<Picture> pictureItemsList)
        {
            PicturesList picturesList = await AttemptAndRetry(() => _database.Table<PicturesList>().FirstOrDefaultAsync(p => p.ProgenyId == progenyId && p.AccessLevel == accessLevel)).ConfigureAwait(false);
            if (picturesList == null)
            {
                picturesList = new PicturesList();
                picturesList.ProgenyId = progenyId;
                picturesList.AccessLevel = accessLevel;
                picturesList.PictureItemsString = JsonConvert.SerializeObject(pictureItemsList);
                return await AttemptAndRetry(() => _database.InsertAsync(picturesList)).ConfigureAwait(false);
            }
            picturesList.PictureItemsString = JsonConvert.SerializeObject(pictureItemsList);
            return await AttemptAndRetry(() => _database.UpdateAsync(picturesList)).ConfigureAwait(false);
        }

        public async Task<List<Sleep>> GetSleepDetailsAsync(int accessLevel, int sleepId, int sortOrder)
        {
            SleepDetails sleepList = await AttemptAndRetry(() => _database.Table<SleepDetails>().FirstOrDefaultAsync(s => s.AccessLevel == accessLevel && s.SleepId == sleepId && s.SortOrder == sortOrder)).ConfigureAwait(false);
            List<Sleep> sleepItems = JsonConvert.DeserializeObject<List<Sleep>>(sleepList.SleepItemsString);
            return sleepItems;
        }
        public async Task<int> SaveSleepDetailsAsync(int accessLevel, int sleepId, int sortOrder, List<Sleep> sleepItemsList)
        {
            SleepDetails sleepList = await AttemptAndRetry(() => _database.Table<SleepDetails>().FirstOrDefaultAsync(s => s.AccessLevel == accessLevel && s.SleepId == sleepId && s.SortOrder == sortOrder)).ConfigureAwait(false);
            if (sleepList == null)
            {
                sleepList = new SleepDetails();
                sleepList.AccessLevel = accessLevel;
                sleepList.SleepId = sleepId;
                sleepList.SortOrder = sortOrder;
                sleepList.SleepItemsString = JsonConvert.SerializeObject(sleepItemsList);
                return await AttemptAndRetry(() => _database.InsertAsync(sleepList)).ConfigureAwait(false);
            }
            sleepList.SleepItemsString = JsonConvert.SerializeObject(sleepItemsList);
            return await AttemptAndRetry(() => _database.UpdateAsync(sleepList)).ConfigureAwait(false);
        }

        public async Task<TimeLineItem> GetTimeLineItemByItemIdAsync(int itemId, int timeLineType)
        {
            TimeLineItemDto timeLineItemDto = await AttemptAndRetry(() => _database.Table<TimeLineItemDto>().FirstOrDefaultAsync(t => t.ItemId == itemId && t.ItemType == timeLineType)).ConfigureAwait(false);
            TimeLineItem timeLineItem = JsonConvert.DeserializeObject<TimeLineItem>(timeLineItemDto.TimeLineItemString);
            return timeLineItem;
        }
        public async Task<int> SaveTimeLineItemByItemIdAsync(int itemId, int itemType, TimeLineItem timeLineItem)
        {
            TimeLineItemDto timeLineItemDto = await AttemptAndRetry(() => _database.Table<TimeLineItemDto>().FirstOrDefaultAsync(t => t.ItemId == itemId && t.ItemType == timeLineItem.ItemType)).ConfigureAwait(false);
            if (timeLineItemDto == null)
            {
                timeLineItemDto = new TimeLineItemDto();
                timeLineItemDto.TimeLineId = timeLineItem.TimeLineId;
                timeLineItemDto.ItemId = itemId;
                timeLineItemDto.ItemType = itemType;
                timeLineItemDto.TimeLineItemString = JsonConvert.SerializeObject(timeLineItem);
                return await AttemptAndRetry(() => _database.InsertAsync(timeLineItemDto)).ConfigureAwait(false);
            }
            timeLineItemDto.TimeLineItemString = JsonConvert.SerializeObject(timeLineItem);
            return await AttemptAndRetry(() => _database.UpdateAsync(timeLineItemDto)).ConfigureAwait(false);
        }

        public async Task<List<string>> GetLocationAutoSuggestListAsync(int progenyId, int accessLevel)
        {
            LocationAutoSuggestList autoSuggestList = await AttemptAndRetry(() => _database.Table<LocationAutoSuggestList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel)).ConfigureAwait(false);
            List<string> autoSuggestItems = JsonConvert.DeserializeObject<List<string>>(autoSuggestList.AutoSuggestListString);
            return autoSuggestItems;
        }
        public async Task<int> SaveLocationAutoSuggestListAsync(int progenyId, int accessLevel, List<string> autoSuggestItemsList)
        {
            LocationAutoSuggestList autoSuggestList = await AttemptAndRetry(() => _database.Table<LocationAutoSuggestList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel)).ConfigureAwait(false);
            if (autoSuggestList == null)
            {
                autoSuggestList = new LocationAutoSuggestList();
                autoSuggestList.ProgenyId = progenyId;
                autoSuggestList.AccessLevel = accessLevel;
                autoSuggestList.AutoSuggestListString = JsonConvert.SerializeObject(autoSuggestItemsList);
                return await AttemptAndRetry(() => _database.InsertAsync(autoSuggestList)).ConfigureAwait(false);
            }
            autoSuggestList.AutoSuggestListString = JsonConvert.SerializeObject(autoSuggestItemsList);
            return await AttemptAndRetry(() => _database.UpdateAsync(autoSuggestList)).ConfigureAwait(false);
        }

        public async Task<List<string>> GetTagsAutoSuggestListAsync(int progenyId, int accessLevel)
        {
            TagsAutoSuggestList autoSuggestList = await AttemptAndRetry(() => _database.Table<TagsAutoSuggestList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel)).ConfigureAwait(false);
            List<string> autoSuggestItems = JsonConvert.DeserializeObject<List<string>>(autoSuggestList.AutoSuggestListString);
            return autoSuggestItems;
        }
        public async Task<int> SaveTagsAutoSuggestListAsync(int progenyId, int accessLevel, List<string> autoSuggestItemsList)
        {
            TagsAutoSuggestList autoSuggestList = await AttemptAndRetry(() => _database.Table<TagsAutoSuggestList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel)).ConfigureAwait(false);
            if (autoSuggestList == null)
            {
                autoSuggestList = new TagsAutoSuggestList();
                autoSuggestList.ProgenyId = progenyId;
                autoSuggestList.AccessLevel = accessLevel;
                autoSuggestList.AutoSuggestListString = JsonConvert.SerializeObject(autoSuggestItemsList);
                return await AttemptAndRetry(() => _database.InsertAsync(autoSuggestList)).ConfigureAwait(false);
            }
            autoSuggestList.AutoSuggestListString = JsonConvert.SerializeObject(autoSuggestItemsList);
            return await AttemptAndRetry(() => _database.UpdateAsync(autoSuggestList)).ConfigureAwait(false);
        }

        public async Task<List<string>> GetCategoryAutoSuggestListAsync(int progenyId, int accessLevel)
        {
            CategoryAutoSuggestList autoSuggestList = await AttemptAndRetry(() => _database.Table<CategoryAutoSuggestList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel)).ConfigureAwait(false);
            List<string> autoSuggestItems = JsonConvert.DeserializeObject<List<string>>(autoSuggestList.AutoSuggestListString);
            return autoSuggestItems;
        }
        public async Task<int> SaveCategoryAutoSuggestListAsync(int progenyId, int accessLevel, List<string> autoSuggestItemsList)
        {
            CategoryAutoSuggestList autoSuggestList = await AttemptAndRetry(() => _database.Table<CategoryAutoSuggestList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel)).ConfigureAwait(false);
            if (autoSuggestList == null)
            {
                autoSuggestList = new CategoryAutoSuggestList();
                autoSuggestList.ProgenyId = progenyId;
                autoSuggestList.AccessLevel = accessLevel;
                autoSuggestList.AutoSuggestListString = JsonConvert.SerializeObject(autoSuggestItemsList);
                return await AttemptAndRetry(() => _database.InsertAsync(autoSuggestList)).ConfigureAwait(false);
            }
            autoSuggestList.AutoSuggestListString = JsonConvert.SerializeObject(autoSuggestItemsList);
            return await AttemptAndRetry(() => _database.UpdateAsync(autoSuggestList)).ConfigureAwait(false);
        }

        public async Task<List<string>> GetContextAutoSuggestListAsync(int progenyId, int accessLevel)
        {
            ContextAutoSuggestList autoSuggestList = await AttemptAndRetry(() => _database.Table<ContextAutoSuggestList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel)).ConfigureAwait(false);
            List<string> autoSuggestItems = JsonConvert.DeserializeObject<List<string>>(autoSuggestList.AutoSuggestListString);
            return autoSuggestItems;
        }
        public async Task<int> SaveContextAutoSuggestListAsync(int progenyId, int accessLevel, List<string> autoSuggestItemsList)
        {
            ContextAutoSuggestList autoSuggestList = await AttemptAndRetry(() => _database.Table<ContextAutoSuggestList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel)).ConfigureAwait(false);
            if (autoSuggestList == null)
            {
                autoSuggestList = new ContextAutoSuggestList();
                autoSuggestList.ProgenyId = progenyId;
                autoSuggestList.AccessLevel = accessLevel;
                autoSuggestList.AutoSuggestListString = JsonConvert.SerializeObject(autoSuggestItemsList);
                return await AttemptAndRetry(() => _database.InsertAsync(autoSuggestList)).ConfigureAwait(false);
            }
            autoSuggestList.AutoSuggestListString = JsonConvert.SerializeObject(autoSuggestItemsList);
            return await AttemptAndRetry(() => _database.UpdateAsync(autoSuggestList)).ConfigureAwait(false);
        }

        public async Task<UserInfo> GetUserInfoAsync(string email)
        {
            UserInfoDto uiDto = await AttemptAndRetry(() => _database.Table<UserInfoDto>().FirstOrDefaultAsync(u => u.Email.ToUpper() == email.ToUpper())).ConfigureAwait(false);
            if (uiDto == null)
            {
                return OfflineDefaultData.DefaultUserInfo;
            }
            UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(uiDto.UserInfoString);
            return ui;
        }
        public async Task<int> SaveUserInfoAsync(UserInfo userinfo)
        {
            UserInfoDto uiDto = await AttemptAndRetry(() => _database.Table<UserInfoDto>().FirstOrDefaultAsync(u => u.Email.ToUpper() == userinfo.UserEmail.ToUpper())).ConfigureAwait(false);
            if (uiDto == null)
            {
                uiDto = new UserInfoDto();
                uiDto.Email = userinfo.UserEmail;
                uiDto.UserInfoString = JsonConvert.SerializeObject(userinfo);
                return await AttemptAndRetry(() => _database.InsertAsync(uiDto)).ConfigureAwait(false);
            }
            uiDto.UserInfoString = JsonConvert.SerializeObject(userinfo);
            return await AttemptAndRetry(() => _database.UpdateAsync(uiDto)).ConfigureAwait(false);
        }

        public async Task<string> GetUserPictureAsync(string pictureId)
        {
            UserPictureDto userPictureDto = await AttemptAndRetry(() => _database.Table<UserPictureDto>().FirstOrDefaultAsync(u => u.PictureId == pictureId)).ConfigureAwait(false);
            return userPictureDto.PictureString;

        }
        public async Task<int> SaveUserPictureAsync(string pictureId, string pictureString)
        {
            UserPictureDto userPictureDto = await AttemptAndRetry(() => _database.Table<UserPictureDto>().FirstOrDefaultAsync(u => u.PictureId == pictureId)).ConfigureAwait(false);
            if (userPictureDto == null)
            {
                userPictureDto = new UserPictureDto();
                userPictureDto.PictureId = pictureId;
                userPictureDto.PictureString = pictureString;
                return await AttemptAndRetry(() => _database.InsertAsync(userPictureDto)).ConfigureAwait(false);
            }
            userPictureDto.PictureString = pictureString;
            return await AttemptAndRetry(() => _database.UpdateAsync(userPictureDto)).ConfigureAwait(false);
        }
    }
}
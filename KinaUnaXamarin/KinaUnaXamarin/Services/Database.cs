using System.Collections.Generic;
using System.Threading.Tasks;
using KinaUnaXamarin.Models;
using KinaUnaXamarin.Models.KinaUna;
using KinaUnaXamarin.Models.SQLite;
using Newtonsoft.Json;
using SQLite;

namespace KinaUnaXamarin.Services
{
    public class Database
    {
        readonly SQLiteAsyncConnection _database;
        private static object collisionLock = new object();

        public Database(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
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
        public async Task<List<Progeny>> GetProgenyListAsync()
        {
            List<ProgenyDto> dtos = await _database.Table<ProgenyDto>().ToListAsync();
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
            ProgenyDto progDto = await _database.Table<ProgenyDto>().FirstOrDefaultAsync(p => p.ProgenyId == progenyId);
            if (progDto == null)
            {
                return OfflineDefaultData.DefaultProgeny;
            }
            Progeny prog = JsonConvert.DeserializeObject<Progeny>(progDto.ProgenyString);
            return prog;
        }
        public async Task<int> SaveProgenyAsync(Progeny progeny)
        {
            ProgenyDto progDto = await _database.Table<ProgenyDto>().FirstOrDefaultAsync(p => p.ProgenyId == progeny.Id);
            if (progDto == null)
            {
                progDto = new ProgenyDto();
                progDto.ProgenyId = progeny.Id;
                progDto.ProgenyString = JsonConvert.SerializeObject(progeny);
                return await _database.InsertAsync(progDto);
            }

            progDto.ProgenyString = JsonConvert.SerializeObject(progeny);
            return await _database.UpdateAsync(progDto);
        }

        public async Task<Comment> GetCommentAsync(int commentId)
        {
            CommentDto cmntDto = await _database.Table<CommentDto>().FirstOrDefaultAsync(c => c.CommentId == commentId);
            Comment comment = JsonConvert.DeserializeObject<Comment>(cmntDto.CommentString);
            return comment;
        }
        public async Task<int> SaveCommentAsync(Comment comment)
        {
            CommentDto cmntDto = await _database.Table<CommentDto>().FirstOrDefaultAsync(c => c.CommentId == comment.CommentId);
            if (cmntDto == null)
            {
                cmntDto = new CommentDto();
                cmntDto.CommentId = comment.CommentId;
                cmntDto.CommentString = JsonConvert.SerializeObject(comment);
                return await _database.InsertAsync(cmntDto);
            }

            cmntDto.CommentString = JsonConvert.SerializeObject(comment);
            return await _database.UpdateAsync(cmntDto);
        }

        public async Task<string> GetCommentThreadAsync(int commentThread)
        {
            CommentsList comList = await _database.Table<CommentsList>().FirstOrDefaultAsync(c => c.CommentThread == commentThread);
            return comList?.CommentsListString ?? "";
        }

        public async Task<int> SaveCommentThreadAsync(int commentThread, List<Comment> commentsList)
        {
            CommentsList comList = await _database.Table<CommentsList>().FirstOrDefaultAsync(c => c.CommentThread == commentThread);
            if (comList == null)
            {
                comList = new CommentsList();
                comList.CommentThread = commentThread;
                comList.CommentsListString = JsonConvert.SerializeObject(commentsList);
                return await _database.InsertAsync(comList);
            }
            comList.CommentsListString = JsonConvert.SerializeObject(commentsList);
            return await _database.UpdateAsync(comList);
        }

        public async Task<string> GetProgenyListAsync(string userEmail)
        {
            ProgenyList progList = await _database.Table<ProgenyList>().FirstOrDefaultAsync(p => p.UserEmail.ToUpper() == userEmail.ToUpper());
            return progList?.ProgenyListString ?? "";
        }

        public async Task<int> SaveProgenyListAsync(string userEmail, List<Progeny> progenyList)
        {
            ProgenyList progList = await _database.Table<ProgenyList>().FirstOrDefaultAsync(p => p.UserEmail.ToUpper() == userEmail.ToUpper());
            if (progList == null)
            {
                progList = new ProgenyList();
                progList.UserEmail = userEmail;
                progList.ProgenyListString = JsonConvert.SerializeObject(progenyList);
                return await _database.InsertAsync(progList);
            }
            progList.ProgenyListString = JsonConvert.SerializeObject(progenyList);
            return await _database.UpdateAsync(progList);
        }

        public async Task<UserAccess> GetUserAccessAsync(string email, int progenyId)
        {
            UserAccessDto uaDto = await _database.Table<UserAccessDto>().FirstOrDefaultAsync(u => u.Email.ToUpper() == email.ToUpper() && u.ProgenyId == progenyId);
            UserAccess ua = new UserAccess();
            if (uaDto != null)
            {
                ua = JsonConvert.DeserializeObject<UserAccess>(uaDto.UserAccessString);
            }
            
            return ua;
        }
        public async Task<int> SaveUserAccessAsync(UserAccess userAccess)
        {
            UserAccessDto uaDto = await _database.Table<UserAccessDto>().FirstOrDefaultAsync(u => u.Email.ToUpper() == userAccess.UserId.ToUpper() && u.ProgenyId == userAccess.ProgenyId);
            if (uaDto == null)
            {
                uaDto = new UserAccessDto();
                uaDto.Email = userAccess.UserId;
                uaDto.ProgenyId = userAccess.ProgenyId;
                uaDto.UserAccessString = JsonConvert.SerializeObject(userAccess);
                return await _database.InsertAsync(uaDto);
            }
            uaDto.UserAccessString = JsonConvert.SerializeObject(userAccess);
            return await _database.UpdateAsync(uaDto);
        }

        public async Task<string> GetUpcomingEventsAsync(int progenyId, int accessLevel)
        {
            UpcomingEvents eventsList = await _database.Table<UpcomingEvents>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.AccessLevel == accessLevel);
            return eventsList?.EventsListString ?? "";
        }

        public async Task<int> SaveUpcomingEventsAsync(int progenyId, int accessLevel, List<CalendarItem> eventsList)
        {
            UpcomingEvents upcomingList = await _database.Table<UpcomingEvents>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.AccessLevel == accessLevel);
            if (upcomingList == null)
            {
                upcomingList = new UpcomingEvents();
                upcomingList.ProgenyId = progenyId;
                upcomingList.AccessLevel = accessLevel;
                upcomingList.EventsListString = JsonConvert.SerializeObject(eventsList);
                return await _database.InsertAsync(upcomingList);
            }
            upcomingList.EventsListString = JsonConvert.SerializeObject(eventsList);
            return await _database.UpdateAsync(upcomingList);
        }

        public async Task<string> GetTimeLineListAsync(int progenyId, int accessLevel, int count, int start)
        {
            TimeLineList timeLineItems = await _database.Table<TimeLineList>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.AccessLevel == accessLevel && e.Count == count && e.Start == start);
            return timeLineItems?.TimeLineItemsString ?? "";
        }

        public async Task<int> SaveTimeLineListAsync(int progenyId, int accessLevel, int count, int start, List<TimeLineItem> timeLineItemsList)
        {
            TimeLineList timeLineList = await _database.Table<TimeLineList>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.AccessLevel == accessLevel && e.Count == count && e.Start == start);
            if (timeLineList == null)
            {
                timeLineList = new TimeLineList();
                timeLineList.ProgenyId = progenyId;
                timeLineList.AccessLevel = accessLevel;
                timeLineList.Count = count;
                timeLineList.Start = start;
                timeLineList.TimeLineItemsString = JsonConvert.SerializeObject(timeLineItemsList);
                return await _database.InsertAsync(timeLineList);
            }
            timeLineList.TimeLineItemsString = JsonConvert.SerializeObject(timeLineItemsList);
            return await _database.UpdateAsync(timeLineList);
        }

        public async Task<string> GetTimeLineLatestAsync(int progenyId, int accessLevel)
        {
            TimeLineLatest timeLineLatestItems = await _database.Table<TimeLineLatest>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.AccessLevel == accessLevel);
            return timeLineLatestItems?.TimeLineItemsString ?? "";
        }

        public async Task<int> SaveTimeLineLatestAsync(int progenyId, int accessLevel, List<TimeLineItem> timeLineItemsList)
        {
            TimeLineLatest timeLineList = await _database.Table<TimeLineLatest>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.AccessLevel == accessLevel);
            if (timeLineList == null)
            {
                timeLineList = new TimeLineLatest();
                timeLineList.ProgenyId = progenyId;
                timeLineList.AccessLevel = accessLevel;
                timeLineList.TimeLineItemsString = JsonConvert.SerializeObject(timeLineItemsList);
                return await _database.InsertAsync(timeLineList);
            }
            timeLineList.TimeLineItemsString = JsonConvert.SerializeObject(timeLineItemsList);
            return await _database.UpdateAsync(timeLineList);
        }

        public async Task<Picture> GetPictureAsync(int pictureId)
        {
            PictureDto pictureDto = await _database.Table<PictureDto>().FirstOrDefaultAsync(p => p.PictureId == pictureId);
            Picture picture = JsonConvert.DeserializeObject<Picture>(pictureDto.PictureString);
            return picture;
        }
        public async Task<int> SavePictureAsync(Picture picture)
        {
            PictureDto pictureDto = await _database.Table<PictureDto>().FirstOrDefaultAsync(p => p.PictureId == picture.PictureId);
            if (pictureDto == null)
            {
                pictureDto = new PictureDto();
                pictureDto.PictureId = picture.PictureId;
                pictureDto.PictureString = JsonConvert.SerializeObject(picture);
                return await _database.InsertAsync(pictureDto);
            }
            pictureDto.PictureString = JsonConvert.SerializeObject(picture);
            return await _database.UpdateAsync(pictureDto);
        }

        public async Task<string> GetPicturePageListAsync(int progenyId, int pageNumber, int pageSize, int sortBy, string tagFilter)
        {
            PicturePageList picturePageList = await _database.Table<PicturePageList>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.PageNumber == pageNumber && e.PageSize == pageSize && e.SortBy == sortBy && e.TagFilter.ToUpper() == tagFilter.ToUpper());
            return picturePageList?.PictureItemsString ?? "";
        }

        public async Task<int> SavePicturePageListAsync(int progenyId, int pageNumber, int pageSize, int sortBy, string tagFilter, PicturePage picturePage)
        {
            PicturePageList picturePageList = await _database.Table<PicturePageList>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.PageNumber == pageNumber && e.PageSize == pageSize && e.SortBy == sortBy && e.TagFilter.ToUpper() == tagFilter.ToUpper());
            if (picturePageList == null)
            {
                picturePageList = new PicturePageList();
                picturePageList.ProgenyId = progenyId;
                picturePageList.PageNumber = pageNumber;
                picturePageList.PageSize = pageSize;
                picturePageList.SortBy = sortBy;
                picturePageList.TagFilter = tagFilter;
                picturePageList.PictureItemsString = JsonConvert.SerializeObject(picturePage);
                return await _database.InsertAsync(picturePageList);
            }
            picturePageList.PictureItemsString = JsonConvert.SerializeObject(picturePage);
            return await _database.UpdateAsync(picturePageList);
        }

        public async Task<PictureViewModel> GetPictureViewModelAsync(int pictureId)
        {
            PictureViewModelDto pictureViewModelDto = await _database.Table<PictureViewModelDto>().FirstOrDefaultAsync(p => p.PictureId == pictureId);
            PictureViewModel pictureViewModel =
                JsonConvert.DeserializeObject<PictureViewModel>(pictureViewModelDto.PictureViewModelString);
            return pictureViewModel;
        }
        public async Task<int> SavePictureViewModelAsync(PictureViewModel pictureViewModel)
        {
            PictureViewModelDto pictureViewModelDto = await _database.Table<PictureViewModelDto>().FirstOrDefaultAsync(p => p.PictureId == pictureViewModel.PictureId);
            if (pictureViewModelDto == null)
            {
                pictureViewModelDto = new PictureViewModelDto();
                pictureViewModelDto.PictureId = pictureViewModel.PictureId;
                pictureViewModelDto.PictureViewModelString = JsonConvert.SerializeObject(pictureViewModel);
                return await _database.InsertAsync(pictureViewModelDto);
            }
            pictureViewModelDto.PictureViewModelString = JsonConvert.SerializeObject(pictureViewModel);
            return await _database.UpdateAsync(pictureViewModelDto);
        }

        public async Task<Video> GetVideoAsync(int videoId)
        {
            VideoDto videoDto = await _database.Table<VideoDto>().FirstOrDefaultAsync(v => v.VideoId == videoId);
            Video video = JsonConvert.DeserializeObject<Video>(videoDto.VideoString);
            return video;
        }
        public async Task<int> SaveVideoAsync(Video video)
        {
            VideoDto videoDto = await _database.Table<VideoDto>().FirstOrDefaultAsync(v => v.VideoId == video.VideoId);
            if (videoDto == null)
            {
                videoDto = new VideoDto();
                videoDto.VideoId = video.VideoId;
                videoDto.VideoString = JsonConvert.SerializeObject(video);
                return await _database.InsertAsync(videoDto);
            }
            videoDto.VideoString = JsonConvert.SerializeObject(video);
            return await _database.UpdateAsync(videoDto);
        }

        public async Task<string> GetVideoPageListAsync(int progenyId, int pageNumber, int pageSize, int sortBy, string tagFilter)
        {
            VideoPageList videoPageList = await _database.Table<VideoPageList>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.PageNumber == pageNumber && e.PageSize == pageSize && e.SortBy == sortBy && e.TagFilter.ToUpper() == tagFilter.ToUpper());
            return videoPageList?.VideoItemsString ?? "";
        }

        public async Task<int> SaveVideoPageListAsync(int progenyId, int pageNumber, int pageSize, int sortBy, string tagFilter, VideoPage videoPage)
        {
            VideoPageList videoPageList = await _database.Table<VideoPageList>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.PageNumber == pageNumber && e.PageSize == pageSize && e.SortBy == sortBy && e.TagFilter.ToUpper() == tagFilter.ToUpper());
            if (videoPageList == null)
            {
                videoPageList = new VideoPageList();
                videoPageList.ProgenyId = progenyId;
                videoPageList.PageNumber = pageNumber;
                videoPageList.PageSize = pageSize;
                videoPageList.SortBy = sortBy;
                videoPageList.TagFilter = tagFilter;
                videoPageList.VideoItemsString = JsonConvert.SerializeObject(videoPage);
                return await _database.InsertAsync(videoPageList);
            }
            videoPageList.VideoItemsString = JsonConvert.SerializeObject(videoPage);
            return await _database.UpdateAsync(videoPageList);
        }

        public async Task<VideoViewModel> GetVideoViewModelAsync(int videoId)
        {
            VideoViewModelDto videoViewModelDto = await _database.Table<VideoViewModelDto>().FirstOrDefaultAsync(v => v.VideoId == videoId);
            VideoViewModel videoViewModel =
                JsonConvert.DeserializeObject<VideoViewModel>(videoViewModelDto.VideoViewModelString);
            return videoViewModel;
        }
        public async Task<int> SaveVideoViewModelAsync(VideoViewModel videoViewModel)
        {
            VideoViewModelDto videoViewModelDto = await _database.Table<VideoViewModelDto>().FirstOrDefaultAsync(v => v.VideoId == videoViewModel.VideoId);
            if (videoViewModelDto == null)
            {
                videoViewModelDto = new VideoViewModelDto();
                videoViewModelDto.VideoId = videoViewModelDto.VideoId;
                videoViewModelDto.VideoViewModelString = JsonConvert.SerializeObject(videoViewModel);
                return await _database.InsertAsync(videoViewModelDto);
            }
            videoViewModelDto.VideoViewModelString = JsonConvert.SerializeObject(videoViewModel);
            return await _database.UpdateAsync(videoViewModelDto);
        }

        public async Task<CalendarItem> GetCalendarItemAsync(int eventId)
        {
            EventDto evtDto = await _database.Table<EventDto>().FirstOrDefaultAsync(c => c.EventId == eventId);
            CalendarItem calendarItem = JsonConvert.DeserializeObject<CalendarItem>(evtDto.EventString);
            return calendarItem;
        }
        public async Task<int> SaveCalendarItemAsync(CalendarItem calendarItem)
        {
            EventDto eventDto = await _database.Table<EventDto>().FirstOrDefaultAsync(c => c.EventId == calendarItem.EventId);
            if (eventDto == null)
            {
                eventDto = new EventDto();
                eventDto.EventId = calendarItem.EventId;
                eventDto.EventString = JsonConvert.SerializeObject(calendarItem);
                return await _database.InsertAsync(eventDto);
            }
            eventDto.EventString = JsonConvert.SerializeObject(calendarItem);
            return await _database.UpdateAsync(eventDto);
        }

        public async Task<List<CalendarItem>> GetCalendarListAsync(int progenyId, int accessLevel)
        {
            CalendarList calList = await _database.Table<CalendarList>().FirstOrDefaultAsync(c => c.ProgenyId == progenyId && c.AccessLevel == accessLevel);
            List<CalendarItem> calendarItems =
                JsonConvert.DeserializeObject<List<CalendarItem>>(calList.CalendarListString);
            return calendarItems;
        }

        public async Task<int> SaveCalendarListAsync(int progenyId, int accessLevel, List<CalendarItem> calendarItems)
        {
            CalendarList calList = await _database.Table<CalendarList>().FirstOrDefaultAsync(c => c.ProgenyId == progenyId && c.AccessLevel == accessLevel);
            if (calList == null)
            {
                calList = new CalendarList();
                calList.ProgenyId = progenyId;
                calList.AccessLevel = accessLevel;
                calList.CalendarListString = JsonConvert.SerializeObject(calendarItems);
                return await _database.InsertAsync(calList);
            }
            calList.CalendarListString = JsonConvert.SerializeObject(calendarItems);
            return await _database.UpdateAsync(calList);
        }

        public async Task<Location> GetLocationAsync(int locationId)
        {
            LocationDto locDto = await _database.Table<LocationDto>().FirstOrDefaultAsync(c => c.LocationId == locationId);
            Location location = JsonConvert.DeserializeObject<Location>(locDto.LocationString);
            return location;
        }
        public async Task<int> SaveLocationAsync(Location location)
        {
            LocationDto locDto = await _database.Table<LocationDto>().FirstOrDefaultAsync(c => c.LocationId == location.LocationId);
            if (locDto == null)
            {
                locDto = new LocationDto();
                locDto.LocationId = location.LocationId;
                locDto.LocationString = JsonConvert.SerializeObject(location);
                return await _database.InsertAsync(locDto);
            }
            locDto.LocationString = JsonConvert.SerializeObject(location);
            return await _database.UpdateAsync(locDto);
        }

        public async Task<VocabularyItem> GetVocabularyItemAsync(int wordId)
        {
            WordDto wordDto = await _database.Table<WordDto>().FirstOrDefaultAsync(w => w.WordId == wordId);
            VocabularyItem vocabularyItem = JsonConvert.DeserializeObject<VocabularyItem>(wordDto.WordString);
            return vocabularyItem;
        }
        public async Task<int> SaveVocabularyItemAsync(VocabularyItem vocabularyItem)
        {
            WordDto wordDto = await _database.Table<WordDto>().FirstOrDefaultAsync(w => w.WordId == vocabularyItem.WordId);
            if (wordDto == null)
            {
                wordDto = new WordDto();
                wordDto.WordId = vocabularyItem.WordId;
                wordDto.WordString = JsonConvert.SerializeObject(vocabularyItem);
                return await _database.InsertAsync(wordDto);
            }
            wordDto.WordString = JsonConvert.SerializeObject(vocabularyItem);
            return await _database.UpdateAsync(wordDto);
        }

        public async Task<Skill> GetSkillAsync(int skillId)
        {
            SkillDto skillDto = await _database.Table<SkillDto>().FirstOrDefaultAsync(s => s.SkillId == skillId);
            Skill skillItem = JsonConvert.DeserializeObject<Skill>(skillDto.SkillString);
            return skillItem;
        }
        public async Task<int> SaveSkillAsync(Skill skill)
        {
            SkillDto skillDto = await _database.Table<SkillDto>().FirstOrDefaultAsync(s => s.SkillId == skill.SkillId);
            if (skillDto == null)
            {
                skillDto = new SkillDto();
                skillDto.SkillId = skill.SkillId;
                skillDto.SkillString = JsonConvert.SerializeObject(skill);
                return await _database.InsertAsync(skillDto);
            }
            skillDto.SkillString = JsonConvert.SerializeObject(skill);
            return await _database.UpdateAsync(skillDto);
        }

        public async Task<Friend> GetFriendAsync(int friendId)
        {
            FriendDto friendDto = await _database.Table<FriendDto>().FirstOrDefaultAsync(f => f.FriendId == friendId);
            Friend friendItem = JsonConvert.DeserializeObject<Friend>(friendDto.FriendString);
            return friendItem;
        }
        public async Task<int> SaveFriendAsync(Friend friend)
        {
            FriendDto friendDto = await _database.Table<FriendDto>().FirstOrDefaultAsync(f => f.FriendId == friend.FriendId);
            if (friendDto == null)
            {
                friendDto = new FriendDto();
                friendDto.FriendId = friend.FriendId;
                friendDto.FriendString = JsonConvert.SerializeObject(friend);
                return await _database.InsertAsync(friendDto);
            }
            friendDto.FriendString = JsonConvert.SerializeObject(friend);
            return await _database.UpdateAsync(friendDto);
        }

        public async Task<Measurement> GetMeasurementAsync(int measurementId)
        {
            MeasurementDto measurementDto = await _database.Table<MeasurementDto>().FirstOrDefaultAsync(m => m.MeasurementId == measurementId);
            Measurement measurementItem = JsonConvert.DeserializeObject<Measurement>(measurementDto.MeasurementString);
            return measurementItem;
        }
        public async Task<int> SaveMeasurementAsync(Measurement measurement)
        {
            MeasurementDto measurementDto = await _database.Table<MeasurementDto>().FirstOrDefaultAsync(m => m.MeasurementId == measurement.MeasurementId);
            if (measurementDto == null)
            {
                measurementDto = new MeasurementDto();
                measurementDto.MeasurementId = measurement.MeasurementId;
                measurementDto.MeasurementString = JsonConvert.SerializeObject(measurement);
                return await _database.InsertAsync(measurementDto);
            }
            measurementDto.MeasurementString = JsonConvert.SerializeObject(measurement);
            return await _database.UpdateAsync(measurementDto);
        }

        public async Task<Sleep> GetSleepAsync(int sleepId)
        {
            SleepDto sleepDto = await _database.Table<SleepDto>().FirstOrDefaultAsync(s => s.SleepId == sleepId);
            Sleep sleepItem = JsonConvert.DeserializeObject<Sleep>(sleepDto.SleepString);
            return sleepItem;
        }

        public async Task<int> SaveSleepAsync(Sleep sleep)
        {
            SleepDto sleepDto = await _database.Table<SleepDto>().FirstOrDefaultAsync(s => s.SleepId == sleep.SleepId);
            if (sleepDto == null)
            {
                sleepDto = new SleepDto();
                sleepDto.SleepId = sleep.SleepId;
                sleepDto.SleepString = JsonConvert.SerializeObject(sleep);
                return await _database.InsertAsync(sleepDto);
            }
            sleepDto.SleepString = JsonConvert.SerializeObject(sleep);
            return await _database.UpdateAsync(sleepDto);
        }

        public async Task<List<Sleep>> GetSleepListAsync(int progenyId, int accessLevel)
        {
            SleepList sleepList = await _database.Table<SleepList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel);
            List<Sleep> sleepItems = JsonConvert.DeserializeObject<List<Sleep>>(sleepList.SleepItemsString);
            return sleepItems;
        }
        public async Task<int> SaveSleepListAsync(int progenyId, int accessLevel, List<Sleep> sleepItemsList)
        {
            SleepList sleepList = await _database.Table<SleepList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel);
            if (sleepList == null)
            {
                sleepList = new SleepList();
                sleepList.ProgenyId = progenyId;
                sleepList.AccessLevel = accessLevel;
                sleepList.SleepItemsString = JsonConvert.SerializeObject(sleepItemsList);
                return await _database.InsertAsync(sleepList);
            }
            sleepList.SleepItemsString = JsonConvert.SerializeObject(sleepItemsList);
            return await _database.UpdateAsync(sleepList);
        }

        public async Task<SleepStatsModel> GetSleepStatsModelAsync(int progenyId, int accessLevel)
        {
            SleepStatsModelDto sleepStatsModelDto = await _database.Table<SleepStatsModelDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel);
            SleepStatsModel sleepStats = JsonConvert.DeserializeObject<SleepStatsModel>(sleepStatsModelDto.SleepStatsString);
            return sleepStats;
        }
        public async Task<int> SaveSleepStatsModelAsync(int progenyId, int accessLevel, SleepStatsModel sleepStats)
        {
            SleepStatsModelDto sleepStatsModelDto = await _database.Table<SleepStatsModelDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel);
            if (sleepStatsModelDto == null)
            {
                sleepStatsModelDto = new SleepStatsModelDto();
                sleepStatsModelDto.ProgenyId = progenyId;
                sleepStatsModelDto.AccessLevel = accessLevel;
                sleepStatsModelDto.SleepStatsString = JsonConvert.SerializeObject(sleepStats);
                return await _database.InsertAsync(sleepStatsModelDto);
            }
            sleepStatsModelDto.SleepStatsString = JsonConvert.SerializeObject(sleepStats);
            return await _database.UpdateAsync(sleepStatsModelDto);
        }

        public async Task<List<Sleep>> GetSleepChartAsync(int progenyId, int accessLevel)
        {
            SleepChartDto sleepList = await _database.Table<SleepChartDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel);
            List<Sleep> sleepItems = JsonConvert.DeserializeObject<List<Sleep>>(sleepList.SleepChartString);
            return sleepItems;
        }
        public async Task<int> SaveSleepChartAsync(int progenyId, int accessLevel, List<Sleep> sleepItemsList)
        {
            SleepChartDto sleepChart = await _database.Table<SleepChartDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel);
            if (sleepChart == null)
            {
                sleepChart = new SleepChartDto();
                sleepChart.ProgenyId = progenyId;
                sleepChart.AccessLevel = accessLevel;
                sleepChart.SleepChartString = JsonConvert.SerializeObject(sleepItemsList);
                return await _database.InsertAsync(sleepChart);
            }
            sleepChart.SleepChartString = JsonConvert.SerializeObject(sleepItemsList);
            return await _database.UpdateAsync(sleepChart);
        }

        public async Task<Note> GetNoteAsync(int noteId)
        {
            NoteDto noteDto = await _database.Table<NoteDto>().FirstOrDefaultAsync(n => n.NoteId == noteId);
            Note noteItem = JsonConvert.DeserializeObject<Note>(noteDto.NoteString);
            return noteItem;
        }
        public async Task<int> SaveNoteAsync(Note note)
        {
            NoteDto noteDto = await _database.Table<NoteDto>().FirstOrDefaultAsync(n => n.NoteId == note.NoteId);
            if (noteDto == null)
            {
                noteDto = new NoteDto();
                noteDto.NoteId = note.NoteId;
                noteDto.NoteString = JsonConvert.SerializeObject(note);
                return await _database.InsertAsync(noteDto);
            }
            noteDto.NoteString = JsonConvert.SerializeObject(note);
            return await _database.UpdateAsync(noteDto);
        }

        public async Task<Contact> GetContactAsync(int contactId)
        {
            ContactDto contactDto = await _database.Table<ContactDto>().FirstOrDefaultAsync(c => c.ContactId == contactId);
            Contact contactItem = JsonConvert.DeserializeObject<Contact>(contactDto.ContactString);
            return contactItem;
        }
        public async Task<int> SaveContactAsync(Contact contact)
        {
            ContactDto contactDto = await _database.Table<ContactDto>().FirstOrDefaultAsync(c => c.ContactId == contact.ContactId);
            if (contactDto == null)
            {
                contactDto = new ContactDto();
                contactDto.ContactId = contact.ContactId;
                contactDto.ContactString = JsonConvert.SerializeObject(contact);
                return await _database.InsertAsync(contactDto);
            }
            contactDto.ContactString = JsonConvert.SerializeObject(contact);
            return await _database.UpdateAsync(contactDto);
        }

        public async Task<List<Contact>> GetContactListAsync(int progenyId, int accessLevel)
        {
            ContactList contactList = await _database.Table<ContactList>().FirstOrDefaultAsync(c => c.ProgenyId == progenyId && c.AccessLevel == accessLevel);
            List<Contact> contactItems = JsonConvert.DeserializeObject<List<Contact>>(contactList.ContactItemsString);
            return contactItems;
        }
        public async Task<int> SaveContactListAsync(int progenyId, int accessLevel, List<Contact> contactItemsList)
        {
            ContactList contactList = await _database.Table<ContactList>().FirstOrDefaultAsync(c => c.ProgenyId == progenyId && c.AccessLevel == accessLevel);
            if (contactList == null)
            {
                contactList = new ContactList();
                contactList.ProgenyId = progenyId;
                contactList.AccessLevel = accessLevel;
                contactList.ContactItemsString = JsonConvert.SerializeObject(contactItemsList);
                return await _database.InsertAsync(contactList);
            }
            contactList.ContactItemsString = JsonConvert.SerializeObject(contactItemsList);
            return await _database.UpdateAsync(contactList);
        }

        public async Task<Vaccination> GetVaccinationAsync(int vaccinationId)
        {
            VaccinationDto vacDto = await _database.Table<VaccinationDto>().FirstOrDefaultAsync(s => s.VaccinationId == vaccinationId);
            Vaccination vacItem = JsonConvert.DeserializeObject<Vaccination>(vacDto.VaccinationString);
            return vacItem;
        }
        public async Task<int> SaveVaccinationAsync(Vaccination vaccination)
        {
            VaccinationDto vaccinationDto = await _database.Table<VaccinationDto>().FirstOrDefaultAsync(s => s.VaccinationId == vaccination.VaccinationId);
            if (vaccinationDto == null)
            {
                vaccinationDto = new VaccinationDto();
                vaccinationDto.VaccinationId = vaccination.VaccinationId;
                vaccinationDto.VaccinationString = JsonConvert.SerializeObject(vaccination);
                return await _database.InsertAsync(vaccinationDto);
            }
            vaccinationDto.VaccinationString = JsonConvert.SerializeObject(vaccination);
            return await _database.UpdateAsync(vaccinationDto);
        }

        public async Task<List<UserAccess>> GetProgenyAccessListAsync(int progenyId)
        {
            ProgenyAccessList progenyAccessList = await _database.Table<ProgenyAccessList>().FirstOrDefaultAsync(p => p.ProgenyId == progenyId);
            List<UserAccess> userAccessList =
                JsonConvert.DeserializeObject<List<UserAccess>>(progenyAccessList.AccessListString);
            return userAccessList;
        }

        public async Task<int> SaveProgenyAccessListAsync(int progenyId, List<UserAccess> userAccessList)
        {
            ProgenyAccessList progenyAccessList = await _database.Table<ProgenyAccessList>().FirstOrDefaultAsync(p => p.ProgenyId == progenyId);
            if (progenyAccessList == null)
            {
                progenyAccessList = new ProgenyAccessList();
                progenyAccessList.ProgenyId = progenyId;
                progenyAccessList.AccessListString = JsonConvert.SerializeObject(userAccessList);
                return await _database.InsertAsync(progenyAccessList);
            }
            progenyAccessList.AccessListString = JsonConvert.SerializeObject(userAccessList);
            return await _database.UpdateAsync(progenyAccessList);
        }

        public async Task<SleepListPage> GetSleepListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder)
        {
            SleepListPageDto sleepListPageDto = await _database.Table<SleepListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder);
            SleepListPage sleepListPage = JsonConvert.DeserializeObject<SleepListPage>(sleepListPageDto.SleepListPageString);
            return sleepListPage;
        }
        public async Task<int> SaveSleepListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder, SleepListPage sleepListPage)
        {
            SleepListPageDto sleepListPageDto = await _database.Table<SleepListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder);
            if (sleepListPageDto == null)
            {
                sleepListPageDto = new SleepListPageDto();
                sleepListPageDto.ProgenyId = progenyId;
                sleepListPageDto.AccessLevel = accessLevel;
                sleepListPageDto.PageNumber = pageNumber;
                sleepListPageDto.PageSize = pageSize;
                sleepListPageDto.SortOrder = sortOrder;
                sleepListPageDto.SleepListPageString = JsonConvert.SerializeObject(sleepListPage);
                return await _database.InsertAsync(sleepListPageDto);
            }
            sleepListPageDto.SleepListPageString = JsonConvert.SerializeObject(sleepListPage);
            return await _database.UpdateAsync(sleepListPageDto);
        }

        public async Task<List<Friend>> GetFriendsListAsync(int progenyId, int accessLevel)
        {
            FriendsList friendsList = await _database.Table<FriendsList>().FirstOrDefaultAsync(f => f.ProgenyId == progenyId && f.AccessLevel == accessLevel);
            List<Friend> friendItems = JsonConvert.DeserializeObject<List<Friend>>(friendsList.FriendItemsString);
            return friendItems;
        }
        public async Task<int> SaveFriendsListAsync(int progenyId, int accessLevel, List<Friend> friendItemsList)
        {
            FriendsList friendsList = await _database.Table<FriendsList>().FirstOrDefaultAsync(f => f.ProgenyId == progenyId && f.AccessLevel == accessLevel);
            if (friendsList == null)
            {
                friendsList = new FriendsList();
                friendsList.ProgenyId = progenyId;
                friendsList.AccessLevel = accessLevel;
                friendsList.FriendItemsString = JsonConvert.SerializeObject(friendItemsList);
                return await _database.InsertAsync(friendsList);
            }
            friendsList.FriendItemsString = JsonConvert.SerializeObject(friendItemsList);
            return await _database.UpdateAsync(friendsList);
        }

        public async Task<MeasurementsListPage> GetMeasurementsListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder)
        {
            MeasurementsListPageDto measurementsListPageDto = await _database.Table<MeasurementsListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder);
            MeasurementsListPage sleepListPage = JsonConvert.DeserializeObject<MeasurementsListPage>(measurementsListPageDto.MeasurementsListPageString);
            return sleepListPage;
        }
        public async Task<int> SaveMeasurementsListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder, MeasurementsListPage measurementsListPage)
        {
            MeasurementsListPageDto measurementsListPageDto = await _database.Table<MeasurementsListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder);
            if (measurementsListPageDto == null)
            {
                measurementsListPageDto = new MeasurementsListPageDto();
                measurementsListPageDto.ProgenyId = progenyId;
                measurementsListPageDto.AccessLevel = accessLevel;
                measurementsListPageDto.PageNumber = pageNumber;
                measurementsListPageDto.PageSize = pageSize;
                measurementsListPageDto.SortOrder = sortOrder;
                measurementsListPageDto.MeasurementsListPageString = JsonConvert.SerializeObject(measurementsListPage);
                return await _database.InsertAsync(measurementsListPageDto);
            }
            measurementsListPageDto.MeasurementsListPageString = JsonConvert.SerializeObject(measurementsListPage);
            return await _database.UpdateAsync(measurementsListPageDto);
        }

        public async Task<List<Measurement>> GetMeasurementsListAsync(int progenyId, int accessLevel)
        {
            MeasurementsList measurementsList = await _database.Table<MeasurementsList>().FirstOrDefaultAsync(m => m.ProgenyId == progenyId && m.AccessLevel == accessLevel);
            List<Measurement> measurements = JsonConvert.DeserializeObject<List<Measurement>>(measurementsList.MeasurementsItemsString);
            return measurements;
        }
        public async Task<int> SaveMeasurementsListAsync(int progenyId, int accessLevel, List<Measurement> measurementItemsList)
        {
            MeasurementsList measurementsList = await _database.Table<MeasurementsList>().FirstOrDefaultAsync(m => m.ProgenyId == progenyId && m.AccessLevel == accessLevel);
            if (measurementsList == null)
            {
                measurementsList = new MeasurementsList();
                measurementsList.ProgenyId = progenyId;
                measurementsList.AccessLevel = accessLevel;
                measurementsList.MeasurementsItemsString = JsonConvert.SerializeObject(measurementItemsList);
                return await _database.InsertAsync(measurementsList);
            }
            measurementsList.MeasurementsItemsString = JsonConvert.SerializeObject(measurementItemsList);
            return await _database.UpdateAsync(measurementsList);
        }

        public async Task<SkillsListPage> GetSkillsListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder)
        {
            SkillsListPageDto skillsListPageDto = await _database.Table<SkillsListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder);
            SkillsListPage skillsListPage = JsonConvert.DeserializeObject<SkillsListPage>(skillsListPageDto.SkillsListPageString);
            return skillsListPage;
        }
        public async Task<int> SaveSkillsListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder, SkillsListPage skillsListPage)
        {
            SkillsListPageDto skillsListPageDto = await _database.Table<SkillsListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder);
            if (skillsListPageDto == null)
            {
                skillsListPageDto = new SkillsListPageDto();
                skillsListPageDto.ProgenyId = progenyId;
                skillsListPageDto.AccessLevel = accessLevel;
                skillsListPageDto.PageNumber = pageNumber;
                skillsListPageDto.PageSize = pageSize;
                skillsListPageDto.SortOrder = sortOrder;
                skillsListPageDto.SkillsListPageString = JsonConvert.SerializeObject(skillsListPage);
                return await _database.InsertAsync(skillsListPageDto);
            }
            skillsListPageDto.SkillsListPageString = JsonConvert.SerializeObject(skillsListPage);
            return await _database.UpdateAsync(skillsListPageDto);
        }

        public async Task<VocabularyListPage> GetVocabularyListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder)
        {
            VocabularyListPageDto vocabularyListPageDto = await _database.Table<VocabularyListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder);
            VocabularyListPage vocabularyListPage = JsonConvert.DeserializeObject<VocabularyListPage>(vocabularyListPageDto.VocabularyListPageString);
            return vocabularyListPage;
        }
        public async Task<int> SaveVocabularyListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder, VocabularyListPage vocabularyListPage)
        {
            VocabularyListPageDto vocabularyListPageDto = await _database.Table<VocabularyListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder);
            if (vocabularyListPageDto == null)
            {
                vocabularyListPageDto = new VocabularyListPageDto();
                vocabularyListPageDto.ProgenyId = progenyId;
                vocabularyListPageDto.AccessLevel = accessLevel;
                vocabularyListPageDto.PageNumber = pageNumber;
                vocabularyListPageDto.PageSize = pageSize;
                vocabularyListPageDto.SortOrder = sortOrder;
                vocabularyListPageDto.VocabularyListPageString = JsonConvert.SerializeObject(vocabularyListPage);
                return await _database.InsertAsync(vocabularyListPageDto);
            }
            vocabularyListPageDto.VocabularyListPageString = JsonConvert.SerializeObject(vocabularyListPage);
            return await _database.UpdateAsync(vocabularyListPageDto);
        }

        public async Task<List<VocabularyItem>> GetVocabularyListAsync(int progenyId, int accessLevel)
        {
            VocabularyList vocabularyList = await _database.Table<VocabularyList>().FirstOrDefaultAsync(v => v.ProgenyId == progenyId && v.AccessLevel == accessLevel);
            List<VocabularyItem> vocabularyItems = JsonConvert.DeserializeObject<List<VocabularyItem>>(vocabularyList.VocabularyItemsString);
            return vocabularyItems;
        }
        public async Task<int> SaveVocabularyListAsync(int progenyId, int accessLevel, List<VocabularyItem> vocabularyItemsList)
        {
            VocabularyList vocabularyList = await _database.Table<VocabularyList>().FirstOrDefaultAsync(v => v.ProgenyId == progenyId && v.AccessLevel == accessLevel);
            if (vocabularyList == null)
            {
                vocabularyList = new VocabularyList();
                vocabularyList.ProgenyId = progenyId;
                vocabularyList.AccessLevel = accessLevel;
                vocabularyList.VocabularyItemsString = JsonConvert.SerializeObject(vocabularyItemsList);
                return await _database.InsertAsync(vocabularyList);
            }
            vocabularyList.VocabularyItemsString = JsonConvert.SerializeObject(vocabularyItemsList);
            return await _database.UpdateAsync(vocabularyList);
        }

        public async Task<List<Vaccination>> GetVaccinationsListAsync(int progenyId, int accessLevel)
        {
            VaccinationsList vaccinationsList = await _database.Table<VaccinationsList>().FirstOrDefaultAsync(v => v.ProgenyId == progenyId && v.AccessLevel == accessLevel);
            List<Vaccination> vaccinations = JsonConvert.DeserializeObject<List<Vaccination>>(vaccinationsList.VaccinationItemsString);
            return vaccinations;
        }
        public async Task<int> SaveVaccinationsListAsync(int progenyId, int accessLevel, List<Vaccination> vaccinationItemsList)
        {
            VaccinationsList vaccinationsList = await _database.Table<VaccinationsList>().FirstOrDefaultAsync(v => v.ProgenyId == progenyId && v.AccessLevel == accessLevel);
            if (vaccinationsList == null)
            {
                vaccinationsList = new VaccinationsList();
                vaccinationsList.ProgenyId = progenyId;
                vaccinationsList.AccessLevel = accessLevel;
                vaccinationsList.VaccinationItemsString = JsonConvert.SerializeObject(vaccinationItemsList);
                return await _database.InsertAsync(vaccinationsList);
            }
            vaccinationsList.VaccinationItemsString = JsonConvert.SerializeObject(vaccinationItemsList);
            return await _database.UpdateAsync(vaccinationsList);
        }

        public async Task<NotesListPage> GetNotesListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder)
        {
            NotesListPageDto notesListPageDto = await _database.Table<NotesListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder);
            NotesListPage notesListPage = JsonConvert.DeserializeObject<NotesListPage>(notesListPageDto.NotesListPageString);
            return notesListPage;
        }
        public async Task<int> SaveNotesListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder, NotesListPage notesListPage)
        {
            NotesListPageDto notesListPageDto = await _database.Table<NotesListPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder);
            if (notesListPageDto == null)
            {
                notesListPageDto = new NotesListPageDto();
                notesListPageDto.ProgenyId = progenyId;
                notesListPageDto.AccessLevel = accessLevel;
                notesListPageDto.PageNumber = pageNumber;
                notesListPageDto.PageSize = pageSize;
                notesListPageDto.SortOrder = sortOrder;
                notesListPageDto.NotesListPageString = JsonConvert.SerializeObject(notesListPage);
                return await _database.InsertAsync(notesListPageDto);
            }
            notesListPageDto.NotesListPageString = JsonConvert.SerializeObject(notesListPage);
            return await _database.UpdateAsync(notesListPageDto);
        }

        public async Task<LocationsListPage> GetLocationsListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder)
        {
            LocationsPageDto locationsPageDto = await _database.Table<LocationsPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder);
            LocationsListPage locationsListPage = JsonConvert.DeserializeObject<LocationsListPage>(locationsPageDto.LocationsPageString);
            return locationsListPage;
        }
        public async Task<int> SaveLocationsListPageAsync(int progenyId, int accessLevel, int pageNumber, int pageSize, int sortOrder, LocationsListPage locationsListPage)
        {
            LocationsPageDto locationsPageDto = await _database.Table<LocationsPageDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel && s.PageNumber == pageNumber && s.PageSize == pageSize && s.SortOrder == sortOrder);
            if (locationsPageDto == null)
            {
                locationsPageDto = new LocationsPageDto();
                locationsPageDto.ProgenyId = progenyId;
                locationsPageDto.AccessLevel = accessLevel;
                locationsPageDto.PageNumber = pageNumber;
                locationsPageDto.PageSize = pageSize;
                locationsPageDto.SortOrder = sortOrder;
                locationsPageDto.LocationsPageString = JsonConvert.SerializeObject(locationsListPage);
                return await _database.InsertAsync(locationsPageDto);
            }
            locationsPageDto.LocationsPageString = JsonConvert.SerializeObject(locationsListPage);
            return await _database.UpdateAsync(locationsPageDto);
        }

        public async Task<List<Location>> GeLocationsListAsync(int progenyId, int accessLevel)
        {
            LocationsList locationsList = await _database.Table<LocationsList>().FirstOrDefaultAsync(m => m.ProgenyId == progenyId && m.AccessLevel == accessLevel);
            List<Location> locations = JsonConvert.DeserializeObject<List<Location>>(locationsList.LocationItemsString);
            return locations;
        }
        public async Task<int> SaveLocationsListAsync(int progenyId, int accessLevel, List<Location> locationItemsList)
        {
            LocationsList locationsList = await _database.Table<LocationsList>().FirstOrDefaultAsync(m => m.ProgenyId == progenyId && m.AccessLevel == accessLevel);
            if (locationsList == null)
            {
                locationsList = new LocationsList();
                locationsList.ProgenyId = progenyId;
                locationsList.AccessLevel = accessLevel;
                locationsList.LocationItemsString = JsonConvert.SerializeObject(locationItemsList);
                return await _database.InsertAsync(locationsList);
            }
            locationsList.LocationItemsString = JsonConvert.SerializeObject(locationItemsList);
            return await _database.UpdateAsync(locationsList);
        }

        public async Task<List<Picture>> GetPicturesListAsync(int progenyId, int accessLevel)
        {
            PicturesList picturesList = await _database.Table<PicturesList>().FirstOrDefaultAsync(p => p.ProgenyId == progenyId && p.AccessLevel == accessLevel);
            List<Picture> pictures = JsonConvert.DeserializeObject<List<Picture>>(picturesList.PictureItemsString);
            return pictures;
        }
        public async Task<int> SavePicturesListAsync(int progenyId, int accessLevel, List<Picture> pictureItemsList)
        {
            PicturesList picturesList = await _database.Table<PicturesList>().FirstOrDefaultAsync(p => p.ProgenyId == progenyId && p.AccessLevel == accessLevel);
            if (picturesList == null)
            {
                picturesList = new PicturesList();
                picturesList.ProgenyId = progenyId;
                picturesList.AccessLevel = accessLevel;
                picturesList.PictureItemsString = JsonConvert.SerializeObject(pictureItemsList);
                return await _database.InsertAsync(picturesList);
            }
            picturesList.PictureItemsString = JsonConvert.SerializeObject(pictureItemsList);
            return await _database.UpdateAsync(picturesList);
        }

        public async Task<List<Sleep>> GetSleepDetailsAsync(int accessLevel, int sleepId, int sortOrder)
        {
            SleepDetails sleepList = await _database.Table<SleepDetails>().FirstOrDefaultAsync(s => s.AccessLevel == accessLevel && s.SleepId == sleepId && s.SortOrder == sortOrder);
            List<Sleep> sleepItems = JsonConvert.DeserializeObject<List<Sleep>>(sleepList.SleepItemsString);
            return sleepItems;
        }
        public async Task<int> SaveSleepDetailsAsync(int accessLevel, int sleepId, int sortOrder, List<Sleep> sleepItemsList)
        {
            SleepDetails sleepList = await _database.Table<SleepDetails>().FirstOrDefaultAsync(s => s.AccessLevel == accessLevel && s.SleepId == sleepId && s.SortOrder == sortOrder);
            if (sleepList == null)
            {
                sleepList = new SleepDetails();
                sleepList.AccessLevel = accessLevel;
                sleepList.SleepId = sleepId;
                sleepList.SortOrder = sortOrder;
                sleepList.SleepItemsString = JsonConvert.SerializeObject(sleepItemsList);
                return await _database.InsertAsync(sleepList);
            }
            sleepList.SleepItemsString = JsonConvert.SerializeObject(sleepItemsList);
            return await _database.UpdateAsync(sleepList);
        }

        public async Task<TimeLineItem> GetTimeLineItemByItemIdAsync(int itemId, int timeLineType)
        {
            TimeLineItemDto timeLineItemDto = await _database.Table<TimeLineItemDto>().FirstOrDefaultAsync(t => t.ItemId == itemId && t.ItemType == timeLineType);
            TimeLineItem timeLineItem = JsonConvert.DeserializeObject<TimeLineItem>(timeLineItemDto.TimeLineItemString);
            return timeLineItem;
        }
        public async Task<int> SaveTimeLineItemByItemIdAsync(int itemId, int itemType, TimeLineItem timeLineItem)
        {
            TimeLineItemDto timeLineItemDto = await _database.Table<TimeLineItemDto>().FirstOrDefaultAsync(t => t.ItemId == itemId && t.ItemType == timeLineItem.ItemType);
            if (timeLineItemDto == null)
            {
                timeLineItemDto = new TimeLineItemDto();
                timeLineItemDto.TimeLineId = timeLineItem.TimeLineId;
                timeLineItemDto.ItemId = itemId;
                timeLineItemDto.ItemType = itemType;
                timeLineItemDto.TimeLineItemString = JsonConvert.SerializeObject(timeLineItem);
                return await _database.InsertAsync(timeLineItemDto);
            }
            timeLineItemDto.TimeLineItemString = JsonConvert.SerializeObject(timeLineItem);
            return await _database.UpdateAsync(timeLineItemDto);
        }

        public async Task<List<string>> GetLocationAutoSuggestListAsync(int progenyId, int accessLevel)
        {
            LocationAutoSuggestList autoSuggestList = await _database.Table<LocationAutoSuggestList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel);
            List<string> autoSuggestItems = JsonConvert.DeserializeObject<List<string>>(autoSuggestList.AutoSuggestListString);
            return autoSuggestItems;
        }
        public async Task<int> SaveLocationAutoSuggestListAsync(int progenyId, int accessLevel, List<string> autoSuggestItemsList)
        {
            LocationAutoSuggestList autoSuggestList = await _database.Table<LocationAutoSuggestList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel);
            if (autoSuggestList == null)
            {
                autoSuggestList = new LocationAutoSuggestList();
                autoSuggestList.ProgenyId = progenyId;
                autoSuggestList.AccessLevel = accessLevel;
                autoSuggestList.AutoSuggestListString = JsonConvert.SerializeObject(autoSuggestItemsList);
                return await _database.InsertAsync(autoSuggestList);
            }
            autoSuggestList.AutoSuggestListString = JsonConvert.SerializeObject(autoSuggestItemsList);
            return await _database.UpdateAsync(autoSuggestList);
        }

        public async Task<List<string>> GetTagsAutoSuggestListAsync(int progenyId, int accessLevel)
        {
            TagsAutoSuggestList autoSuggestList = await _database.Table<TagsAutoSuggestList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel);
            List<string> autoSuggestItems = JsonConvert.DeserializeObject<List<string>>(autoSuggestList.AutoSuggestListString);
            return autoSuggestItems;
        }
        public async Task<int> SaveTagsAutoSuggestListAsync(int progenyId, int accessLevel, List<string> autoSuggestItemsList)
        {
            TagsAutoSuggestList autoSuggestList = await _database.Table<TagsAutoSuggestList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel);
            if (autoSuggestList == null)
            {
                autoSuggestList = new TagsAutoSuggestList();
                autoSuggestList.ProgenyId = progenyId;
                autoSuggestList.AccessLevel = accessLevel;
                autoSuggestList.AutoSuggestListString = JsonConvert.SerializeObject(autoSuggestItemsList);
                return await _database.InsertAsync(autoSuggestList);
            }
            autoSuggestList.AutoSuggestListString = JsonConvert.SerializeObject(autoSuggestItemsList);
            return await _database.UpdateAsync(autoSuggestList);
        }

        public async Task<List<string>> GetCategoryAutoSuggestListAsync(int progenyId, int accessLevel)
        {
            CategoryAutoSuggestList autoSuggestList = await _database.Table<CategoryAutoSuggestList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel);
            List<string> autoSuggestItems = JsonConvert.DeserializeObject<List<string>>(autoSuggestList.AutoSuggestListString);
            return autoSuggestItems;
        }
        public async Task<int> SaveCategoryAutoSuggestListAsync(int progenyId, int accessLevel, List<string> autoSuggestItemsList)
        {
            CategoryAutoSuggestList autoSuggestList = await _database.Table<CategoryAutoSuggestList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel);
            if (autoSuggestList == null)
            {
                autoSuggestList = new CategoryAutoSuggestList();
                autoSuggestList.ProgenyId = progenyId;
                autoSuggestList.AccessLevel = accessLevel;
                autoSuggestList.AutoSuggestListString = JsonConvert.SerializeObject(autoSuggestItemsList);
                return await _database.InsertAsync(autoSuggestList);
            }
            autoSuggestList.AutoSuggestListString = JsonConvert.SerializeObject(autoSuggestItemsList);
            return await _database.UpdateAsync(autoSuggestList);
        }

        public async Task<List<string>> GetContextAutoSuggestListAsync(int progenyId, int accessLevel)
        {
            ContextAutoSuggestList autoSuggestList = await _database.Table<ContextAutoSuggestList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel);
            List<string> autoSuggestItems = JsonConvert.DeserializeObject<List<string>>(autoSuggestList.AutoSuggestListString);
            return autoSuggestItems;
        }
        public async Task<int> SaveContextAutoSuggestListAsync(int progenyId, int accessLevel, List<string> autoSuggestItemsList)
        {
            ContextAutoSuggestList autoSuggestList = await _database.Table<ContextAutoSuggestList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel);
            if (autoSuggestList == null)
            {
                autoSuggestList = new ContextAutoSuggestList();
                autoSuggestList.ProgenyId = progenyId;
                autoSuggestList.AccessLevel = accessLevel;
                autoSuggestList.AutoSuggestListString = JsonConvert.SerializeObject(autoSuggestItemsList);
                return await _database.InsertAsync(autoSuggestList);
            }
            autoSuggestList.AutoSuggestListString = JsonConvert.SerializeObject(autoSuggestItemsList);
            return await _database.UpdateAsync(autoSuggestList);
        }

        public async Task<UserInfo> GetUserInfoAsync(string email)
        {
            UserInfoDto uiDto = await _database.Table<UserInfoDto>().FirstOrDefaultAsync(u => u.Email.ToUpper() == email.ToUpper());
            if (uiDto == null)
            {
                return OfflineDefaultData.DefaultUserInfo;
            }
            UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(uiDto.UserInfoString);
            return ui;
        }
        public async Task<int> SaveUserInfoAsync(UserInfo userinfo)
        {
            UserInfoDto uiDto = await _database.Table<UserInfoDto>().FirstOrDefaultAsync(u => u.Email.ToUpper() == userinfo.UserEmail.ToUpper());
            if (uiDto == null)
            {
                uiDto = new UserInfoDto();
                uiDto.Email = userinfo.UserEmail;
                uiDto.UserInfoString = JsonConvert.SerializeObject(userinfo);
                return await _database.InsertAsync(uiDto);
            }
            uiDto.UserInfoString = JsonConvert.SerializeObject(userinfo);
            return await _database.UpdateAsync(uiDto);
        }

        public async Task<string> GetUserPictureAsync(string pictureId)
        {
            UserPictureDto userPictureDto = await _database.Table<UserPictureDto>().FirstOrDefaultAsync(u => u.PictureId == pictureId);
            return userPictureDto.PictureString;
            
        }
        public async Task<int> SaveUserPictureAsync(string pictureId, string pictureString)
        {
            UserPictureDto userPictureDto = await _database.Table<UserPictureDto>().FirstOrDefaultAsync(u => u.PictureId == pictureId);
            if (userPictureDto == null)
            {
                userPictureDto = new UserPictureDto();
                userPictureDto.PictureId = pictureId;
                userPictureDto.PictureString = pictureString;
                return await _database.InsertAsync(userPictureDto);
            }
            userPictureDto.PictureString = pictureString;
            return await _database.UpdateAsync(userPictureDto);
        }
    }
}

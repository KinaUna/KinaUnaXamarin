using System;
using System.Collections.Generic;
using System.Text;
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
            Progeny prog = JsonConvert.DeserializeObject<Progeny>(progDto.ProgenyString);
            return prog;
        }
        public async Task<int> SaveProgenyAsync(Progeny progeny)
        {
            ProgenyDto progDto = new ProgenyDto();
            progDto.ProgenyId = progeny.Id;
            progDto.ProgenyString = JsonConvert.SerializeObject(progeny);
            return await _database.InsertAsync(progDto);
        }

        public async Task<Comment> GetCommentAsync(int commentId)
        {
            CommentDto cmntDto = await _database.Table<CommentDto>().FirstOrDefaultAsync(c => c.CommentId == commentId);
            Comment comment = JsonConvert.DeserializeObject<Comment>(cmntDto.CommentString);
            return comment;
        }
        public async Task<int> SaveCommentAsync(Comment comment)
        {
            CommentDto cmntDto = new CommentDto();
            cmntDto.CommentId = comment.CommentId;
            cmntDto.CommentString = JsonConvert.SerializeObject(comment);
            return await _database.InsertAsync(cmntDto);
        }

        public async Task<string> GetCommentThreadAsync(int commentThread)
        {
            CommentsList comList = await _database.Table<CommentsList>().FirstOrDefaultAsync(c => c.CommentThread == commentThread);
            return comList?.CommentsListString ?? "";
        }

        public async Task<int> SaveCommentThreadAsync(int commentThread, List<Comment> commentsList)
        {
            CommentsList comList = new CommentsList();
            comList.CommentThread = commentThread;
            comList.CommentsListString = JsonConvert.SerializeObject(commentsList);
            return await _database.InsertAsync(comList);
        }

        public async Task<string> GetProgenyListAsync(string userEmail)
        {
            ProgenyList progList = await _database.Table<ProgenyList>().FirstOrDefaultAsync(p => p.UserEmail.ToUpper() == userEmail.ToUpper());
            return progList?.ProgenyListString ?? "";
        }

        public async Task<int> SaveProgenyListAsync(string userEmail, List<Progeny> progenyList)
        {
            ProgenyList progList = new ProgenyList();
            progList.UserEmail = userEmail;
            progList.ProgenyListString = JsonConvert.SerializeObject(progenyList);
            return await _database.InsertAsync(progList);
        }

        public async Task<UserAccess> GetUserAccessAsync(string email, int progenyId)
        {
            UserAccessDto uaDto = await _database.Table<UserAccessDto>().FirstOrDefaultAsync(u => u.Email.ToUpper() == email.ToUpper() && u.ProgenyId == progenyId);
            UserAccess ua = JsonConvert.DeserializeObject<UserAccess>(uaDto.UserAccessString);
            return ua;
        }
        public async Task<int> SaveUserAccessAsync(UserAccess userAccess)
        {
            UserAccessDto uaDto = new UserAccessDto();
            uaDto.Email = userAccess.UserId;
            uaDto.ProgenyId = userAccess.ProgenyId;
            uaDto.UserAccessString = JsonConvert.SerializeObject(userAccess);
            return await _database.InsertAsync(uaDto);
        }

        public async Task<string> GetUpcomingEventsAsync(int progenyId, int accessLevel)
        {
            UpcomingEvents eventsList = await _database.Table<UpcomingEvents>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.AccessLevel == accessLevel);
            return eventsList?.EventsListString ?? "";
        }

        public async Task<int> SaveUpcomingEventsAsync(int progenyId, int accessLevel, List<CalendarItem> eventsList)
        {
            UpcomingEvents upcomingList = new UpcomingEvents();
            upcomingList.ProgenyId = progenyId;
            upcomingList.AccessLevel = accessLevel;
            upcomingList.EventsListString = JsonConvert.SerializeObject(eventsList);
            return await _database.InsertAsync(upcomingList);
        }

        public async Task<string> GetTimeLineListAsync(int progenyId, int accessLevel, int count, int start)
        {
            TimeLineList timeLineItems = await _database.Table<TimeLineList>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.AccessLevel == accessLevel && e.Count == count && e.Start == start);
            return timeLineItems?.TimeLineItemsString ?? "";
        }

        public async Task<int> SaveTimeLineListAsync(int progenyId, int accessLevel, int count, int start, List<TimeLineItem> timeLineItemsList)
        {
            TimeLineList timeLineList = new TimeLineList();
            timeLineList.ProgenyId = progenyId;
            timeLineList.AccessLevel = accessLevel;
            timeLineList.Count = count;
            timeLineList.Start = start;
            timeLineList.TimeLineItemsString = JsonConvert.SerializeObject(timeLineItemsList);
            return await _database.InsertAsync(timeLineList);
        }

        public async Task<string> GetTimeLineLatestAsync(int progenyId, int accessLevel)
        {
            TimeLineLatest timeLineLatestItems = await _database.Table<TimeLineLatest>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.AccessLevel == accessLevel);
            return timeLineLatestItems?.TimeLineItemsString ?? "";
        }

        public async Task<int> SaveTimeLineLatestAsync(int progenyId, int accessLevel, List<TimeLineItem> timeLineItemsList)
        {
            TimeLineLatest timeLineList = new TimeLineLatest();
            timeLineList.ProgenyId = progenyId;
            timeLineList.AccessLevel = accessLevel;
            timeLineList.TimeLineItemsString = JsonConvert.SerializeObject(timeLineItemsList);
            return await _database.InsertAsync(timeLineList);
        }

        public async Task<Picture> GetPictureAsync(int pictureId)
        {
            PictureDto pictureDto = await _database.Table<PictureDto>().FirstOrDefaultAsync(p => p.PictureId == pictureId);
            Picture picture = JsonConvert.DeserializeObject<Picture>(pictureDto.PictureString);
            return picture;
        }
        public async Task<int> SavePictureAsync(Picture picture)
        {
            PictureDto pictureDto = new PictureDto();
            pictureDto.PictureId = picture.PictureId;
            pictureDto.PictureString = JsonConvert.SerializeObject(picture);
            return await _database.InsertAsync(pictureDto);
        }

        public async Task<string> GetPicturePageListAsync(int progenyId, int pageNumber, int pageSize, int sortBy, string tagFilter)
        {
            PicturePageList picturePageList = await _database.Table<PicturePageList>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.PageNumber == pageNumber && e.PageSize == pageSize && e.SortBy == sortBy && e.TagFilter.ToUpper() == tagFilter.ToUpper());
            return picturePageList?.PictureItemsString ?? "";
        }

        public async Task<int> SavePicturePageListAsync(int progenyId, int pageNumber, int pageSize, int sortBy, string tagFilter, PicturePage picturePage)
        {
            PicturePageList picturePageList = new PicturePageList();
            picturePageList.ProgenyId = progenyId;
            picturePageList.PageNumber = pageNumber;
            picturePageList.PageSize = pageSize;
            picturePageList.SortBy = sortBy;
            picturePageList.TagFilter = tagFilter;
            picturePageList.PictureItemsString = JsonConvert.SerializeObject(picturePage);
            return await _database.InsertAsync(picturePageList);
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
            PictureViewModelDto pictureViewModelDto = new PictureViewModelDto();
            pictureViewModelDto.PictureId = pictureViewModel.PictureId;
            pictureViewModelDto.PictureViewModelString = JsonConvert.SerializeObject(pictureViewModel);
            return await _database.InsertAsync(pictureViewModelDto);
        }

        public async Task<Video> GetVideoAsync(int videoId)
        {
            VideoDto videoDto = await _database.Table<VideoDto>().FirstOrDefaultAsync(v => v.VideoId == videoId);
            Video video = JsonConvert.DeserializeObject<Video>(videoDto.VideoString);
            return video;
        }
        public async Task<int> SaveVideoAsync(Video video)
        {
            VideoDto videoDto = new VideoDto();
            videoDto.VideoId = video.VideoId;
            videoDto.VideoString = JsonConvert.SerializeObject(video);
            return await _database.InsertAsync(videoDto);
        }

        public async Task<string> GetVideoPageListAsync(int progenyId, int pageNumber, int pageSize, int sortBy, string tagFilter)
        {
            VideoPageList videoPageList = await _database.Table<VideoPageList>().FirstOrDefaultAsync(e => e.ProgenyId == progenyId && e.PageNumber == pageNumber && e.PageSize == pageSize && e.SortBy == sortBy && e.TagFilter.ToUpper() == tagFilter.ToUpper());
            return videoPageList?.VideoItemsString ?? "";
        }

        public async Task<int> SaveVideoPageListAsync(int progenyId, int pageNumber, int pageSize, int sortBy, string tagFilter, VideoPage videoPage)
        {
            VideoPageList videoPageList = new VideoPageList();
            videoPageList.ProgenyId = progenyId;
            videoPageList.PageNumber = pageNumber;
            videoPageList.PageSize = pageSize;
            videoPageList.SortBy = sortBy;
            videoPageList.TagFilter = tagFilter;
            videoPageList.VideoItemsString = JsonConvert.SerializeObject(videoPage);
            return await _database.InsertAsync(videoPageList);
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
            VideoViewModelDto videoViewModelDto = new VideoViewModelDto();
            videoViewModelDto.VideoId = videoViewModelDto.VideoId;
            videoViewModelDto.VideoViewModelString = JsonConvert.SerializeObject(videoViewModel);
            return await _database.InsertAsync(videoViewModelDto);
        }

        public async Task<CalendarItem> GetCalendarItemAsync(int eventId)
        {
            EventDto evtDto = await _database.Table<EventDto>().FirstOrDefaultAsync(c => c.EventId == eventId);
            CalendarItem calendarItem = JsonConvert.DeserializeObject<CalendarItem>(evtDto.EventString);
            return calendarItem;
        }
        public async Task<int> SaveCalendarItemAsync(CalendarItem calendarItem)
        {
            EventDto eventDto = new EventDto();
            eventDto.EventId = calendarItem.EventId;
            eventDto.EventString = JsonConvert.SerializeObject(calendarItem);
            return await _database.InsertAsync(eventDto);
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
            CalendarList calList = new CalendarList();
            calList.ProgenyId = progenyId;
            calList.AccessLevel = accessLevel;
            calList.CalendarListString = JsonConvert.SerializeObject(calendarItems);
            return await _database.InsertAsync(calList);
        }

        public async Task<Location> GetLocationAsync(int locationId)
        {
            LocationDto locDto = await _database.Table<LocationDto>().FirstOrDefaultAsync(c => c.LocationId == locationId);
            Location location = JsonConvert.DeserializeObject<Location>(locDto.LocationString);
            return location;
        }
        public async Task<int> SaveLocationAsync(Location location)
        {
            LocationDto locDto = new LocationDto();
            locDto.LocationId = location.LocationId;
            locDto.LocationString = JsonConvert.SerializeObject(location);
            return await _database.InsertAsync(locDto);
        }

        public async Task<VocabularyItem> GetVocabularyItemAsync(int wordId)
        {
            WordDto wordDto = await _database.Table<WordDto>().FirstOrDefaultAsync(w => w.WordId == wordId);
            VocabularyItem vocabularyItem = JsonConvert.DeserializeObject<VocabularyItem>(wordDto.WordString);
            return vocabularyItem;
        }
        public async Task<int> SaveVocabularyItemAsync(VocabularyItem vocabularyItem)
        {
            WordDto wordDto = new WordDto();
            wordDto.WordId = vocabularyItem.WordId;
            wordDto.WordString = JsonConvert.SerializeObject(vocabularyItem);
            return await _database.InsertAsync(wordDto);
        }

        public async Task<Skill> GetSkillAsync(int skillId)
        {
            SkillDto skillDto = await _database.Table<SkillDto>().FirstOrDefaultAsync(s => s.SkillId == skillId);
            Skill skillItem = JsonConvert.DeserializeObject<Skill>(skillDto.SkillString);
            return skillItem;
        }
        public async Task<int> SaveSkillAsync(Skill skill)
        {
            SkillDto skillDto = new SkillDto();
            skillDto.SkillId = skill.SkillId;
            skillDto.SkillString = JsonConvert.SerializeObject(skill);
            return await _database.InsertAsync(skillDto);
        }

        public async Task<Friend> GetFriendAsync(int friendId)
        {
            FriendDto friendDto = await _database.Table<FriendDto>().FirstOrDefaultAsync(f => f.FriendId == friendId);
            Friend friendItem = JsonConvert.DeserializeObject<Friend>(friendDto.FriendString);
            return friendItem;
        }
        public async Task<int> SaveFriendAsync(Friend friend)
        {
            FriendDto friendDto = new FriendDto();
            friendDto.FriendId = friend.FriendId;
            friendDto.FriendString = JsonConvert.SerializeObject(friend);
            return await _database.InsertAsync(friendDto);
        }

        public async Task<Measurement> GetMeasurementAsync(int measurementId)
        {
            MeasurementDto measurementDto = await _database.Table<MeasurementDto>().FirstOrDefaultAsync(m => m.MeasurementId == measurementId);
            Measurement measurementItem = JsonConvert.DeserializeObject<Measurement>(measurementDto.MeasurementString);
            return measurementItem;
        }
        public async Task<int> SaveMeasurementAsync(Measurement measurement)
        {
            MeasurementDto measurementDto = new MeasurementDto();
            measurementDto.MeasurementId = measurement.MeasurementId;
            measurementDto.MeasurementString = JsonConvert.SerializeObject(measurement);
            return await _database.InsertAsync(measurementDto);
        }

        public async Task<Sleep> GetSleepAsync(int sleepId)
        {
            SleepDto sleepDto = await _database.Table<SleepDto>().FirstOrDefaultAsync(s => s.SleepId == sleepId);
            Sleep sleepItem = JsonConvert.DeserializeObject<Sleep>(sleepDto.SleepString);
            return sleepItem;
        }
        public async Task<int> SaveSleepAsync(Sleep sleep)
        {
            SleepDto sleepDto = new SleepDto();
            sleepDto.SleepId = sleep.SleepId;
            sleepDto.SleepString = JsonConvert.SerializeObject(sleep);
            return await _database.InsertAsync(sleepDto);
        }

        public async Task<List<Sleep>> GetSleepListAsync(int progenyId, int accessLevel)
        {
            SleepList sleepList = await _database.Table<SleepList>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel);
            List<Sleep> sleepItems = JsonConvert.DeserializeObject<List<Sleep>>(sleepList.SleepItemsString);
            return sleepItems;
        }
        public async Task<int> SaveSleepListAsync(int progenyId, int accessLevel, List<Sleep> sleepItemsList)
        {
            SleepList sleepList = new SleepList();
            sleepList.ProgenyId = progenyId;
            sleepList.AccessLevel = accessLevel;
            sleepList.SleepItemsString = JsonConvert.SerializeObject(sleepItemsList);
            return await _database.InsertAsync(sleepList);
        }

        public async Task<SleepStatsModel> GetSleepStatsModelAsync(int progenyId, int accessLevel)
        {
            SleepStatsModelDto sleepStatsModelDto = await _database.Table<SleepStatsModelDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel);
            SleepStatsModel sleepStats = JsonConvert.DeserializeObject<SleepStatsModel>(sleepStatsModelDto.SleepStatsString);
            return sleepStats;
        }
        public async Task<int> SaveSleepStatsModelAsync(int progenyId, int accessLevel, SleepStatsModel sleepStats)
        {
            SleepStatsModelDto sleepStatsModelDto = new SleepStatsModelDto();
            sleepStatsModelDto.ProgenyId = progenyId;
            sleepStatsModelDto.AccessLevel = accessLevel;
            sleepStatsModelDto.SleepStatsString = JsonConvert.SerializeObject(sleepStats);
            return await _database.InsertAsync(sleepStatsModelDto);
        }

        public async Task<List<Sleep>> GetSleepChartAsync(int progenyId, int accessLevel)
        {
            SleepChartDto sleepList = await _database.Table<SleepChartDto>().FirstOrDefaultAsync(s => s.ProgenyId == progenyId && s.AccessLevel == accessLevel);
            List<Sleep> sleepItems = JsonConvert.DeserializeObject<List<Sleep>>(sleepList.SleepChartString);
            return sleepItems;
        }
        public async Task<int> SaveSleepChartAsync(int progenyId, int accessLevel, List<Sleep> sleepItemsList)
        {
            SleepChartDto sleepChart = new SleepChartDto();
            sleepChart.ProgenyId = progenyId;
            sleepChart.AccessLevel = accessLevel;
            sleepChart.SleepChartString = JsonConvert.SerializeObject(sleepItemsList);
            return await _database.InsertAsync(sleepChart);
        }

        public async Task<Note> GetNoteAsync(int noteId)
        {
            NoteDto noteDto = await _database.Table<NoteDto>().FirstOrDefaultAsync(n => n.NoteId == noteId);
            Note noteItem = JsonConvert.DeserializeObject<Note>(noteDto.NoteString);
            return noteItem;
        }
        public async Task<int> SaveNoteAsync(Note note)
        {
            NoteDto noteDto = new NoteDto();
            noteDto.NoteId = note.NoteId;
            noteDto.NoteString = JsonConvert.SerializeObject(note);
            return await _database.InsertAsync(noteDto);
        }

        public async Task<Contact> GetContactAsync(int contactId)
        {
            ContactDto contactDto = await _database.Table<ContactDto>().FirstOrDefaultAsync(c => c.ContactId == contactId);
            Contact contactItem = JsonConvert.DeserializeObject<Contact>(contactDto.ContactString);
            return contactItem;
        }
        public async Task<int> SaveContactAsync(Contact contact)
        {
            ContactDto contactDto = new ContactDto();
            contactDto.ContactId = contact.ContactId;
            contactDto.ContactString = JsonConvert.SerializeObject(contact);
            return await _database.InsertAsync(contactDto);
        }

        public async Task<List<Contact>> GetContactListAsync(int progenyId, int accessLevel)
        {
            ContactList contactList = await _database.Table<ContactList>().FirstOrDefaultAsync(c => c.ProgenyId == progenyId && c.AccessLevel == accessLevel);
            List<Contact> contactItems = JsonConvert.DeserializeObject<List<Contact>>(contactList.ContactItemsString);
            return contactItems;
        }
        public async Task<int> SaveContactListAsync(int progenyId, int accessLevel, List<Contact> contactItemsList)
        {
            ContactList contactList = new ContactList();
            contactList.ProgenyId = progenyId;
            contactList.AccessLevel = accessLevel;
            contactList.ContactItemsString = JsonConvert.SerializeObject(contactItemsList);
            return await _database.InsertAsync(contactList);
        }

        public async Task<Vaccination> GetVaccinationAsync(int vaccinationId)
        {
            VaccinationDto vacDto = await _database.Table<VaccinationDto>().FirstOrDefaultAsync(s => s.VaccinationId == vaccinationId);
            Vaccination vacItem = JsonConvert.DeserializeObject<Vaccination>(vacDto.VaccinationString);
            return vacItem;
        }
        public async Task<int> SaveVaccinationAsync(Vaccination vaccination)
        {
            VaccinationDto vaccinationDto = new VaccinationDto();
            vaccinationDto.VaccinationId = vaccination.VaccinationId;
            vaccinationDto.VaccinationString = JsonConvert.SerializeObject(vaccination);
            return await _database.InsertAsync(vaccinationDto);
        }
    }
}

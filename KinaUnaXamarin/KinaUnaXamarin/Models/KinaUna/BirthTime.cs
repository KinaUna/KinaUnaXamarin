using System;
using System.Globalization;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class BirthTime
    {
        private DateTime _birthDay;
        private readonly TimeZoneInfo _birthTimeZone;
        private readonly DateTime _currentDateTime;

        public string CurrentTime => TimeZoneInfo.ConvertTimeFromUtc(_currentDateTime, _birthTimeZone).ToString("dd MMMM yyyy HH:mm:ss");

        public BirthTime(DateTime bday, TimeZoneInfo bdayTz)
        {
            _birthTimeZone = bdayTz;
            _birthDay = TimeZoneInfo.ConvertTimeToUtc(bday, bdayTz);

            _currentDateTime = DateTime.UtcNow; // TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
        }


        public string CalcYears()
        {

            TimeSpan age = _currentDateTime - _birthDay;
            double ageYears = age.TotalSeconds / (365.0 * 24.0 * 60.0 * 60.0);

            return ageYears.ToString("F6");
        }

        public string CalcMonths()
        {
            int ageMonths = GetTotalMonthsFrom(_currentDateTime, _birthDay);

            return ageMonths.ToString();
        }

        public string[] CalcWeeks()
        {
            int ageWeeks = (DateTime.Today.ToUniversalTime() - new DateTime(_birthDay.Year, _birthDay.Month, _birthDay.Day)).Days / 7;
            int ageWeeksDays = (DateTime.Today.ToUniversalTime() - new DateTime(_birthDay.Year, _birthDay.Month, _birthDay.Day)).Days % 7;
            string[] ageWeeksResult = new string[2];
            ageWeeksResult[0] = ageWeeks.ToString();
            ageWeeksResult[1] = ageWeeksDays.ToString();
            return ageWeeksResult;
        }

        public string CalcDays()
        {
            double ageDays = (_currentDateTime - _birthDay).TotalDays;
            return ageDays.ToString("F2");
        }

        public string CalcHours()
        {
            double ageHours = (_currentDateTime - _birthDay).TotalHours;
            return ageHours.ToString("F4");
        }

        public string CalcMinutes()
        {
            double ageMinutes = (_currentDateTime - _birthDay).TotalMinutes;
            return ageMinutes.ToString("F2");
        }

        public string CalcNextBirthday()
        {
            int nextBday;

            if (DateTime.Today.ToUniversalTime() < new DateTime(_currentDateTime.Year, _birthDay.Month, _birthDay.Day))
            {
                nextBday = (new DateTime(_currentDateTime.Year, _birthDay.Month, _birthDay.Day, 0, 0, 0, DateTimeKind.Utc) -
                            new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0, DateTimeKind.Utc)).Days;
            }
            else
            {
                nextBday = (new DateTime(_currentDateTime.Year + 1, _birthDay.Month, _birthDay.Day, 0, 0, 0, DateTimeKind.Utc) -
                            new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0, DateTimeKind.Utc)).Days;
            }

            return nextBday.ToString();
        }

        public string[] CalcMileStoneWeeks()
        {
            double ageWeeks = Math.Floor((_currentDateTime - _birthDay).TotalDays / 7);
            double milestone = Math.Pow(10, Math.Ceiling(Math.Log10(ageWeeks)));
            DateTime milestoneDate = _birthDay + TimeSpan.FromDays(milestone * 7);
            string[] cmsw = new string[2];
            cmsw[0] = milestone.ToString(CultureInfo.InvariantCulture);
            cmsw[1] = milestoneDate.ToString("dddd, dd MMMM yyyy");
            return cmsw;
        }

        public string[] CalcMileStoneDays()
        {
            double ageDays = (_currentDateTime - _birthDay).TotalDays;
            double milestone = Math.Pow(10, Math.Ceiling(Math.Log10(ageDays)));
            DateTime milestoneDate = _birthDay + TimeSpan.FromDays(milestone);
            string[] cmsw = new string[2];
            cmsw[0] = milestone.ToString(CultureInfo.InvariantCulture);
            cmsw[1] = milestoneDate.ToString("dddd, dd MMMM yyyy");
            return cmsw;
        }

        public string[] CalcMileStoneHours()
        {
            double ageHours = (_currentDateTime - _birthDay).TotalHours;
            double milestone = Math.Pow(10, Math.Ceiling(Math.Log10(ageHours)));
            DateTime milestoneDate = _birthDay + TimeSpan.FromHours(milestone);
            string[] cmsw = new string[2];
            cmsw[0] = milestone.ToString(CultureInfo.InvariantCulture);
            cmsw[1] = milestoneDate.ToString("dddd, dd MMMM yyyy HH:mm");
            return cmsw;

        }

        public string[] CalcMileStoneMinutes()
        {
            double ageMinutes = (_currentDateTime - _birthDay).TotalMinutes;
            double milestone = Math.Pow(10, Math.Ceiling(Math.Log10(ageMinutes)));
            DateTime milestoneDate = _birthDay + TimeSpan.FromMinutes(milestone);
            string[] cmsw = new string[2];
            cmsw[0] = milestone.ToString(CultureInfo.InvariantCulture);
            cmsw[1] = milestoneDate.ToString("dddd, dd MMMM yyyy HH:mm");
            return cmsw;

        }

        public static int GetTotalMonthsFrom(DateTime dt1, DateTime dt2)
        {
            DateTime earlyDate = (dt1 > dt2) ? dt2.Date : dt1.Date;
            DateTime lateDate = (dt1 > dt2) ? dt1.Date : dt2.Date;

            // Start with 1 month's difference and keep incrementing
            // until we overshoot the late date
            int monthsDiff = 1;
            while (earlyDate.AddMonths(monthsDiff) <= lateDate)
            {
                monthsDiff++;
            }

            return monthsDiff - 1;
        }
    }
}

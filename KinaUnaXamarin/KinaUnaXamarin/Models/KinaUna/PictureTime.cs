using System;
using TimeZoneConverter;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class PictureTime
    {
        private readonly DateTime _pictureUtcTime;
        private readonly DateTime _birthDayUtc;

        public string PictureDateTime => _pictureUtcTime.ToString("dd MMMM yyyy HH:mm");

        public PictureTime(DateTime bday, DateTime? pictureTaken, TimeZoneInfo bdayTz)
        {
            _birthDayUtc = TimeZoneInfo.ConvertTimeToUtc(bday, bdayTz);
            
            if (pictureTaken != null)
            {
                _pictureUtcTime = pictureTaken.Value;  //TimeZoneInfo.ConvertTimeToUtc((DateTime)pictureTaken, bdayTz);
            }
            else
            {
                _pictureUtcTime = TimeZoneInfo.ConvertTimeToUtc(bday, bdayTz);
            }
        }


        public string CalcYears()
        {

            TimeSpan age = _pictureUtcTime - _birthDayUtc;
            double ageYears = age.TotalSeconds / (365.0 * 24.0 * 60.0 * 60.0);

            return ageYears.ToString("F6");
        }

        public string CalcMonths()
        {
            int ageMonths = GetTotalMonthsFrom(_pictureUtcTime, _birthDayUtc);

            return ageMonths.ToString();
        }

        public string[] CalcWeeks()
        {
            int ageWeeks = (new DateTime(_pictureUtcTime.Year, _pictureUtcTime.Month, _pictureUtcTime.Day) - new DateTime(_birthDayUtc.Year, _birthDayUtc.Month, _birthDayUtc.Day)).Days / 7;
            int ageWeeksDays = (new DateTime(_pictureUtcTime.Year, _pictureUtcTime.Month, _pictureUtcTime.Day) - new DateTime(_birthDayUtc.Year, _birthDayUtc.Month, _birthDayUtc.Day)).Days % 7;
            string[] ageWeeksResult = new string[2];
            ageWeeksResult[0] = ageWeeks.ToString();
            ageWeeksResult[1] = ageWeeksDays.ToString();
            return ageWeeksResult;
        }

        public string CalcDays()
        {
            double ageDays = (_pictureUtcTime - _birthDayUtc).TotalDays;
            return ageDays.ToString("F2");
        }

        public string CalcHours()
        {
            double ageHours = (_pictureUtcTime - _birthDayUtc).TotalHours;
            return ageHours.ToString("F4");
        }

        public string CalcMinutes()
        {
            double ageMinutes = (_pictureUtcTime - _birthDayUtc).TotalMinutes;
            return ageMinutes.ToString("F2");
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

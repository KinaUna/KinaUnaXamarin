using System;

namespace KinaUnaXamarin.Models.KinaUna
{
    public class SleepStatsModel
    {
        public TimeSpan SleepTotal { get; set; }
        public TimeSpan TotalAverage { get; set; }
        public TimeSpan SleepLastMonth { get; set; }
        public TimeSpan LastMonthAverage { get; set; }
        public TimeSpan SleepLastYear { get; set; }
        public TimeSpan LastYearAverage { get; set; }
    }
}

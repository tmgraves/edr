using EDR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDR.Utilities
{
    public static class ApplicationUtility
    {
        public static DateTime GetNextDate(DateTime start, Frequency frequency, int interval, DayOfWeek day)
        {
            DateTime date = start;
            if (start < DateTime.Today)
            {
                switch (frequency)
                {
                    case Frequency.Daily:
                        date = DateTime.Today.AddDays(1);
                        break;
                    case Frequency.Weekly:
                        TimeSpan diff = DateTime.Today - start;
                        int totalIntervals = Convert.ToInt32(diff.TotalDays / 7 * interval) + 1;
                        date = start.AddDays(7 * interval * totalIntervals);
                        break;
                    case Frequency.Monthly:
                        var nth = Convert.ToInt32(start.Day / 7);
                        DateTime month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                        date = month.AddDays((((int)day - (int)month.DayOfWeek + 7) % 7)).AddDays(nth * 7);
                        break;
                    case Frequency.Yearly:
                        var yearNth = Convert.ToInt32(start.Day / 7);
                        DateTime yearMonth = new DateTime(start.AddYears(interval).Year, start.Month, 1);
                        date = yearMonth.AddDays((((int)day - (int)yearMonth.DayOfWeek + 7) % 7)).AddDays(yearNth * 7);
                        break;
                }
            }

            return (date);
        }

        public static int CalculateTime(int days, int hours, int minutes)
        {
            return ((days * 24 * 60) + (hours * 60) + minutes);
        }
    }
}
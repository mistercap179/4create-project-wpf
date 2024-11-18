using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient
{
    public static class WorkdayHelper
    {
        public static int CountWorkdays(DateTime startDate, DateTime endDate)
        {
            int workdays = 0;
            DateTime currentDate = startDate;

            while (currentDate <= endDate)
            {
                if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    workdays++;
                }
                currentDate = currentDate.AddDays(1);
            }

            return workdays;
        }
    }

}

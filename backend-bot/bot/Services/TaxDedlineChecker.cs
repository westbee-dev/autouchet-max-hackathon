using System;

namespace Autouchet_Bot.Services
{
    public static class TaxDeadlineChecker
    {
        public static DateTime GetTaxDeadline(DateTime date)
        {
            DateTime deadline = new DateTime(date.Year, date.Month, 28);

            if (deadline.DayOfWeek == DayOfWeek.Saturday)
            {
                deadline = deadline.AddDays(2);
            }
            else if (deadline.DayOfWeek == DayOfWeek.Sunday)
            {
                deadline = deadline.AddDays(1);
            }

            return deadline;
        }

        public static bool IsInTaxNotificationPeriod(DateTime date)
        {
            DateTime startDate = new DateTime(date.Year, date.Month, 25);
            DateTime deadlineDate = GetTaxDeadline(date);

            DateTime currentDate = date.Date;

            return currentDate >= startDate && currentDate <= deadlineDate;
        }
    }
}
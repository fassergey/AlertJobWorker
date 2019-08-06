using System;

namespace AlertJobWorker
{
    public class AlertSchedule
    {
        public int AlertId { get; set; }
        public string AlertName { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastCalled { get; set; }
        public int LastIntervalUsed { get; set; }
        public bool IsOneTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan TimeToCall { get; set; }
        public DateTime? NextExecutingDate { get; set; }
        public bool IsEnabled { get; set; }
        public int UserId { get; set; }
        public int ReportId { get; set; }
        public string ReportUrl { get; set; }
        public string ReportFilters { get; set; }

        public AlertInterval[] Intervals { get; set; }
    }
}

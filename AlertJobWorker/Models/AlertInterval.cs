namespace AlertJobWorker
{
    public class AlertInterval
    {
        public int IntervalId { get; set; }
        public int DurationDays { get; set; }
        public int DurationMonths { get; set; }
        public int AlertId { get; set; }
        public int NextIntervalId { get; set; }
        public int? WeekNumber { get; set; }
        public int? DayOfWeek { get; set; }
    }
}

using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Dapper;
using System.Linq;

namespace AlertJobWorker
{
    public static class AlertsRepository
    {
        private static string connectionStr = ConfigurationManager.ConnectionStrings["WebCrawler3Security"].ConnectionString;

        public static IEnumerable<AlertSchedule> GetTodayNotificationSchedules()
        {
            IEnumerable<AlertSchedule> alerts = new AlertSchedule[] { };

            using (var conn = new SqlConnection(connectionStr))
            {
                alerts = conn.Query<AlertSchedule>("[Alert].[GetTodayNotifications]", commandType: System.Data.CommandType.StoredProcedure);
            }


            foreach (var alert in alerts)
            {
                alert.Intervals = GetNotificationIntervals(alert.AlertId).ToArray();
            }

            return alerts;
        }

        private static IEnumerable<AlertInterval> GetNotificationIntervals(int alertId)
        {
            IEnumerable<AlertInterval> intervals = new AlertInterval[] { };

            using (var conn = new SqlConnection(connectionStr))
            {
                intervals = conn.Query<AlertInterval>("[Alert].[GetNotificationIntervals]", new { @alertId = alertId }, commandType: System.Data.CommandType.StoredProcedure);
            }

            return intervals;
        }

        public static void UpdateNotificationSchedule(AlertSchedule alert)
        {
            using (var conn = new SqlConnection(connectionStr))
            {
                conn.Execute("[Alert].[UpdateNotificationSchedule]", 
                    new {
                          @NextExecutingDate = alert.NextExecutingDate,
                          @lastUsedInterval = alert.LastIntervalUsed,
                          @LastCalled = alert.LastCalled,
                          @Id = alert.AlertId
                    },
                    commandType: System.Data.CommandType.StoredProcedure);
            }
        }
    }
}

using NLog;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;

namespace AlertJobWorker
{
  public class JobToCheckAlertsToSend : IJob
  {
    private static Logger _logger = LogManager.GetCurrentClassLogger();

    public void Execute(IJobExecutionContext context)
    {
      try
      {
        _logger.Info("Job to check alerts start Execute method was called");

        IEnumerable<AlertSchedule> alerts = AlertsRepository.GetTodayNotificationSchedules();

        if (alerts.Any())
        {
          int timeDeviation = 0;
          int.TryParse(ConfigurationManager.AppSettings["timeDeviation"], out timeDeviation);

          foreach (var alert in alerts)
          {
            if (DateTime.Now.TimeOfDay.Add(new TimeSpan(0, timeDeviation, 0)) >= alert.TimeToCall)
            {
              HttpStatusCode code = SendRequest(alert);
              if (code == HttpStatusCode.OK)
              {
                _logger.Info("Report was emailed successfully");
              }
              else
              {
                _logger.Info("Report wasn't emailed. Status code - " + code);
              }

              // write LastCalled, LastUsedInterval and NextExecutingDate into Alert.Schedule
              CalculateAlertScheduleSettingsForNextCall(alert);
              _logger.Info("LastCalled, LastUsedInterval and NextExecutingDate were recalculated");
            }
            else
            {
              _logger.Info("It's not a time for emailing alerts. Now is " + DateTime.Now.TimeOfDay + ". Time to call: " + alert.TimeToCall);
            }
          }
        }
        else
        {
          _logger.Info("There are no alerts for emailing");
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex, "ALERT EMAIL JOB ERROR: " + ex.ToString());
      }
    }

    private HttpStatusCode SendRequest(AlertSchedule alert)
    {
      string login = ConfigurationManager.AppSettings["virtualUserName"];
      string hashPassword = ConfigurationManager.AppSettings["passwordHash"];
      string API_URL = string.Format(ConfigurationManager.AppSettings["apiUrl"], login, hashPassword, alert.ReportUrl, alert.UserId, alert.AlertId);

      _logger.Info("Sending request to the URL=" + API_URL);

      HttpWebRequest webRequest = WebRequest.Create(API_URL) as HttpWebRequest;
      webRequest.Timeout = 120000;
      using (var response = webRequest.GetResponse())
      {
        return ((HttpWebResponse)response).StatusCode;
      }
    }

    private void CalculateAlertScheduleSettingsForNextCall(AlertSchedule alert)
    {
      alert.LastCalled = DateTime.Now;
      if (!alert.IsOneTime)
      {
        if (alert.Intervals != null && alert.Intervals.Any())
        {
          // first time emailing
          if (alert.LastIntervalUsed == 0)
          {
            alert.LastIntervalUsed = alert.Intervals.Min(x => x.IntervalId);
          }
          else
          {
            alert.LastIntervalUsed = alert.Intervals.First(x => x.IntervalId == alert.LastIntervalUsed).NextIntervalId;
          }

          // calculate NextExecutingDate
          AlertInterval currInterval = alert.Intervals.First(x => x.IntervalId == alert.LastIntervalUsed);
          if (currInterval.WeekNumber.HasValue && currInterval.DayOfWeek.HasValue)
          {
            alert.NextExecutingDate = GetOrderedDateByCertainDayOfWeek(currInterval.WeekNumber.Value, currInterval.DayOfWeek.Value, currInterval.DurationMonths);
          }
          else if (currInterval.NextIntervalId == currInterval.IntervalId)
          {
            alert.NextExecutingDate = alert.LastCalled.AddDays(currInterval.DurationDays).AddMonths(currInterval.DurationMonths);
          }
          else
          {
            AlertInterval nextInterval = alert.Intervals.First(x => x.IntervalId == currInterval.NextIntervalId);
            alert.NextExecutingDate = alert.LastCalled.AddDays(nextInterval.DurationDays).AddMonths(nextInterval.DurationMonths);
          }
        }
      }
      AlertsRepository.UpdateNotificationSchedule(alert);
    }

    private DateTime GetOrderedDateByCertainDayOfWeek(int orderedWeekNumber, int orderedDayOfWeek, int monthPeriod)
    {
      DateTime date = DateTime.Now.AddMonths(monthPeriod);

      DateTime result;
      if (orderedWeekNumber != 4)
      {
        result = GetFirstDayOfMonth(date);
        while ((int)result.DayOfWeek != orderedDayOfWeek)
        {
          result = result.AddDays(1);
        }
        result = result.AddDays(7 * orderedWeekNumber);
      }
      else
      {
        result = GetLastDayOfMonth(date);
        while ((int)result.DayOfWeek < orderedDayOfWeek)
        {
          result.AddDays(-1);
        }
      }

      return result;
    }

    private static DateTime GetLastDayOfMonth(DateTime d)
    {
      return new DateTime(d.Year, d.Month, DateTime.DaysInMonth(d.Year, d.Month));
    }

    private static DateTime GetFirstDayOfMonth(DateTime d)
    {
      return new DateTime(d.Year, d.Month, 1);
    }
  }
}
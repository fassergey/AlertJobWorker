using Quartz;
using Quartz.Impl;
using System.Configuration;
using System.ServiceProcess;

namespace AlertJobWorker
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun = new ServiceBase[]
            {
          new AlertSenderService()
            };
            ServiceBase.Run(ServicesToRun);



            //IScheduler sched;
            //var period = ConfigurationManager.AppSettings["period"];
            //int callPeriod = 10;
            //int.TryParse(period, out callPeriod);

            ////// construct a scheduler factory
            //ISchedulerFactory schedFact = new StdSchedulerFactory();
            //if (schedFact != null)
            //{
            //  sched = schedFact.GetScheduler();
            //  sched.Start();

            //  // create job to grab alerts
            //  IJobDetail JobToCheckAlertsToSend = JobBuilder.Create<JobToCheckAlertsToSend>()
            //          .WithIdentity("JobToCheckAlertsToSend", "group1")
            //          .Build();

            //  // create trigger
            //  ITrigger trigger1 = TriggerBuilder.Create()
            //      .WithIdentity("trigger1", "group1")
            //      .WithSimpleSchedule(x => x.WithIntervalInSeconds(callPeriod).RepeatForever())
            //      .StartNow()
            //      .Build();

            //  // Schedule the job using the job and trigger 
            //  sched.ScheduleJob(JobToCheckAlertsToSend, trigger1);
            //}
        }
    }
}

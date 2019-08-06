using NLog;
using Quartz;
using Quartz.Impl;
using System.Configuration;
using System.ServiceProcess;

namespace AlertJobWorker
{
    public partial class AlertSenderService : ServiceBase
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        //// get a scheduler, start the schedular before triggers or anything else
        private static IScheduler sched;

        public AlertSenderService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {                
                var period = ConfigurationManager.AppSettings["period"];
                int callPeriod = 10;
                int.TryParse(period, out callPeriod);
                _logger.Info("Service is started with period " + callPeriod + " sec");

                //// construct a scheduler factory
                ISchedulerFactory schedFact = new StdSchedulerFactory();
                if (schedFact != null)
                {
                    sched = schedFact.GetScheduler();
                    sched.Start();

                    // create job to grab alerts
                    IJobDetail JobToCheckAlertsToSend = JobBuilder.Create<JobToCheckAlertsToSend>()
                            .WithIdentity("JobToCheckAlertsToSend", "group1")
                            .Build();

                    // create trigger
                    ITrigger trigger1 = TriggerBuilder.Create()
                        .WithIdentity("trigger1", "group1")
                        .WithSimpleSchedule(x => x.WithIntervalInSeconds(callPeriod).RepeatForever())
                        .StartNow()
                        .Build();

                    // Schedule the job using the job and trigger 
                    sched.ScheduleJob(JobToCheckAlertsToSend, trigger1);
                }
            }
            catch(System.Exception ex)
            {
                _logger.Error(ex);
            }
        }

        protected override void OnStop()
        {
            _logger.Info("Service is stopped");
            if (sched != null) sched.Shutdown();
        }
    }
}

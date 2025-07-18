using Quartz;
using Quartz.Impl;

namespace Intellidesk.AcadNet.Services.Jobs
{
    public class EmailScheduler
    {
        public static void Start()
        {
            var scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<EmailSender>().Build();

            var repeatFrequency = 1;
            ITrigger trigger = TriggerBuilder.Create() // создаем триггер
                .WithIdentity("Run Infinitely every 2nd day of the month", "Monthly_Day_2") // trigger name and group
                .StartNow() // start right after the start of execution
                //.WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(17, 12, 0))
                .WithCalendarIntervalSchedule(x => x.WithIntervalInMonths(repeatFrequency))
                .WithCalendarIntervalSchedule(x => x.WithMisfireHandlingInstructionFireAndProceed())
                //.WithSimpleSchedule(x => x            
                //    .WithIntervalInMinutes(1)         // throught 1 minute
                //    .RepeatForever())                 // repeat without end
                .Build(); // create trigger

            scheduler.ScheduleJob(job, trigger); // starting job
        }

        public static void Clear()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
            scheduler.Clear();
        }
    }
}
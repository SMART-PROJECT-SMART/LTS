using LTS.Common;
using LTS.Services.Quartz.Jobs;
using Quartz;

namespace LTS.Services.Quartz.TelemetryBroadcast
{
    public class TelemetryBroadcastSchedular : ITelemetryBroadcastSchedular
    {
        private readonly IScheduler _scheduler;

        public TelemetryBroadcastSchedular(ISchedulerFactory schedulerFactory)
        {
            _scheduler = schedulerFactory.GetScheduler().Result;
        }

        public async Task<bool> StartSchedular(int intervalSeconds)
        {
            IJobDetail job = CreateTelemetryBroadcastJob();
            ITrigger trigger = CreateTelemetryBroadcastTrigger(intervalSeconds);

            await _scheduler.ScheduleJob(job, trigger);
            await _scheduler.Start();
            return true;
        }

        public Task<bool> StopSchedular()
        {
            throw new NotImplementedException();
        }

        private IJobDetail CreateTelemetryBroadcastJob()
        {
            return JobBuilder
                .Create<TelemetryBroadcastJob>()
                .WithIdentity(
                    LTSConstants.Quartz.TELEMETRY_BROADCAST_JOB_ID,
                    LTSConstants.Quartz.TELEMETRY_BROADCAST_JOB_GROUP
                )
                .Build();
        }

        private ITrigger CreateTelemetryBroadcastTrigger(int intervalSeconds)
        {
            return TriggerBuilder
                .Create()
                .WithIdentity(
                    LTSConstants.Quartz.TELEMETRY_BROADCAST_TRIGGER_ID,
                    LTSConstants.Quartz.TELEMETRY_BROADCAST_JOB_GROUP
                )
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(intervalSeconds).RepeatForever())
                .Build();
        }
    }
}

using LTS.Common;
using LTS.Services.Quartz.Jobs;
using LTS.Services.Quartz.TelemetryBroadcast.Interfaces;
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
            ITrigger trigger = CreateTelemetryBroadcastTrigger(intervalSeconds);
            await _scheduler.ScheduleJob(trigger);
            return true;
        }

        public Task<bool> StopSchedular()
        {
            throw new NotImplementedException();
        }

        private ITrigger CreateTelemetryBroadcastTrigger(int intervalSeconds)
        {
            JobKey jobKey = new JobKey(
                LTSConstants.Quartz.TELEMETRY_BROADCAST_JOB_ID,
                LTSConstants.Quartz.TELEMETRY_BROADCAST_JOB_GROUP
            );

            return TriggerBuilder
                .Create()
                .WithIdentity(
                    LTSConstants.Quartz.TELEMETRY_BROADCAST_TRIGGER_ID,
                    LTSConstants.Quartz.TELEMETRY_BROADCAST_JOB_GROUP
                )
                .ForJob(jobKey)
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(intervalSeconds).RepeatForever())
                .Build();
        }
    }
}

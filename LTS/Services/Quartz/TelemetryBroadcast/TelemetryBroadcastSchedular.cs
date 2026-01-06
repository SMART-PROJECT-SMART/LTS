using LTS.Common;
using LTS.Services.Quartz.Jobs;
using LTS.Services.Quartz.TelemetryBroadcast.Interfaces;
using Quartz;

namespace LTS.Services.Quartz.TelemetryBroadcast
{
    public class TelemetryBroadcastSchedular : ITelemetryBroadcastSchedular
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private IScheduler? _scheduler;

        public TelemetryBroadcastSchedular(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        public async Task<bool> StartSchedular(int intervalSeconds)
        {
            _scheduler ??= await _schedulerFactory.GetScheduler();
            ITrigger trigger = CreateTelemetryBroadcastTrigger(intervalSeconds);
            await _scheduler.ScheduleJob(trigger);
            return true;
        }

        public async Task<bool> StopSchedular()
        {
            if (_scheduler == null)
            {
                return false;
            }

            TriggerKey triggerKey = new TriggerKey(
                LTSConstants.Quartz.TELEMETRY_BROADCAST_TRIGGER_ID,
                LTSConstants.Quartz.TELEMETRY_BROADCAST_JOB_GROUP
            );

            bool unscheduled = await _scheduler.UnscheduleJob(triggerKey);
            return unscheduled;
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

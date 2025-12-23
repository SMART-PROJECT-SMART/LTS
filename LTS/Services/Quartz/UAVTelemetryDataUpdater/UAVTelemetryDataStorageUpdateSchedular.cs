using LTS.Common;
using LTS.Services.Quartz.Jobs;
using LTS.Services.Quartz.UAVTelemetryDataUpdater.Interfaces;
using Quartz;

namespace LTS.Services.Quartz.UAVTelemetryDataUpdater
{
    public class UAVTelemetryDataStorageUpdateSchedular : IUAVTelemetryDataStorageUpdateSchedular
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private IScheduler? _scheduler;

        public UAVTelemetryDataStorageUpdateSchedular(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        public async Task<bool> StartSchedular(int intervalSeconds)
        {
            _scheduler ??= await _schedulerFactory.GetScheduler();
            ITrigger trigger = CreateUAVTelemetryDataConsumerTrigger(intervalSeconds);
            await _scheduler.ScheduleJob(trigger);
            return true;
        }

        private ITrigger CreateUAVTelemetryDataConsumerTrigger(int intervalSeconds)
        {
            JobKey jobKey = new JobKey(
                LTSConstants.Quartz.UAV_TELEMETRY_DATA_CONSUME_JOB_ID,
                LTSConstants.Quartz.UAV_TELEMETRY_DATA_CONSUME_JOB_GROUP
            );

            return TriggerBuilder
                .Create()
                .WithIdentity(
                    LTSConstants.Quartz.UAV_TELEMETRY_DATA_CONSUME_TRIGGER_ID,
                    LTSConstants.Quartz.UAV_TELEMETRY_DATA_CONSUME_JOB_GROUP
                )
                .ForJob(jobKey)
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(intervalSeconds).RepeatForever())
                .Build();
        }
    }
}

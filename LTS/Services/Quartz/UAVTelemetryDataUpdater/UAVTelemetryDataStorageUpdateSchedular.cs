using LTS.Common;
using LTS.Services.Quartz.Jobs;
using Quartz;

namespace LTS.Services.Quartz.UAVTelemetryDataUpdater
{
    public class UAVTelemetryDataStorageUpdateSchedular : IUAVTelemetryDataStorageUpdateSchedular
    {
        private readonly IScheduler _scheduler;
        public async Task<bool> StartSchedular(int intervalSeconds)
        {
            IJobDetail job = CreateUAVTelemetryDataConsumerJob();
            ITrigger trigger = CreateUAVTelemetryDataConsumerTrigger(intervalSeconds);

            await _scheduler.ScheduleJob(job, trigger);
            await _scheduler.Start();
            return true;
        }

        public Task<bool> StopSchedular()
        {
            throw new NotImplementedException();
        }

        private IJobDetail CreateUAVTelemetryDataConsumerJob()
        {
            return JobBuilder.Create<UAVTelemetryDataConsumeJob>()
                .WithIdentity(LTSConstants.Quartz.UAV_TELEMETRY_DATA_CONSUME_JOB_ID,
                    LTSConstants.Quartz.UAV_TELEMETRY_DATA_CONSUME_JOB_GROUP)
                .Build();
        }
        private ITrigger CreateUAVTelemetryDataConsumerTrigger(int intervalSeconds)
        {
            return TriggerBuilder.Create()
                .WithIdentity(LTSConstants.Quartz.UAV_TELEMETRY_DATA_CONSUME_TRIGGER_ID,
                    LTSConstants.Quartz.UAV_TELEMETRY_DATA_CONSUME_JOB_GROUP)
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(intervalSeconds)
                    .RepeatForever())
                .Build();
        }
    }
}

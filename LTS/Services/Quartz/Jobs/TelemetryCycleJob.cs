using Quartz;

namespace LTS.Services.Quartz.Jobs
{
    [DisallowConcurrentExecution]
    public class TelemetryCycleJob : IJob
    {
        private readonly UAVTelemetryDataConsumeJob _consumeJob;
        private readonly TelemetryBroadcastJob _broadcastJob;

        public TelemetryCycleJob(
            UAVTelemetryDataConsumeJob consumeJob,
            TelemetryBroadcastJob broadcastJob
        )
        {
            _consumeJob = consumeJob;
            _broadcastJob = broadcastJob;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _consumeJob.Execute(context);
            await _broadcastJob.Execute(context);
        }
    }
}

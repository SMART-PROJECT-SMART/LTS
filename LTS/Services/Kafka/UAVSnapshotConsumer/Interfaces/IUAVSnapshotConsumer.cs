using LTS.Dto;

namespace LTS.Services.Kafka.UAVSnapshotConsumer.Interfaces
{
    public interface IUAVSnapshotConsumer
    {
        Task<IEnumerable<UAVTelemetryDataDto>> PeekAllUAVSnapshots();
    }
}

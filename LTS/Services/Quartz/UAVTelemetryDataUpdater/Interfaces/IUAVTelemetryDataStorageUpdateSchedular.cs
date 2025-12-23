namespace LTS.Services.Quartz.UAVTelemetryDataUpdater.Interfaces
{
    public interface IUAVTelemetryDataStorageUpdateSchedular
    {
        Task<bool> StartSchedular(int intervalSeconds);
    }
}

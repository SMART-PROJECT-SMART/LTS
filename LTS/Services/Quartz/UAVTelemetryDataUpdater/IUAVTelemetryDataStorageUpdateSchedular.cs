namespace LTS.Services.Quartz.UAVTelemetryDataUpdater
{
    public interface IUAVTelemetryDataStorageUpdateSchedular
    {
        Task<bool> StartSchedular(int intervalSeconds);
        Task<bool> StopSchedular();
    }
}

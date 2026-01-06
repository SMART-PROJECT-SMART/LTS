namespace LTS.Services.Quartz.TelemetryBroadcast.Interfaces
{
    public interface ITelemetryBroadcastSchedular
    {
        Task<bool> StartSchedular(int intervalSeconds);
    }
}

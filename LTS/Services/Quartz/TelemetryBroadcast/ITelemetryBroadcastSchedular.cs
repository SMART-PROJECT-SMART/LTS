namespace LTS.Services.Quartz.TelemetryBroadcast
{
    public interface ITelemetryBroadcastSchedular
    {
        Task<bool> StartSchedular(int intervalSeconds);
        Task<bool> StopSchedular();
    }
}

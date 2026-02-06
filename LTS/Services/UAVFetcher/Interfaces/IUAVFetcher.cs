using LTS.Dto;

namespace LTS.Services.ActiveUAVFetcher.Interfaces
{
    public interface IUAVFetcher
    {
        Task<IEnumerable<int>> GetActiveUAVsTailIdAsync(CancellationToken cancellationToken);
        Task<IEnumerable<SimulatorUAVDto>> GetAllUAVsDataAsync(CancellationToken cancellationToken = default);
    }
}

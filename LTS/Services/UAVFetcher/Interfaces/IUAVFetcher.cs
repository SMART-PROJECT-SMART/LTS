namespace LTS.Services.ActiveUAVFetcher.Interfaces
{
    public interface IUAVFetcher
    {
        Task<IEnumerable<int>> GetActiveUAVsTailIdAsync(CancellationToken cancellationToken);
        Task<IEnumerable<int>> GetAllUAVsTailIdAsync(CancellationToken cancellationToken = default);
    }
}

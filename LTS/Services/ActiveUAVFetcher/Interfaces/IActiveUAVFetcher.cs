namespace LTS.Services.ActiveUAVFetcher.Interfaces
{
    public interface IActiveUAVFetcher
    {
        Task<IEnumerable<int>> GetActiveUAVsTailId(CancellationToken cancellationToken);
    }
}

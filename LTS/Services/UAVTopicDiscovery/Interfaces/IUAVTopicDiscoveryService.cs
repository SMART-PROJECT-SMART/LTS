namespace LTS.Services.UAVTopicDiscovery.Interfaces
{
    public interface IUAVTopicDiscoveryService
    {
        Task DiscoverAndCacheUAVTopicsAsync();
        IEnumerable<int> GetAllCachedUAVIds();
        void AddUAVTopic(int tailId);
    }
}

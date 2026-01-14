using LTS.Common;
using LTS.Services.ActiveUAVFetcher.Interfaces;

namespace LTS.Services.ActiveUAVFetcher
{
    public class ActiveUAVFetcher : IActiveUAVFetcher
    {
        private readonly HttpClient _simulatorHttpClient;

        public ActiveUAVFetcher(IHttpClientFactory httpClientFactory)
        {
            _simulatorHttpClient = httpClientFactory.CreateClient(LTSConstants.HttpClients.SIMULATOR_HTTP_CLIENT);
        }

        public async Task<IEnumerable<int>> GetActiveUAVsTailId(CancellationToken cancellationToken)
        {
            HttpResponseMessage activeUAVResponse =
                await _simulatorHttpClient.GetAsync(LTSConstants.SimulatorEndpoints.GET_ACTIVE_UAV_ENDPOINT,cancellationToken);

            activeUAVResponse.EnsureSuccessStatusCode();

            IEnumerable<int> activeUAVs =
                (await activeUAVResponse.Content.ReadFromJsonAsync<IEnumerable<int>>(cancellationToken));

            return activeUAVs ?? Enumerable.Empty<int>();
        }
    }
}

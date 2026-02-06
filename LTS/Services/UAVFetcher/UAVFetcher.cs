using LTS.Common;
using LTS.Dto;
using LTS.Services.ActiveUAVFetcher.Interfaces;

namespace LTS.Services.ActiveUAVFetcher
{
    public class UAVFetcher : IUAVFetcher
    {
        private readonly HttpClient _simulatorHttpClient;

        public UAVFetcher(IHttpClientFactory httpClientFactory)
        {
            _simulatorHttpClient = httpClientFactory.CreateClient(LTSConstants.HttpClients.SIMULATOR_HTTP_CLIENT);
        }

        public async Task<IEnumerable<int>> GetActiveUAVsTailIdAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage activeUAVResponse =
                await _simulatorHttpClient.GetAsync(LTSConstants.SimulatorEndpoints.GET_ACTIVE_UAV_ENDPOINT,cancellationToken);

            activeUAVResponse.EnsureSuccessStatusCode();

            IEnumerable<int>? activeUAVs =
                await activeUAVResponse.Content.ReadFromJsonAsync<IEnumerable<int>>(cancellationToken);

            return activeUAVs ?? Enumerable.Empty<int>();
        }

        public async Task<IEnumerable<SimulatorUAVDto>> GetAllUAVsDataAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage allUAVResponse =
                await _simulatorHttpClient.GetAsync(LTSConstants.SimulatorEndpoints.GET_ALL_UAV_ENDPOINT, cancellationToken);

            allUAVResponse.EnsureSuccessStatusCode();

            IEnumerable<SimulatorUAVDto>? allUAVs =
                await allUAVResponse.Content.ReadFromJsonAsync<IEnumerable<SimulatorUAVDto>>(cancellationToken);

            return allUAVs ?? Enumerable.Empty<SimulatorUAVDto>();
        }
    }
}

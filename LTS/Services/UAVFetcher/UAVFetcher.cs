using LTS.Common;
using LTS.Dto;
using LTS.Services.ActiveUAVFetcher.Interfaces;
using Microsoft.Extensions.Logging;

namespace LTS.Services.ActiveUAVFetcher
{
    public class UAVFetcher : IUAVFetcher
    {
        private readonly HttpClient _simulatorHttpClient;
        private readonly ILogger<UAVFetcher> _logger;

        public UAVFetcher(IHttpClientFactory httpClientFactory, ILogger<UAVFetcher> logger)
        {
            _simulatorHttpClient = httpClientFactory.CreateClient(LTSConstants.HttpClients.SIMULATOR_HTTP_CLIENT);
            _logger = logger;
        }

        public async Task<IEnumerable<int>> GetActiveUAVsTailIdAsync(CancellationToken cancellationToken)
        {
            try
            {
                HttpResponseMessage activeUAVResponse =
                    await _simulatorHttpClient.GetAsync(LTSConstants.SimulatorEndpoints.GET_ACTIVE_UAV_ENDPOINT, cancellationToken);

                activeUAVResponse.EnsureSuccessStatusCode();

                IEnumerable<int>? activeUAVs =
                    await activeUAVResponse.Content.ReadFromJsonAsync<IEnumerable<int>>(cancellationToken);

                return activeUAVs ?? Enumerable.Empty<int>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch active UAVs from simulator service.");
                return Enumerable.Empty<int>();
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "Request to fetch active UAVs was cancelled or timed out.");
                return Enumerable.Empty<int>();
            }
        }

        public async Task<IEnumerable<SimulatorUAVDto>> GetAllUAVsDataAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                HttpResponseMessage allUAVResponse =
                    await _simulatorHttpClient.GetAsync(LTSConstants.SimulatorEndpoints.GET_ALL_UAV_ENDPOINT, cancellationToken);

                allUAVResponse.EnsureSuccessStatusCode();

                IEnumerable<SimulatorUAVDto>? allUAVs =
                    await allUAVResponse.Content.ReadFromJsonAsync<IEnumerable<SimulatorUAVDto>>(cancellationToken);

                return allUAVs ?? Enumerable.Empty<SimulatorUAVDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch all UAVs data from simulator service.");
                return Enumerable.Empty<SimulatorUAVDto>();
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "Request to fetch all UAVs data was cancelled or timed out.");
                return Enumerable.Empty<SimulatorUAVDto>();
            }
        }
    }
}

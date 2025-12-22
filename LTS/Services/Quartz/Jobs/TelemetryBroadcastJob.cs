using Core.Common.Enums;
using LTS.Common;
using LTS.Dto;
using LTS.Services.SubscriptionManager;
using LTS.Services.UAVDataStorage;
using LTS.Services.WebSocket.Hubs;
using Microsoft.AspNetCore.SignalR;
using Quartz;

namespace LTS.Services.Quartz.Jobs
{
    public class TelemetryBroadcastJob : IJob
    {
        private readonly IUAVTelemetryDataStorage _uavTelemetryDataStorage;
        private readonly IWantedUAVFieldsManager _wantedUavFieldsManager;
        private readonly IHubContext<SessionWantedFieldsHub> _hubContext;

        public TelemetryBroadcastJob(
            IUAVTelemetryDataStorage uavTelemetryDataStorage,
            IWantedUAVFieldsManager wantedUavFieldsManager,
            IHubContext<SessionWantedFieldsHub> hubContext
        )
        {
            _uavTelemetryDataStorage = uavTelemetryDataStorage;
            _wantedUavFieldsManager = wantedUavFieldsManager;
            _hubContext = hubContext;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            IEnumerable<string> sessionIds = _wantedUavFieldsManager.GetAllSessionIds();

            foreach (string sessionId in sessionIds)
            {
                Dictionary<int, HashSet<TelemetryFields>>? sessionWantedFields =
                    _wantedUavFieldsManager.GetSessionWantedFieldsById(sessionId);

                if (sessionWantedFields == null)
                {
                    continue;
                }

                List<UAVTelemetryFieldsDto> uavDataList = sessionWantedFields
                    .Select(uavSubscription =>
                    {
                        IEnumerable<KeyValuePair<TelemetryFields, double>> telemetryData =
                            _uavTelemetryDataStorage.GetUAVTelemetryData(uavSubscription.Key);

                        Dictionary<TelemetryFields, double> filteredFields =
                            telemetryData?
                                .Where(kvp => uavSubscription.Value.Contains(kvp.Key))
                                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                            ?? new Dictionary<TelemetryFields, double>();

                        return new UAVTelemetryFieldsDto(uavSubscription.Key, filteredFields);
                    })
                    .Where(dto => dto.Fields.Any())
                    .ToList();

                if (uavDataList.Any())
                {
                    TelemetryBroadcastDto broadcastDto = new TelemetryBroadcastDto(uavDataList);
                    await _hubContext
                        .Clients.Group(sessionId)
                        .SendAsync(
                            LTSConstants.WebSocket.RECIVE_TELEMETRY_DATA_METHOD,
                            broadcastDto
                        );
                }
            }
        }
    }
}

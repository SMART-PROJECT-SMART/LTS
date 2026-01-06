using Core.Common.Enums;
using LTS.Common;
using LTS.Dto;
using LTS.Services.UAVDataStorage.Interfaces;
using LTS.Services.WantedFieldsManager.Interfaces;
using LTS.Services.WebSocket.Hubs;
using Microsoft.AspNetCore.SignalR;
using Quartz;

namespace LTS.Services.Quartz.Jobs
{
    public class TelemetryBroadcastJob : IJob
    {
        private readonly IUAVTelemetryDataStorage _uavTelemetryDataStorage;
        private readonly IWantedUAVFieldsManager _wantedUavFieldsManager;
        private readonly IHubContext<TelemetryBroadcastHub> _hubContext;

        public TelemetryBroadcastJob(
            IUAVTelemetryDataStorage uavTelemetryDataStorage,
            IWantedUAVFieldsManager wantedUavFieldsManager,
            IHubContext<TelemetryBroadcastHub> hubContext
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
                await ProcessSessionBroadcast(sessionId);
            }
        }

        private async Task ProcessSessionBroadcast(string sessionId)
        {
            if (!_wantedUavFieldsManager.DoesSessionExist(sessionId))
            {
                return;
            }

            Dictionary<int, HashSet<TelemetryFields>> sessionWantedFields =
                _wantedUavFieldsManager.GetSessionWantedFieldsById(sessionId)!;

            List<UAVTelemetryFieldsDto> uavDataList = BuildUAVTelemetryDataList(
                sessionWantedFields
            );

            if (uavDataList.Count > 0)
            {
                TelemetryBroadcastDto broadcastDto = new TelemetryBroadcastDto(uavDataList);
                await SendBroadcastToSession(sessionId, broadcastDto);
            }
        }

        private List<UAVTelemetryFieldsDto> BuildUAVTelemetryDataList(
            Dictionary<int, HashSet<TelemetryFields>> sessionWantedFields
        )
        {
            return sessionWantedFields
                .Select(uavSubscription =>
                    BuildUAVTelemetryDto(uavSubscription.Key, uavSubscription.Value)
                )
                .Where(dto => dto.Fields.Count > 0)
                .ToList();
        }

        private UAVTelemetryFieldsDto BuildUAVTelemetryDto(
            int tailId,
            HashSet<TelemetryFields> wantedFields
        )
        {
            Dictionary<TelemetryFields, double> filteredFields = GetFilteredTelemetryData(
                tailId,
                wantedFields
            );

            return new UAVTelemetryFieldsDto(tailId, filteredFields);
        }

        private Dictionary<TelemetryFields, double> GetFilteredTelemetryData(
            int tailId,
            HashSet<TelemetryFields> wantedFields
        )
        {
            IEnumerable<KeyValuePair<TelemetryFields, double>>? telemetryData =
                _uavTelemetryDataStorage.GetUAVTelemetryData(tailId);

            if (telemetryData == null)
            {
                return new Dictionary<TelemetryFields, double>();
            }

            return telemetryData
                .Where(telemetryField => wantedFields.Contains(telemetryField.Key))
                .ToDictionary(telemetryField => telemetryField.Key, telemetryField => telemetryField.Value);
        }

        private async Task SendBroadcastToSession(
            string sessionId,
            TelemetryBroadcastDto broadcastDto
        )
        {
            await _hubContext
                .Clients.Group(sessionId)
                .SendAsync(LTSConstants.WebSocket.RECIVE_TELEMETRY_DATA_METHOD, broadcastDto);
        }
    }
}

using Core.Common.Enums;

namespace LTS.Services.WantedFieldsManager.Interfaces
{
    public interface IUAVTelemetryFieldReferenceCounter
    {
        void IncrementFieldReferences(int uavId, HashSet<TelemetryFields> fields);
        void DecrementFieldReferences(int uavId, HashSet<TelemetryFields> fields);
        HashSet<TelemetryFields>? GetGlobalWantedFieldsForUAV(int uavId);
        IEnumerable<int> GetAllWantedUAVs();
    }
}

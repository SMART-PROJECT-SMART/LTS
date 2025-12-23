using System.Collections.Concurrent;
using Core.Common.Enums;
using LTS.Services.UAVTelemetryFieldsReferenceCounter.Interfaces;

namespace LTS.Services.UAVTelemetryFieldsReferenceCounter
{
    public class UAVTelemetryFieldReferenceCounter : IUAVTelemetryFieldReferenceCounter
    {
        private readonly ConcurrentDictionary<
            int,
            HashSet<TelemetryFields>
        > _globalWantedFieldsByUavId;
        private readonly ConcurrentDictionary<
            int,
            ConcurrentDictionary<TelemetryFields, int>
        > _fieldReferenceCounts;

        public UAVTelemetryFieldReferenceCounter()
        {
            _globalWantedFieldsByUavId = new ConcurrentDictionary<int, HashSet<TelemetryFields>>();
            _fieldReferenceCounts =
                new ConcurrentDictionary<int, ConcurrentDictionary<TelemetryFields, int>>();
        }

        public void IncrementFieldReferences(int uavId, HashSet<TelemetryFields> fields)
        {
            ConcurrentDictionary<TelemetryFields, int> fieldCounts = GetOrCreateFieldCounts(uavId);
            HashSet<TelemetryFields> globalFields = GetOrCreateGlobalFields(uavId);

            foreach (TelemetryFields field in fields)
            {
                int newCount = fieldCounts.AddOrUpdate(field, 1, (_, count) => count + 1);

                if (newCount == 1)
                {
                    globalFields.Add(field);
                }
            }
        }

        public void DecrementFieldReferences(int uavId, HashSet<TelemetryFields> fields)
        {
            if (
                !_fieldReferenceCounts.TryGetValue(
                    uavId,
                    out ConcurrentDictionary<TelemetryFields, int>? fieldCounts
                )
            )
            {
                return;
            }

            if (
                !_globalWantedFieldsByUavId.TryGetValue(
                    uavId,
                    out HashSet<TelemetryFields>? globalFields
                )
            )
            {
                return;
            }

            foreach (TelemetryFields field in fields)
            {
                if (fieldCounts.TryGetValue(field, out int currentCount))
                {
                    int newCount = currentCount - 1;

                    if (newCount <= 0)
                    {
                        fieldCounts.TryRemove(field, out _);
                        globalFields.Remove(field);
                    }
                    else
                    {
                        fieldCounts[field] = newCount;
                    }
                }
            }

            if (globalFields.Count == 0)
            {
                _globalWantedFieldsByUavId.TryRemove(uavId, out _);
                _fieldReferenceCounts.TryRemove(uavId, out _);
            }
        }

        public HashSet<TelemetryFields>? GetGlobalWantedFieldsForUAV(int uavId)
        {
            return _globalWantedFieldsByUavId.GetValueOrDefault(uavId);
        }

        public IEnumerable<int> GetAllWantedUAVs()
        {
            return _globalWantedFieldsByUavId.Keys;
        }

        private ConcurrentDictionary<TelemetryFields, int> GetOrCreateFieldCounts(int uavId)
        {
            return _fieldReferenceCounts.GetOrAdd(
                uavId,
                _ => new ConcurrentDictionary<TelemetryFields, int>()
            );
        }

        private HashSet<TelemetryFields> GetOrCreateGlobalFields(int uavId)
        {
            return _globalWantedFieldsByUavId.GetOrAdd(uavId, _ => new HashSet<TelemetryFields>());
        }
    }
}

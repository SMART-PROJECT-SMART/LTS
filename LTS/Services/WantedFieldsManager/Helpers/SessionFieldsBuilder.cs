using Core.Common.Enums;
using LTS.Models;

namespace LTS.Services.WantedFieldsManager.Helpers
{
    public static class SessionFieldsBuilder
    {
        public static Dictionary<int, HashSet<TelemetryFields>> BuildFromSubscriptions(
            IEnumerable<UAVFieldSubscription> subscriptions
        )
        {
            Dictionary<int, HashSet<TelemetryFields>> sessionFields =
                new Dictionary<int, HashSet<TelemetryFields>>();

            foreach (UAVFieldSubscription subscription in subscriptions)
            {
                HashSet<TelemetryFields> fieldsSet = new HashSet<TelemetryFields>(
                    subscription.WantedFields
                );
                sessionFields[subscription.TailId] = fieldsSet;
            }

            return sessionFields;
        }

        public static HashSet<int> GetAllAffectedUavIds(
            Dictionary<int, HashSet<TelemetryFields>> oldFields,
            Dictionary<int, HashSet<TelemetryFields>> newFields
        )
        {
            HashSet<int> affectedUavIds = new HashSet<int>(oldFields.Keys);
            affectedUavIds.UnionWith(newFields.Keys);
            return affectedUavIds;
        }
    }
}

namespace LTS.Common
{
    public static class LTSConstants
    {
        public static class Kafka
        {
            public const string KAFKA_CONFIGURATION_SECTION = "kafka";
            public const string UAV_DATA_TOPIC_PREFIX = "telemetry-tailId-";
            public const string SNAPSHOT_CONSUMER_GROUP_SUFFIX = "-snapshot";
            public const int DEFAULT_PARTITION = 1;
            public const int METADATA_TIMEOUT_SECONDS = 10;
            public const int CONSUME_TIMEOUT_SECONDS = 5;
        }

        public static class WebSocket
        {
            public const string SESSION_ID_FIELD = "sessionId";
            public const string SESSION_ID_KEY = "SessionId";
            public const string WANTED_UAVS_FIELDS_FIELD = "wantedUAVsFields";
            public const string RECIVE_TELEMETRY_DATA_METHOD = "ReceiveTelemetryData";
        }

        public static class Quartz
        {
            public const string UAV_TELEMETRY_DATA_CONSUME_JOB_ID = "UAVTelemetryDataConsumeJob";
            public const string UAV_TELEMETRY_DATA_CONSUME_JOB_GROUP =
                "UAVTelemetryDataConsumeJobGroup";
            public const string UAV_TELEMETRY_DATA_CONSUME_TRIGGER_ID =
                "UAVTelemetryDataConsumeTrigger";
            public const string TELEMETRY_BROADCAST_JOB_ID = "TelemetryBroadcastJob";
            public const string TELEMETRY_BROADCAST_JOB_GROUP = "TelemetryBroadcastJobGroup";
            public const string TELEMETRY_BROADCAST_TRIGGER_ID = "TelemetryBroadcastTrigger";
        }

        public static class Scheduler
        {
            public const string SCHEDULER_CONFIGURATION_SECTION = "Scheduler";
        }

        public static class Hub
        {
            public const string TELEMETRY_HUB_ENDPOINT = "/hub/telemetry";
        }
    }
}

namespace LTS.Common
{
    public static class LTSConstants
    {
        public static class Kafka
        {
            public const string KAFKA_CONFIGURATION_SECTION = "kafka";
            public const string UAV_DATA_TOPIC_PREFIX = "telemetry-uav-";
        }

        public static class WebSocket
        {
            public const string SESSION_ID_FIELD = "sessionId";
            public const string WANTED_UAVS_FIELDS_FIELD = "wantedUAVsFields";
            public const string RECIVE_TELEMETRY_DATA_METHOD = "ReceiveTelemetryData";
        }
    }
}

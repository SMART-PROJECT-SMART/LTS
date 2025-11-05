using LTS.Common;
using LTS.Models;
using LTS.Services.Kafka.UAVTelemetryDataConsumer;
using LTS.Services.Kafka.UAVTelmetryDataConsumerManager;
using LTS.Services.SubscriptionManager;
using LTS.Services.SubscriptionManager;

namespace LTS.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKafkaServices(this IServiceCollection services, IConfiguration appConfiguration)
        {
            services.Configure<KafkaConsumerConfiguration>(
                appConfiguration.GetSection(LTSConstants.Kafka.KAFKA_CONFIGURATION_SECTION));
            
            AddUAVTelemetryDataKafkaConsumer(services);
            AddUAVTelemetryConsumerManager(services);

            return services;
        }

        public static IServiceCollection AddWantedUAVFieldsManager(this IServiceCollection services)
        {
            services.AddSingleton<IWantedUAVFieldsManager, WantedUavFieldsManager>();
            return services;
        }

        private static IServiceCollection AddUAVTelemetryConsumerManager(this IServiceCollection services)
        {
            services.AddSingleton<IUAVTelemetryDataKafkaConsumerManager, UAVTelemetryDataKafkaConsumerManager>();
            return services;
        }

        private static IServiceCollection AddUAVTelemetryDataKafkaConsumer(this IServiceCollection services)
        {
            services.AddTransient<IUAVTelemetryDataKafkaConsumer, UAVTelemetryDataKafkaConsumer>();
            return services;
        }
    }
}

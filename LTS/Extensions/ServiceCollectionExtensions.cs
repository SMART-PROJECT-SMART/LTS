using LTS.Common;
using LTS.Models;

namespace LTS.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKafkaServices(this IServiceCollection services, IConfiguration appConfiguration)
        {
            services.Configure<KafkaConsumerConfiguration>(
                appConfiguration.GetSection(LTSConstants.Kafka.KAFKA_CONFIGURATION_SECTION));

            return services;
        }
    }
}

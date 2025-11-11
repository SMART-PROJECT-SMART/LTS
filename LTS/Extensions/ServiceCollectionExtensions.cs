using LTS.Common;
using LTS.Models;
using LTS.Services.Kafka.UAVTelemetryDataConsumer;
using LTS.Services.Kafka.UAVTelmetryDataConsumerManager;
using LTS.Services.SubscriptionManager;
using LTS.Services.UAVDataStorage;
using LTS.Services.Quartz.UAVTelemetryDataUpdater;
using LTS.Services.Quartz.Jobs;
using Quartz;

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

        public static IServiceCollection AddUAVTelemetryDataStorage(this IServiceCollection services)
        {
            services.AddSingleton<IUAVTelemetryDataStorage, UAVTelemetryDataStorage>();
            return services;
        }

        public static IServiceCollection AddQuartzScheduler(this IServiceCollection services)
        {
            // Add Quartz services
            services.AddQuartz(q =>
            {
                // Use a scoped job factory to support DI in jobs
                q.UseMicrosoftDependencyInjectionJobFactory();

                // Register the job
                q.AddJob<UAVTelemetryDataConsumeJob>(opts => opts
                    .WithIdentity(LTSConstants.Quartz.UAV_TELEMETRY_DATA_CONSUME_JOB_ID,
                    LTSConstants.Quartz.UAV_TELEMETRY_DATA_CONSUME_JOB_GROUP));
            });

            // Add Quartz hosted service
            services.AddQuartzHostedService(options =>
            {
                // Wait for jobs to complete before shutdown
                options.WaitForJobsToComplete = true;
            });

            // Register the scheduler service
            services.AddSingleton<IUAVTelemetryDataStorageUpdateSchedular, UAVTelemetryDataStorageUpdateSchedular>();

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

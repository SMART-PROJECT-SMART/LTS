using LTS.Common;
using LTS.Configuration;
using LTS.Services.Kafka.UAVSnapshotConsumer;
using LTS.Services.Kafka.UAVSnapshotConsumer.Interfaces;
using LTS.Services.Kafka.UAVTelemetryDataConsumer;
using LTS.Services.Kafka.UAVTelemetryDataConsumer.Interfaces;
using LTS.Services.Kafka.UAVTelmetryDataConsumerManager;
using LTS.Services.Kafka.UAVTelmetryDataConsumerManager.Interfaces;
using LTS.Services.Kafka.UAVTopicDiscovery;
using LTS.Services.Kafka.UAVTopicDiscovery.Interfaces;
using LTS.Services.Quartz.Jobs;
using LTS.Services.Quartz.TelemetryBroadcast;
using LTS.Services.Quartz.TelemetryBroadcast.Interfaces;
using LTS.Services.Quartz.UAVTelemetryDataUpdater;
using LTS.Services.Quartz.UAVTelemetryDataUpdater.Interfaces;
using LTS.Services.SessionManagement;
using LTS.Services.SessionManagement.Interfaces;
using LTS.Services.UAVDataStorage;
using LTS.Services.UAVDataStorage.Interfaces;
using LTS.Services.UAVTelemetryFieldsReferenceCounter;
using LTS.Services.UAVTelemetryFieldsReferenceCounter.Interfaces;
using LTS.Services.UAVTopicDiscovery;
using LTS.Services.WantedFieldsManager;
using LTS.Services.WantedFieldsManager.Interfaces;
using LTS.Services.WebSocket.Hubs;
using Microsoft.Extensions.Options;
using Quartz;

namespace LTS.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebApi(this IServiceCollection services)
        {
            services
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(
                        new Newtonsoft.Json.Converters.StringEnumConverter()
                    );
                });
            services.AddEndpointsApiExplorer();
            services.AddRouting();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .SetIsOriginAllowed(_ => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });
            return services;
        }

        public static IServiceCollection AddKafkaServices(
            this IServiceCollection services,
            IConfiguration appConfiguration
        )
        {
            services.Configure<KafkaConsumerConfiguration>(
                appConfiguration.GetSection(LTSConstants.Kafka.KAFKA_CONFIGURATION_SECTION)
            );

            AddUAVTelemetryConsumerManager(services);

            services.AddSingleton<UAVTopicDiscoveryService>();
            services.AddSingleton<IUAVTopicDiscoveryService>(sp =>
                sp.GetRequiredService<UAVTopicDiscoveryService>()
            );
            services.AddHostedService<UAVTopicDiscoveryService>(sp =>
                sp.GetRequiredService<UAVTopicDiscoveryService>()
            );

            services.AddScoped<IUAVSnapshotConsumer, UAVSnapshotConsumer>();

            return services;
        }

        public static IServiceCollection AddWantedUAVFieldsManager(this IServiceCollection services)
        {
            services.AddSingleton<
                IUAVTelemetryFieldReferenceCounter,
                UAVTelemetryFieldReferenceCounter
            >();
            services.AddSingleton<IWantedUAVFieldsManager, WantedUavFieldsManager>();
            services.AddSingleton<ISessionManagementService, SessionManagementService>();
            return services;
        }

        public static IServiceCollection AddUAVTelemetryDataStorage(
            this IServiceCollection services
        )
        {
            services.AddSingleton<IUAVTelemetryDataStorage, UAVTelemetryDataStorage>();
            return services;
        }

        public static IServiceCollection AddQuartzScheduler(
            this IServiceCollection services,
            IConfiguration appConfiguration
        )
        {
            services.Configure<SchedulerConfiguration>(
                appConfiguration.GetSection(LTSConstants.Scheduler.SCHEDULER_CONFIGURATION_SECTION)
            );

            services.AddQuartz(q =>
            {
                q.AddJob<UAVTelemetryDataConsumeJob>(opts =>
                    opts.WithIdentity(
                            LTSConstants.Quartz.UAV_TELEMETRY_DATA_CONSUME_JOB_ID,
                            LTSConstants.Quartz.UAV_TELEMETRY_DATA_CONSUME_JOB_GROUP
                        )
                        .StoreDurably()
                );

                q.AddJob<TelemetryBroadcastJob>(opts =>
                    opts.WithIdentity(
                            LTSConstants.Quartz.TELEMETRY_BROADCAST_JOB_ID,
                            LTSConstants.Quartz.TELEMETRY_BROADCAST_JOB_GROUP
                        )
                        .StoreDurably()
                );
            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            services.AddSingleton<
                IUAVTelemetryDataStorageUpdateSchedular,
                UAVTelemetryDataStorageUpdateSchedular
            >();

            services.AddSingleton<ITelemetryBroadcastSchedular, TelemetryBroadcastSchedular>();

            return services;
        }

        private static IServiceCollection AddUAVTelemetryConsumerManager(
            this IServiceCollection services
        )
        {
            services.AddSingleton<
                IUAVTelemetryDataKafkaConsumerManager,
                UAVTelemetryDataKafkaConsumerManager
            >();
            return services;
        }
    }

    public static class WebApplicationExtensions
    {
        public static WebApplication ConfigureHub(this WebApplication app)
        {
            app.MapHub<TelemetryBroadcastHub>(LTSConstants.Hub.TELEMETRY_HUB_ENDPOINT);
            return app;
        }

        public static async Task<WebApplication> StartSchedulers(this WebApplication app)
        {
            IOptions<SchedulerConfiguration> schedulerConfig = app.Services.GetRequiredService<
                IOptions<SchedulerConfiguration>
            >();

            IUAVTelemetryDataStorageUpdateSchedular consumeSchedular =
                app.Services.GetRequiredService<IUAVTelemetryDataStorageUpdateSchedular>();
            ITelemetryBroadcastSchedular broadcastSchedular =
                app.Services.GetRequiredService<ITelemetryBroadcastSchedular>();

            await consumeSchedular.StartSchedular(schedulerConfig.Value.ConsumeIntervalSeconds);
            await broadcastSchedular.StartSchedular(schedulerConfig.Value.BroadcastIntervalSeconds);

            return app;
        }
    }
}

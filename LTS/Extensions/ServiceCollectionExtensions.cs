namespace LTS.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUAVWantedFieldsManager(this IServiceCollection services)
        {
            services.AddSingleton<Services.UAVWantedFieldsManager.IUAVWantedFieldsManager, Services.UAVWantedFieldsManager.UAVWantedFieldsManager>();
            return services;
        }
    }
}

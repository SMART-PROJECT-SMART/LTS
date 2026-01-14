using LTS.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddWebApi();
builder.Services.AddSignalR();
builder.Services.AddKafkaServices(builder.Configuration);
builder.Services.AddWantedUAVFieldsManager();
builder.Services.AddUAVTelemetryDataStorage();
builder.Services.AddQuartzScheduler(builder.Configuration);
builder.Services.AddHttpClients(builder.Configuration);

WebApplication app = builder.Build();
app.UseCors();
app.UseRouting();
app.MapControllers();
app.ConfigureHub();
await app.StartSchedulers();
app.Run();

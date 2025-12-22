using LTS.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddWebApi();
builder.Services.AddSignalR();
builder.Services.AddKafkaServices(builder.Configuration);
builder.Services.AddWantedUAVFieldsManager();
builder.Services.AddUAVTelemetryDataStorage();
builder.Services.AddQuartzScheduler();
var app = builder.Build();
app.UseRouting();
app.MapControllers();
app.Run();

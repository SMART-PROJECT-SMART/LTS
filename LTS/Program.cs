using LTS.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddKafkaServices(builder.Configuration);
var app = builder.Build();
app.Run();

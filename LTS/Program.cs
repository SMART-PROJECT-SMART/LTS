using LTS.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddUAVWantedFieldsManager();
var app = builder.Build();
app.Run();

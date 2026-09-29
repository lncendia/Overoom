using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Application.ScopedDictionary;
using Common.DI.Extensions;
using Common.DI.Middlewares;
using Common.DI.WebApi.Extensions;
using Common.Infrastructure.JsonConverters;
using Common.Infrastructure.ScopedDictionary;
using Microsoft.AspNetCore.SignalR;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.RoomEvents;
using Rooms.Application.Services.CommandHandlers;
using Rooms.Application.Services.QueryHandlers;
using Rooms.Infrastructure.Storage.DatabaseInitialization;
using Rooms.Infrastructure.Web.HubFilters;
using Rooms.Infrastructure.Web.Metrics;
using Rooms.Infrastructure.Web.Rooms.Hubs;
using Rooms.Start.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.InitializeMongoDb();
builder.AddLoggingServices();
builder.AddJwtAuthentication();
builder.AddCorsServices();
builder.Services.AddScoped<IScopedContext, ScopedContext>();
builder.AddStorageServices();
builder.AddMassTransitServices();
builder.Services.AddMediatorServices(typeof(CreateRoomCommandHandler), typeof(GetRoomMessagesQueryHandler));

builder.Services.AddSignalR(options =>
{
  options.AddFilter<HubMetricsFilter>();
  options.AddFilter<HubExceptionFilter>();
  options.AddFilter<HubConnectionIdFilter>();
}).AddJsonProtocol(options =>
{
  options.PayloadSerializerOptions.Converters.Add(new TypeNameJsonConverter<RoomBaseEvent>());
  options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
  options.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
});

builder.Services.AddOpenTelemetryServices(Constants.OpenTelemetry.ServiceName, RoomsConnectionMetrics.MeterName);
WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
  await DatabaseInitializer.InitAsync(scope.ServiceProvider);
}

app.UseCors(CorsServices.CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<RoomHub>("/room");
app.MapPrometheusScrapingEndpointWithBasicAuth();
await app.RunAsync();
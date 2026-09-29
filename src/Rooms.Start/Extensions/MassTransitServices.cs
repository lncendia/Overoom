using System.Text.Json;
using System.Text.Json.Serialization;
using Common.DI.Extensions;
using Common.Infrastructure.JsonConverters;
using MassTransit;
using MassTransit.SignalR;
using MongoDB.Driver;
using Rooms.Application.Abstractions.RoomEvents;
using Rooms.Application.Abstractions.Services;
using Rooms.Infrastructure.Bus.Rooms;
using Rooms.Infrastructure.Bus.Users;
using Rooms.Infrastructure.Web.Rooms.Hubs;
using Rooms.Infrastructure.Web.Services;

namespace Rooms.Start.Extensions;

/// <summary>
/// Статический класс для регистрации сервиса MassTransit в контейнере зависимостей (DI)
/// </summary>
public static class MassTransitServices
{
  /// <summary>
  /// Метод регистрирует сервис MassTransit в контейнере DI с настройкой RabbitMQ и MongoDB Outbox
  /// </summary>
  /// <param name="builder">Построитель веб-приложения для регистрации сервисов</param>
  public static void AddMassTransitServices(this IHostApplicationBuilder builder)
  {
    string rmq = builder.Configuration.GetRequiredValue<string>("RabbitMQ:ConnectionString");
    string massTransitDatabaseName = builder.Configuration.GetRequiredValue<string>("MongoDB:MassTransitDB");
    string? instanceName = builder.Configuration.GetValue<string>("Instance:Name");
    builder.Services.AddScoped<IRoomEventSender, HubRoomEventSender>();

    builder.Services.AddMassTransit(busConfigurator =>
    {
      busConfigurator.AddConsumer<CleanMessagesConsumer, CleanMessagesConsumerDefinition>();
      busConfigurator.AddConsumer<RoomCreatedConsumer, RoomCreatedConsumerDefinition>();
      busConfigurator.AddConsumer<RoomDeletedConsumer, RoomDeletedConsumerDefinition>();
      busConfigurator.AddConsumer<RoomViewerJoinedConsumer, RoomViewerJoinedConsumerDefinition>();
      busConfigurator.AddConsumer<RoomViewerLeavedConsumer, RoomViewerLeavedConsumerDefinition>();
      busConfigurator.AddConsumer<RoomViewerKickedConsumer, RoomViewerKickedConsumerDefinition>();
      busConfigurator.AddConsumer<UserInfoChangedConsumer, UserInfoChangedConsumerDefinition>();
      busConfigurator.AddConsumer<UserSettingsChangedConsumer, UserSettingsChangedConsumerDefinition>();

      busConfigurator.AddSignalRHub<RoomHub>(cfg =>
      {
        if (instanceName != null) cfg.ServerName = instanceName;
      });

      busConfigurator.AddDelayedMessageScheduler();

      busConfigurator.UsingRabbitMq((ctx, cfg) =>
      {
        cfg.Host(rmq);
        cfg.ConfigureEndpoints(ctx, new KebabCaseEndpointNameFormatter("rooms", false));
        cfg.UseDelayedMessageScheduler();

        cfg.ConfigureJsonSerializerOptions(opt =>
        {
          opt.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
          opt.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
          opt.Converters.Add(new TypeNameJsonConverter<RoomBaseEvent>());
          return opt;
        });
      });

      busConfigurator.AddMongoDbOutbox(o =>
      {
        o.QueryDelay = TimeSpan.FromSeconds(5);
        o.DuplicateDetectionWindow = TimeSpan.FromDays(2);
        o.ClientFactory(provider => provider.GetRequiredService<IMongoClient>());

        o.DatabaseFactory(provider =>
          provider.GetRequiredService<IMongoClient>().GetDatabase(massTransitDatabaseName));
      });
    });
  }
}
using Common.DI.Extensions;
using Films.Infrastructure.Bus.Uploader;
using Films.Infrastructure.Bus.Users;
using MassTransit;
using MongoDB.Driver;

namespace Films.Start.Extensions;

/// <summary>
/// Статический класс для регистрации сервиса MassTransit в контейнере DI 
/// </summary>
public static class MassTransitServices
{
  /// <summary>
  /// Метод регистрирует сервис MassTransit в контейнере DI 
  /// </summary>
  /// <param name="builder">Построитель веб-приложения.</param>
  public static void AddMassTransitServices(this IHostApplicationBuilder builder)
  {
    string rmq = builder.Configuration.GetRequiredValue<string>("RabbitMQ:ConnectionString");
    string massTransitDatabaseName = builder.Configuration.GetRequiredValue<string>("MongoDB:MassTransitDB");

    builder.Services.AddMassTransit(busConfigurator =>
    {
      busConfigurator.AddConsumer<UserRegisteredConsumer, UserRegisteredConsumerDefinition>();
      busConfigurator.AddConsumer<UserInfoChangedConsumer, UserInfoChangedConsumerDefinition>();
      busConfigurator.AddConsumer<VersionDownloadedConsumer, VersionDownloadedConsumerDefinition>();
      busConfigurator.AddDelayedMessageScheduler();

      busConfigurator.UsingRabbitMq((ctx, cfg) =>
      {
        cfg.Host(rmq);
        cfg.ConfigureEndpoints(ctx, new KebabCaseEndpointNameFormatter("films", false));
        cfg.UseDelayedMessageScheduler();
      });

      busConfigurator.AddMongoDbOutbox(o =>
      {
        o.QueryDelay = TimeSpan.FromSeconds(5);
        o.DuplicateDetectionWindow = TimeSpan.FromDays(2);
        o.ClientFactory(provider => provider.GetRequiredService<IMongoClient>());

        o.DatabaseFactory(provider =>
          provider.GetRequiredService<IMongoClient>().GetDatabase(massTransitDatabaseName));

        o.UseBusOutbox();
      });
    });
  }
}
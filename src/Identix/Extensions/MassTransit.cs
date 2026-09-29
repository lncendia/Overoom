using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Application.EmailService;
using Common.Application.Transactions;
using Common.DI.Extensions;
using Common.Infrastructure.JsonConverters;
using Common.Infrastructure.Transactions;
using Identix.Infrastructure.Bus;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace Identix.Extensions;

/// <summary>
/// Статический класс для регистрации сервиса MassTransit в контейнере DI 
/// </summary>
public static class MassTransit
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
      busConfigurator.AddConsumer<SendEmailConsumer, SendEmailConsumerDefinition>();
      busConfigurator.AddDelayedMessageScheduler();

      busConfigurator.UsingRabbitMq((context, cfg) =>
      {
        cfg.Host(rmq);
        cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter("identix", false));
        cfg.UseDelayedMessageScheduler();

        cfg.ConfigureJsonSerializerOptions(opt =>
        {
          opt.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
          opt.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
          opt.Converters.Add(new TypeNameJsonConverter<EmailMessage>());
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

        o.UseBusOutbox();
      });
    });

    builder.Services.AddScoped<MongoTransactionContext>();
    builder.Services.AddScoped<ITransactionContext>(sp => sp.GetRequiredService<MongoTransactionContext>());
    builder.Services.AddScoped<ITransactionManager>(sp => sp.GetRequiredService<MongoTransactionContext>());
  }
}
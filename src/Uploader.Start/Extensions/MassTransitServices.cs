using Common.DI.Extensions;
using MassTransit;
using MassTransit.Contracts.JobService;
using MongoDB.Driver;
using Uploader.Application.Abstractions.Events;
using Uploader.Application.Abstractions.Jobs;
using Uploader.Infrastructure.Bus;
using Uploader.Infrastructure.Bus.Jobs;

namespace Uploader.Start.Extensions;

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
      busConfigurator.AddConsumer<DownloadFilmConsumer>(cfg =>
      {
        cfg.Options<JobOptions<DownloadFilm>>(options =>
        {
          options.SetJobTimeout(TimeSpan.FromHours(5));
          options.SetConcurrentJobLimit(10);
        });
      });

      busConfigurator.AddDelayedMessageScheduler();

      busConfigurator.SetMongoDbSagaRepositoryProvider(cfg =>
      {
        cfg.ClientFactory(provider => provider.GetRequiredService<IMongoClient>());

        cfg.DatabaseFactory(provider =>
          provider.GetRequiredService<IMongoClient>().GetDatabase(massTransitDatabaseName));
      });

      busConfigurator.AddJobSagaStateMachines();
      busConfigurator.AddRequestClient<GetJobState>();

      busConfigurator.UsingRabbitMq((ctx, cfg) =>
      {
        cfg.Host(rmq);
        cfg.UseDelayedMessageScheduler();
        cfg.ConfigureEndpoints(ctx, new KebabCaseEndpointNameFormatter("uploader", false));
      });
    });

    builder.Services.AddScoped<IJobsManager>(sp => new JobsManager(
      sp.GetRequiredService<IMongoClient>().GetDatabase(massTransitDatabaseName),
      sp.GetRequiredService<IPublishEndpoint>(),
      sp.GetRequiredService<IRequestClient<GetJobState>>(),
      sp.GetRequiredService<ILogger<JobsManager>>()));
  }
}
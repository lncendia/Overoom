using Common.DI.Extensions;
using MassTransit;
using MongoDB.Driver;
using Uploader.Application.Abstractions.Events;
using Uploader.Infrastructure.Bus;

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
        // Получаем строку подключения к RabbitMq из конфигурации
        var rmq = builder.Configuration.GetRequiredValue<string>("RabbitMQ:ConnectionString");

        // Получаем имя базы данных MongoDB для MassTransit из конфигурации
        var massTransitDatabaseName = builder.Configuration.GetRequiredValue<string>("MongoDB:MassTransitDB");
        
        // Регистрируем и конфигурируем MassTransit в DI-контейнере
        builder.Services.AddMassTransit(busConfigurator =>
        {
            // Добавляем потребителя (consumer) для обработки сообщений DownloadFilm
            busConfigurator.AddConsumer<DownloadFilmConsumer>(cfg =>
            {
                // Устанавливаем лимит одновременных задач для джобов
                cfg.Options<JobOptions<DownloadFilm>>(options =>
                {
                    options.SetJobTimeout(TimeSpan.FromHours(5));
                    options.SetConcurrentJobLimit(10);
                });
            });

            // Добавляем планировщик отложенных сообщений
            busConfigurator.AddDelayedMessageScheduler();
    
            // Настраиваем провайдер репозитория саг MongoDB
            busConfigurator.SetMongoDbSagaRepositoryProvider(cfg =>
            {
                // Настраиваем фабрику для создания MongoDB клиента
                cfg.ClientFactory(provider => provider.GetRequiredService<IMongoClient>());

                // Настраиваем фабрику для создания базы данных MongoDB
                cfg.DatabaseFactory(provider =>
                    provider.GetRequiredService<IMongoClient>().GetDatabase(massTransitDatabaseName));
            });
    
            // Добавляем автоматы состояний для джоб-саг
            busConfigurator.AddJobSagaStateMachines();
            
            // Настраиваем использование RabbitMQ в качестве брокера сообщений
            busConfigurator.UsingRabbitMq((ctx, cfg) =>
            {
                // Указываем хост RabbitMQ для подключения
                cfg.Host(rmq);
                
                // Использовать планировщик отложенных сообщений
                cfg.UseDelayedMessageScheduler();
                
                // Автоматически настраиваем конечные точки (endpoints) для потребителей
                cfg.ConfigureEndpoints(ctx);
            });
        });
    }
}
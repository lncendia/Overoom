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
        // Получаем строку подключения к RabbitMq
        var rmq = builder.Configuration.GetRequiredValue<string>("RabbitMQ:ConnectionString");

        // Получаем строку подключения к RabbitMq
        var massTransitDatabaseName = builder.Configuration.GetRequiredValue<string>("MongoDB:MassTransitDB");

        // Конфигурируем MassTransit
        builder.Services.AddMassTransit(busConfigurator =>
        {
            // Регистрируем обработчик
            busConfigurator.AddConsumer<UserRegisteredConsumer, UserRegisteredConsumerDefinition>();

            // Регистрируем обработчик
            busConfigurator.AddConsumer<UserInfoChangedConsumer, UserInfoChangedConsumerDefinition>();
            
            // Регистрируем обработчик
            busConfigurator.AddConsumer<VersionDownloadedConsumer, VersionDownloadedConsumerDefinition>();
            
            // Добавляем планировщик отложенных сообщений
            busConfigurator.AddDelayedMessageScheduler();
            
            // Настройка использования RabbitMQ в качестве транспорта для MassTransit.
            busConfigurator.UsingRabbitMq((ctx, cfg) =>
            {
                // Указываем параметры подключения к RabbitMQ-серверу.
                cfg.Host(rmq);
                
                // Настраиваем очереди
                cfg.ConfigureEndpoints(ctx);
                
                // Использовать планировщик отложенных сообщений
                cfg.UseDelayedMessageScheduler();
            });

            // Настройка MongoDB Outbox для обеспечения отказоустойчивости и согласованности при отправке событий.
            busConfigurator.AddMongoDbOutbox(o =>
            {
                // Задаем задержку между запросами к базе данных для проверки неотправленных сообщений.
                o.QueryDelay = TimeSpan.FromSeconds(5);

                // Устанавливаем окно дедупликации, чтобы избежать повторной отправки сообщений.
                o.DuplicateDetectionWindow = TimeSpan.FromDays(2);

                // Настройка фабрики клиентов MongoDB для создания экземпляра IMongoClient.
                o.ClientFactory(provider => provider.GetRequiredService<IMongoClient>());

                // Настройка фабрики базы данных MongoDB.
                o.DatabaseFactory(provider =>
                    provider.GetRequiredService<IMongoClient>().GetDatabase(massTransitDatabaseName));

                // Включаем функциональность Bus Outbox, которая позволяет сохранять сообщения в базе данных перед их отправкой.
                // Это гарантирует, что сообщения будут отправлены только после успешного завершения транзакции.
                o.UseBusOutbox();
            });
        });
    }
}
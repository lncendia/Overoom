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
using Rooms.Infrastructure.Bus.Services;
using Rooms.Infrastructure.Bus.Users;
using Rooms.Infrastructure.Web.Rooms.Hubs;

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
        // Получаем строку подключения к RabbitMQ из конфигурации
        var rmq = builder.Configuration.GetRequiredValue<string>("RabbitMQ:ConnectionString");

        // Получаем имя базы данных MongoDB для MassTransit Outbox из конфигурации
        var massTransitDatabaseName = builder.Configuration.GetRequiredValue<string>("MongoDB:MassTransitDB");

        // Получаем имя экземпляра сервиса из конфигурации
        var instanceName = builder.Configuration.GetValue<string>("Instance:Name");

        // Регистрируем сервис для отправки событий комнат через SignalR Hub
        builder.Services.AddScoped<IRoomEventSender, HubRoomEventSender>();

        // Конфигурируем MassTransit с настройкой шины событий
        builder.Services.AddMassTransit(busConfigurator =>
        {
            // Обработчик события создания комнаты
            busConfigurator.AddConsumer<CleanMessagesConsumer, CleanMessagesConsumerDefinition>();
            
            // Обработчик события создания комнаты
            busConfigurator.AddConsumer<RoomCreatedConsumer, RoomCreatedConsumerDefinition>();

            // Обработчик события удаления комнаты
            busConfigurator.AddConsumer<RoomDeletedConsumer, RoomDeletedConsumerDefinition>();

            // Обработчик события подключения зрителя к комнате
            busConfigurator.AddConsumer<RoomViewerJoinedConsumer, RoomViewerJoinedConsumerDefinition>();

            // Обработчик события выхода зрителя из комнаты
            busConfigurator.AddConsumer<RoomViewerLeavedConsumer, RoomViewerLeavedConsumerDefinition>();

            // Обработчик события исключения зрителя из комнаты
            busConfigurator.AddConsumer<RoomViewerKickedConsumer, RoomViewerKickedConsumerDefinition>();

            // Обработчик события изменения информации о пользователе
            busConfigurator.AddConsumer<UserInfoChangedConsumer, UserInfoChangedConsumerDefinition>();

            // Обработчик события изменения настроек пользователя
            busConfigurator.AddConsumer<UserSettingsChangedConsumer, UserSettingsChangedConsumerDefinition>();

            // Add this for each Hub you have
            busConfigurator.AddSignalRHub<RoomHub>(cfg =>
            {
                if (instanceName != null) cfg.ServerName = instanceName;
            });
            
            // Добавляем планировщик отложенных сообщений
            busConfigurator.AddDelayedMessageScheduler();

            // Настройка использования RabbitMQ в качестве транспорта для MassTransit
            busConfigurator.UsingRabbitMq((ctx, cfg) =>
            {
                // Указываем хост RabbitMQ для подключения
                cfg.Host(rmq);

                // Автоматически настраиваем конечные точки (endpoints) для всех зарегистрированных потребителей
                cfg.ConfigureEndpoints(ctx);
                
                // Использовать планировщик отложенных сообщений
                cfg.UseDelayedMessageScheduler();

                // Настраиваем параметры сериализации JSON для MassTransit
                cfg.ConfigureJsonSerializerOptions(opt =>
                {
                    // Используем camelCase для имен свойств
                    opt.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

                    // Всегда сериализуем все свойства
                    opt.DefaultIgnoreCondition = JsonIgnoreCondition.Never;

                    // Добавляем кастомный конвертер для полиморфной сериализации событий комнаты
                    opt.Converters.Add(new TypeNameJsonConverter<RoomBaseEvent>());
                    return opt;
                });
            });

            // Настройка MongoDB Outbox для обеспечения отказоустойчивости и согласованности при отправке событий
            // Outbox паттерн гарантирует, что события будут отправлены только после успешного сохранения в БД
            busConfigurator.AddMongoDbOutbox(o =>
            {
                // Задаем задержку между запросами к базе данных для проверки неотправленных сообщений (5 секунд)
                o.QueryDelay = TimeSpan.FromSeconds(5);

                // Устанавливаем окно дедупликации в 2 дня для избежания повторной отправки сообщений
                o.DuplicateDetectionWindow = TimeSpan.FromDays(2);

                // Настройка фабрики клиентов MongoDB для создания экземпляра IMongoClient
                o.ClientFactory(provider => provider.GetRequiredService<IMongoClient>());

                // Настройка фабрики базы данных MongoDB для Outbox
                o.DatabaseFactory(provider =>
                    provider.GetRequiredService<IMongoClient>().GetDatabase(massTransitDatabaseName));
            });
        });
    }
}
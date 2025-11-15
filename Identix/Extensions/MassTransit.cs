using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Application.EmailService;
using Common.DI.Extensions;
using Common.Infrastructure.JsonConverters;
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
        // Получаем строку подключения к RabbitMq
        var rmq = builder.Configuration.GetRequiredValue<string>("RabbitMQ:ConnectionString");
        
        // Получаем строку подключения к RabbitMq
        var massTransitDatabaseName = builder.Configuration.GetRequiredValue<string>("MongoDB:MassTransitDB");

        // конфигурируем MassTransit
        builder.Services.AddMassTransit(busConfigurator =>
        {
            // Обработчик события изменения настроек пользователя
            busConfigurator.AddConsumer<SendEmailConsumer, SendEmailConsumerDefinition>();
            
            // Добавляем планировщик отложенных сообщений
            busConfigurator.AddDelayedMessageScheduler();
            
            // Настройка использования RabbitMQ в качестве транспорта для MassTransit.
            busConfigurator.UsingRabbitMq((context, cfg) =>
            {
                // Указываем параметры подключения к RabbitMQ-серверу.
                cfg.Host(rmq);

                // Конфигурируем точки входа для обработки сообщений.
                cfg.ConfigureEndpoints(context);
                
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
                    opt.Converters.Add(new TypeNameJsonConverter<EmailMessage>());
                    return opt;
                });
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
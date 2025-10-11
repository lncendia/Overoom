using Common.DI.Extensions;
using MassTransit;

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
        // Получаем строку подключения к RabbitMq
        var rmq = builder.Configuration.GetRequiredValue<string>("RabbitMQ:ConnectionString");

        // Конфигурируем MassTransit
        builder.Services.AddMassTransit(busConfigurator =>
        {
            // Настройка использования RabbitMQ в качестве транспорта для MassTransit.
            busConfigurator.UsingRabbitMq((ctx, cfg) =>
            {
                // Указываем параметры подключения к RabbitMQ-серверу.
                cfg.Host(rmq);
                
                // Настраиваем очереди
                cfg.ConfigureEndpoints(ctx);
            });
        });
    }
}
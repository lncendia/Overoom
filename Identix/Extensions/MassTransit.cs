using Common.DI.Extensions;
using MassTransit;
using Microsoft.Extensions.Hosting;

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

        // конфигурируем MassTransit
        builder.Services.AddMassTransit(busConfigurator =>
        {
            // Настройка использования RabbitMQ в качестве транспорта для MassTransit.
            busConfigurator.UsingRabbitMq((context, cfg) =>
            {
                // Указываем параметры подключения к RabbitMQ-серверу.
                cfg.Host(rmq);

                // Конфигурируем точки входа для обработки сообщений.
                cfg.ConfigureEndpoints(context);
            });
        });
    }
}
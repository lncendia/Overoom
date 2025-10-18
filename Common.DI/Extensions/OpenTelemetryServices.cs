using Common.Infrastructure.Repositories.Metrics;
using MassTransit.Monitoring;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Common.DI.Extensions;

/// <summary>
/// Статический класс расширений для конфигурации OpenTelemetry сервисов.
/// </summary>
public static class OpenTelemetryServices
{
    /// <summary>
    /// Расширяющий метод для добавления OpenTelemetry метрик в коллекцию служб DI.
    /// </summary>
    /// <param name="services">Коллекция служб Microsoft.Extensions.DependencyInjection.</param>
    /// <param name="serviceName">Имя сервиса, которое будет отображаться в ресурсах OpenTelemetry.</param>
    /// <param name="meters">Список дополнительных Meter'ов (источников метрик), которые необходимо подключить.</param>
    public static void AddOpenTelemetryServices(this IServiceCollection services, string serviceName,
        params string[] meters)
    {
        // Конфигурируем OpenTelemetry
        services.AddOpenTelemetry()
            // Добавляем ресурсы сервиса: имя, версия, окружение и др.
            .ConfigureResource(r => r.AddService(serviceName))
            .WithMetrics(mb =>
            {
                // Системные метрики runtime (.NET сборщик мусора, потоковые таймеры, CPU, память и т.д.)
                mb.AddRuntimeInstrumentation();

                // Метрики исходящих HTTP-запросов через HttpClient
                mb.AddHttpClientInstrumentation();

                // Метрики входящих HTTP-запросов через ASP.NET Core
                mb.AddAspNetCoreInstrumentation();

                // Добавляем кастомный Meter из проекта (InstrumentationOptions)
                mb.AddMeter(InstrumentationOptions.MeterName);

                // Добавляем метрики репозиториев (счётчики транзакций, гистограммы времени выполнения)
                mb.AddMeter(RepositoryMetrics.MeterName);

                // Добавляем все Meter'ы, переданные в параметрах метода
                mb.AddMeter(meters);

                // Экспорт метрик в Prometheus (подключается эндпоинт /metrics)
                mb.AddPrometheusExporter();
            });
    }
}
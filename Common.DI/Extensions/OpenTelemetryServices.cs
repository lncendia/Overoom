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
    services.AddOpenTelemetry()
      .ConfigureResource(r => r.AddService(serviceName))
      .WithMetrics(mb =>
      {
        mb.AddRuntimeInstrumentation();
        mb.AddHttpClientInstrumentation();
        mb.AddAspNetCoreInstrumentation();
        mb.AddMeter(InstrumentationOptions.MeterName);
        mb.AddMeter(RepositoryMetrics.MeterName);
        mb.AddMeter(meters);
        mb.AddPrometheusExporter();
      });
  }
}
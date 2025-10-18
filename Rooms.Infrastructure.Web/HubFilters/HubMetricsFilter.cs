using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;
using Rooms.Infrastructure.Web.Metrics;

namespace Rooms.Infrastructure.Web.HubFilters;

/// <summary>
/// Фильтр для сбора метрик SignalR хабов и измерения времени выполнения методов
/// </summary>
public class HubMetricsFilter : IHubFilter
{
    /// <summary>
    /// Вызывается при каждом вызове метода хаба
    /// </summary>
    /// <param name="invocationContext">Контекст вызова метода хаба</param>
    /// <param name="next">Делегат для вызова следующего фильтра или метода хаба</param>
    /// <returns>Результат выполнения метода хаба</returns>
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        // Создаём таймер для измерения длительности вызова метода
        var sw = Stopwatch.StartNew();

        try
        {
            // Выполняем следующий фильтр или сам метод хаба
            var result = await next(invocationContext);
            return result;
        }
        finally
        {
            // Останавливаем таймер после завершения метода
            sw.Stop();

            // Создаём набор тегов/лейблов для метрики
            var labels = new KeyValuePair<string, object?>[]
            {
                new("method", invocationContext.HubMethodName)
            };

            // Регистрируем длительность метода в гистограмме OpenTelemetry
            RoomsConnectionMetrics.MethodDuration.Record(sw.Elapsed.TotalMilliseconds, labels);
        }
    }
}

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
    var sw = Stopwatch.StartNew();

    try
    {
      object? result = await next(invocationContext);
      return result;
    }
    finally
    {
      sw.Stop();

      var labels = new KeyValuePair<string, object?>[]
      {
        new("method", invocationContext.HubMethodName)
      };

      RoomsConnectionMetrics.MethodDuration.Record(sw.Elapsed.TotalMilliseconds, labels);
    }
  }
}
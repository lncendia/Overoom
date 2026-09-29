using Common.Application.ScopedDictionary;
using Microsoft.AspNetCore.SignalR;
using Rooms.Application.Abstractions;

namespace Rooms.Infrastructure.Web.HubFilters;

/// <summary>
/// Фильтр для централизованной обработки идентификаторов подключений в SignalR хабах
/// </summary>
public class HubConnectionIdFilter : IHubFilter
{
  /// <summary>
  /// Обрабатывает вызов методов хаба, сохраняя ConnectionId в контексте области
  /// </summary>
  /// <param name="invocationContext">Контекст вызова метода хаба</param>
  /// <param name="next">Делегат для вызова следующего фильтра или метода хаба</param>
  public async ValueTask<object?> InvokeMethodAsync(
    HubInvocationContext invocationContext,
    Func<HubInvocationContext, ValueTask<object?>> next)
  {
    IScopedContext context = invocationContext.ServiceProvider.GetRequiredService<IScopedContext>();

    using (context.CreateScope())
    {
      context.Current.Add(Constants.ScopedDictionary.CurrentConnectionIdKey, invocationContext.Context.ConnectionId);

      return await next(invocationContext);
    }
  }
}

using Common.Application.Transactions;

using Microsoft.AspNetCore.Mvc.Filters;

namespace Common.DI.Filters;

/// <summary>
/// Выполняет действия, помеченные <see cref="TransactionalAttribute"/>, в одной транзакции MongoDB.
/// </summary>
/// <remarks>
/// Транзакция фиксируется до формирования ответа, поэтому клиент получает результат только после записи.
/// Необработанное исключение действия откатывает транзакцию.
/// </remarks>
/// <param name="transaction">Текущая транзакция области</param>
public class TransactionFilter(ITransactionManager transaction) : IAsyncActionFilter
{
  /// <inheritdoc/>
  public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
  {
    if (!context.ActionDescriptor.EndpointMetadata.OfType<TransactionalAttribute>().Any())
    {
      await next();
      return;
    }

    await transaction.BeginAsync(context.HttpContext.RequestAborted);

    ActionExecutedContext executed;

    try
    {
      executed = await next();
    }
    catch
    {
      await transaction.AbortAsync(CancellationToken.None);
      throw;
    }

    // Отмена запроса не должна прерывать фиксацию или откат уже выполненной работы
    if (executed is { Exception: not null, ExceptionHandled: false })
      await transaction.AbortAsync(CancellationToken.None);
    else
      await transaction.CommitAsync(CancellationToken.None);
  }
}

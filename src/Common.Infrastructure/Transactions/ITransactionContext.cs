using MongoDB.Driver;

namespace Common.Infrastructure.Transactions;

/// <summary>
/// Текущая транзакция MongoDB в рамках области (HTTP-запрос, обработка сообщения) со стороны хранилища.
/// </summary>
/// <remarks>
/// Открывает и фиксирует транзакцию точка входа через <see cref="Common.Application.Transactions.ITransactionManager"/>,
/// а репозитории и обработчики хранилища только работают в ней.
/// </remarks>
public interface ITransactionContext
{
  /// <summary>
  /// Сессия активной транзакции или null, если транзакция не открыта.
  /// </summary>
  IClientSessionHandle? Session { get; }

  /// <summary>
  /// Выполняет действие после фиксации транзакции, открытой точкой входа, или сразу, если такой транзакции нет.
  /// </summary>
  /// <param name="action">Действие</param>
  /// <param name="token">Токен отмены операции</param>
  Task OnCommittedAsync(Func<CancellationToken, Task> action, CancellationToken token = default);
}

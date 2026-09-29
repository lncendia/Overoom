namespace Common.Application.Transactions;

/// <summary>
/// Управление транзакцией текущей области (HTTP-запрос, вызов хаба) со стороны точки входа.
/// </summary>
public interface ITransactionManager
{
  /// <summary>
  /// Открывает транзакцию. Все чтения и сохранения области будут выполняться в ней.
  /// </summary>
  /// <param name="token">Токен отмены операции</param>
  Task BeginAsync(CancellationToken token = default);

  /// <summary>
  /// Фиксирует транзакцию и выполняет действия, отложенные до фиксации.
  /// </summary>
  /// <param name="token">Токен отмены операции</param>
  Task CommitAsync(CancellationToken token = default);

  /// <summary>
  /// Откатывает транзакцию и отменяет действия, отложенные до фиксации.
  /// </summary>
  /// <param name="token">Токен отмены операции</param>
  Task AbortAsync(CancellationToken token = default);
}

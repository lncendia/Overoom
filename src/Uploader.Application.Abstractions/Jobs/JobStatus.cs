namespace Uploader.Application.Abstractions.Jobs;

/// <summary>
/// Обобщённое состояние фоновой задачи
/// </summary>
public enum JobStatus
{
  /// <summary>
  /// Ожидает запуска: в очереди, ждёт свободного слота или повторной попытки
  /// </summary>
  Pending,

  /// <summary>
  /// Выполняется
  /// </summary>
  Running,

  /// <summary>
  /// Успешно завершена
  /// </summary>
  Completed,

  /// <summary>
  /// Завершилась ошибкой
  /// </summary>
  Faulted,

  /// <summary>
  /// Отменена
  /// </summary>
  Canceled,

  /// <summary>
  /// Состояние не удалось получить
  /// </summary>
  Unknown
}

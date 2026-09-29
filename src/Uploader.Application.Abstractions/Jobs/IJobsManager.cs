using Common.Application.DTOs;
using Uploader.Application.Abstractions.Events;

namespace Uploader.Application.Abstractions.Jobs;

/// <summary>
/// Управление фоновыми задачами загрузки фильмов
/// </summary>
public interface IJobsManager
{
  /// <summary>
  /// Ставит задачу загрузки фильма в очередь
  /// </summary>
  /// <param name="job">Данные задачи</param>
  /// <param name="token">Токен отмены</param>
  /// <returns>Идентификатор задачи</returns>
  Task<Guid> SubmitAsync(DownloadFilm job, CancellationToken token = default);

  /// <summary>
  /// Возвращает страницу задач, новые сначала
  /// </summary>
  /// <param name="skip">Количество пропускаемых задач</param>
  /// <param name="take">Количество возвращаемых задач</param>
  /// <param name="token">Токен отмены</param>
  Task<CountResult<JobDto>> GetAsync(int skip, int take, CancellationToken token = default);

  /// <summary>
  /// Отменяет выполняющуюся или ожидающую задачу
  /// </summary>
  /// <param name="id">Идентификатор задачи</param>
  /// <param name="token">Токен отмены</param>
  /// <exception cref="JobNotFoundException">Если задача не найдена</exception>
  /// <exception cref="JobStateConflictException">Если задача уже завершена</exception>
  Task CancelAsync(Guid id, CancellationToken token = default);

  /// <summary>
  /// Повторно запускает упавшую или отменённую задачу. Уже пройденные этапы пропускаются
  /// </summary>
  /// <param name="id">Идентификатор задачи</param>
  /// <param name="token">Токен отмены</param>
  /// <exception cref="JobNotFoundException">Если задача не найдена</exception>
  /// <exception cref="JobStateConflictException">Если задача не упала и не отменена</exception>
  Task RetryAsync(Guid id, CancellationToken token = default);

  /// <summary>
  /// Удаляет завершённую, упавшую или отменённую задачу из хранилища
  /// </summary>
  /// <param name="id">Идентификатор задачи</param>
  /// <param name="token">Токен отмены</param>
  /// <exception cref="JobNotFoundException">Если задача не найдена</exception>
  /// <exception cref="JobStateConflictException">Если задача ещё выполняется или ожидает запуска</exception>
  Task DeleteAsync(Guid id, CancellationToken token = default);
}

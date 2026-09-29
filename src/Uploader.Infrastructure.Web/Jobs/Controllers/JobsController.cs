using Common.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uploader.Application.Abstractions.Jobs;
using Uploader.Infrastructure.Web.Jobs.InputModels;

namespace Uploader.Infrastructure.Web.Jobs.Controllers;

/// <summary>
/// Контроллер администрирования фоновых задач загрузки фильмов
/// </summary>
/// <param name="jobs">Сервис управления фоновыми задачами</param>
[ApiController]
[Authorize(Policy = "admin")]
[Route("api/jobs")]
public class JobsController(IJobsManager jobs) : ControllerBase
{
  /// <summary>
  /// Получить список задач, новые сначала
  /// </summary>
  /// <param name="model">Параметры пагинации</param>
  /// <param name="token">Токен для отмены операции</param>
  /// <returns>Страница задач и их общее количество</returns>
  /// <response code="200">Запрос успешно выполнен</response>
  /// <response code="400">Некорректные параметры пагинации</response>
  /// <response code="401">Пользователь не авторизован</response>
  /// <response code="403">Недостаточно прав</response>
  [HttpGet]
  public Task<CountResult<JobDto>> GetJobs([FromQuery] GetJobsInputModel model, CancellationToken token = default)
  {
    return jobs.GetAsync(model.Skip, model.Take, token);
  }

  /// <summary>
  /// Отменить выполняющуюся или ожидающую задачу
  /// </summary>
  /// <param name="id">Идентификатор задачи</param>
  /// <param name="token">Токен для отмены операции</param>
  /// <response code="202">Команда отмены отправлена</response>
  /// <response code="404">Задача не найдена</response>
  /// <response code="409">Задача уже завершена</response>
  [HttpPost("{id:guid}/cancel")]
  public async Task<IActionResult> Cancel(Guid id, CancellationToken token = default)
  {
    await jobs.CancelAsync(id, token);
    return Accepted();
  }

  /// <summary>
  /// Повторно запустить упавшую или отменённую задачу
  /// </summary>
  /// <param name="id">Идентификатор задачи</param>
  /// <param name="token">Токен для отмены операции</param>
  /// <response code="202">Команда повтора отправлена</response>
  /// <response code="404">Задача не найдена</response>
  /// <response code="409">Задача не упала и не отменена</response>
  [HttpPost("{id:guid}/retry")]
  public async Task<IActionResult> Retry(Guid id, CancellationToken token = default)
  {
    await jobs.RetryAsync(id, token);
    return Accepted();
  }

  /// <summary>
  /// Удалить завершённую, упавшую или отменённую задачу
  /// </summary>
  /// <param name="id">Идентификатор задачи</param>
  /// <param name="token">Токен для отмены операции</param>
  /// <response code="202">Команда удаления отправлена</response>
  /// <response code="404">Задача не найдена</response>
  /// <response code="409">Задача ещё выполняется или ожидает запуска</response>
  [HttpDelete("{id:guid}")]
  public async Task<IActionResult> Delete(Guid id, CancellationToken token = default)
  {
    await jobs.DeleteAsync(id, token);
    return Accepted();
  }
}

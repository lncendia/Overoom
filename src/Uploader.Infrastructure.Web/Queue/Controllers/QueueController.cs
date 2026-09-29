using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uploader.Application.Abstractions.Events;
using Uploader.Application.Abstractions.Jobs;
using Uploader.Infrastructure.Web.Queue.InputModels;

namespace Uploader.Infrastructure.Web.Queue.Controllers;

/// <summary>
/// Контроллер постановки задач загрузки фильмов в очередь
/// </summary>
/// <param name="jobs">Сервис управления фоновыми задачами</param>
[ApiController]
[Authorize(Policy = "admin")]
[Route("api/queue")]
public class QueueController(IJobsManager jobs) : ControllerBase
{
  /// <summary>
  /// Поставить задачу загрузки фильма в очередь
  /// </summary>
  /// <param name="model">Данные задачи</param>
  /// <param name="token">Токен для отмены операции</param>
  /// <returns>Идентификатор созданной задачи</returns>
  /// <response code="201">Задача поставлена в очередь</response>
  /// <response code="400">Некорректные входные данные</response>
  /// <response code="401">Пользователь не авторизован</response>
  /// <response code="403">Недостаточно прав</response>
  [HttpPost]
  public async Task<IActionResult> Queue([FromBody] QueueInputModel model, CancellationToken token = default)
  {
    var command = new DownloadFilm
    {
      FilmRecord = new FilmRecord
      {
        Id = model.FilmId,
        Season = model.Season,
        Episode = model.Episode,
        Resolution = model.Resolution,
        Version = model.Version!,
      },
      MagnetUri = model.MagnetUri!,
      FileName = model.FileName,
      FilmTitle = model.FilmTitle,
    };

    Guid id = await jobs.SubmitAsync(command, token);

    return Created((string?)null, new { id });
  }
}

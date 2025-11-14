using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uploader.Application.Abstractions.Events;
using Uploader.Infrastructure.Web.Queue.InputModels;

namespace Uploader.Infrastructure.Web.Queue.Controllers;

/// <summary>
/// Контроллер для работы с комментариями к фильмам
/// </summary>
/// <param name="publishEndpoint">Сервис для публикации событий.</param>
[ApiController]
[Authorize(Policy = "admin")]
[Route("api/queue")]
public class QueueController(IPublishEndpoint publishEndpoint) : ControllerBase
{
    /// <summary>
    /// Добавить новый комментарий к фильму
    /// </summary>
    /// <param name="model">Данные нового комментария</param>
    /// <param name="token">Токен для отмены операции</param>
    /// <returns>Созданный комментарий</returns>
    /// <response code="201">Комментарий успешно создан</response>
    /// <response code="400">Некорректные входные данные</response>
    /// <response code="401">Пользователь не авторизован</response>
    [Authorize]
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
        };

        await publishEndpoint.Publish(command, token);

        // Возвращаем статус 201 Created
        return Created();
    }
}
using Common.Domain.Enums;
using Films.Application.Abstractions.Queries.Media;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Films.Infrastructure.Web.Media.Controllers;

/// <summary>
/// Контроллер для работы с фотографиями пользователей, хранящимися в Amazon S3
/// </summary>
/// <param name="mediator">Медиатор для обработки CQRS-запросов</param>
[ApiController]
[Route("api")]
public class MediaController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Получить файл фотографии пользователя из S3 хранилища
    /// </summary>
    /// <param name="key">Путь к файлу в формате "user/thumbnail/{GUID}"</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Файловый поток с фотографией</returns>
    /// <response code="200">Возвращает файл фотографии</response>
    /// <response code="400">Некорректный формат ключа или параметра запроса</response>
    /// <response code="404">Файл не найден в хранилище</response>
    [HttpGet("photo")]
    public async Task<IActionResult> GetFile(
        [FromQuery] string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Отправляем запрос на получение фото через медиатор
            var result = await mediator.Send(new GetPhotoQuery(key), cancellationToken);

            // Возвращаем файл как результат
            return File(result.Stream, result.ContentType, result.FileName);
        }
        catch (FileNotFoundException)
        {
            // Файл не найден в S3
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            // Некорректный формат ключа или другие ошибки валидации
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Получить файл фотографии пользователя из S3 хранилища
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <param name="filmId"></param>
    /// <param name="version"></param>
    /// <param name="season"></param>
    /// <param name="episode"></param>
    /// <param name="resolution"></param>
    /// <returns>Файловый поток с фотографией</returns>
    /// <response code="200">Возвращает файл фотографии</response>
    /// <response code="400">Некорректный формат ключа или параметра запроса</response>
    /// <response code="404">Файл не найден в хранилище</response>
    [HttpGet("hls/{filmId:guid}/{version}/{fileName}")]
    [HttpGet("hls/{filmId:guid}/{version}/{resolution}/{fileName}")]
    [HttpGet("hls/{filmId:guid}/{season:int}/{episode:int}/{version}/{fileName}")]
    [HttpGet("hls/{filmId:guid}/{season:int}/{episode:int}/{version}/{resolution}/{fileName}")]
    public async Task<IActionResult> GetPart(
        Guid filmId,
        string version,
        int? season,
        int? episode,
        string? resolution,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Преобразуем входную модель в CQRS запрос
            var query = new GetFilmPartQuery
            {
                Id = filmId,
                Version = version,
                Season = season,
                Episode = episode,
                FileName = fileName
            };

            if (Enum.TryParse<FilmResolution>(resolution, out var resolutionEnum))
            {
                query.Resolution = resolutionEnum;
            }

            // Отправляем запрос на получение фото через медиатор
            var result = await mediator.Send(query, cancellationToken);

            // Возвращаем файл как результат
            return File(result.Stream, result.ContentType, result.FileName);
        }
        catch (FileNotFoundException)
        {
            // Файл не найден в S3
            return NotFound();
        }
    }
}
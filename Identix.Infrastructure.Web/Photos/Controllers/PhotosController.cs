using MediatR;
using Microsoft.AspNetCore.Mvc;
using Identix.Application.Abstractions.Queries;

namespace Identix.Infrastructure.Web.Photos.Controllers;

/// <summary>
/// Контроллер для работы с фотографиями пользователей, хранящимися в Amazon S3
/// </summary>
/// <param name="mediator">Медиатор для обработки CQRS-запросов</param>
[ApiController]
[Route("[controller]")]
public class PhotosController(ISender mediator) : ControllerBase
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
    [HttpGet]
    public async Task<IActionResult> GetFile(
        [FromQuery] string key, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Отправляем запрос на получение фото через медиатор
            var result = await mediator.Send(new PhotoQuery(key), cancellationToken);

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
}
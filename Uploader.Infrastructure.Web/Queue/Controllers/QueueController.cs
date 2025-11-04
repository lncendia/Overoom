using AutoMapper;
using Uploader.Application.Abstractions.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uploader.Infrastructure.Web.Queue.InputModels;

namespace Uploader.Infrastructure.Web.Queue.Controllers;

/// <summary>
/// Контроллер для работы с комментариями к фильмам
/// </summary>
/// <param name="mediator">Mediator для обработки CQRS запросов</param>
/// <param name="mapper">AutoMapper для преобразования объектов</param>
[ApiController]
[Authorize(Policy = "admin")]
[Route("api/queue")]
public class QueueController(ISender mediator, IMapper mapper) : ControllerBase
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
        // Создаем команду, передавая ID пользователя и фильма
        var command = mapper.Map<EnqueueDownloadCommand>(model);

        // Отправляем команду через медиатор
        await mediator.Send(command, token);

        // Возвращаем статус 201 Created
        return Created();
    }
}
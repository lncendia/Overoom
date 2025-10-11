using AutoMapper;
using Films.Application.Abstractions.Commands.Films;
using Films.Infrastructure.Web.FilmsManagement.InputModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Films.Infrastructure.Web.FilmsManagement.Controllers;

/// <summary>
/// Контроллер для управления фильмами (администраторские функции)
/// </summary>
/// <param name="mediator">Mediator для обработки CQRS команд</param>
/// <param name="mapper">AutoMapper для преобразования объектов</param>
[ApiController]
// [Authorize(Policy = "admin")]
[Route("api/admin/films")]
public class FilmsManagementController(ISender mediator, IMapper mapper) : ControllerBase
{
    /// <summary>
    /// Добавить новый фильм
    /// </summary>
    /// <param name="model">Данные для создания фильма</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Результат операции</returns>
    /// <response code="201">Фильм успешно создан</response>
    /// <response code="400">Некорректные данные</response>
    /// <response code="401">Не авторизован</response>
    /// <response code="403">Нет прав администратора</response>
    [HttpPost]
    public async Task<IActionResult> CreateFilm(AddFilmInputModel model, CancellationToken token = default)
    {
        // Преобразуем входную модель в команду
        var command = mapper.Map<AddFilmCommand>(model);

        // Отправляем команду через медиатор
        var result = await mediator.Send(command, token);

        // Возвращаем статус 201 с Location header
        return CreatedAtAction(
            actionName: "GetById",
            controllerName: "Films",
            new { id = result },
            result);
    }

    /// <summary>
    /// Обновить информацию о фильме
    /// </summary>
    /// <param name="id">Идентификатор фильма</param>
    /// <param name="model">Данные для обновления</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Фильм успешно обновлен</response>
    /// <response code="400">Некорректные данные</response>
    /// <response code="401">Не авторизован</response>
    /// <response code="403">Нет прав администратора</response>
    /// <response code="404">Фильм не найден</response>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateFilm(
        Guid id,
        ChangeFilmInputModel model,
        CancellationToken token = default)
    {
        // Преобразуем входную модель в команду
        var command = mapper.Map<ChangeFilmCommand>(model);
        command.Id = id;

        await mediator.Send(command, token);

        return NoContent();
    }

    /// <summary>
    /// Обновить постер фильма
    /// </summary>
    /// <param name="id">Идентификатор фильма</param>
    /// <param name="model">Данные для обновления</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Фильм успешно обновлен</response>
    /// <response code="400">Некорректные данные</response>
    /// <response code="401">Не авторизован</response>
    /// <response code="403">Нет прав администратора</response>
    /// <response code="404">Фильм не найден</response>
    [HttpPut("{id:guid}/poster")]
    public async Task<IActionResult> UpdatePoster(
        Guid id,
        ChangeFilmPosterInputModel model,
        CancellationToken token = default)
    {
        // Преобразуем входную модель в команду
        var command = mapper.Map<ChangeFilmPosterCommand>(model);
        command.Id = id;

        try
        {
            await mediator.Send(command, token);

            return NoContent();
        }
        finally
        {
            // Закрываем поток с файлом вручную после выполнения команды
            await command.Poster.File.DisposeAsync();
        }
    }

    /// <summary>
    /// Удалить фильм
    /// </summary>
    /// <param name="id">Идентификатор фильма</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Фильм успешно удален</response>
    /// <response code="401">Не авторизован</response>
    /// <response code="403">Нет прав администратора</response>
    /// <response code="404">Фильм не найден</response>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFilm(Guid id, CancellationToken token = default)
    {
        await mediator.Send(new DeleteFilmCommand { Id = id }, token);
        return NoContent();
    }
}
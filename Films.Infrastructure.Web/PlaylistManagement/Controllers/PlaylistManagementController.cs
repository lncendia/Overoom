using AutoMapper;
using Films.Application.Abstractions.Commands.Playlists;
using Films.Infrastructure.Web.PlaylistManagement.InputModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Films.Infrastructure.Web.PlaylistManagement.Controllers;

/// <summary>
/// Контроллер для управления плейлистами (администраторские функции)
/// </summary>
/// <param name="mediator">Mediator для обработки CQRS команд</param>
/// <param name="mapper">AutoMapper для преобразования объектов</param>
[ApiController]
[Authorize(Policy = "admin")]
[Route("api/admin/playlists")]
public class PlaylistManagementController(ISender mediator, IMapper mapper) : ControllerBase
{
    /// <summary>
    /// Создать новый плейлист
    /// </summary>
    /// <param name="model">Данные для создания плейлиста</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Результат операции</returns>
    /// <response code="201">Плейлист успешно создан</response>
    /// <response code="400">Некорректные данные</response>
    /// <response code="401">Не авторизован</response>
    /// <response code="403">Нет прав администратора</response>
    [HttpPost]
    public async Task<IActionResult> CreatePlaylist([FromBody] CreatePlaylistInputModel model,
        CancellationToken token = default)
    {
        var command = mapper.Map<CreatePlaylistCommand>(model);

        var result = await mediator.Send(command, token);

        return CreatedAtAction(
            actionName: "GetById",
            controllerName: "Playlists",
            new { id = result },
            result);
    }

    /// <summary>
    /// Обновить плейлист
    /// </summary>
    /// <param name="id">Идентификатор плейлиста</param>
    /// <param name="model">Данные для обновления</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Плейлист успешно обновлен</response>
    /// <response code="400">Некорректные данные</response>
    /// <response code="401">Не авторизован</response>
    /// <response code="403">Нет прав администратора</response>
    /// <response code="404">Плейлист не найден</response>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdatePlaylist(
        Guid id,
        [FromBody] ChangePlaylistInputModel model,
        CancellationToken token = default)
    {
        var command = mapper.Map<ChangePlaylistCommand>(model);
        command.Id = id;

        await mediator.Send(command, token);

        // Возвращаем статус 204 No Content
        return NoContent();
    }

    /// <summary>
    /// Обновить постер подборки
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
        ChangePlaylistPosterInputModel model,
        CancellationToken token = default)
    {
        // Преобразуем входную модель в команду
        var command = mapper.Map<ChangePlaylistPosterCommand>(model);
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
    /// Удалить плейлист
    /// </summary>
    /// <param name="id">Идентификатор плейлиста</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Плейлист успешно удален</response>
    /// <response code="401">Не авторизован</response>
    /// <response code="403">Нет прав администратора</response>
    /// <response code="404">Плейлист не найден</response>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePlaylist(Guid id, CancellationToken token = default)
    {
        await mediator.Send(new DeletePlaylistCommand { Id = id }, token);

        // Возвращаем статус 204 No Content
        return NoContent();
    }
}
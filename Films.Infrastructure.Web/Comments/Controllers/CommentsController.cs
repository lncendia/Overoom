using AutoMapper;
using Common.Application.DTOs;
using Films.Application.Abstractions.Commands.Comments;
using Films.Application.Abstractions.DTOs.Comments;
using Films.Application.Abstractions.Queries.Comments;
using Films.Infrastructure.Web.Comments.InputModels;
using Films.Infrastructure.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Films.Infrastructure.Web.Comments.Controllers;

/// <summary>
/// Контроллер для работы с комментариями к фильмам
/// </summary>
/// <param name="mediator">Mediator для обработки CQRS запросов</param>
/// <param name="mapper">AutoMapper для преобразования объектов</param>
[ApiController]
[Route("api/films/{filmId:guid}/comments")]
public class CommentsController(ISender mediator, IMapper mapper) : ControllerBase
{
    /// <summary>
    /// Получить комментарии к фильму
    /// </summary>
    /// <param name="filmId">Идентификатор фильма</param>
    /// <param name="model">Параметры пагинации и фильтрации</param>
    /// <param name="token">Токен для отмены операции</param>
    /// <returns>Список комментариев с информацией о пагинации</returns>
    /// <response code="200">Запрос успешно выполнен</response>
    /// <response code="400">Некорректные входные данные или невалидный запрос</response>
    /// <response code="500">Возникла ошибка на сервере</response>
    [HttpGet]
    public async Task<CountResult<CommentDto>> GetComments(
        Guid filmId,
        [FromQuery] GetCommentsInputModel model,
        CancellationToken token = default)
    {
        // Создаем запрос, добавляя ID фильма из URL
        var query = mapper.Map<GetFilmCommentsQuery>(model);
        query.FilmId = filmId;

        // Отправляем запрос через медиатор и возвращаем результат
        return await mediator.Send(query, token);
    }

    /// <summary>
    /// Удалить комментарий
    /// </summary>
    /// <param name="filmId">Идентификатор фильма</param>
    /// <param name="commentId">Идентификатор комментария</param>
    /// <param name="token">Токен для отмены операции</param>
    /// <returns>Пустой ответ при успешном удалении</returns>
    /// <response code="204">Комментарий успешно удален</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Нет прав на удаление комментария</response>
    /// <response code="404">Комментарий не найден</response>
    [Authorize]
    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(Guid filmId, Guid commentId, CancellationToken token = default)
    {
        var command = new RemoveCommentCommand
        {
            CommentId = commentId,
            UserId = User.GetId()
        };

        // Отправляем команду через медиатор
        await mediator.Send(command, token);

        // Возвращаем статус 204 No Content
        return NoContent();
    }

    /// <summary>
    /// Добавить новый комментарий к фильму
    /// </summary>
    /// <param name="filmId">Идентификатор фильма</param>
    /// <param name="model">Данные нового комментария</param>
    /// <param name="token">Токен для отмены операции</param>
    /// <returns>Созданный комментарий</returns>
    /// <response code="201">Комментарий успешно создан</response>
    /// <response code="400">Некорректные входные данные</response>
    /// <response code="401">Пользователь не авторизован</response>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddComment(
        Guid filmId,
        [FromBody] AddCommentInputModel model,
        CancellationToken token = default)
    {
        // Создаем команду, передавая ID пользователя и фильма
        var command = new AddCommentCommand
        {
            UserId = User.GetId(),
            FilmId = filmId,
            Text = model.Text!
        };

        // Отправляем команду через медиатор
        var result = await mediator.Send(command, token);

        // Возвращаем статус 201 Created с созданным комментарием
        return Created((string?)null, value: new { id = result });
    }
}
using AutoMapper;
using Common.Application.DTOs;
using Films.Application.Abstractions.Commands.Profile;
using Films.Application.Abstractions.DTOs.Films;
using Films.Application.Abstractions.DTOs.Profile;
using Films.Application.Abstractions.Queries.Profile;
using Films.Infrastructure.Web.Extensions;
using Films.Infrastructure.Web.Profile.InputModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Films.Infrastructure.Web.Profile.Controllers;

/// <summary>
/// Контроллер для работы с профилем пользователя
/// </summary>
/// <param name="mediator">Mediator для обработки CQRS запросов</param>
/// <param name="mapper">AutoMapper для преобразования объектов</param>
[Authorize]
[ApiController]
[Route("api/profile")]
public class ProfileController(ISender mediator, IMapper mapper) : ControllerBase
{
    /// <summary>
    /// Получить профиль текущего пользователя
    /// </summary>
    /// <param name="token">Токен отмены</param>
    /// <response code="200">Запрос успешно выполнен</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="500">Возникла ошибка на сервере</response>
    [HttpGet]
    public async Task<UserProfileDto> GetProfile(CancellationToken token = default)
    {
        // Преобразуем входную модель в CQRS запрос
        var query = new GetUserProfileQuery { Id = User.GetId() };

        // Отправляем запрос через медиатор и возвращаем результат
        return await mediator.Send(query, token);
    }

    /// <summary>
    /// Получить список фильмов в "хочу посмотреть" пользователя
    /// </summary>
    /// <param name="token">Токен отмены</param>
    /// <response code="200">Запрос успешно выполнен</response> 
    /// <response code="400">Некорректные входные данные или невалидный запрос</response>
    /// <response code="500">Возникла ошибка на сервере</response>
    [HttpGet("watchlist")]
    [Authorize]
    public Task<IReadOnlyList<FilmShortDto>> GetWatchlist(CancellationToken token = default)
    {
        // Создаем запрос с ID текущего пользователя
        var query = new GetUserWatchlistQuery { Id = User.GetId() };

        // Отправляем запрос через медиатор и возвращаем результат
        return mediator.Send(query, token);
    }

    /// <summary>
    /// Получить историю просмотров пользователя
    /// </summary>
    /// <param name="token">Токен отмены</param>
    /// <response code="200">Запрос успешно выполнен</response> 
    /// <response code="400">Некорректные входные данные или невалидный запрос</response>
    /// <response code="500">Возникла ошибка на сервере</response>
    [HttpGet("history")]
    [Authorize]
    public Task<IReadOnlyList<FilmShortDto>> GetHistory(CancellationToken token = default)
    {
        // Создаем запрос с ID текущего пользователя
        var query = new GetUserHistoryQuery { Id = User.GetId() };

        // Отправляем запрос через медиатор и возвращаем результат
        return mediator.Send(query, token);
    }

    /// <summary>
    /// Получить оценку текущего пользователя для фильма
    /// </summary>
    /// <param name="model">Данные пагинации</param>
    /// <param name="token">Токен отмены операции</param>
    /// <returns>Данные об оценке</returns>
    /// <response code="200">Оценка найдена</response>
    /// <response code="400">Некорректные параметры запроса</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Доступ запрещен</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("ratings")]
    public async Task<CountResult<UserRatingDto>> GetFilmRating(
        [FromQuery] GetRatingsInputModel model,
        CancellationToken token = default)
    {
        // Преобразуем входную модель в CQRS запрос
        var query = new GetUserRatingsQuery { Id = User.GetId(), Skip = model.Skip, Take = model.Take };
        
        // Устанавливаем ID пользователя из контекста авторизации
        query.Id = User.GetId();

        // Отправляем запрос через медиатор и возвращаем результат
        return await mediator.Send(query, token);
    }
    
    /// <summary>
    /// Добавить фильм в историю просмотров
    /// </summary>
    /// <param name="filmId">Идентификатор фильма</param>
    /// <param name="token">Токен отмены</param>
    /// <response code="204">Фильм успешно добавлен в историю</response>
    /// <response code="400">Некорректный идентификатор фильма</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="500">Возникла ошибка на сервере</response>
    [HttpPost("history/{filmId:guid}")]
    public async Task<IActionResult> AddFilmToHistory(Guid filmId, CancellationToken token = default)
    {
        // Преобразуем входную модель в CQRS запрос
        var command = new AddToHistoryCommand
        {
            UserId = User.GetId(),
            FilmId = filmId
        };

        // Отправляем запрос через медиатор и возвращаем результат
        await mediator.Send(command, token);

        // Возвращаем статус 204 No Content
        return NoContent();
    }


    /// <summary>
    /// Добавить фильм в список "Хочу посмотреть"
    /// </summary>
    /// <param name="filmId">Идентификатор фильма</param>
    /// <param name="token">Токен отмены</param>
    /// <response code="204">Фильм успешно добавлен в список</response>
    /// <response code="400">Некорректный идентификатор фильма</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="500">Возникла ошибка на сервере</response>
    [HttpPost("watchlist/{filmId:guid}")]
    public async Task<IActionResult> ToggleWatchListCommand(Guid filmId, CancellationToken token = default)
    {
        // Преобразуем входную модель в CQRS запрос
        var command = new ToggleWatchListCommand
        {
            UserId = User.GetId(),
            FilmId = filmId
        };

        // Отправляем запрос через медиатор и возвращаем результат
        await mediator.Send(command, token);

        // Возвращаем статус 204 No Content
        return NoContent();
    }

    /// <summary>
    /// Изменить настройки уведомлений пользователя
    /// </summary>
    /// <param name="model">Новые настройки уведомлений</param>
    /// <param name="token">Токен отмены</param>
    /// <response code="204">Настройки успешно обновлены</response>
    /// <response code="400">Некорректные данные настроек</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="500">Возникла ошибка на сервере</response>
    [HttpPut("settings")]
    public async Task<IActionResult> UpdateNotificationSettings([FromBody] UpdateRoomSettingsInputModel model,
        CancellationToken token = default)
    {
        // Преобразуем входную модель в CQRS запрос
        var command = mapper.Map<UpdateRoomSettingsCommand>(model);
        command.UserId = User.GetId();

        // Отправляем запрос через медиатор и возвращаем результат
        await mediator.Send(command, token);

        // Возвращаем статус 204 No Content
        return NoContent();
    }
}
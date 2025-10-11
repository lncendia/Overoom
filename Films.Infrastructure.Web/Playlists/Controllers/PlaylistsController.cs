using AutoMapper;
using Common.Application.DTOs;
using Films.Application.Abstractions.DTOs.Playlists;
using Films.Application.Abstractions.Queries.Playlists;
using Films.Infrastructure.Web.Playlists.InputModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Films.Infrastructure.Web.Playlists.Controllers;

/// <summary>
/// Контроллер для работы с плейлистами
/// </summary>
/// <param name="mediator">Mediator для обработки запросов</param>
/// <param name="mapper">AutoMapper для преобразования объектов</param>
[ApiController]
[Route("api/playlists")]
public class PlaylistsController(ISender mediator, IMapper mapper) : ControllerBase
{
    /// <summary>
    /// Поиск плейлистов с пагинацией
    /// </summary>
    /// <param name="model">Параметры поиска и пагинации</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Результат поиска с информацией о пагинации</returns>
    /// <response code="200">Запрос успешно выполнен</response>
    /// <response code="400">Некорректные параметры запроса</response>
    [HttpGet]
    public async Task<CountResult<PlaylistDto>> Search(
        [FromQuery] SearchPlaylistsInputModel model,
        CancellationToken token = default)
    {
        // Преобразуем входную модель в запрос
        var query = mapper.Map<SearchPlaylistsQuery>(model);

        // Получаем данные
        return await mediator.Send(query, token);
    }

    /// <summary>
    /// Получить плейлист по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор плейлиста</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Информация о плейлисте</returns>
    /// <response code="200">Плейлист найден</response>
    /// <response code="404">Плейлист не найден</response>
    [HttpGet("{id:guid}")]
    public async Task<PlaylistDto> GetById(Guid id, CancellationToken token = default)
    {
        // Преобразуем входную модель в запрос
        return await mediator.Send(new GetPlaylistByIdQuery { Id = id }, token);
    }
}
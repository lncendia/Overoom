using AutoMapper;
using Common.Application.DTOs;
using Common.Application.Transactions;
using Films.Application.Abstractions.Commands.Ratings;
using Films.Application.Abstractions.DTOs.Films;
using Films.Application.Abstractions.Queries.Films;
using Films.Infrastructure.Web.Extensions;
using Films.Infrastructure.Web.Films.InputModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Films.Infrastructure.Web.Films.Controllers;

/// <summary>
/// Контроллер для работы с фильмами
/// </summary>
/// <param name="mediator">Mediator для обработки CQRS запросов</param>
/// <param name="mapper">AutoMapper для преобразования объектов</param>
[ApiController]
[Route("api/films")]
public class FilmsController(ISender mediator, IMapper mapper) : ControllerBase
{
  /// <summary>
  /// Получить популярные фильмы
  /// </summary>
  /// <param name="model">Параметры фильтрации и пагинации</param>
  /// <param name="token">Токен отмены</param>
  /// <response code="200">Запрос успешно выполнен</response> 
  /// <response code="400">Некорректные входные данные или невалидный запрос</response>
  /// <response code="500">Возникла ошибка на сервере</response>
  [HttpGet("popular")]
  public async Task<IEnumerable<FilmShortDto>> GetPopular([FromQuery] GetPopularFilmsInputModel model,
    CancellationToken token = default)
  {
    GetPopularFilmsQuery? query = mapper.Map<GetPopularFilmsQuery>(model);

    return await mediator.Send(query, token);
  }

  /// <summary>
  /// Поиск фильмов
  /// </summary>
  /// <param name="model">Параметры поиска</param>
  /// <param name="token">Токен отмены</param>
  /// <response code="200">Запрос успешно выполнен</response> 
  /// <response code="400">Некорректные входные данные или невалидный запрос</response>
  /// <response code="500">Возникла ошибка на сервере</response>
  [HttpGet("search")]
  public Task<CountResult<FilmShortDto>> Search([FromQuery] SearchFilmsInputModel model,
    CancellationToken token = default)
  {
    SearchFilmsQuery? query = mapper.Map<SearchFilmsQuery>(model);

    return mediator.Send(query, token);
  }

  /// <summary>
  /// Получить фильм по идентификатору
  /// </summary>
  /// <param name="id">Идентификатор фильма</param>
  /// <param name="token">Токен отмены</param>
  /// <response code="200">Запрос успешно выполнен</response> 
  /// <response code="400">Некорректные входные данные или невалидный запрос</response>
  /// <response code="500">Возникла ошибка на сервере</response>
  [HttpGet("{id:guid}")]
  public Task<FilmDto> GetById(Guid id, CancellationToken token = default)
  {
    var query = new GetFilmByIdQuery
    {
      Id = id,
      UserId = User.Identity is { IsAuthenticated: true } ? User.GetId() : null
    };

    return mediator.Send(query, token);
  }

  /// <summary>
  /// Поставить оценку фильму
  /// </summary>
  /// <param name="filmId">Идентификатор фильма</param>
  /// <param name="model">Данные оценки</param>
  /// <param name="token">Токен отмены операции</param>
  /// <returns>Данные об оценке</returns>
  /// <response code="201">Оценка успешно создана</response>
  /// <response code="400">Некорректные входные данные</response>
  /// <response code="401">Пользователь не авторизован</response>
  /// <response code="409">Оценка уже существует</response>
  /// <response code="500">Внутренняя ошибка сервера</response>
  [Authorize]
  [Transactional]
  [HttpPost("{filmId:guid}/ratings")]
  public async Task<IActionResult> RateFilm(
    Guid filmId,
    [FromBody] RateFilmInputModel model,
    CancellationToken token = default)
  {
    var command = new SetRatingCommand
    {
      FilmId = filmId,
      UserId = User.GetId(),
      Score = model.Score
    };

    await mediator.Send(command, token);

    return NoContent();
  }
}
using AutoMapper;
using Common.Application.DTOs;
using Common.Application.Transactions;
using Films.Application.Abstractions.Commands.Rooms;
using Films.Application.Abstractions.DTOs.Rooms;
using Films.Application.Abstractions.Queries.Rooms;
using Films.Infrastructure.Web.Extensions;
using Films.Infrastructure.Web.Rooms.InputModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Films.Infrastructure.Web.Rooms.Controllers;

/// <summary>
/// Контроллер для работы с комнатами просмотра фильмов
/// </summary>
/// <param name="mediator">Mediator для обработки CQRS запросов</param>
/// <param name="mapper">AutoMapper для преобразования объектов</param>
[ApiController]
[Route("api/rooms")]
public class RoomsController(ISender mediator, IMapper mapper) : ControllerBase
{
  /// <summary>
  /// Создать новую комнату для просмотра фильма
  /// </summary>
  /// <param name="model">Данные для создания комнаты</param>
  /// <param name="token">Токен отмены операции</param>
  /// <returns>Данные созданной комнаты</returns>
  /// <response code="201">Комната успешно создана</response>
  /// <response code="400">Некорректные входные данные</response>
  /// <response code="401">Пользователь не авторизован</response>
  /// <response code="403">Пользователь не имеет прав</response>
  /// <response code="500">Внутренняя ошибка сервера</response>
  [Authorize]
  [Transactional]
  [HttpPost]
  public async Task<IActionResult> Create([FromBody] CreateRoomInputModel model, CancellationToken token = default)
  {
    CreateRoomCommand? command = mapper.Map<CreateRoomCommand>(model);
    command.UserId = User.GetId();
    Guid room = await mediator.Send(command, token);

    return CreatedAtAction(
      actionName: "GetRoom",
      controllerName: "Rooms",
      new { id = room },
      room);
  }

  /// <summary>
  /// Подключиться к существующей комнате
  /// </summary>
  /// <param name="id">Идентификатор комнаты</param>
  /// <param name="model">Данные для подключения</param>
  /// <param name="token">Токен отмены операции</param>
  /// <returns>Данные комнаты</returns>
  /// <response code="204">Успешное подключение к комнате</response>
  /// <response code="400">Некорректные входные данные</response>
  /// <response code="401">Пользователь не авторизован</response>
  /// <response code="403">Доступ запрещен</response>
  /// <response code="404">Комната не найдена</response>
  /// <response code="500">Внутренняя ошибка сервера</response>
  [Authorize]
  [Transactional]
  [HttpPost("{id:guid}/join")]
  public async Task<ActionResult> Join(Guid id, [FromBody] JoinRoomInputModel model,
    CancellationToken token = default)
  {
    var command = new JoinRoomCommand
    {
      UserId = User.GetId(),
      RoomId = id,
      Code = model.Code
    };

    await mediator.Send(command, token);

    return NoContent();
  }

  /// <summary>
  /// Исключает зрителя из комнаты
  /// </summary>
  /// <param name="id">Идентификатор комнаты</param>
  /// <param name="targetId">Идентификатор исключаемого зрителя</param>
  /// <param name="token">Токен отмены операции</param>
  /// <returns>Пустой ответ при успешном выполнении</returns>
  /// <response code="204">Зритель успешно исключен из комнаты</response>
  /// <response code="400">Некорректные входные данные (неверный формат GUID)</response>
  /// <response code="401">Пользователь не авторизован</response>
  /// <response code="403">Доступ запрещен</response>
  /// <response code="404">Комната или целевой зритель не найдены</response>
  /// <response code="500">Внутренняя ошибка сервера</response>
  [Authorize]
  [Transactional]
  [HttpPost("{id:guid}/kick/{targetId:guid}")]
  public async Task<ActionResult> Kick(Guid id, Guid targetId, CancellationToken token = default)
  {
    var command = new KickViewerCommand
    {
      UserId = User.GetId(),
      RoomId = id,
      TargetId = targetId
    };

    await mediator.Send(command, token);

    return NoContent();
  }

  /// <summary>
  /// Покинуть комнату
  /// </summary>
  /// <param name="id">Идентификатор комнаты</param>
  /// <param name="token">Токен отмены операции</param>
  /// <returns>Пустой ответ при успешном выполнении</returns>
  /// <response code="204">Успешный выход из комнаты</response>
  /// <response code="400">Некорректные входные данные</response>
  /// <response code="401">Пользователь не авторизован</response>
  /// <response code="403">Доступ запрещен</response>
  /// <response code="404">Комната не найдена</response>
  /// <response code="500">Внутренняя ошибка сервера</response>
  [Authorize]
  [Transactional]
  [HttpPost("{id:guid}/leave")]
  public async Task<ActionResult> Leave(Guid id, CancellationToken token = default)
  {
    var command = new LeaveRoomCommand
    {
      UserId = User.GetId(),
      RoomId = id
    };

    await mediator.Send(command, token);

    return NoContent();
  }

  /// <summary>
  /// Удалить комнату
  /// </summary>
  /// <param name="id">Идентификатор комнаты</param>
  /// <param name="token">Токен отмены операции</param>
  /// <returns>Пустой ответ при успешном выполнении</returns>
  /// <response code="204">Успешное удаление комнаты</response>
  /// <response code="400">Некорректные входные данные</response>
  /// <response code="401">Пользователь не авторизован</response>
  /// <response code="403">Доступ запрещен</response>
  /// <response code="404">Комната не найдена</response>
  /// <response code="500">Внутренняя ошибка сервера</response>
  /// <remarks>
  /// Если выходящий пользователь был владельцем комнаты, 
  /// комната будет автоматически удалена после выхода всех участников
  /// </remarks>
  [Authorize]
  [Transactional]
  [HttpDelete("{id:guid}")]
  public async Task<ActionResult> Delete(Guid id, CancellationToken token = default)
  {
    var command = new DeleteRoomCommand
    {
      UserId = User.GetId(),
      RoomId = id
    };

    await mediator.Send(command, token);

    return NoContent();
  }

  /// <summary>
  /// Поиск комнат для просмотра фильмов
  /// </summary>
  /// <param name="model">Параметры поиска и пагинации</param>
  /// <param name="token">Токен отмены операции</param>
  /// <returns>Список комнат с информацией о пагинации</returns>
  /// <response code="200">Запрос успешно выполнен</response>
  /// <response code="400">Некорректные параметры поиска</response>
  /// <response code="500">Внутренняя ошибка сервера</response>
  [HttpGet]
  public async Task<CountResult<RoomShortDto>> Search([FromQuery] SearchRoomsInputModel model,
    CancellationToken token = default)
  {
    SearchRoomsQuery? query = mapper.Map<SearchRoomsQuery>(model);

    return await mediator.Send(query, token);
  }

  /// <summary>
  /// Получить список комнат текущего пользователя
  /// </summary>
  /// <param name="token">Токен отмены операции</param>
  /// <returns>Список комнат пользователя</returns>
  /// <response code="200">Запрос успешно выполнен</response>
  /// <response code="401">Пользователь не авторизован</response>
  /// <response code="403">Доступ запрещен</response>
  /// <response code="500">Внутренняя ошибка сервера</response>
  [Authorize]
  [HttpGet("my")]
  public async Task<IReadOnlyList<RoomShortDto>> GetUserRooms(CancellationToken token = default)
  {
    var query = new GetUserRoomsQuery { UserId = User.GetId() };

    return await mediator.Send(query, token);
  }

  /// <summary>
  /// Получить детальную информацию о комнате
  /// </summary>
  /// <param name="id">Идентификатор комнаты</param>
  /// <param name="token">Токен отмены операции</param>
  /// <returns>Данные комнаты</returns>
  /// <response code="200">Комната найдена</response>
  /// <response code="401">Неавторизованный доступ к приватной комнате</response>
  /// <response code="403">Доступ запрещен</response>
  /// <response code="404">Комната не найдена</response>
  /// <response code="500">Внутренняя ошибка сервера</response>
  [HttpGet("{id:guid}")]
  public async Task<RoomDto> GetRoom(Guid id, CancellationToken token = default)
  {
    var query = new GetRoomByIdQuery
    {
      Id = id,
      UserId = User.Identity?.IsAuthenticated == true ? User.GetId() : null
    };

    return await mediator.Send(query, token);
  }

  /// <summary>
  /// Получить код подключения к комнате
  /// </summary>
  /// <param name="id">Идентификатор комнаты</param>
  /// <param name="token">Токен отмены операции</param>
  /// <returns>Код подключения к комнате</returns>
  /// <response code="200">Комната найдена</response>
  /// <response code="401">Неавторизованный доступ к приватной комнате</response>
  /// <response code="403">Доступ запрещен</response>
  /// <response code="404">Комната не найдена</response>
  /// <response code="500">Внутренняя ошибка сервера</response>
  [HttpGet("{id:guid}/code")]
  [Authorize]
  public async Task<string?> GetCode(Guid id, CancellationToken token = default)
  {
    var query = new GetRoomCodeQuery
    {
      RoomId = id,
      UserId = User.GetId()
    };

    return await mediator.Send(query, token);
  }
}
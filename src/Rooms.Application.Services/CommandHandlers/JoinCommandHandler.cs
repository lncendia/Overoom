using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.DTOs;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Application.Services.CommandHandlers;

/// <summary>
/// Обработчик команды на подключение к комнате
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class JoinCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<JoinCommand, RoomDto>
{
  /// <summary>
  /// Обрабатывает команду подключения к комнате
  /// </summary>
  /// <param name="request">Команда с данными о пользователе и комнате</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>Данные комнаты после подключения</returns>
  /// <exception cref="RoomNotFoundException">Если комната с указанным ID не найдена</exception>
  public async Task<RoomDto> Handle(JoinCommand request, CancellationToken cancellationToken)
  {
    Room? room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);
    if (room == null) throw new RoomNotFoundException(request.RoomId);

    room.Join(new Viewer(request.Viewer.Id));
    room.SetUserName(request.Viewer.Id, request.Viewer.UserName);
    room.SetPhoto(request.Viewer.Id, request.Viewer.PhotoKey);
    room.SetSettings(request.Viewer.Id, request.Viewer.Settings);
    await unitOfWork.RoomRepository.Value.UpdateAsync(room, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

    return new RoomDto
    {
      Id = room.Id,
      OwnerId = room.Owner.Id,
      FilmId = room.FilmId,
      IsSerial = room.IsSerial,
      Viewers = [.. room.Viewers.Values.Select(ViewerDto.Create)]
    };
  }
}
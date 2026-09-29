using Films.Application.Abstractions.Commands.Rooms;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using Films.Domain.Rooms;

using MediatR;

namespace Films.Application.Services.CommandHandlers.Rooms;

/// <summary>
/// Обработчик команды отключения пользователя от комнаты
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class LeaveRoomCommandHandler(IUnitOfWork unitOfWork)
  : IRequestHandler<LeaveRoomCommand>
{
  /// <summary>
  /// Обрабатывает запрос на отключение пользователя от комнаты
  /// </summary>
  /// <param name="request">Команда с данными для отключения (ID комнаты и ID пользователя)</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="RoomNotFoundException">Если комната с указанным ID не найдена</exception>
  public async Task Handle(LeaveRoomCommand request, CancellationToken cancellationToken)
  {
    Room? room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);
    if (room == null) throw new RoomNotFoundException(request.RoomId);

    room.Leave(request.UserId);
    await unitOfWork.RoomRepository.Value.UpdateAsync(room, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);
  }
}

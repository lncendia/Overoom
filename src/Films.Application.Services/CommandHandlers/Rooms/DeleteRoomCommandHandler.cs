using Films.Application.Abstractions.Commands.Rooms;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using Films.Domain.Rooms;

using MediatR;

namespace Films.Application.Services.CommandHandlers.Rooms;

/// <summary>
/// Обработчик команды удаления комнаты для просмотра фильма
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class DeleteRoomCommandHandler(IUnitOfWork unitOfWork)
  : IRequestHandler<DeleteRoomCommand>
{
  /// <summary>
  /// Удаляет комнату для совместного просмотра фильма
  /// </summary>
  /// <param name="request">Данные для удаления комнаты (ID комнаты, ID пользователя)</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>Информация о созданной комнате</returns>
  /// <exception cref="RoomNotFoundException">Если указанная комната не найдена</exception>
  public async Task Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
  {
    Room? room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);
    if (room == null) throw new RoomNotFoundException(request.RoomId);

    room.CanDelete(request.UserId);
    await unitOfWork.RoomRepository.Value.DeleteAsync(room, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);
  }
}
using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms;
using Rooms.Domain.Rooms.Specifications;

namespace Rooms.Application.Services.CommandHandlers;

/// <summary>
/// Обработчик команды изменения данных пользователя во всех комнатах
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class ChangeViewersCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ChangeViewersCommand>
{
  /// <summary>
  /// Обновляет основные данные пользователя во всех комнатах
  /// </summary>
  /// <param name="request">Команда с новыми данными пользователя</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  public async Task Handle(ChangeViewersCommand request, CancellationToken cancellationToken)
  {
    IReadOnlyList<Room> rooms = await unitOfWork.RoomRepository.Value.FindAsync(new RoomsByViewerSpecification(request.UserId),
      cancellationToken: cancellationToken);

    foreach (Room room in rooms)
    {
      room.SetUserName(request.UserId, request.UserName);
      room.SetPhoto(request.UserId, request.PhotoKey);
      await unitOfWork.RoomRepository.Value.UpdateAsync(room, cancellationToken);
    }

    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
  }
}

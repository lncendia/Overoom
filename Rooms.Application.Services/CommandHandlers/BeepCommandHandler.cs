using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms;

namespace Rooms.Application.Services.CommandHandlers;

/// <summary>
/// Обработчик команды на отправку звукового сигнала другому пользователю
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class BeepCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<BeepCommand>
{
  /// <summary>
  /// Обрабатывает команду отправки звукового сигнала
  /// </summary>
  /// <param name="request">Команда с данными об отправителе и получателе</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="RoomNotFoundException">Если комната с указанным ID не найдена</exception>
  public async Task Handle(BeepCommand request, CancellationToken cancellationToken)
  {
    Room? room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);
    if (room == null) throw new RoomNotFoundException(request.RoomId);

    room.Beep(request.ViewerId, request.TargetId);
    await unitOfWork.RoomRepository.Value.UpdateAsync(room, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
  }
}

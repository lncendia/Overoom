using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms;

namespace Rooms.Application.Services.CommandHandlers;

/// <summary>
/// Обработчик команды на установку позиции воспроизведения (таймлайна)
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class SetTimeLineCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<SetTimeLineCommand>
{
  /// <summary>
  /// Обрабатывает команду установки таймлайна воспроизведения
  /// </summary>
  /// <param name="request">Команда с данными о позиции воспроизведения</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="RoomNotFoundException">Если комната с указанным ID не найдена</exception>
  public async Task Handle(SetTimeLineCommand request, CancellationToken cancellationToken)
  {
    Room? room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);
    if (room == null) throw new RoomNotFoundException(request.RoomId);

    room.SetTimeLine(request.ViewerId, request.TimeLine);
    await unitOfWork.RoomRepository.Value.UpdateAsync(room, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
  }
}

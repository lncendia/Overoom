using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms;

namespace Rooms.Application.Services.CommandHandlers;

/// <summary>
/// Обработчик команды на управление звуком (включение/выключение)
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class SetVolumeCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<SetVolumeCommand>
{
  /// <summary>
  /// Обрабатывает команду управления звуком
  /// </summary>
  /// <param name="request">Команда с данными о состоянии звука</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="RoomNotFoundException">Если комната с указанным ID не найдена</exception>
  public async Task Handle(SetVolumeCommand request, CancellationToken cancellationToken)
  {
    Room? room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);
    if (room == null) throw new RoomNotFoundException(request.RoomId);

    room.SetMuted(request.ViewerId, request.Muted);
    await unitOfWork.RoomRepository.Value.UpdateAsync(room, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
  }
}
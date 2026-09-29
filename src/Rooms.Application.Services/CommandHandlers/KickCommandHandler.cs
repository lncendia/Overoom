using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms;

namespace Rooms.Application.Services.CommandHandlers;

/// <summary>
/// Обработчик команды на исключение пользователя из комнаты
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class KickCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<KickCommand>
{
  /// <summary>
  /// Обрабатывает команду исключения пользователя из комнаты
  /// </summary>
  /// <param name="request">Команда с данными о пользователе и комнате</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="RoomNotFoundException">Если комната с указанным ID не найдена</exception>
  public async Task Handle(KickCommand request, CancellationToken cancellationToken)
  {
    Room? room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);
    if (room == null) throw new RoomNotFoundException(request.RoomId);

    room.Kick(request.ViewerId);
    await unitOfWork.RoomRepository.Value.UpdateAsync(room, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
  }
}

using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms;

namespace Rooms.Application.Services.CommandHandlers;

/// <summary>
/// Обработчик команды на установку статуса подключения зрителя
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class SetOnlineCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<SetOnlineCommand>
{
  /// <summary>
  /// Обрабатывает команду установки онлайн-статуса пользователя
  /// </summary>
  /// <param name="request">Команда с данными о комнате и пользователе</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="RoomNotFoundException">Если комната с указанным ID не найдена</exception>
  public async Task Handle(SetOnlineCommand request, CancellationToken cancellationToken)
  {
    Room? room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);
    if (room == null) throw new RoomNotFoundException(request.RoomId);

    room.SetOnline(request.ViewerId, request.Online);
    await unitOfWork.RoomRepository.Value.UpdateAsync(room, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
  }
}

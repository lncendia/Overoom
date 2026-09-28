using Common.Infrastructure.Repositories;
using Films.Application.Abstractions.Commands.Rooms;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using Films.Domain.Rooms;

using MediatR;

namespace Films.Application.Services.CommandHandlers.Rooms;

/// <summary>
/// Обработчик команды блокировки зрителя в комнате
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class KickViewerCommandHandler(ISessionHandlerFactory sessionHandlerFactory, IUnitOfWork unitOfWork)
  : IRequestHandler<KickViewerCommand>
{
  /// <summary>
  /// Выполняет блокировку зрителя в указанной комнате
  /// </summary>
  /// <param name="request">Команда с данными для блокировки (ID комнаты и ID пользователя)</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="RoomNotFoundException">Выбрасывается, если комната не найдена</exception>
  public async Task Handle(KickViewerCommand request, CancellationToken cancellationToken)
  {
    Room? room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);
    if (room == null) throw new RoomNotFoundException(request.RoomId);

    room.Kick(request.UserId, request.TargetId);
    await unitOfWork.RoomRepository.Value.UpdateAsync(room, cancellationToken);
    await unitOfWork.SaveChangesAsync(sessionHandlerFactory.CreateOutboxHandler(), cancellationToken);
  }
}
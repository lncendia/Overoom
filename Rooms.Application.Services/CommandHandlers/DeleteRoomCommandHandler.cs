using Common.Infrastructure.Repositories;
using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms;

namespace Rooms.Application.Services.CommandHandlers;

/// <summary>
/// Обработчик команды на удаление комнаты
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class DeleteRoomCommandHandler(IUnitOfWork unitOfWork, ISessionHandlerFactory sessionHandlerFactory)
  : IRequestHandler<DeleteRoomCommand>
{
  /// <summary>
  /// Обрабатывает команду удаления комнаты
  /// </summary>
  /// <param name="request">Команда с идентификатором комнаты</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="RoomNotFoundException">Если комната с указанным ID не найдена</exception>
  public async Task Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
  {
    Room? room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);
    if (room == null) throw new RoomNotFoundException(request.RoomId);

    await unitOfWork.RoomRepository.Value.DeleteAsync(room, cancellationToken);
    await unitOfWork.SaveChangesAsync(sessionHandlerFactory.CreateInboxHandler(), cancellationToken: cancellationToken);
  }
}
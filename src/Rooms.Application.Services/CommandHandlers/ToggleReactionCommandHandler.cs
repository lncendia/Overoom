using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Domain.Messages;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms;

namespace Rooms.Application.Services.CommandHandlers;

/// <summary>
/// Обработчик команды на установку или снятие реакции на сообщение
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class ToggleReactionCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ToggleReactionCommand>
{
  /// <summary>
  /// Обрабатывает команду установки или снятия реакции
  /// </summary>
  /// <param name="request">Команда с данными о реакции</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="RoomNotFoundException">Если комната не найдена</exception>
  /// <exception cref="MessageNotFoundException">Если сообщение не найдено</exception>
  public async Task Handle(ToggleReactionCommand request, CancellationToken cancellationToken)
  {
    Room? room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);
    if (room == null) throw new RoomNotFoundException(request.RoomId);

    Message? message = await unitOfWork.MessageRepository.Value.GetAsync(request.MessageId, cancellationToken);
    if (message == null) throw new MessageNotFoundException(request.MessageId);

    message.ToggleReaction(room, request.ViewerId, request.Reaction);
    await unitOfWork.MessageRepository.Value.UpdateAsync(message, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);
  }
}

using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Domain.Messages;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms;

namespace Rooms.Application.Services.CommandHandlers;

/// <summary>
/// Обработчик команды на отправку сообщения в комнату
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class SendMessageCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<SendMessageCommand>
{
  /// <summary>
  /// Обрабатывает команду отправки сообщения
  /// </summary>
  /// <param name="request">Команда с данными о сообщении</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="RoomNotFoundException">Если комната с указанным ID не найдена</exception>
  public async Task Handle(SendMessageCommand request, CancellationToken cancellationToken)
  {
    Room? room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);
    if (room == null) throw new RoomNotFoundException(request.RoomId);

    var message = new Message(room, request.ViewerId, request.Message);
    await unitOfWork.MessageRepository.Value.AddAsync(message, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
  }
}

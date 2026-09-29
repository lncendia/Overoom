using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms;

namespace Rooms.Application.Services.CommandHandlers;

/// <summary>
/// Обработчик команды на установку режима полного экрана
/// Управляет полноэкранным режимом просмотра для пользователя
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class SetFullscreenCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<SetFullscreenCommand>
{
  /// <summary>
  /// Обрабатывает команду установки полноэкранного режима
  /// </summary>
  /// <param name="request">Команда с данными о состоянии полноэкранного режима</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="RoomNotFoundException">Если комната с указанным ID не найдена</exception>
  public async Task Handle(SetFullscreenCommand request, CancellationToken cancellationToken)
  {
    Room? room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);
    if (room == null) throw new RoomNotFoundException(request.RoomId);

    room.SetFullScreen(request.ViewerId, request.Fullscreen);
    await unitOfWork.RoomRepository.Value.UpdateAsync(room, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
  }
}
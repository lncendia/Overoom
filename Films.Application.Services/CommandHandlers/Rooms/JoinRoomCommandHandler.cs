using Common.Infrastructure.Repositories;
using Films.Application.Abstractions.Commands.Rooms;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using Films.Domain.Rooms;
using Films.Domain.Users;

using MediatR;

namespace Films.Application.Services.CommandHandlers.Rooms;

/// <summary>
/// Обработчик команды подключения пользователя к комнате
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class JoinRoomCommandHandler(ISessionHandlerFactory sessionHandlerFactory, IUnitOfWork unitOfWork)
  : IRequestHandler<JoinRoomCommand>
{
  /// <summary>
  /// Обрабатывает запрос на подключение к комнате
  /// </summary>
  /// <param name="request">Команда подключения (ID комнаты, ID пользователя и код доступа)</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>Информация о комнате (DTO)</returns>
  /// <exception cref="RoomNotFoundException">Если комната не найдена</exception>
  /// <exception cref="UserNotFoundException">Если пользователь не найден</exception>
  public async Task Handle(JoinRoomCommand request, CancellationToken cancellationToken)
  {
    Room? room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);
    if (room == null) throw new RoomNotFoundException(request.RoomId);

    User? user = await unitOfWork.UserRepository.Value.GetAsync(request.UserId, cancellationToken);
    if (user == null) throw new UserNotFoundException(request.UserId);

    room.Join(user, request.Code);
    await unitOfWork.RoomRepository.Value.UpdateAsync(room, cancellationToken);
    await unitOfWork.SaveChangesAsync(sessionHandlerFactory.CreateOutboxHandler(), cancellationToken);
  }
}
using Common.Infrastructure.Repositories;
using Films.Application.Abstractions.Commands.Rooms;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Films;
using Films.Domain.Repositories;
using Films.Domain.Rooms;
using Films.Domain.Users;

using MediatR;

namespace Films.Application.Services.CommandHandlers.Rooms;

/// <summary>
/// Обработчик команды создания новой комнаты для просмотра фильма
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class CreateRoomCommandHandler(ISessionHandlerFactory sessionHandlerFactory, IUnitOfWork unitOfWork)
  : IRequestHandler<CreateRoomCommand, Guid>
{
  /// <summary>
  /// Создает новую комнату для совместного просмотра фильма
  /// </summary>
  /// <param name="request">Данные для создания комнаты (ID фильма, ID пользователя, настройки доступа)</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>Информация о созданной комнате</returns>
  /// <exception cref="FilmNotFoundException">Если указанный фильм не найден</exception>
  /// <exception cref="UserNotFoundException">Если пользователь не найден</exception>
  public async Task<Guid> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
  {
    Film? film = await unitOfWork.FilmRepository.Value.GetAsync(request.FilmId, cancellationToken);
    if (film == null) throw new FilmNotFoundException(request.FilmId);

    User? user = await unitOfWork.UserRepository.Value.GetAsync(request.UserId, cancellationToken);
    if (user == null) throw new UserNotFoundException(request.UserId);

    var room = new Room(
      id: Guid.NewGuid(),
      user: user,
      film: film,
      isOpen: request.IsOpen);

    await unitOfWork.RoomRepository.Value.AddAsync(room, cancellationToken);
    await unitOfWork.SaveChangesAsync(sessionHandlerFactory.CreateOutboxHandler(), cancellationToken);

    return room.Id;
  }
}
using Common.Infrastructure.Repositories;
using Films.Application.Abstractions.Commands.Rooms;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using Films.Domain.Rooms;
using MediatR;

namespace Films.Application.Services.CommandHandlers.Rooms;

/// <summary>
/// Обработчик команды создания новой комнаты для просмотра фильма
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class CreateRoomCommandHandler(ISessionHandlerFactory sessionHandlerFactory, IUnitOfWork unitOfWork) : IRequestHandler<CreateRoomCommand, Guid>
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
        // Получаем информацию о фильме по ID из запроса
        var film = await unitOfWork.FilmRepository.Value.GetAsync(request.FilmId, cancellationToken);

        // Проверяем существование фильма
        if (film == null) throw new FilmNotFoundException(request.FilmId);

        // Получаем данные пользователя-создателя комнаты
        var user = await unitOfWork.UserRepository.Value.GetAsync(request.UserId, cancellationToken);

        // Проверяем существование пользователя
        if (user == null) throw new UserNotFoundException(request.UserId);
        
        // Создаем новую комнату с уникальным идентификатором
        var room = new Room(
            id: Guid.NewGuid(),
            user: user,
            film: film,
            isOpen: request.IsOpen);

        // Добавляем комнату в репозиторий
        await unitOfWork.RoomRepository.Value.AddAsync(room, cancellationToken);

        // Сохраняем изменения в базе данных
        await unitOfWork.SaveChangesAsync(sessionHandlerFactory.CreateOutboxHandler(), cancellationToken);
        
        // Возвращаем идентификатор созданной комнаты
        return room.Id;
    }
}
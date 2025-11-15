using Common.Infrastructure.Repositories;
using Films.Application.Abstractions.Commands.Rooms;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using MediatR;

namespace Films.Application.Services.CommandHandlers.Rooms;

/// <summary>
/// Обработчик команды удаления комнаты для просмотра фильма
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class DeleteRoomCommandHandler(ISessionHandlerFactory sessionHandlerFactory, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteRoomCommand>
{
    /// <summary>
    /// Удаляет комнату для совместного просмотра фильма
    /// </summary>
    /// <param name="request">Данные для удаления комнаты (ID комнаты, ID пользователя)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о созданной комнате</returns>
    /// <exception cref="RoomNotFoundException">Если указанная комната не найдена</exception>
    public async Task Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
        // Получаем комнату по идентификатору из запроса
        var room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);

        // Проверяем существование комнаты
        if (room == null) throw new RoomNotFoundException(request.RoomId);

        // Проверка, может ли текущий пользователь удалить комнату
        room.CanDelete(request.UserId);

        // Удаляем комнату из репозитория
        await unitOfWork.RoomRepository.Value.DeleteAsync(request.RoomId, cancellationToken);

        // Сохраняем изменения в базе данных
        await unitOfWork.SaveChangesAsync(sessionHandlerFactory.CreateOutboxHandler(), cancellationToken);
    }
}
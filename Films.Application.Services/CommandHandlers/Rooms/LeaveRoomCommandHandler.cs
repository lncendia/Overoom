using Common.Infrastructure.Repositories;
using Films.Application.Abstractions.Commands.Rooms;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using MediatR;

namespace Films.Application.Services.CommandHandlers.Rooms;

/// <summary>
/// Обработчик команды отключения пользователя от комнаты
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class LeaveRoomCommandHandler(ISessionHandlerFactory sessionHandlerFactory, IUnitOfWork unitOfWork)
    : IRequestHandler<LeaveRoomCommand>
{
    /// <summary>
    /// Обрабатывает запрос на отключение пользователя от комнаты
    /// </summary>
    /// <param name="request">Команда с данными для отключения (ID комнаты и ID пользователя)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <exception cref="RoomNotFoundException">Если комната с указанным ID не найдена</exception>
    public async Task Handle(LeaveRoomCommand request, CancellationToken cancellationToken)
    {
        // Получаем комнату по идентификатору из запроса
        var room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);

        // Проверяем существование комнаты
        if (room == null) throw new RoomNotFoundException(request.RoomId);

        // Выполняем отключение пользователя от комнаты
        room.Leave(request.UserId);

        // Обновляем состояние комнаты в репозитории
        await unitOfWork.RoomRepository.Value.UpdateAsync(room, cancellationToken);

        // Сохраняем изменения в базе данных
        await unitOfWork.SaveChangesAsync(sessionHandlerFactory.CreateOutboxHandler(), cancellationToken);
    }
}
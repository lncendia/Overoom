using Common.Infrastructure.Repositories;
using Films.Application.Abstractions.Commands.Rooms;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using MediatR;

namespace Films.Application.Services.CommandHandlers.Rooms;

/// <summary>
/// Обработчик команды блокировки зрителя в комнате
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class KickViewerCommandHandler(ISessionHandlerFactory sessionHandlerFactory, IUnitOfWork unitOfWork) : IRequestHandler<KickViewerCommand>
{
    /// <summary>
    /// Выполняет блокировку зрителя в указанной комнате
    /// </summary>
    /// <param name="request">Команда с данными для блокировки (ID комнаты и ID пользователя)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <exception cref="RoomNotFoundException">Выбрасывается, если комната не найдена</exception>
    public async Task Handle(KickViewerCommand request, CancellationToken cancellationToken)
    {
        // Получаем комнату по идентификатору из запроса
        var room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);
        
        // Проверяем существование комнаты
        if (room == null) throw new RoomNotFoundException(request.RoomId);

        // Выполняем блокировку пользователя в комнате
        room.Kick(request.UserId, request.TargetId);

        // Обновляем данные комнаты в репозитории
        await unitOfWork.RoomRepository.Value.UpdateAsync(room, cancellationToken);
        
        // Сохраняем изменения в базе данных
        await unitOfWork.SaveChangesAsync(sessionHandlerFactory.CreateOutboxHandler(), cancellationToken);
    }
}
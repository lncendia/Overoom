using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Specifications;

namespace Rooms.Application.Services.CommandHandlers;

/// <summary>
/// Обработчик команды изменения настроек пользователя во всех комнатах, где он присутствует
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class ChangeViewersSettingsCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ChangeViewersSettingsCommand>
{
    /// <summary>
    /// Обрабатывает команду изменения настроек пользователя, обновляя его данные во всех активных комнатах
    /// </summary>
    /// <param name="request">Команда, содержащая идентификатор пользователя и новые настройки</param>
    /// <param name="cancellationToken">Токен отмены для асинхронной операции</param>
    /// <returns>Задача, представляющая асинхронную операцию обработки команды</returns>
    public async Task Handle(ChangeViewersSettingsCommand request, CancellationToken cancellationToken)
    {
        // Получаем все комнаты, где пользователь присутствует в качестве зрителя
        var rooms = await unitOfWork.RoomRepository.Value.FindAsync(
            new RoomsByViewerSpecification(request.UserId),
            cancellationToken: cancellationToken);
        
        // Обновляем настройки пользователя в каждой найденной комнате
        foreach (var room in rooms)
        {
            room.SetSettings(request.UserId, request.Settings);
            await unitOfWork.RoomRepository.Value.UpdateAsync(room, cancellationToken);
        }
        
        // Сохраняем все изменения в базе данных единой транзакцией
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
    }
}
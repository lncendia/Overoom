using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Domain.Repositories;

namespace Rooms.Application.Services.CommandHandlers;

/// <summary>
/// Обработчик команды на удаление комнаты
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class DeleteRoomCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteRoomCommand>
{
    /// <summary>
    /// Обрабатывает команду удаления комнаты
    /// </summary>
    /// <param name="request">Команда с идентификатором комнаты</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <exception cref="RoomNotFoundException">Если комната с указанным ID не найдена</exception>
    public async Task Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
        // Получаем комнату по ID из репозитория
        var room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);

        // Проверяем существование комнаты
        if (room == null) throw new RoomNotFoundException(request.RoomId);

        // Удаляем комнату из репозитория
        await unitOfWork.RoomRepository.Value.DeleteAsync(room.Id, cancellationToken);

        // Сохраняем изменения в базе данных
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
    }
}
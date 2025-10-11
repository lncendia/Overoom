using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Domain.Repositories;

namespace Rooms.Application.Services.CommandHandlers;

/// <summary>
/// Обработчик команды на исключение пользователя из комнаты
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class KickCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<KickCommand>
{
    /// <summary>
    /// Обрабатывает команду исключения пользователя из комнаты
    /// </summary>
    /// <param name="request">Команда с данными о пользователе и комнате</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <exception cref="RoomNotFoundException">Если комната с указанным ID не найдена</exception>
    public async Task Handle(KickCommand request, CancellationToken cancellationToken)
    {
        // Получаем комнату по ID из репозитория
        var room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);

        // Проверяем существование комнаты
        if (room == null) throw new RoomNotFoundException(request.RoomId);

        // Исключаем пользователя из комнаты
        room.Kick(request.ViewerId);

        // Обновляем комнату в репозитории
        await unitOfWork.RoomRepository.Value.UpdateAsync(room, cancellationToken);

        // Сохраняем изменения в базе данных
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
    }
}
using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Domain.Repositories;

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
        // Получаем комнату по ID из репозитория
        var room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);

        // Проверяем существование комнаты
        if (room == null) throw new RoomNotFoundException(request.RoomId);

        // Устанавливаем флаг полноэкранного режима для пользователя
        room.SetFullScreen(request.ViewerId, request.Fullscreen);

        // Обновляем комнату в репозитории
        await unitOfWork.RoomRepository.Value.UpdateAsync(room, cancellationToken);

        // Сохраняем изменения в базе данных
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
    }
}
using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Application.Abstractions.DTOs;
using Rooms.Application.Abstractions.Exceptions;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Application.Services.CommandHandlers;

/// <summary>
/// Обработчик команды на подключение к комнате
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class JoinCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<JoinCommand, RoomDto>
{
    /// <summary>
    /// Обрабатывает команду подключения к комнате
    /// </summary>
    /// <param name="request">Команда с данными о пользователе и комнате</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Данные комнаты после подключения</returns>
    /// <exception cref="RoomNotFoundException">Если комната с указанным ID не найдена</exception>
    public async Task<RoomDto> Handle(JoinCommand request, CancellationToken cancellationToken)
    {
        // Получаем комнату по ID из репозитория
        var room = await unitOfWork.RoomRepository.Value.GetAsync(request.RoomId, cancellationToken);

        // Проверяем существование комнаты
        if (room == null) throw new RoomNotFoundException(request.RoomId);

        // Подключаем пользователя к комнате
        room.Join(new Viewer(request.Viewer.Id));
        
        // Обновляем данные пользователя в комнате
        room.SetUserName(request.Viewer.Id, request.Viewer.UserName);
        room.SetPhoto(request.Viewer.Id, request.Viewer.PhotoKey);
        room.SetSettings(request.Viewer.Id, request.Viewer.Settings);

        // Обновляем комнату в репозитории
        await unitOfWork.RoomRepository.Value.UpdateAsync(room, cancellationToken);

        // Сохраняем изменения в базе данных
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
        // Преобразовываем комнату в DTO и возвращаем
        return new RoomDto
        {
            Id = room.Id,
            OwnerId = room.Owner.Id,
            FilmId = room.FilmId,
            IsSerial = room.IsSerial,
            Viewers = room.Viewers.Values.Select(ViewerDto.Create).ToArray()
        };
    }
}
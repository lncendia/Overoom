using MediatR;
using Rooms.Application.Abstractions.Commands;
using Rooms.Domain.Repositories;
using Rooms.Domain.Rooms;
using Rooms.Domain.Rooms.Entities;

namespace Rooms.Application.Services.CommandHandlers;

/// <summary>
/// Обработчик команды создания комнаты
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class CreateRoomCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateRoomCommand>
{
    /// <summary>
    /// Обрабатывает команду создания новой комнаты
    /// </summary>
    /// <param name="request">Команда с данными для создания комнаты</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    public async Task Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        // Создаем новую комнату с указанными параметрами
        var room = new Room(request.Id, request.FilmId, request.IsSerial, new Viewer(request.Owner.Id));

        // Устанавливаем данные владельца комнаты
        room.SetUserName(request.Owner.Id, request.Owner.UserName);
        room.SetPhoto(request.Owner.Id, request.Owner.PhotoKey);
        room.SetSettings(request.Owner.Id, request.Owner.Settings);

        // Добавляем комнату в репозиторий
        await unitOfWork.RoomRepository.Value.AddAsync(room, cancellationToken);

        // Сохраняем изменения в базе данных
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
    }
}
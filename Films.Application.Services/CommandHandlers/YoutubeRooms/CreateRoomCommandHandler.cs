using Films.Application.Abstractions.Commands.YoutubeRooms;
using Films.Application.Abstractions.DTOs.Rooms;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Abstractions.Interfaces;
using Films.Domain.Rooms.YoutubeRooms;
using Films.Domain.Servers.Specifications;
using MediatR;

namespace Films.Application.Services.CommandHandlers.YoutubeRooms;

/// <summary>
/// Обработчик команды на создание комнаты ютуб
/// </summary>
public class CreateRoomCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateRoomCommand, RoomServerDto>
{
    public async Task<RoomServerDto> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.Value.GetAsync(request.UserId);
        if (user == null) throw new UserNotFoundException();

        var servers = await unitOfWork.ServerRepository.Value.FindAsync(new ServerByEnabledSpecification(true));

        var room = new YoutubeRoom(user, servers, request.IsOpen)
        {
            VideoAccess = request.VideoAccess
        };

        await unitOfWork.YoutubeRoomRepository.Value.AddAsync(room);
        await unitOfWork.SaveChangesAsync();

        return new RoomServerDto
        {
            Id = room.Id,
            ServerUrl = servers.First(s => s.Id == room.ServerId).Url,
            Code = room.Code
        };
    }
}
﻿using Microsoft.Extensions.Caching.Memory;
using Films.Application.Abstractions;
using Films.Application.Abstractions.Common.Exceptions;
using Films.Application.Abstractions.Rooms.DTOs.Youtube;
using Films.Application.Abstractions.Rooms.Interfaces;
using Films.Domain.Abstractions.Repositories.UnitOfWorks;

namespace Films.Application.Services.Rooms;

public class YoutubeRoomManager(IUnitOfWork unitOfWork, IYoutubeRoomMapper youtubeRoomMapper, IMemoryCache memoryCache)
    : IYoutubeRoomManager
{
    public Task<(Guid roomId, int viewerId)> CreateAnonymouslyAsync(CreateYoutubeRoomDto dto, string name)
    {
        var viewer = new ViewerDto(name, ApplicationConstants.DefaultAvatar);
        var room = new YoutubeRoom(dto.Url, dto.Access, dto.IsOpen, viewer);
        return AddAsync(room);
    }

    public async Task<(Guid roomId, int viewerId)> CreateAsync(CreateYoutubeRoomDto dto, Guid userId)
    {
        var user = await unitOfWork.UserRepository.Value.GetAsync(userId);
        if (user == null) throw new UserNotFoundException();
        var viewer = new ViewerDto(user);
        var room = new YoutubeRoom(dto.Url, dto.Access, dto.IsOpen, viewer);
        return await AddAsync(room);
    }

    public async Task<int> ConnectAnonymouslyAsync(Guid roomId, string name)
    {
        var room = await GetRoomAsync(roomId);
        var viewer = new ViewerDto(name, ApplicationConstants.DefaultAvatar);
        var id = room.Connect(viewer);
        await SaveRoomAsync(room);
        return id;
    }

    public async Task<int> ConnectAsync(Guid roomId, Guid userId)
    {
        var room = await GetRoomAsync(roomId);
        var user = await unitOfWork.UserRepository.Value.GetAsync(userId);
        if (user == null) throw new UserNotFoundException();
        var viewer = new ViewerDto(user);
        var id = room.Connect(viewer);
        await SaveRoomAsync(room);
        return id;
    }


    public async Task<string> AddVideoAsync(Guid roomId, int viewerId, Uri url)
    {
        var room = await GetRoomAsync(roomId);
        var id = room.AddVideo(viewerId, url);
        await SaveRoomAsync(room);
        return id;
    }

    public async Task ChangeVideoAsync(Guid roomId, int viewerId, string id)
    {
        var room = await GetRoomAsync(roomId);
        room.ChangeVideo(viewerId, id);
        await SaveRoomAsync(room);
    }

    public async Task ReConnectAsync(Guid roomId, int viewerId)
    {
        var room = await GetRoomAsync(roomId);
        room.SetOnline(viewerId, true);
        await SaveRoomAsync(room);
    }

    public async Task BeepAsync(Guid roomId, int viewerId, int target)
    {
        var room = await GetRoomAsync(roomId);
        room.Beep(viewerId, target);
        await SaveRoomAsync(room);
    }

    public async Task ScreamAsync(Guid roomId, int viewerId, int target)
    {
        var room = await GetRoomAsync(roomId);
        room.Scream(viewerId, target);
        await SaveRoomAsync(room);
    }

    public async Task ChangeNameAsync(Guid roomId, int viewerId, int target, string name)
    {
        var room = await GetRoomAsync(roomId);
        room.ChangeName(viewerId, target, name);
        await SaveRoomAsync(room);
    }

    public async Task KickAsync(Guid roomId, int viewerId, int target)
    {
        var room = await GetRoomAsync(roomId);
        room.Kick(viewerId, target);
        await SaveRoomAsync(room);
    }

    public async Task SendMessageAsync(Guid roomId, int viewerId, string message)
    {
        var room = await GetRoomAsync(roomId);
        room.SendMessage(viewerId, message);
        await SaveRoomAsync(room);
    }


    public async Task PauseAsync(Guid roomId, int viewerId, bool pause)
    {
        var room = await GetRoomAsync(roomId);
        room.SetPause(viewerId, pause);
        await SaveRoomAsync(room);
    }

    public async Task FullScreenAsync(Guid roomId, int viewerId, bool fullScreen)
    {
        var room = await GetRoomAsync(roomId);
        room.SetFullScreen(viewerId, fullScreen);
        await SaveRoomAsync(room);
    }

    public async Task SeekAsync(Guid roomId, int viewerId, TimeSpan time)
    {
        var room = await GetRoomAsync(roomId);
        room.SetTimeLine(viewerId, time);
        await SaveRoomAsync(room);
    }

    public async Task DisconnectAsync(Guid roomId, int viewerId)
    {
        var room = await GetRoomAsync(roomId);
        room.SetOnline(viewerId, false);
        await SaveRoomAsync(room);
    }


    public async Task<YoutubeRoomDto> GetAsync(Guid roomId)
    {
        var room = await GetRoomAsync(roomId);
        return youtubeRoomMapper.Map(room);
    }

    public async Task<YoutubeViewerDto> GetAsync(Guid roomId, int viewerId)
    {
        var room = await GetRoomAsync(roomId);
        var viewer = room.Viewers.FirstOrDefault(x => x.Id == viewerId);
        if (viewer == null) throw new ViewerNotFoundException();
        return youtubeRoomMapper.Map(viewer);
    }


    private async Task SaveRoomAsync(YoutubeRoom room)
    {
        await unitOfWork.YoutubeRoomRepository.Value.UpdateAsync(room);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task<YoutubeRoom> GetRoomAsync(Guid id)
    {
        if (!memoryCache.TryGetValue(id, out YoutubeRoom? room))
        {
            room = await unitOfWork.YoutubeRoomRepository.Value.GetAsync(id);
            if (room == null) throw new RoomNotFoundException();
            memoryCache.Set(id, room, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));
        }
        else
        {
            if (room == null) throw new RoomNotFoundException();
        }

        return room;
    }


    private async Task<(Guid roomId, int viewerId)> AddAsync(YoutubeRoom room)
    {
        await unitOfWork.YoutubeRoomRepository.Value.AddAsync(room);
        await unitOfWork.SaveChangesAsync();
        memoryCache.Set(room.Id, room, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));
        return (room.Id, room.Owner.Id);
    }
}
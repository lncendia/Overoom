using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Overoom.Application.Abstractions.Exceptions.Rooms;
using Overoom.Application.Abstractions.Interfaces.Rooms;
using Overoom.WEB.Hubs.Models;
using Overoom.Domain.Rooms.BaseRoom.Exceptions;

namespace Overoom.WEB.Hubs;

[Authorize(Policy = "FilmRoom")]
public class FilmRoomHub : HubBase
{
    private readonly IFilmRoomManager _roomManager;
    private readonly IRoomDeleterManager _roomDeleterManager;

    public FilmRoomHub(IFilmRoomManager roomService, IRoomDeleterManager roomDeleterManager) : base(roomService)
    {
        _roomManager = roomService;
        _roomDeleterManager = roomDeleterManager;
    }

    public async Task ChangeSeries(int season, int series)
    {
        try
        {
            var data = GetData();
            await _roomManager.ChangeSeries(data.RoomId, data.Id, season, series);
            await Clients.OthersInGroup(data.RoomIdString).SendAsync("Change", data.Id, season, series);
        }
        catch (Exception ex)
        {
            var error = ex switch
            {
                RoomNotFoundException => "Комната не найдена.",
                ViewerNotFoundException => "Зритель не найден.",
                _ => "Неизвестная ошибка."
            };
            await Clients.Caller.SendAsync("ReceiveMessage", error);
        }
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var data = GetData();
            var viewer = await _roomManager.ConnectAsync(data.RoomId, data.Id);
            await Groups.AddToGroupAsync(Context.ConnectionId, data.RoomIdString);
            var json = JsonSerializer.Serialize(new FilmViewerModel(viewer.Id, viewer.Username, viewer.AvatarUrl,
                    (int) viewer.Time.TotalSeconds, viewer.Season, viewer.Series),
                new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
            await Clients.Group(data.RoomIdString).SendAsync("Connect", json);
            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            var error = ex switch
            {
                RoomNotFoundException => "Комната не найдена.",
                ViewerNotFoundException => "Зритель не найден.",
                _ => "Неизвестная ошибка."
            };
            await Clients.Caller.SendAsync("ReceiveMessage", error);
        }
    }
}
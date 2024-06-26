﻿using Films.Infrastructure.Storage.Models.Rooms;

namespace Films.Infrastructure.Storage.Models.YoutubeRoom;

public class YoutubeRoomModel : RoomModel
{
    /// <summary>
    /// Участники комнаты.
    /// </summary>
    public List<ViewerModel<YoutubeRoomModel>> Viewers { get; set; } = [];
    
    /// <summary>
    /// Заблокированные пользователи.
    /// </summary>
    public List<BannedModel<YoutubeRoomModel>> Banned { get; set; } = [];

    /// <summary>
    /// Флаг доступа к списку видео.
    /// </summary>
    public bool VideoAccess { get; set; }
}
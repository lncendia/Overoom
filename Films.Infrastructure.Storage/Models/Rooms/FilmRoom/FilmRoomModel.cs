﻿using System.ComponentModel.DataAnnotations;
using Films.Infrastructure.Storage.Models.Film;
using Films.Infrastructure.Storage.Models.Rooms.BaseRoom;

namespace Films.Infrastructure.Storage.Models.Rooms.FilmRoom;

public class FilmRoomModel : RoomModel
{
    /// <summary>
    /// Участники комнаты.
    /// </summary>
    public List<ViewerModel<FilmRoomModel>> Viewers { get; set; } = [];
    
    /// <summary>
    /// Заблокированные пользователи.
    /// </summary>
    public List<BannedModel<FilmRoomModel>> Banned { get; set; } = [];

    /// <summary> 
    /// Идентификатор фильма.
    /// </summary> 
    public Guid FilmId { get; set; }

    /// <summary>
    /// Имя CDN.
    /// </summary>
    [MaxLength(30)]
    public string CdnName { get; set; } = null!;

    /// <summary>
    /// Фильм.
    /// </summary>
    public FilmModel Film { get; set; } = null!;
}
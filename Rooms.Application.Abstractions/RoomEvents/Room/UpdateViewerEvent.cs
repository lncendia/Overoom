using Common.Domain.Rooms;
using Rooms.Application.Abstractions.DTOs;

namespace Rooms.Application.Abstractions.RoomEvents.Room;

/// <summary>
/// Модель данных для события обновления зрителя
/// </summary>
public class UpdateViewerEvent : RoomBaseEvent
{
    /// <summary>Идентификатор зрителя</summary>
    public required Guid Id { get; init; }

    /// <summary>Имя пользователя</summary>
    public string? UserName { get; set; }

    /// <summary>Ключ фотографии</summary>
    public string? PhotoKey { get; set; }

    /// <summary>Флаг онлайн-статуса</summary>
    public bool? Online { get; set; }

    /// <summary>Права пользователя на действия в комнате</summary>
    public RoomSettings? Settings { get; set; }
    
    /// <summary>Список тегов зрителя</summary>
    public IReadOnlyList<ViewerTagDto>? Tags { get; set; }
    
    /// <summary>Список обновленных полей</summary>
    public required IReadOnlyList<string> UpdatedFields { get; init; }
}
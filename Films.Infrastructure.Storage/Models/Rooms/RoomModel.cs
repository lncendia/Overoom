using Films.Domain.Rooms.Snapshots;
using MongoDB.Driver;
using MongoTracker.Entities;

namespace Films.Infrastructure.Storage.Models.Rooms;

/// <summary>
/// Модель комнаты для совместного просмотра фильмов.
/// Наследуется от абстрактного класса UpdatedEntity,
/// который предоставляет возможности отслеживания изменений и управления состоянием сущности.
/// </summary>
public class RoomModel : VersionedUpdatedEntity<RoomModel>
{
    private TrackedCollection<Guid, RoomModel> _viewers = new();
    private TrackedCollection<Guid, RoomModel> _bannedUsers = new();

    /// <summary>
    /// Уникальный идентификатор комнаты
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Секретный код комнаты для подключения (может быть null)
    /// </summary>
    public string? Code
    {
        get;
        set => field = TrackChange(nameof(Code), field, value);
    }

    /// <summary>
    /// Список идентификаторов участников комнаты
    /// </summary>
    public List<Guid> Viewers
    {
        get => _viewers.Collection;
        set => _viewers = TrackCollection(nameof(Viewers), _viewers, value)!;
    }

    /// <summary>
    /// Список идентификаторов заблокированных пользователей
    /// </summary>
    public List<Guid> BannedUsers
    {
        get => _bannedUsers.Collection;
        set => _bannedUsers = TrackCollection(nameof(BannedUsers), _bannedUsers, value)!;
    }

    /// <summary>
    /// Идентификатор текущего фильма в комнате
    /// </summary>
    public Guid FilmId
    {
        get;
        set => field = TrackStructChange(nameof(FilmId), field, value);
    }

    /// <summary>
    /// Идентификатор владельца комнаты
    /// </summary>
    public Guid OwnerId
    {
        get;
        set => field = TrackStructChange(nameof(OwnerId), field, value);
    }

    /// <summary>
    /// Дата и время создания комнаты
    /// </summary>
    public DateTime CreatedAt
    {
        get;
        set => field = TrackStructChange(nameof(CreatedAt), field, value);
    }

    /// <inheritdoc/>
    public override UpdateDefinition<RoomModel> UpdateDefinition
    {
        get
        {
            var baseUpdate = base.UpdateDefinition;

            var viewersUpdate = _viewers.GetUpdateDefinition(null, nameof(Viewers), AddedValueObjects);
            var bannedUsersUpdate = _bannedUsers.GetUpdateDefinition(null, nameof(BannedUsers), AddedValueObjects);

            return Combine(
                baseUpdate,
                viewersUpdate,
                bannedUsersUpdate
            );
        }
    }

    /// <inheritdoc/>
    public override EntityState EntityState => Combine(
        _viewers.IsModified,
        _bannedUsers.IsModified
    );

    /// <inheritdoc/>
    public override void ClearChanges()
    {
        base.ClearChanges();
        _viewers.ClearChanges();
        _bannedUsers.ClearChanges();
    }
    
    public void UpdateFromSnapshot(RoomSnapshot snapshot)
    {
        FilmId = snapshot.FilmId;
        Code = snapshot.Code;
        OwnerId = snapshot.OwnerId;
        CreatedAt = snapshot.CreatedAt;
        Viewers = snapshot.Viewers.ToList();
        BannedUsers = snapshot.BannedUsers.ToList();
    }

    public RoomSnapshot GetSnapshot() => new()
    {
        Id = Id,
        FilmId = FilmId,
        Code = Code,
        OwnerId = OwnerId,
        CreatedAt = CreatedAt,
        Viewers = Viewers.AsReadOnly(),
        BannedUsers = BannedUsers.AsReadOnly()
    };
}
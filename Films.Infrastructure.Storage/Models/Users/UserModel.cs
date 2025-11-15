using Common.Domain.Rooms;
using Films.Domain.Users.Snapshots;
using Films.Domain.Users.ValueObjects;
using MongoDB.Driver;
using MongoTracker.Entities;

namespace Films.Infrastructure.Storage.Models.Users;

/// <summary>
/// Модель пользователя для работы с базой данных.
/// Наследуется от абстрактного класса UpdatedEntity,
/// который предоставляет возможности отслеживания изменений и управления состоянием сущности.
/// </summary>
public class UserModel : VersionedUpdatedEntity<UserModel>
{
    private TrackedCollection<FilmNote, UserModel> _watchlist = new();
    private TrackedCollection<FilmNote, UserModel> _history = new();
    private TrackedCollection<string, UserModel> _genres = new();

    /// <summary>
    /// Уникальный идентификатор пользователя
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Имя пользователя (максимальная длина - 40 символов)
    /// </summary>
    public string Username
    {
        get;
        set => field = TrackChange(nameof(Username), field, value)!;
    } = null!;

    /// <summary>
    /// Ссылка на фото пользователя (может быть null)
    /// </summary>
    public string? PhotoKey
    {
        get;
        set => field = TrackChange(nameof(PhotoKey), field, value);
    }

    /// <summary>
    /// Список фильмов в "хочу посмотреть" пользователя
    /// </summary>
    public List<FilmNote> Watchlist
    {
        get => _watchlist.Collection;
        set => _watchlist = TrackCollection(nameof(Watchlist), _watchlist, value)!;
    }

    /// <summary>
    /// История просмотров пользователя
    /// </summary>
    public List<FilmNote> History
    {
        get => _history.Collection;
        set => _history = TrackCollection(nameof(History), _history, value)!;
    }

    /// <summary>
    /// Список предпочитаемых жанров пользователя
    /// </summary>
    public List<string> Genres
    {
        get => _genres.Collection;
        set => _genres = TrackCollection(nameof(Genres), _genres, value)!;
    }

    /// <summary>
    /// Настройка комнат
    /// </summary>
    public RoomSettings RoomSettings
    {
        get;
        set => field = TrackChange(nameof(RoomSettings), field, value)!;
    } = null!;

    /// <inheritdoc/>
    public override UpdateDefinition<UserModel> UpdateDefinition
    {
        get
        {
            var baseUpdate = base.UpdateDefinition;

            var watchlistUpdate = _watchlist.GetUpdateDefinition(null, nameof(Watchlist), AddedValueObjects);
            var historyUpdate = _history.GetUpdateDefinition(null, nameof(History), AddedValueObjects);
            var genresUpdate = _genres.GetUpdateDefinition(null, nameof(Genres), AddedValueObjects);

            return Combine(
                baseUpdate,
                watchlistUpdate,
                historyUpdate,
                genresUpdate
            );
        }
    }

    /// <inheritdoc/>
    public override EntityState EntityState => Combine(
        _watchlist.IsModified,
        _history.IsModified,
        _genres.IsModified
    );

    /// <inheritdoc/>
    public override void ClearChanges()
    {
        base.ClearChanges();
        _watchlist.ClearChanges();
        _history.ClearChanges();
        _genres.ClearChanges();
    }

    public void UpdateFromSnapshot(UserSnapshot snapshot)
    {
        Username = snapshot.Username;
        PhotoKey = snapshot.PhotoKey;
        RoomSettings = snapshot.RoomSettings;
        Watchlist = snapshot.Watchlist.ToList();
        History = snapshot.History.ToList();
        Genres = snapshot.Genres.ToList();
    }

    public UserSnapshot GetSnapshot() => new()
    {
        Id = Id,
        Username = Username,
        PhotoKey = PhotoKey,
        RoomSettings = RoomSettings,
        Watchlist = Watchlist,
        History = History,
        Genres = Genres
    };
}
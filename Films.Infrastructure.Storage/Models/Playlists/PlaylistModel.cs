using Films.Domain.Playlists.Snapshots;
using MongoDB.Driver;
using MongoTracker.Entities;

namespace Films.Infrastructure.Storage.Models.Playlists;

/// <summary>
/// Модель плейлиста для работы с базой данных.
/// Наследуется от абстрактного класса UpdatedEntity,
/// который предоставляет возможности отслеживания изменений и управления состоянием сущности.
/// </summary>
public class PlaylistModel : VersionedUpdatedEntity<PlaylistModel>
{
    private TrackedCollection<Guid, PlaylistModel> _films = new();
    private TrackedCollection<string, PlaylistModel> _genres = new();

    /// <summary>
    /// Уникальный идентификатор плейлиста
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Название плейлиста (максимальная длина - 200 символов)
    /// </summary>
    public string Name
    {
        get;
        set => field = TrackChange(nameof(Name), field, value)!;
    } = null!;

    /// <summary>
    /// Описание плейлиста (максимальная длина - 500 символов)
    /// </summary>
    public string Description
    {
        get;
        set => field = TrackChange(nameof(Description), field, value)!;
    } = null!;

    /// <summary>
    /// Список идентификаторов фильмов в плейлисте
    /// </summary>
    public List<Guid> Films
    {
        get => _films.Collection;
        set => _films = TrackCollection(nameof(Films), _films, value)!;
    }

    /// <summary>
    /// Список жанров плейлиста
    /// </summary>
    public List<string> Genres
    {
        get => _genres.Collection;
        set => _genres = TrackCollection(nameof(Genres), _genres, value)!;
    }

    /// <summary>
    /// Дата и время последнего обновления плейлиста
    /// </summary>
    public DateTime UpdatedAt
    {
        get;
        set => field = TrackStructChange(nameof(UpdatedAt), field, value);
    }

    /// <summary>
    /// Ссылка на постер плейлиста
    /// </summary>
    public string PosterKey
    {
        get;
        set => field = TrackChange(nameof(PosterKey), field, value)!;
    } = null!;

    /// <inheritdoc/>
    /// <summary>
    /// Возвращает определение для обновления MongoDB, объединяя изменения всех отслеживаемых свойств
    /// </summary>
    public override UpdateDefinition<PlaylistModel> UpdateDefinition
    {
        get
        {
            // Базовое определение обновления из родительского класса
            var baseUpdate = base.UpdateDefinition;

            // Получаем определение для обновления списка фильмов, если он был изменен
            var filmsUpdate = _films.GetUpdateDefinition(
                parentPropertyName: null, 
                propertyName: nameof(Films), 
                blockedParentPropertyNames: AddedValueObjects);

            // Получаем определение для обновления списка жанров, если он был изменен
            var genresUpdate = _genres.GetUpdateDefinition(
                parentPropertyName: null, 
                propertyName: nameof(Genres), 
                blockedParentPropertyNames: AddedValueObjects);

            // Объединяем все определения обновления в одно
            return Combine(
                baseUpdate,
                filmsUpdate,
                genresUpdate
            );
        }
    }

    /// <inheritdoc/>
    /// <summary>
    /// Возвращает текущее состояние сущности, объединяя состояния всех отслеживаемых свойств
    /// </summary>
    public override EntityState EntityState => Combine(
        // Состояние списка фильмов, если он был изменен
        _films.IsModified,

        // Состояние списка жанров, если он был изменен
        _genres.IsModified
    );

    /// <inheritdoc/>
    /// <summary>
    /// Очищает все изменения в сущности и всех отслеживаемых свойствах
    /// </summary>
    public override void ClearChanges()
    {
        // Очищаем изменения в базовой сущности
        base.ClearChanges();

        // Очищаем изменения в списке фильмов
        _films.ClearChanges();

        // Очищаем изменения в списке жанров
        _genres.ClearChanges();
    }
    
    public void UpdateFromSnapshot(PlaylistSnapshot snapshot)
    {
        Name = snapshot.Name;
        Description = snapshot.Description;
        PosterKey = snapshot.PosterKey;
        UpdatedAt = snapshot.UpdatedAt;
        Films = snapshot.Films.ToList();
        Genres = snapshot.Genres.ToList();
    }

    public PlaylistSnapshot GetSnapshot() => new()
    {
        Id = Id,
        Name = Name,
        Description = Description,
        PosterKey = PosterKey,
        UpdatedAt = UpdatedAt,
        Films = Films,
        Genres = Genres
    };
}
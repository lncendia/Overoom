using MongoDB.Driver;
using MongoTracker.Entities;
using Rooms.Domain.Rooms.Snapshots;

namespace Rooms.Infrastructure.Storage.Models.Rooms;

/// <summary>
/// Модель комнаты для работы с базой данных.
/// Наследуется от абстрактного класса UpdatedEntity,
/// который предоставляет возможности отслеживания изменений и управления состоянием сущности.
/// </summary>
public class RoomModel : UpdatedEntity<RoomModel>
{
    private TrackedValueObjectCollection<ViewerModel, RoomModel> _viewers = new();

    /// <summary>
    /// Уникальный идентификатор комнаты
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Идентификатор фильма
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
    /// Признак того, является ли контент сериалом
    /// </summary>
    public bool IsSerial
    {
        get;
        set => field = TrackStructChange(nameof(IsSerial), field, value);
    }

    /// <summary>
    /// Список пользователей, подключенных к комнате
    /// </summary>
    public List<ViewerModel> Viewers
    {
        get => _viewers.Collection;
        set => _viewers = TrackValueObjectCollection(nameof(Viewers), _viewers, value)!;
    }

    /// <inheritdoc/>
    /// <summary>
    /// Возвращает определение для обновления MongoDB, объединяя изменения всех отслеживаемых свойств
    /// </summary>
    public override UpdateDefinition<RoomModel> UpdateDefinition =>
        Combine(
            base.UpdateDefinition,
            _viewers.GetUpdateDefinition(
                parentPropertyName: null,
                propertyName: nameof(Viewers),
                blockedParentPropertyNames: AddedValueObjects
            )
        );

    /// <inheritdoc/>
    /// <summary>
    /// Возвращает текущее состояние сущности, включая отслеживаемые коллекции
    /// </summary>
    public override EntityState EntityState => Combine(_viewers.IsModified);

    /// <inheritdoc/>
    /// <summary>
    /// Очищает все зафиксированные изменения в сущности и вложенных коллекциях
    /// </summary>
    public override void ClearChanges()
    {
        base.ClearChanges();
        _viewers.ClearChanges();
    }

    /// <summary>
    /// Создаёт снапшот текущей комнаты со всеми зрителями
    /// </summary>
    public RoomSnapshot GetSnapshot() => new()
    {
        Id = Id,
        FilmId = FilmId,
        OwnerId = OwnerId,
        IsSerial = IsSerial,
        Viewers = Viewers.Select(v => v.GetSnapshot()).ToList()
    };

    /// <summary>
    /// Обновляет модель комнаты из снапшота
    /// </summary>
    public void UpdateFromSnapshot(RoomSnapshot snapshot)
    {
        FilmId = snapshot.FilmId;
        OwnerId = snapshot.OwnerId;
        IsSerial = snapshot.IsSerial;

        // Удаляем зрителей, которых больше нет в снапшоте
        Viewers.RemoveAll(viewerModel => snapshot.Viewers.All(viewer => viewerModel.Id != viewer.Id));

        // Добавляем новых зрителей, которых нет в модели
        var newViewers = snapshot.Viewers
            .Where(x => Viewers.All(m => x.Id != m.Id))
            .Select(c =>
            {
                var viewerModel = new ViewerModel { Id = c.Id };
                viewerModel.UpdateFromSnapshot(c);
                return viewerModel;
            })
            .ToArray();

        // Обновляем существующих зрителей
        Viewers.ForEach(v => v.UpdateFromSnapshot(snapshot.Viewers.First(sv => sv.Id == v.Id)));

        // Добавляем новых зрителей в модель
        Viewers.AddRange(newViewers);
    }
}
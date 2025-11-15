using MongoDB.Driver;
using MongoTracker.Entities;

namespace Films.Infrastructure.Storage.Models.Films;

/// <summary>
/// Модель эпизода сериала для хранения в MongoDB.
/// Наследует функциональность отслеживания изменений из UpdatedValueObject.
/// </summary>
public class EpisodeModel : UpdatedValueObject<FilmModel>
{
    private TrackedCollection<string, FilmModel> _versions = new();

    /// <summary>
    /// Номер эпизода в сезоне
    /// </summary>
    public int Number
    {
        get;
        set => field = TrackStructChange(nameof(Number), field, value);
    }

    /// <summary>
    /// Список версий медиаконтента для эпизода
    /// </summary>
    public List<string> Versions
    {
        get => _versions.Collection;
        set => _versions = TrackCollection(nameof(Versions), _versions, value)!;
    }

    /// <summary>
    /// Формирует определение для обновления эпизода в MongoDB
    /// </summary>
    public override UpdateDefinition<FilmModel>? GetUpdateDefinition(
        string? parentPropertyName, 
        string propertyName,
        IReadOnlyCollection<string> blockedParentPropertyNames)
    {
        if (blockedParentPropertyNames.Contains(propertyName)) 
            return null;

        var versionsDefinition = _versions.GetUpdateDefinition(
            Combine(parentPropertyName, propertyName), 
            nameof(Versions), 
            AddedValueObjects);

        var baseDefinition = base.GetUpdateDefinition(
            parentPropertyName, 
            propertyName, 
            blockedParentPropertyNames);

        return Combine(versionsDefinition, baseDefinition);
    }

    /// <summary>
    /// Определяет, были ли изменения в эпизоде или его версиях
    /// </summary>
    public override bool IsModified => base.IsModified || _versions.IsModified;

    /// <summary>
    /// Сбрасывает состояние изменений эпизода и его версий
    /// </summary>
    public override void ClearChanges()
    {
        base.ClearChanges();
        _versions.ClearChanges();
    }
}
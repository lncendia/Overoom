using MongoDB.Driver;
using MongoTracker.Entities;

namespace Films.Infrastructure.Storage.Models.Films;

/// <summary>
/// Модель медиаконтента для хранения в MongoDB.
/// Наследует функциональность отслеживания изменений из UpdatedValueObject.
/// </summary>
public class MediaContentModel : UpdatedValueObject<FilmModel>
{
    private TrackedCollection<string, FilmModel> _versions = new();
    
    /// <summary>
    /// Список версий медиаконтента
    /// </summary>
    public List<string> Versions
    {
        get => _versions.Collection;
        set => _versions = TrackCollection(nameof(Versions), _versions, value)!;
    }

    /// <summary>
    /// Формирует определение для обновления медиаконтента в MongoDB
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
    /// Определяет, были ли изменения в контенте или его версиях
    /// </summary>
    public override bool IsModified => base.IsModified || _versions.IsModified;

    /// <summary>
    /// Сбрасывает состояние изменений контента и его версий
    /// </summary>
    public override void ClearChanges()
    {
        base.ClearChanges();
        _versions.ClearChanges();
    }
}
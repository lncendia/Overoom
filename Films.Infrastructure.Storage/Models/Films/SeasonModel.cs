using MongoDB.Driver;
using MongoTracker.Entities;

namespace Films.Infrastructure.Storage.Models.Films;

/// <summary>
/// Модель сезона сериала для хранения в MongoDB.
/// Наследует функциональность отслеживания изменений из UpdatedValueObject.
/// </summary>
public class SeasonModel : UpdatedValueObject<FilmModel>
{
    private int _number;
    private TrackedValueObjectCollection<EpisodeModel, FilmModel> _episodes = new();

    /// <summary>
    /// Номер сезона
    /// </summary>
    public int Number
    {
        get => _number;
        set => _number = TrackStructChange(nameof(Number), _number, value);
    }

    /// <summary>
    /// Список эпизодов сезона
    /// </summary>
    public List<EpisodeModel> Episodes
    {
        get => _episodes.Collection;
        set => _episodes = TrackValueObjectCollection(nameof(Episodes), _episodes, value)!;
    }

    /// <summary>
    /// Формирует определение для обновления сезона в MongoDB
    /// </summary>
    public override UpdateDefinition<FilmModel>? GetUpdateDefinition(
        string? parentPropertyName, 
        string propertyName,
        IReadOnlyCollection<string> blockedParentPropertyNames)
    {
        if (blockedParentPropertyNames.Contains(propertyName)) 
            return null;

        var episodesDefinition = _episodes.GetUpdateDefinition(
            Combine(parentPropertyName, propertyName), 
            nameof(Episodes), 
            AddedValueObjects);

        var baseDefinition = base.GetUpdateDefinition(
            parentPropertyName, 
            propertyName, 
            blockedParentPropertyNames);

        return Combine(episodesDefinition, baseDefinition);
    }

    /// <summary>
    /// Определяет, были ли изменения в сезоне или его эпизодах
    /// </summary>
    public override bool IsModified => base.IsModified || _episodes.IsModified;

    /// <summary>
    /// Сбрасывает состояние изменений сезона и его эпизодов
    /// </summary>
    public override void ClearChanges()
    {
        base.ClearChanges();
        _episodes.ClearChanges();
    }
}
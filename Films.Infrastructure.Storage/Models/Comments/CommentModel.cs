using Films.Domain.Comments.Snapshots;
using MongoTracker.Entities;

namespace Films.Infrastructure.Storage.Models.Comments;

/// <summary>
/// Модель комментария для работы с базой данных.
/// Наследуется от абстрактного класса UpdatedEntity,
/// который предоставляет возможности отслеживания изменений и управления состоянием сущности.
/// </summary>
public class CommentModel : VersionedUpdatedEntity<CommentModel>
{
    /// <summary>
    /// Уникальный идентификатор комментария
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Текст комментария (максимальная длина - 1000 символов)
    /// </summary>
    public string Text
    {
        get;
        set => field = TrackChange(nameof(Text), field, value)!;
    } = null!;

    /// <summary>
    /// Дата и время создания комментария
    /// </summary>
    public DateTime CreatedAt
    {
        get;
        set => field = TrackStructChange(nameof(CreatedAt), field, value);
    }

    /// <summary>
    /// Идентификатор пользователя, оставившего комментарий (может быть null)
    /// </summary>
    public Guid UserId
    {
        get;
        set => field = TrackStructChange(nameof(UserId), field, value);
    }

    /// <summary>
    /// Идентификатор фильма, к которому относится комментарий
    /// </summary>
    public Guid FilmId
    {
        get;
        set => field = TrackStructChange(nameof(FilmId), field, value);
    }

    public CommentSnapshot GetSnapshot() => new()
    {
        Id = Id,
        FilmId = FilmId,
        UserId = UserId,
        Text = Text,
        CreatedAt = CreatedAt
    };

    public void UpdateFromSnapshot(CommentSnapshot snapshot)
    {
        FilmId = snapshot.FilmId;
        UserId = snapshot.UserId;
        Text = snapshot.Text;
        CreatedAt = snapshot.CreatedAt;
    }
}
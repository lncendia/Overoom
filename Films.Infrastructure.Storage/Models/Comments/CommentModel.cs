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
    private string _text = null!;
    private DateTime _createdAt;
    private Guid _userId;
    private Guid _filmId;

    /// <summary>
    /// Уникальный идентификатор комментария
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Текст комментария (максимальная длина - 1000 символов)
    /// </summary>
    public string Text
    {
        get => _text;
        set => _text = TrackChange(nameof(Text), _text, value)!;
    }

    /// <summary>
    /// Дата и время создания комментария
    /// </summary>
    public DateTime CreatedAt
    {
        get => _createdAt;
        set => _createdAt = TrackStructChange(nameof(CreatedAt), _createdAt, value);
    }

    /// <summary>
    /// Идентификатор пользователя, оставившего комментарий (может быть null)
    /// </summary>
    public Guid UserId
    {
        get => _userId;
        set => _userId = TrackStructChange(nameof(UserId), _userId, value);
    }

    /// <summary>
    /// Идентификатор фильма, к которому относится комментарий
    /// </summary>
    public Guid FilmId
    {
        get => _filmId;
        set => _filmId = TrackStructChange(nameof(FilmId), _filmId, value);
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
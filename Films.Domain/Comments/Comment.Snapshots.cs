using System.Reflection;
using Films.Domain.Comments.Snapshots;

namespace Films.Domain.Comments;

public partial class Comment
{
    internal static Comment FromSnapshot(CommentSnapshot snapshot)
    {
        // Получаем тип Comment
        var filmType = typeof(Comment);
    
        // Получаем внутренний конструктор, который принимает CommentSnapshot
        var constructor = filmType.GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(CommentSnapshot)],
            null);
    
        // Вызываем конструктор и возвращаем результат
        return (Comment)constructor!.Invoke([snapshot]);
    }
    
    internal CommentSnapshot GetSnapshot() => new()
    {
        Id = Id,
        FilmId = FilmId,
        UserId = UserId,
        Text = Text,
        CreatedAt = CreatedAt
    };
    
    /// <summary>
    /// Внутренний конструктор для гидратации из снапшота или БД.
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private Comment(CommentSnapshot snapshot) : base(snapshot.Id)
    {
        FilmId = snapshot.FilmId;
        UserId = snapshot.UserId;
        Text = snapshot.Text;
        CreatedAt = snapshot.CreatedAt;
    }
}
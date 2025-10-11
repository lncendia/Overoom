namespace Films.Infrastructure.Web.Comments.InputModels;

/// <summary>
/// Модель данных для добавления нового комментария к фильму
/// </summary>
public class AddCommentInputModel
{
    /// <summary>
    /// Текст комментария. Должен содержать от 1 до 1000 символов
    /// </summary>
    public string? Text { get; init; }
}

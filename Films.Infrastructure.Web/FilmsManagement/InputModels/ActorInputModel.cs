namespace Films.Infrastructure.Web.FilmsManagement.InputModels;

/// <summary>
/// Модель данных об актёре
/// </summary>
public class ActorInputModel
{
    /// <summary>
    /// Полное имя актёра
    /// </summary>
    public string? Name { get; init; }
    
    /// <summary>
    /// Роль актёра в фильме
    /// </summary>
    public string? Role { get; init; }
}
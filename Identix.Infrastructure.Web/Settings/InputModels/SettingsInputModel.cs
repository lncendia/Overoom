namespace Identix.Infrastructure.Web.Settings.InputModels;

/// <summary>
/// Модель страницы настроек.
/// </summary>
public class SettingsInputModel
{
    /// <summary>
    /// Сообщение для пользователя.
    /// </summary>
    public string? Message { get; init; }
    
    /// <summary>
    /// Сообщение об ошибке.
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Номер вкладки, которая должна быть раскрыта.
    /// </summary>
    public int ExpandElement { get; init; } = 1;
}
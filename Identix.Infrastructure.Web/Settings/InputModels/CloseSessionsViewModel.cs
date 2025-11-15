namespace Identix.Infrastructure.Web.Settings.InputModels;

/// <summary>
/// Модель закрытия сеансов
/// </summary>
public class CloseSessionsInputModel
{
    /// <summary>
    /// Url для возврата
    /// </summary>
    public string ReturnUrl { get; set; } = "/";

    /// <summary>
    /// Идентификатор раскрытого элемента
    /// </summary>
    public int ExpandElement { get; set; } = 1;
}

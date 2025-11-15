using Identix.Infrastructure.Web.Settings.InputModels;

namespace Identix.Infrastructure.Web.Settings.ViewModels;

/// <summary>
/// Модель представления для изменения аватара.
/// </summary>
public class ChangeAvatarViewModel : ChangeAvatarInputModel
{
    /// <summary>
    /// Ссылка на миниатюру.
    /// </summary>
    public string? Thumbnail { get; init; }
}
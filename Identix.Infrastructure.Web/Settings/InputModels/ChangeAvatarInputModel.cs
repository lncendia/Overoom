using System.ComponentModel.DataAnnotations;

namespace Identix.Infrastructure.Web.Settings.InputModels;

/// <summary>
/// Модель ввода для изменения аватара.
/// </summary>
public class ChangeAvatarInputModel
{
    /// <summary>
    /// Получает или задает файл изображения.
    /// </summary>
    [Required(ErrorMessageResourceName = "Required",
        ErrorMessageResourceType = typeof(Resources.Settings.InputModels.ChangeAvatarInputModel))]
    public IFormFile? File { get; init; }
}
using System.ComponentModel.DataAnnotations;

namespace Identix.Infrastructure.Web.Settings.InputModels;

/// <summary>
/// Модель ввода для изменения аватара.
/// </summary>
public class ChangeAvatarInputModel
{
    /// <summary>
    /// Файл изображения.
    /// </summary>
    [Required(ErrorMessageResourceName = "Required",
        ErrorMessageResourceType = typeof(Resources.Settings.InputModels.ChangeAvatarInputModel))]
    public IFormFile? File { get; init; }
    
    /// <summary>
    /// URL для возврата.
    /// </summary>
    public string ReturnUrl { get; init; } = "/";
}
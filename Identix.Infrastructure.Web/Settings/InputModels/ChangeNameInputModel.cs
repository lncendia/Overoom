using System.ComponentModel.DataAnnotations;

namespace Identix.Infrastructure.Web.Settings.InputModels;

/// <summary>
/// Модель ввода для изменения имени.
/// </summary>
public class ChangeNameInputModel
{
    /// <summary>
    /// Имя пользователя.
    /// </summary>
    [Required(ErrorMessageResourceName = "Required",
        ErrorMessageResourceType = typeof(Resources.Settings.InputModels.ChangeNameInputModel))]
    [Display(Name = "Username",
        ResourceType = typeof(Resources.Settings.InputModels.ChangeNameInputModel))]
    [MaxLength(40, ErrorMessageResourceName = "MaxLength",
        ErrorMessageResourceType = typeof(Resources.Settings.InputModels.ChangeNameInputModel))]
    public string? Username { get; init; }
    
    /// <summary>
    /// URL для возврата.
    /// </summary>
    public string ReturnUrl { get; init; } = "/";
}
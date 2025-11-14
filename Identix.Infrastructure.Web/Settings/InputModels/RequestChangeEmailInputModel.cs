using System.ComponentModel.DataAnnotations;

namespace Identix.Infrastructure.Web.Settings.InputModels;

/// <summary>
/// Модель запроса смены почты
/// </summary>
public class RequestChangeEmailInputModel
{
    /// <summary>
    /// Почта пользователя
    /// </summary>
    [Required(ErrorMessageResourceName = "Required",
        ErrorMessageResourceType = typeof(Resources.Settings.InputModels.RequestChangeEmailInputModel))]
    [EmailAddress(ErrorMessageResourceName = "ValidEmail",
        ErrorMessageResourceType = typeof(Resources.Settings.InputModels.RequestChangeEmailInputModel))]
    [Display(Name = "Email",
        ResourceType = typeof(Resources.Settings.InputModels.RequestChangeEmailInputModel))]
    public string? Email { get; init; }
    
    /// <summary>
    /// Пароль
    /// </summary>
    [DataType(DataType.Password)]
    [Display(Name = "Password", ResourceType = typeof(Resources.Settings.InputModels.RequestChangeEmailInputModel))]
    public string? Password { get; init; }
    
    /// <summary>
    /// URL для возврата.
    /// </summary>
    public string ReturnUrl { get; init; } = "/";
}
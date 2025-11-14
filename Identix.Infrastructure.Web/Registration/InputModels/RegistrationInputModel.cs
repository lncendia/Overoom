using System.ComponentModel.DataAnnotations;

namespace Identix.Infrastructure.Web.Registration.InputModels;

/// <summary>
/// Модель регистрации аккаунта
/// </summary>
public class RegistrationInputModel
{
    /// <summary>
    /// Логин (имя) пользователя
    /// </summary>
    [Required(ErrorMessageResourceName = "Required",
        ErrorMessageResourceType = typeof(Resources.Registration.InputModels.RegistrationInputModel))]
    [EmailAddress(ErrorMessageResourceName = "ValidEmail",
        ErrorMessageResourceType = typeof(Resources.Registration.InputModels.RegistrationInputModel))]
    [Display(Name = "Email",
        ResourceType = typeof(Resources.Registration.InputModels.RegistrationInputModel))]
    public string? Email { get; set; }

    /// <summary>
    /// Пароль
    /// </summary>
    [Required(ErrorMessageResourceName = "Required",
        ErrorMessageResourceType = typeof(Resources.Registration.InputModels.RegistrationInputModel))]
    [DataType(DataType.Password)]
    [Display(Name = "Password",
        ResourceType = typeof(Resources.Registration.InputModels.RegistrationInputModel))]
    public string? Password { get; set; }

    /// <summary>
    /// Подтверждение пароля
    /// </summary>
    [Required(ErrorMessageResourceName = "Required",
        ErrorMessageResourceType = typeof(Resources.Registration.InputModels.RegistrationInputModel))]
    [DataType(DataType.Password)]
    [Display(Name = "PasswordConfirm",
        ResourceType = typeof(Resources.Registration.InputModels.RegistrationInputModel))]
    [Compare("Password", ErrorMessageResourceName = "PasswordConfirmError",
        ErrorMessageResourceType = typeof(Resources.Registration.InputModels.RegistrationInputModel))]
    public string? PasswordConfirm { get; set; }

    /// <summary>
    /// Url адрес для возврата после прохождения аутентификации
    /// </summary>
    public string ReturnUrl { get; init; } = "/";
}
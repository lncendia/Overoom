using System.ComponentModel.DataAnnotations;
using AuthService.Application.Abstractions.Enums;

namespace AuthService.Infrastructure.Web.TwoFactor.InputModels;

/// <summary>
/// Модель для прохождения 2FA
/// </summary>
public class TwoFactorAuthenticateInputModel
{
    /// <summary>
    /// Код выданный аутентификатором
    /// </summary>
    [Display(Name = "Code", ResourceType = typeof(Resources.TwoFactor.InputModels.TwoFactorAuthenticateInputModel))]
    [Required(ErrorMessageResourceName = "Required",
        ErrorMessageResourceType = typeof(Resources.TwoFactor.InputModels.TwoFactorAuthenticateInputModel))]
    public string? Code { get; init; }
    
    /// <summary>
    /// Откуда код
    /// </summary>
    [Required]
    public CodeType CodeType { get; init; } = CodeType.Authenticator;

    /// <summary>
    /// Url адрес возврата после прохождения 2FA
    /// </summary>
    public string ReturnUrl { get; init; } = "/";
}
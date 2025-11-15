using System.ComponentModel.DataAnnotations;
using Identix.Application.Abstractions.Enums;

namespace Identix.Infrastructure.Web.TwoFactor.InputModels;

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
    /// URL для возврата.
    /// </summary>
    public string ReturnUrl { get; init; } = "/";
}
using AuthService.Application.Abstractions.Enums;

namespace AuthService.Infrastructure.Web.TwoFactor.ViewModels;

/// <summary>
/// Модель представления для сброса 2фа
/// </summary>
public class ResetTwoFactorViewModel
{
    /// <summary>
    /// Необходимо ли отображать вариант получения кода по почте (false - если почта не подтверждена)
    /// </summary>
    public bool NeedShowEmail { get; init; }

    /// <summary>
    /// Откуда код
    /// </summary>
    public CodeType CodeType { get; init; }

    /// <summary>
    /// Url адрес возврата после отключения 2FA
    /// </summary>
    public string ReturnUrl { get; init; } = "/";
}
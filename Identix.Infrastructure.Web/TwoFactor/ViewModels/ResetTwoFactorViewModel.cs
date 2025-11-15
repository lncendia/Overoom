using Identix.Application.Abstractions.Enums;

namespace Identix.Infrastructure.Web.TwoFactor.ViewModels;

/// <summary>
/// Модель представления для сброса 2фа
/// </summary>
public class ResetTwoFactorViewModel
{
    /// <summary>
    /// Откуда код
    /// </summary>
    public CodeType CodeType { get; init; }
    
    /// <summary>
    /// URL для возврата.
    /// </summary>
    public string ReturnUrl { get; init; } = "/";
}
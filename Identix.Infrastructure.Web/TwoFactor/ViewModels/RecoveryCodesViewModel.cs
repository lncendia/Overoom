namespace Identix.Infrastructure.Web.TwoFactor.ViewModels;

/// <summary>
/// Модель представления кодов восстановления.
/// </summary>
public class RecoveryCodesViewModel
{
    /// <summary>
    /// Список кодов восстановления.
    /// </summary>
    public required IEnumerable<string> RecoveryCodes { get; init; }
    
    /// <summary>
    /// URL для возврата.
    /// </summary>
    public string ReturnUrl { get; init; } = "/";
}
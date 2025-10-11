namespace Identix.Infrastructure.Web.Account.InputModels;

/// <summary>
/// Модель ввода выхода из системы
/// </summary>
public class LogoutInputModel
{
    /// <summary>
    /// URL для возврата.
    /// </summary>
    public string ReturnUrl { get; init; } = "/";
}
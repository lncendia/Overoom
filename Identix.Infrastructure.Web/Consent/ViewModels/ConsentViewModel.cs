using Identix.Infrastructure.Web.Consent.InputModels;

namespace Identix.Infrastructure.Web.Consent.ViewModels;

/// <summary>
/// ViewModel Согласие
/// </summary>
public class ConsentViewModel : ConsentInputModel
{
    /// <summary>
    /// Получаем или устанавливаем имя клиента.
    /// </summary>
    public required string ClientName { get; init; }
    
    /// <summary>
    /// Получаем или устанавливаем URL клиента.
    /// </summary>
    public string? ClientUrl { get; init; }
    
    /// <summary>
    /// Получаем или устанавливаем URL-лого клиента.
    /// </summary>
    public string? ClientLogoUrl { get; init; }
    
    /// <summary>
    /// Получаем или устанавливаем идентификационных разрешений.
    /// </summary>
    public required IEnumerable<ScopeViewModel> IdentityScopes { get; init; }
    
    /// <summary>
    /// Получаем или устанавливаем разрешения API
    /// </summary>
    public required IEnumerable<ScopeViewModel> ApiScopes { get; init; }
}
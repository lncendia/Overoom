using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Identix.Infrastructure.Web.External.Services;

/// <summary>
/// Интерфейс для маппинга claims из внешних провайдеров аутентификации
/// </summary>
public interface IExternalClaimsMapper
{
    /// <summary>
    /// Имя провайдера
    /// </summary>
    string Provider { get; }
    
    /// <summary>
    /// Проверяет поддержку указанного провайдера аутентификации
    /// </summary>
    /// <param name="provider">Название провайдера</param>
    /// <returns>True если провайдер поддерживается, иначе False</returns>
    bool SupportsProvider(string provider);
    
    /// <summary>
    /// Выполняет маппинг claims из результата внешней аутентификации
    /// </summary>
    /// <param name="result">Результат аутентификации от внешнего провайдера</param>
    /// <returns>ClaimsIdentity с маппированными claims для системы</returns>
    Task<ClaimsIdentity> MapAsync(AuthenticateResult result);
}
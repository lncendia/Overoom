using System.Security.Claims;
using Identix.Infrastructure.Web.Exceptions;
using Microsoft.AspNetCore.Authentication;

namespace Identix.Infrastructure.Web.External.Services;

/// <summary>
/// Базовый класс для реализации мапперов claims из внешних провайдеров аутентификации
/// </summary>
/// <param name="provider">Название провайдера, который обрабатывает этот маппер</param>
public abstract class ExternalClaimsMapperBase(string provider) : IExternalClaimsMapper
{
    /// <summary>
    /// Имя провайдера
    /// </summary>
    public string Provider => provider;
    
    /// <summary>
    /// Проверяет поддержку указанного провайдера
    /// </summary>
    /// <param name="provider1">Название провайдера для проверки</param>
    /// <returns>True если провайдер поддерживается</returns>
    public bool SupportsProvider(string provider1) => 
        string.Equals(provider1, provider, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Маппит claims из результата внешней аутентификации
    /// </summary>
    /// <param name="result">Результат аутентификации от провайдера</param>
    /// <returns>ClaimsIdentity с преобразованными claims</returns>
    public abstract Task<ClaimsIdentity> MapAsync(AuthenticateResult result);

    /// <summary>
    /// Создает базовую ClaimsIdentity с идентификатором пользователя
    /// </summary>
    /// <param name="id">Уникальный идентификатор пользователя от провайдера</param>
    /// <returns>Базовая ClaimsIdentity с claim идентификатора</returns>
    protected static ClaimsIdentity CreateBaseIdentity(string? id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ExternalAuthenticationFailureException("The user ID was not received from an external provider");
        
        // Создаем identity с указанием типов для имени и ролей
        var identity = new ClaimsIdentity(
            authenticationType: "ExternalLogin",
            nameType: ClaimTypes.Name,
            roleType: ClaimTypes.Role);

        // Добавляем обязательный claim с идентификатором пользователя
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, id));
        return identity;
    }
}
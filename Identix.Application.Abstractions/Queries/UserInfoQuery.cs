using System.Collections.Immutable;
using System.Text.Json.Serialization;
using MediatR;
using OpenIddict.Abstractions;

namespace Identix.Application.Abstractions.Queries;

/// <summary>
/// Запрос для получения информации о пользователе в формате OIDC UserInfo endpoint.
/// Соответствует стандарту OpenID Connect Core 1.0 section 5.3.
/// </summary>
public class UserInfoQuery : IRequest<UserInfoDto>
{
    /// <summary>
    /// Уникальный идентификатор пользователя (subject identifier)
    /// Соответствует claim 'sub' в JWT токене
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Список запрошенных scope'ов аутентификации.
    /// Определяет какие claims о пользователе будут возвращены в ответе.
    /// </summary>
    public required ImmutableArray<string> Scopes { get; init; }
}

/// <summary>
/// DTO, представляющий информацию о пользователе в формате OIDC UserInfo.
/// </summary>
public class UserInfoDto
{
    /// <summary>
    /// Уникальный идентификатор пользователя (sub).
    /// </summary>
    [JsonPropertyName(OpenIddictConstants.Claims.Subject)]
    public required string Sub { get; set; }
    
    /// <summary>
    /// Username пользователя (username).
    /// </summary>
    [JsonPropertyName(OpenIddictConstants.Claims.Username)]
    public string? Username { get; set; }
    
    /// <summary>
    /// Предпочитаемое имя пользователя (preferred_username).
    /// </summary>
    [JsonPropertyName(OpenIddictConstants.Claims.PreferredUsername)]
    public string? PreferredUsername { get; set; }

    /// <summary>
    /// Email пользователя (email).
    /// </summary>
    [JsonPropertyName(OpenIddictConstants.Claims.Email)]
    public string? Email { get; set; }

    /// <summary>
    /// Подтвержден ли email (email_verified).
    /// </summary>
    [JsonPropertyName(OpenIddictConstants.Claims.EmailVerified)]
    public bool? EmailVerified { get; set; }

    /// <summary>
    /// Номер телефона (phone_number).
    /// </summary>
    [JsonPropertyName(OpenIddictConstants.Claims.PhoneNumber)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Подтвержден ли номер телефона (phone_number_verified).
    /// </summary>
    [JsonPropertyName(OpenIddictConstants.Claims.PhoneNumberVerified)]
    public bool? PhoneNumberVerified { get; set; }

    /// <summary>
    /// Роли пользователя (role).
    /// </summary>
    [JsonPropertyName(OpenIddictConstants.Claims.Role)]
    public IEnumerable<string>? Roles { get; set; }

    /// <summary>
    /// Фото пользователя (picture).
    /// </summary>
    [JsonPropertyName(OpenIddictConstants.Claims.Picture)]
    public string? Picture { get; set; }

    /// <summary>
    /// Локаль пользователя (locale).
    /// </summary>
    [JsonPropertyName(OpenIddictConstants.Claims.Locale)]
    public string? Locale { get; set; }
}

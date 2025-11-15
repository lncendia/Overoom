using System.Globalization;
using System.Text.Json;
using OpenIddict.Abstractions;

namespace Identix.Application.Abstractions.Extensions;

/// <summary>
/// Расширения для работы с дескрипторами OpenIddict (ApplicationDescriptor и ScopeDescriptor)
/// Предоставляет удобные методы для доступа к кастомным свойствам и метаданным
/// </summary>
public static class OpenIddictDescriptorExtensions
{
    /// <summary>
    /// Получает URI клиентского приложения (ссылку на сайт приложения)
    /// </summary>
    /// <param name="descriptor">Дескриптор приложения OpenIddict</param>
    /// <returns>URI клиента или null если не задан</returns>
    public static string? GetClientUrl(this OpenIddictApplicationDescriptor descriptor) =>
        descriptor.Properties.TryGetValue("client_url", out var json) && json.ValueKind == JsonValueKind.String
            ? json.GetString()
            : null;

    /// <summary>
    /// Получает URI логотипа клиентского приложения
    /// </summary>
    /// <param name="descriptor">Дескриптор приложения OpenIddict</param>
    /// <returns>URI логотипа или null если не задан</returns>
    public static string? GetLogoKey(this OpenIddictApplicationDescriptor descriptor) =>
        descriptor.Properties.TryGetValue("logo_key", out var json) && json.ValueKind == JsonValueKind.String
            ? json.GetString()
            : null;

    /// <summary>
    /// Устанавливает URI клиентского приложения
    /// </summary>
    /// <param name="descriptor">Дескриптор приложения OpenIddict</param>
    /// <param name="url">Ключ клиента</param>
    public static void SetClientUrl(this OpenIddictApplicationDescriptor descriptor, string url) =>
        descriptor.Properties["client_url"] = JsonDocument.Parse($"\"{url}\"").RootElement;

    /// <summary>
    /// Устанавливает URI логотипа клиентского приложения
    /// </summary>
    /// <param name="descriptor">Дескриптор приложения OpenIddict</param>
    /// <param name="key">Ключ логотипа</param>
    public static void SetLogoKey(this OpenIddictApplicationDescriptor descriptor, string key) =>
        descriptor.Properties["logo_key"] = JsonDocument.Parse($"\"{key}\"").RootElement;

    /// <summary>
    /// Проверяет, должен ли scope быть выделен визуально на форме согласия
    /// </summary>
    /// <param name="descriptor">Дескриптор scope OpenIddict</param>
    /// <returns>True если scope требует визуального выделения</returns>
    public static bool GetEmphasize(this OpenIddictScopeDescriptor descriptor) =>
        descriptor.Properties.TryGetValue("emphasize", out var json) && json.ValueKind == JsonValueKind.True;

    /// <summary>
    /// Устанавливает флаг визуального выделения scope на форме согласия
    /// </summary>
    /// <param name="descriptor">Дескриптор scope OpenIddict</param>
    /// <param name="value">True для визуального выделения scope</param>
    public static void SetEmphasize(this OpenIddictScopeDescriptor descriptor, bool value) =>
        descriptor.Properties["emphasize"] = JsonDocument.Parse(value ? "true" : "false").RootElement;

    /// <summary>
    /// Проверяет, является ли scope обязательным для запрашиваемой операции
    /// </summary>
    /// <param name="descriptor">Дескриптор scope OpenIddict</param>
    /// <returns>True если scope обязательный</returns>
    public static bool GetRequired(this OpenIddictScopeDescriptor descriptor) =>
        descriptor.Properties.TryGetValue("required", out var json) && json.ValueKind == JsonValueKind.True;

    /// <summary>
    /// Устанавливает флаг обязательности scope
    /// </summary>
    /// <param name="descriptor">Дескриптор scope OpenIddict</param>
    /// <param name="value">True если scope обязательный</param>
    public static void SetRequired(this OpenIddictScopeDescriptor descriptor, bool value) =>
        descriptor.Properties["required"] = JsonDocument.Parse(value ? "true" : "false").RootElement;

    /// <summary>
    /// Проверяет, является ли scope identity scope (scope идентификации пользователя)
    /// </summary>
    /// <param name="descriptor">Дескриптор scope OpenIddict</param>
    /// <returns>True если scope относится к идентификации пользователя</returns>
    public static bool IsIdentityScope(this OpenIddictScopeDescriptor descriptor) =>
        descriptor.Name is OpenIddictConstants.Scopes.OpenId
                       or OpenIddictConstants.Scopes.Profile
                       or OpenIddictConstants.Scopes.Email
                       or OpenIddictConstants.Scopes.Roles
                       or OpenIddictConstants.Scopes.OfflineAccess;

    /// <summary>
    /// Получает локализованное отображаемое имя scope
    /// </summary>
    /// <param name="descriptor">Дескриптор scope OpenIddict</param>
    /// <param name="culture">Культура для локализации</param>
    /// <returns>Локализованное имя или техническое имя если локализация не найдена</returns>
    public static string? GetDisplayName(this OpenIddictScopeDescriptor descriptor, CultureInfo culture) =>
        descriptor.DisplayNames.TryGetValue(culture, out var display) ? display : descriptor.Name;

    /// <summary>
    /// Получает локализованное описание scope
    /// </summary>
    /// <param name="descriptor">Дескриптор scope OpenIddict</param>
    /// <param name="culture">Культура для локализации</param>
    /// <returns>Локализованное описание или null если не задано</returns>
    public static string? GetDescription(this OpenIddictScopeDescriptor descriptor, CultureInfo culture) =>
        descriptor.Descriptions.GetValueOrDefault(culture);
    
    /// <summary>
    /// Устанавливает описание авторизации
    /// </summary>
    /// <param name="descriptor">Дескриптор авторизации OpenIddict</param>
    /// <param name="description">Текстовое описание авторизации</param>
    public static void SetDescription(this OpenIddictAuthorizationDescriptor descriptor, string description) =>
        descriptor.Properties["description"] = JsonDocument.Parse($"\"{description}\"").RootElement;

    /// <summary>
    /// Получает описание авторизации
    /// </summary>
    /// <param name="descriptor">Дескриптор авторизации OpenIddict</param>
    /// <returns>Текстовое описание авторизации или null, если описание не установлено</returns>
    public static string? GetDescription(this OpenIddictAuthorizationDescriptor descriptor) =>
        descriptor.Properties.TryGetValue("description", out var json) && json.ValueKind == JsonValueKind.String
            ? json.GetString()
            : null;
}
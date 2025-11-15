using System;
using System.Collections.Generic;
using System.Globalization;

namespace Identix.Infrastructure.Common.DatabaseInitialization.Models;

/// <summary>Приложение OpenID.</summary>
internal class OpenIdApplication
{
    /// <summary>Тип приложения.</summary>
    public string? ApplicationType { get; init; }

    /// <summary>Идентификатор клиента.</summary>
    public required string ClientId { get; init; }

    /// <summary>Секрет клиента.</summary>
    public string? ClientSecret { get; init; }

    /// <summary>Тип клиента.</summary>
    public string? ClientType { get; init; }

    /// <summary>Тип согласия.</summary>
    public string? ConsentType { get; init; }

    /// <summary>Отображаемое имя.</summary>
    public string? DisplayName { get; init; }

    /// <summary>Ключ логотипа.</summary>
    public string? LogoKey { get; init; }
    
    /// <summary>Локализованные отображаемые имена.</summary>
    public Dictionary<CultureInfo, string> DisplayNames { get; init; } = [];

    /// <summary>Разрешения приложения.</summary>
    public HashSet<string> Permissions { get; init; } = new(StringComparer.Ordinal);

    /// <summary>URI перенаправления после выхода.</summary>
    public HashSet<Uri> PostLogoutRedirectUris { get; init; } = [];
    
    /// <summary>URI перенаправления.</summary>
    public HashSet<Uri> RedirectUris { get; init; } = [];

    /// <summary>Требования приложения.</summary>
    public HashSet<string> Requirements { get; init; } = new(StringComparer.Ordinal);

    /// <summary>Настройки приложения.</summary>
    public Dictionary<string, string> Settings { get; init; } = new(StringComparer.Ordinal);
}